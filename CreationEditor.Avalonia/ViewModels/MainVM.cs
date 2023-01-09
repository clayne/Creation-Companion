﻿using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Autofac;
using Avalonia.Controls;
using CreationEditor.Avalonia.Extension;
using CreationEditor.Avalonia.Models;
using CreationEditor.Avalonia.Services;
using CreationEditor.Avalonia.Services.Busy;
using CreationEditor.Avalonia.Services.Docking;
using CreationEditor.Avalonia.ViewModels.Mod;
using CreationEditor.Avalonia.ViewModels.Setting;
using CreationEditor.Avalonia.Views.Mod;
using CreationEditor.Avalonia.Views.Setting;
using CreationEditor.Environment;
using CreationEditor.Notification;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
namespace CreationEditor.Avalonia.ViewModels;

public sealed class MainVM : ViewModel {
    private readonly IDockFactory _dockFactory;
    private const string BaseWindowTitle = "Creation Editor";

    public INotificationVM NotificationVM { get; }
    public IBusyService BusyService { get; }
    
    [Reactive] public string WindowTitle { get; set; } = BaseWindowTitle;

    public IDockingManagerService DockingManagerService { get; }

    public ReactiveCommand<Window, Unit> OpenSelectMods { get; }
    public ReactiveCommand<Window, Unit> OpenSettings { get; }
    public ReactiveCommand<Unit, Unit> Save { get; }
    
    public ReactiveCommand<Unit, Unit> OpenLog { get; }
    public ReactiveCommand<Unit, Unit> OpenRecordBrowser { get; }

    public MainVM(
        IComponentContext componentContext,
        INotificationVM notificationVM,
        IBusyService busyService,
        IEditorEnvironment editorEnvironment,
        ModSelectionVM modSelectionVM,
        IDockingManagerService dockingManagerService,
        IDockFactory dockFactory) {
        _dockFactory = dockFactory;
        NotificationVM = notificationVM;
        BusyService = busyService;
        DockingManagerService = dockingManagerService;

        OpenSelectMods = ReactiveCommand.Create<Window>(window => {
            var modSelectionWindow = new ModSelectionWindow(modSelectionVM);
            modSelectionWindow.ShowDialog(window);
        });

        OpenSettings = ReactiveCommand.Create<Window>(window => {
            var settingsWindow = new SettingsWindow(componentContext.Resolve<ISettingsVM>());
            settingsWindow.ShowDialog(window);
        });

        Save = ReactiveCommand.Create(() => {});

        OpenLog = ReactiveCommand.Create(() => _dockFactory.Open(DockElement.Log));

        OpenRecordBrowser = ReactiveCommand.Create(() => _dockFactory.Open(DockElement.RecordBrowser));

        editorEnvironment.ActiveModChanged
            .Subscribe(activeMod => {
                WindowTitle = $"{BaseWindowTitle} - {activeMod}";
            });
    }
}