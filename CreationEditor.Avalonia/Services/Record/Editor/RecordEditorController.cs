﻿using System.Reactive.Disposables;
using Autofac;
using Avalonia.Controls;
using CreationEditor.Avalonia.Models.Docking;
using CreationEditor.Avalonia.Services.Docking;
using CreationEditor.Avalonia.ViewModels.Record.Editor;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Serilog;
namespace CreationEditor.Avalonia.Services.Record.Editor;

using EditorControl = (Control Control, IRecordEditorVM EditorVM);

public sealed class RecordEditorController : IRecordEditorController, IDisposable {
    private readonly CompositeDisposable _disposable = new();
    private readonly ILogger _logger;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly IDockingManagerService _dockingManagerService;
    private readonly IRecordEditorFactory _recordEditorFactory;

    private readonly Dictionary<FormKey, EditorControl> _openRecordEditors = new();

    public RecordEditorController(
        ILogger logger,
        ILifetimeScope lifetimeScope,
        IDockingManagerService dockingManagerService,
        IRecordEditorFactory recordEditorFactory) {
        _logger = logger;
        _lifetimeScope = lifetimeScope.DisposeWith(_disposable);
        _dockingManagerService = dockingManagerService;
        _recordEditorFactory = recordEditorFactory;

        _dockingManagerService.Closed
            .Subscribe(OnClosed)
            .DisposeWith(_disposable);
    }

    public bool AnyEditorsOpen() => _openRecordEditors.Count > 0;

    public void OpenEditor<TMajorRecord, TMajorRecordGetter>(TMajorRecord record)
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter
        where TMajorRecordGetter : class, IMajorRecordGetter {

        if (_openRecordEditors.TryGetValue(record.FormKey, out var editorControl)) {
            // Select editor as active
            _dockingManagerService.Focus(editorControl.Control);
        } else {
            // Open new editor
            if (_lifetimeScope.TryResolve<IRecordEditorVM<TMajorRecord, TMajorRecordGetter>>(out var recordEditorVM)) {
                var control = recordEditorVM.CreateControl(record);

                _dockingManagerService.AddControl(
                    control,
                    new DockConfig {
                        DockInfo = new DockInfo {
                            Header = record.EditorID ?? record.FormKey.ToString(),
                        },
                        Dock = Dock.Right,
                        DockMode = DockMode.Document,
                        GridSize = new GridLength(2, GridUnitType.Star),
                    });
                _openRecordEditors.Add(record.FormKey, new EditorControl(control, recordEditorVM));
            } else {
                _logger.Here().Warning("Cannot open record editor of type {Type} because no such editor is available", typeof(TMajorRecord));
            }
        }
    }

    public void OpenEditor(IMajorRecord record) {
        if (_openRecordEditors.TryGetValue(record.FormKey, out var editorControl)) {
            // Select editor as active
            _dockingManagerService.Focus(editorControl.Control);
        } else {
            // Open new editor
            if (_recordEditorFactory.FromType(record, out var control, out var editor)) {
                _dockingManagerService.AddControl(
                    control,
                    new DockConfig {
                        DockInfo = new DockInfo {
                            Header = record.EditorID ?? record.FormKey.ToString(),
                        },
                        Dock = Dock.Right,
                        DockMode = DockMode.Document,
                        GridSize = new GridLength(2, GridUnitType.Star),
                    });
                _openRecordEditors.Add(record.FormKey, new EditorControl(control, editor));
            } else {
                _logger.Here().Warning("Cannot open record editor of type {Type} because no such editor is available", record.GetType());
            }
        }
    }

    public void CloseEditor(IMajorRecord record) {
        if (_openRecordEditors.TryGetValue(record.FormKey, out var editorControl)) {
            _dockingManagerService.RemoveControl(editorControl.Control);

            RemoveEditorCache(editorControl.Control);
        }
    }

    public void CloseAllEditors() {
        foreach (var control in _openRecordEditors.Values.Select(x => x.Control)) {
            _dockingManagerService.RemoveControl(control);

            RemoveEditorCache(control);
        }

        _openRecordEditors.Clear();
    }

    private void OnClosed(IDockedItem dockedItem) {
        RemoveEditorCache(dockedItem.Control);
    }

    private void RemoveEditorCache(Control editor) {
        var editorsToRemove = _openRecordEditors
            .Where(x => ReferenceEquals(x.Value.Control, editor))
            .ToList();

        foreach (var (formKey, editorControl) in editorsToRemove) {
            editorControl.EditorVM.Save.Execute();

            _openRecordEditors.Remove(formKey);
        }
    }

    public void Dispose() => _disposable.Dispose();
}
