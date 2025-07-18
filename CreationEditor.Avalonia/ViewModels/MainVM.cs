﻿using System.Diagnostics;
using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Linq;
using CreationEditor.Avalonia.Models;
using CreationEditor.Avalonia.Models.Docking;
using CreationEditor.Avalonia.Services;
using CreationEditor.Avalonia.Services.Busy;
using CreationEditor.Avalonia.Services.Docking;
using CreationEditor.Avalonia.Services.Plugin;
using CreationEditor.Avalonia.ViewModels.DataSource;
using CreationEditor.Avalonia.ViewModels.Mod;
using CreationEditor.Avalonia.ViewModels.Notification;
using CreationEditor.Avalonia.ViewModels.Setting;
using CreationEditor.Avalonia.Views;
using CreationEditor.Avalonia.Views.Setting;
using CreationEditor.Services.Environment;
using CreationEditor.Services.Mutagen.Mod.Save;
using CreationEditor.Services.Plugin;
using DynamicData.Binding;
using FluentAvalonia.UI.Windowing;
using Noggog;
using ReactiveUI;
namespace CreationEditor.Avalonia.ViewModels;

public sealed class MainVM : ViewModel {
    private readonly IEditorEnvironment _editorEnvironment;
    private const string BaseWindowTitle = "Creation Companion";

    public INotificationVM NotificationVM { get; }
    public IBusyService BusyService { get; }
    public ModSelectionVM ModSelectionVM { get; }
    public IDataSourceSelectionVM DataSourceSelectionVM { get; }

    public IObservable<string> WindowTitleObs { get; }

    public IDockingManagerService DockingManagerService { get; }
    public IPluginService? PluginService { get; }
    public IObservableCollection<IMenuPluginDefinition> MenuBarPlugins { get; } = new ObservableCollectionExtended<IMenuPluginDefinition>();
    public IObservableCollection<string> Actions { get; } = new ObservableCollectionExtended<string> {
        "Save",
        "Save As",
        "Save All",
        "Close",
        "Close All",
        "Close All But This",
        "Exit",
        "Undo",
        "Redo",
        "Cut",
        "Copy",
        "Paste",
        "Delete",
        "Select All",
        "Find",
        "Find Next",
        "Find Previous",
        "Open Record",
        "Open Record in New Tab",
        "Open Record in New Window",
        "Create Record",
        "Create Record in New Tab",
        "Create Record in New Window",
    };

    public ReactiveCommand<IMenuPluginDefinition, Unit> OpenPlugin { get; }
    public ReactiveCommand<Unit, Unit> OpenSettings { get; }

    public ReactiveCommand<Unit, Unit> OpenGameFolder { get; }
    public ReactiveCommand<Unit, Unit> OpenDataFolder { get; }

    public ReactiveCommand<Unit, Unit> Save { get; }

    public ReactiveCommand<DockElement, Unit> OpenDockElement { get; }

    public MainVM(
        Func<ISettingsVM> settingsVMFactory,
        INotificationVM notificationVM,
        IBusyService busyService,
        IEditorEnvironment editorEnvironment,
        ModSelectionVM modSelectionVM,
        IDataSourceSelectionVM dataSourceSelectionVM,
        IDockingManagerService dockingManagerService,
        IDockFactory dockFactory,
        MainWindow mainWindow,
        IPluginService? pluginService,
        IModSaveService modSaveService,
        IApplicationSplashScreen splashScreen,
        IFileSystem fileSystem) {
        _editorEnvironment = editorEnvironment;
        NotificationVM = notificationVM;
        BusyService = busyService;
        ModSelectionVM = modSelectionVM;
        DataSourceSelectionVM = dataSourceSelectionVM;
        DockingManagerService = dockingManagerService;
        PluginService = pluginService;
        mainWindow.SplashScreen = splashScreen;

        PluginService?.PluginsRegistered
            .Subscribe(newPlugins => {
                MenuBarPlugins.AddRange(newPlugins.OfType<IMenuPluginDefinition>());
            })
            .DisposeWith(this);

        OpenPlugin = ReactiveCommand.Create<IMenuPluginDefinition>(plugin => {
            DockingManagerService.AddControl(
                plugin.GetControl(),
                new DockConfig {
                    DockInfo = new DockInfo { Header = plugin.Name },
                    DockMode = plugin.DockMode,
                    Dock = plugin.Dock,
                });
        });

        OpenGameFolder = ReactiveCommand.Create(() => {
            var gameFolder = fileSystem.Directory.GetParent(_editorEnvironment.GameEnvironment.DataFolderPath);
            if (gameFolder is not null) {
                Process.Start(new ProcessStartInfo {
                    FileName = gameFolder.FullName,
                    UseShellExecute = true,
                    Verb = "open",
                });
            }
        });

        OpenDataFolder = ReactiveCommand.Create(() => {
            Process.Start(new ProcessStartInfo {
                FileName = _editorEnvironment.GameEnvironment.DataFolderPath,
                UseShellExecute = true,
                Verb = "open",
            });
        });

        OpenSettings = ReactiveCommand.Create(() => {
            var settingsVM = settingsVMFactory();
            var settingsWindow = new SettingsWindow(settingsVM);
            settingsWindow.ShowDialog(mainWindow);
        });

        Save = ReactiveCommand.CreateRunInBackground(() => {
            Parallel.ForEach(_editorEnvironment.MutableMods, modSaveService.SaveMod);
        });

        OpenDockElement = ReactiveCommand.CreateFromTask<DockElement>(element => dockFactory.Open(element));

        WindowTitleObs = _editorEnvironment.ActiveModChanged
            .Select(activeMod => $"{BaseWindowTitle} - {activeMod}")
            .StartWith(BaseWindowTitle);
    }

    public bool IsEnvironmentUninitialized() {
        return _editorEnvironment.ActiveMod.ModKey.IsNull;    
    }
}
