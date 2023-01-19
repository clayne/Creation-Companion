﻿using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
namespace CreationEditor.Avalonia.Models.Record;

public class ReferencedRecord<TMajorRecord, TMajorRecordGetter> : ReactiveObject, IReferencedRecord
    where TMajorRecord : IMajorRecordIdentifier
    where TMajorRecordGetter : IMajorRecordIdentifier {

    IMajorRecordGetter IReferencedRecord.Record {
        get => (IMajorRecordGetter) Record;
        set {
            if (value is TMajorRecordGetter tMajor) Record = tMajor;
        }
    }
    
    IMajorRecordIdentifier IReferencedRecordIdentifier.Record {
        get => Record;
        set {
            if (value is TMajorRecordGetter tMajor) Record = tMajor;
        }
    }

    public override bool Equals(object? obj) {
        return obj switch {
            ReferencedRecord<TMajorRecord, TMajorRecordGetter> referencedRecord => referencedRecord.Equals(this),
            TMajorRecordGetter record => record.FormKey.Equals(Record.FormKey),
            _ => false
        };
    }

    private bool Equals(ReferencedRecord<TMajorRecord, TMajorRecordGetter> other) {
        return other.Record.FormKey.Equals(Record.FormKey);
    }
    
    public override int GetHashCode() {
        return HashCode.Combine(Record.FormKey);
    }

    [Reactive] public TMajorRecordGetter Record { get; set; }
    public HashSet<IFormLinkIdentifier> References { get; set; }

    public ReferencedRecord(TMajorRecordGetter record, HashSet<IFormLinkIdentifier>? references = null) {
        Record = record;
        References = references ?? new HashSet<IFormLinkIdentifier>();
    }
}