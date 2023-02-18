﻿using System;
using System.Reactive;
using Avalonia.Controls;
using CreationEditor.Avalonia.Services.Record.Editor;
using CreationEditor.Avalonia.ViewModels;
using CreationEditor.Avalonia.ViewModels.Record.Editor;
using CreationEditor.Services.Environment;
using CreationEditor.Skyrim.Avalonia.Models.Record.Editor.MajorRecord;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using FactionEditor = CreationEditor.Skyrim.Avalonia.Views.Record.Editor.MajorRecord.Faction.FactionEditor;
namespace CreationEditor.Skyrim.Avalonia.ViewModels.Record.Editor.MajorRecord.Faction;

public sealed class FactionEditorVM : ViewModel, IRecordEditorVM<Mutagen.Bethesda.Skyrim.Faction, IFactionGetter> {
    IMajorRecordGetter IRecordEditorVM.Record => Record;
    public Mutagen.Bethesda.Skyrim.Faction Record { get; set; } = null!;
    [Reactive] public EditableFaction EditableRecord { get; set; } = null!;

    [Reactive] public ILinkCache LinkCache { get; set; }

    public ReactiveCommand<Unit, Unit> Save { get; }

    public RelationEditorVM RelationEditorVM { get; set; } = null!;
    public RankEditorVM RankEditorVM { get; set; } = null!;

    public FactionEditorVM(
        IRecordEditorController recordEditorController,
        IEditorEnvironment editorEnvironment) {

        LinkCache = editorEnvironment.LinkCache;

        editorEnvironment.LinkCacheChanged.Subscribe(newLinkCache => LinkCache = newLinkCache);
        
        Save = ReactiveCommand.Create(() => {
            EditableRecord.SetTo(Record);
            
            recordEditorController.CloseEditor(Record);
        });
    }
    
    public Control CreateControl(Mutagen.Bethesda.Skyrim.Faction record) {
        Record = record;
        EditableRecord = new EditableFaction(record);
        RelationEditorVM = new RelationEditorVM(this);
        RankEditorVM = new RankEditorVM(this);
        
        return new FactionEditor(this);
    }
}