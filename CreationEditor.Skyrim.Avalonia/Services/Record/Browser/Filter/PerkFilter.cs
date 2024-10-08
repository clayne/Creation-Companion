﻿using CreationEditor.Avalonia.Services.Record.Browser.Filter;
using Mutagen.Bethesda.Skyrim;
namespace CreationEditor.Skyrim.Avalonia.Services.Record.Browser.Filter;

public sealed class PerkFilter : SimpleRecordFilter<IPerkGetter> {
    public PerkFilter() : base(new List<SimpleRecordFilterEntry> {
        new("Playable", record => record.Playable),
    }) {}
}
