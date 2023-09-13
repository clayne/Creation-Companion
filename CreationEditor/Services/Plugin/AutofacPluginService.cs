﻿using System.IO.Abstractions;
using System.Reflection;
using Autofac;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using Serilog;
namespace CreationEditor.Services.Plugin;

public sealed class AutofacPluginService<TMod, TModGetter> : IPluginService, IDisposable
    where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
    where TModGetter : class, IContextGetterMod<TMod, TModGetter> {
    private const string PluginsFolderName = "Plugins";

    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly List<ILifetimeScope> _pluginScopes = new();
    private readonly PluginContext _pluginContext = new(new Version(1, 0));

    public IReadOnlyList<IPluginDefinition> Plugins { get; private set; } = null!;

    public AutofacPluginService(
        IFileSystem fileSystem,
        ILogger logger,
        ILifetimeScope lifetimeScope) {
        _fileSystem = fileSystem;
        _logger = logger;
        _lifetimeScope = lifetimeScope;
    }

    public void ReloadPlugins() {
        // Get application directory
        var applicationDirectory = AppContext.BaseDirectory;

        // Get plugins folder
        var pluginsFolder = Path.Combine(applicationDirectory, PluginsFolderName);
        if (!_fileSystem.Directory.Exists(pluginsFolder)) {
            _logger.Warning("Couldn't load any plugins because the plugins folder {PluginsFolder} doesn't exist", pluginsFolder);
            return;
        }

        // Get plugin paths
        var pluginPaths = _fileSystem.Directory.GetFiles(pluginsFolder, "*.dll");
        if (pluginPaths.Length == 0) {
            _logger.Information("Couldn't load any plugins because there were no plugins in {PluginsFolder}", pluginsFolder);
            return;
        }

        // Collect plugins objects from files
        Plugins = pluginPaths
            .SelectMany(pluginPath => {
                var assembly = Assembly.LoadFrom(pluginPath);
                var plugins = assembly is null ? Array.Empty<IPlugin>() : CreatePlugins(assembly).ToArray();

                if (plugins.Length == 0) {
                    _logger.Information("Couldn't load a plugin in file {File} because none of the assembly types implement the {Interface} interface", pluginPath, nameof(IPlugin));
                }
                return plugins;
            })
            .NotNull()
            .ToList();

        foreach (var plugin in Plugins.OfType<IPlugin>()) {
            plugin.OnRegistered();
        }
    }

    private IEnumerable<IPlugin> CreatePlugins(Assembly assembly) {
        var pluginScope = _lifetimeScope.BeginLifetimeScope(c => {
            c.RegisterAssemblyModules(assembly);
            c.RegisterInstance(_pluginContext)
                .AsSelf();
        });
        _pluginScopes.Add(pluginScope);

        foreach (var type in assembly.GetTypes()) {
            if (!typeof(IPlugin).IsAssignableFrom(type)) continue;

            // Use game specific plugin if available
            var pluginType = type.IsAssignableTo(typeof(IPlugin<,>)) || type.IsGenericType
                ? type.MakeGenericType(typeof(TMod), typeof(TModGetter))
                : type;

            if (pluginScope.Resolve(pluginType) is not IPlugin result) continue;

            yield return result;
        }
    }

    private void UnregisterPlugins() {
        foreach (var plugin in Plugins.OfType<IPlugin>()) {
            plugin.OnUnregistered();
        }
    }

    public void Dispose() {
        UnregisterPlugins();

        _lifetimeScope.Dispose();
        foreach (var pluginScope in _pluginScopes) {
            pluginScope.Dispose();
        }
    }
}