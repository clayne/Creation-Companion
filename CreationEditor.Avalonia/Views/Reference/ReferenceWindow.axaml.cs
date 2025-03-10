﻿using Avalonia;
using CreationEditor.Avalonia.ViewModels.Reference;
using FluentAvalonia.UI.Windowing;
using Mutagen.Bethesda.Plugins.Records;
namespace CreationEditor.Avalonia.Views.Reference;

public partial class ReferenceWindow : AppWindow {
    public static readonly StyledProperty<ReferenceBrowserVM?> RecordReferenceBrowserVMProperty
        = AvaloniaProperty.Register<ReferenceWindow, ReferenceBrowserVM?>(nameof(ReferenceBrowserVM));

    public ReferenceBrowserVM? ReferenceBrowserVM {
        get => GetValue(RecordReferenceBrowserVMProperty);
        set => SetValue(RecordReferenceBrowserVMProperty, value);
    }

    public ReferenceWindow() {
        InitializeComponent();
    }

    public ReferenceWindow(
        string name,
        ReferenceBrowserVM? referenceBrowserVM = null) : this() {
        Title = $"References of {name}";

        ReferenceBrowserVM = referenceBrowserVM;
    }

    public ReferenceWindow(
        IMajorRecordIdentifierGetter record,
        ReferenceBrowserVM? referenceBrowserVM = null) : this() {
        var editorId = record.EditorID;
        Title = $"References of {record.FormKey}" + (editorId is null ? string.Empty : $" - {editorId}");

        ReferenceBrowserVM = referenceBrowserVM;
    }

    public ReferenceWindow(
        IEnumerable<IMajorRecordIdentifierGetter> records,
        ReferenceBrowserVM? referenceBrowserVM = null) : this() {
        var recordsIdentifiers = string.Join(
            ", ",
            records.Select(record => {
                var editorId = record.EditorID;
                return $"{record.FormKey}" + (editorId is null ? string.Empty : $" - {editorId}");
            }));

        Title = $"References of {recordsIdentifiers}";

        ReferenceBrowserVM = referenceBrowserVM;
    }
}
