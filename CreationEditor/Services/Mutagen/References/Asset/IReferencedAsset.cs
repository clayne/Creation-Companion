﻿using DynamicData.Binding;
using Mutagen.Bethesda.Assets;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
namespace CreationEditor.Services.Mutagen.References.Asset;

public interface IReferencedAsset {
    ModKey ModKey { get; }
    IAssetLinkGetter AssetLink { get; }
    IObservableCollection<IFormLinkGetter> RecordReferences { get; }
    IEnumerable<DataRelativePath> NifReferences { get; }
    IObservableCollection<DataRelativePath> NifDirectoryReferences { get; }
    IObservableCollection<DataRelativePath> NifArchiveReferences { get; }

    bool HasReferences { get; }
    IObservable<int> ReferenceCount { get; }
}
