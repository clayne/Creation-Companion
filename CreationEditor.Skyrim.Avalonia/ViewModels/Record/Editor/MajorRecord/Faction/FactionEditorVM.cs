﻿using System.Reactive;
using Avalonia.Controls;
using CreationEditor.Avalonia.Services.Record.Editor;
using CreationEditor.Avalonia.ViewModels;
using CreationEditor.Avalonia.ViewModels.Record.Editor;
using CreationEditor.Services.Environment;
using CreationEditor.Services.Mutagen.Record;
using CreationEditor.Skyrim.Avalonia.Models.Record.Editor.MajorRecord;
using CreationEditor.Skyrim.Avalonia.Services.Record.Editor;
using CreationEditor.Skyrim.Avalonia.Views.Record.Editor.MajorRecord.Faction;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
namespace CreationEditor.Skyrim.Avalonia.ViewModels.Record.Editor.MajorRecord.Faction;

public sealed class FactionEditorVM : ViewModel, IRecordEditorVM<Mutagen.Bethesda.Skyrim.Faction, IFactionGetter> {
    IMajorRecordGetter IRecordEditorVM.Record => Record;
    public Mutagen.Bethesda.Skyrim.Faction Record { get; set; } = null!;
    [Reactive] public EditableFaction EditableRecord { get; set; } = null!;

    public ReactiveCommand<Unit, Unit> Save { get; }

    public Func<IPlacedGetter, bool> ChestFilter { get; }
    public RelationEditorVM? RelationEditorVM { get; set; }
    public RankEditorVM? RankEditorVM { get; set; }
    public ILinkCacheProvider LinkCacheProvider { get; }
    public IConditionCopyPasteController ConditionsCopyPasteController { get; }

    public FactionEditorVM(
        IRecordEditorController recordEditorController,
        IRecordController recordController,
        ILinkCacheProvider linkCacheProvider,
        IConditionCopyPasteController conditionsCopyPasteController) {
        LinkCacheProvider = linkCacheProvider;
        ConditionsCopyPasteController = conditionsCopyPasteController;

        ChestFilter = placed => {
            if (placed is not IPlacedObjectGetter placedObject) return false;

            var placeableObjectGetter = placedObject.Base.TryResolve(LinkCacheProvider.LinkCache);
            return placeableObjectGetter is IContainerGetter;
        };

        Save = ReactiveCommand.Create(() => {
            recordController.RegisterUpdate(Record, () => EditableRecord.CopyTo(Record));

            recordEditorController.CloseEditor(Record);
        });
    }

    public Control CreateControl(Mutagen.Bethesda.Skyrim.Faction record) {
        Record = record;
        EditableRecord = new EditableFaction(record);
        RelationEditorVM?.Dispose();
        RelationEditorVM = new RelationEditorVM(this);
        RelationEditorVM.DisposeWith(this);
        RankEditorVM?.Dispose();
        RankEditorVM = new RankEditorVM(this);
        RankEditorVM.DisposeWith(this);

        return new FactionEditor(this);
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        RelationEditorVM?.Dispose();
        RankEditorVM?.Dispose();
    }
}
