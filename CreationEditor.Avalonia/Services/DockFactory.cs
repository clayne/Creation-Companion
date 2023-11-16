﻿using System.IO.Abstractions;
using Avalonia.Controls;
using Avalonia.Threading;
using CreationEditor.Avalonia.Models;
using CreationEditor.Avalonia.Models.Docking;
using CreationEditor.Avalonia.Services.Docking;
using CreationEditor.Avalonia.Services.Record.Browser;
using CreationEditor.Avalonia.Services.Viewport;
using CreationEditor.Avalonia.ViewModels.Asset.Browser;
using CreationEditor.Avalonia.ViewModels.Logging;
using CreationEditor.Avalonia.ViewModels.Record.Browser;
using CreationEditor.Avalonia.ViewModels.Scripting;
using CreationEditor.Avalonia.Views.Asset.Browser;
using CreationEditor.Avalonia.Views.Logging;
using CreationEditor.Avalonia.Views.Record;
using CreationEditor.Avalonia.Views.Scripting;
using Mutagen.Bethesda.Environments.DI;
using Noggog;
using Serilog;
namespace CreationEditor.Avalonia.Services;

using DockResult = (Func<Control> GetControl, DockConfig DockConfig);

public sealed class DockFactory(
    Func<ILogVM> logVMFactory,
    Func<IRecordBrowserVM> recordBrowserVMFactory,
    Func<string, IAssetBrowserVM> assetBrowserVMFactory,
    Func<IScriptVM> scriptVMFactory,
    IDataDirectoryProvider dataDirectoryProvider,
    ILogger logger,
    IFileSystem fileSystem,
    IViewportFactory viewportFactory,
    IDockingManagerService dockingManagerService,
    ICellBrowserFactory cellBrowserFactory)
    : IDockFactory {
    private bool _viewportCreated;

    public void Open(DockElement dockElement, DockMode? dockMode = null, Dock? dock = null, object? parameter = null) {
        Task.Run(async () => {
                var dockResult = await GetDock(dockElement, dockMode, dock, parameter);
                if (dockResult.HasValue) {
                    Dispatcher.UIThread.Post(() => dockingManagerService.AddControl(dockResult.Value.GetControl(), dockResult.Value.DockConfig));
                } else {
                    throw new Exception($"Failed to open dock {dockElement}");
                }
            })
            .FireAndForget(e => logger.Here().Warning("Couldn't open dock {DockElement}: {Message}", dockElement, e.Message));
    }

    private async Task<DockResult?> GetDock(DockElement dockElement, DockMode? dockMode, Dock? dock, object? parameter) {
        Func<Control> getControl;
        DockConfig dockConfig;

        switch (dockElement) {
            case DockElement.Log:
                var logVM = logVMFactory();
                getControl = () => new LogView(logVM);
                dockConfig = new DockConfig {
                    DockInfo = new DockInfo {
                        Header = "Log",
                        Size = 150,
                    },
                    Dock = Dock.Bottom,
                    DockMode = DockMode.Side,
                };
                break;
            case DockElement.RecordBrowser:
                var recordBrowserVM = recordBrowserVMFactory();
                getControl = () => new RecordBrowser(recordBrowserVM);
                dockConfig = new DockConfig {
                    DockInfo = new DockInfo {
                        Header = "Record Browser",
                        Size = 500,
                    },
                    Dock = Dock.Left,
                    DockMode = DockMode.Side,
                    GridSize = new GridLength(3, GridUnitType.Star),
                };
                break;
            case DockElement.CellBrowser:
                getControl = cellBrowserFactory.GetBrowser;
                dockConfig = new DockConfig {
                    DockInfo = new DockInfo {
                        Header = "Cell Browser",
                        Size = 400,
                    },
                    Dock = Dock.Left,
                    DockMode = DockMode.Side,
                };
                break;
            case DockElement.AssetBrowser:
                var folder = parameter as string ?? dataDirectoryProvider.Path;
                var assetBrowserVM = assetBrowserVMFactory(folder);
                getControl = () => new AssetBrowser(assetBrowserVM);
                dockConfig = new DockConfig {
                    DockInfo = new DockInfo {
                        Header = $"Asset Browser{(parameter is string str ? $" - {fileSystem.Path.GetFileName(str)}" : string.Empty)}",
                        Size = 400,
                    },
                    Dock = Dock.Left,
                    DockMode = DockMode.Side,
                };
                break;
            case DockElement.ScriptEditor:
                var vm = scriptVMFactory();
                getControl = () => new ScriptEditor(vm);
                dockConfig = new DockConfig {
                    DockInfo = new DockInfo {
                        Header = "Script Editor",
                        Size = 300,
                    },
                    Dock = Dock.Left,
                    DockMode = DockMode.Document,
                };
                break;
            case DockElement.Viewport:
                if (_viewportCreated && !viewportFactory.IsMultiInstanceCapable) return null;

                getControl = await viewportFactory.CreateViewport();
                _viewportCreated = true;

                dockConfig = new DockConfig {
                    DockInfo = new DockInfo {
                        Header = "Viewport",
                        CanClose = viewportFactory.IsMultiInstanceCapable
                    },
                    Dock = Dock.Right,
                    DockMode = DockMode.Layout,
                    GridSize = new GridLength(3, GridUnitType.Star),
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dockElement), dockElement, null);
        }

        if (dockMode is not null) dockConfig = dockConfig with { DockMode = dockMode.Value };
        if (dock is not null) dockConfig = dockConfig with { Dock = dock.Value };

        return (getControl, dockConfig);
    }
}
