﻿using CreationEditor.Avalonia.Models.Record.Browser;
using CreationEditor.Avalonia.Services.Record.Browser.Filter;
using CreationEditor.Skyrim.Avalonia.Resources.Constants;
using Mutagen.Bethesda.Skyrim;
namespace CreationEditor.Skyrim.Avalonia.Services.Record.Browser.Filter;

public sealed class SpellFilter : RecordFilter<ISpellGetter> {
    public override IEnumerable<RecordFilterListing> GetListings(Type type) {
        return EnumConstants.SpellTypes
            .Select(spellType => new RecordFilterListing(spellType.ToString(),
                record => record is ISpellGetter spell && spell.Type == spellType));
    }
}
