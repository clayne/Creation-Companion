﻿using System.Text.RegularExpressions;
using CreationEditor.Services.Environment;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
namespace LeveledList.Services.LeveledList;

public partial class EnchantmentProvider(ILinkCacheProvider linkCacheProvider) {
    [GeneratedRegex(@"\d+$")]
    public static partial Regex EnchantmentRegex { get; }

    public int GetEnchantmentLevel(IMajorRecordGetter record) {
        switch (record) {
            case IEnchantableGetter enchantable:
                if (enchantable.ObjectEffect.IsNull) return -1;

                var enchantment = enchantable.ObjectEffect.TryResolve(linkCacheProvider.LinkCache);
                if (enchantment is null) return -1;

                var editorId = enchantment.EditorID;
                if (editorId is null) return 0;

                var match = EnchantmentRegex.Match(editorId);
                if (!match.Success) return 0;

                if (!int.TryParse(match.Value, out var level)) return -1;

                return level;
            default:
                return -1;
        }
    }
}
