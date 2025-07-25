﻿using System.Reactive.Disposables;
using System.Reactive.Linq;
using CreationEditor.Services.Asset;
using CreationEditor.Services.DataSource;
using CreationEditor.Services.Environment;
using CreationEditor.Services.Mutagen.FormLink;
using CreationEditor.Services.Mutagen.References.Cache;
using CreationEditor.Services.Mutagen.References.Parser;
using CreationEditor.Services.Mutagen.References.Query;
using CreationEditor.Services.Mutagen.Type;
using CreationEditor.Services.Notification;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Assets;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Records;
using ScriptFileParser = CreationEditor.Services.Mutagen.References.Parser.ScriptFileParser;
namespace CreationEditor.Services.Mutagen.References;

public sealed class ReferenceService : IReferenceService {
    private readonly IEditorEnvironment _editorEnvironment;
    private readonly IMutagenCommonAspectsProvider _mutagenCommonAspectsProvider;

    // Asset reference controllers
    private readonly ReferenceController<IModGetter, RecordModPair, AssetReferenceCache<IFormLinkIdentifier>, IAssetLinkGetter, IFormLinkIdentifier, IReferencedAsset> _recordAssetReferenceController;
    private readonly ReferenceController<IDataSource, DataSourceFileLink, AssetReferenceCache<DataRelativePath>, IAssetLinkGetter, DataRelativePath, IReferencedAsset> _nifTextureReferenceController;
    private readonly ReferenceController<IDataSource, DataSourceFileLink, AssetReferenceCache<DataRelativePath>, IAssetLinkGetter, DataRelativePath, IReferencedAsset> _scriptAssetReferenceController;

    // Record reference controllers
    private readonly ReferenceController<IModGetter, RecordModPair, RecordReferenceCache, IFormLinkIdentifier, IFormLinkIdentifier, IReferencedRecord> _recordReferenceController;
    private readonly ReferenceController<IDataSource, DataSourceFileLink, AssetDictionaryReferenceCache<string>, string, DataRelativePath, IReferencedRecord> _nifSoundReferenceController;
    private readonly ReferenceController<IDataSource, DataSourceFileLink, AssetDictionaryReferenceCache<int>, int, DataRelativePath, IReferencedRecord> _nifAddonNodeReferenceController;

    private readonly ReferenceSubscriptionManager<IFormLinkIdentifier, IFormLinkIdentifier, IReferencedRecord> _recordReferenceSubscriptionManager;
    private readonly ReferenceSubscriptionManager<string, DataRelativePath, IReferencedRecord> _assetRecordReferenceSubscriptionManager;
    private readonly ReferenceSubscriptionManager<int, DataRelativePath, IReferencedRecord> _nifAddonNodeReferenceSubscriptionManager;
    private readonly ReferenceSubscriptionManager<IAssetLinkGetter, IFormLinkIdentifier, IReferencedAsset> _recordAssetReferenceSubscriptionManager;
    private readonly ReferenceSubscriptionManager<IAssetLinkGetter, DataRelativePath, IReferencedAsset> _assetReferenceSubscriptionManager;

    public IObservable<bool> IsLoading { get; }
    public IObservable<bool> IsLoadingAssetReferences { get; }
    public IObservable<bool> IsLoadingRecordReferences { get; }

    public ReferenceService(
        IEditorEnvironment editorEnvironment,
        IDataSourceService dataSourceService,
        INotificationService notificationService,
        IMutagenCommonAspectsProvider mutagenCommonAspectsProvider,
        // Update Triggers
        ModReferenceUpdateTrigger<RecordReferenceCache, IFormLinkIdentifier, IReferencedRecord> modReferenceUpdateTrigger,
        ModReferenceUpdateTrigger<AssetReferenceCache<IFormLinkIdentifier>, IAssetLinkGetter, IReferencedAsset> modAssetReferenceUpdateTrigger,
        DataSourceReferenceUpdateTrigger<AssetReferenceCache<DataRelativePath>, IAssetLinkGetter, IReferencedAsset> dataSourceReferenceUpdateTrigger,
        DataSourceReferenceUpdateTrigger<AssetDictionaryReferenceCache<int>, int, IReferencedRecord> addonNodeReferenceUpdateTrigger,
        DataSourceReferenceUpdateTrigger<AssetDictionaryReferenceCache<string>, string, IReferencedRecord> soundRecordReferenceUpdateTrigger,
        // Cache Controllers
        RecordReferenceCacheController recordReferenceCacheController,
        AssetReferenceCacheController<IModGetter, IFormLinkIdentifier> recordAssetReferenceCacheController,
        AssetReferenceCacheController<IDataSource, DataRelativePath> assetAssetReferenceCacheController,
        DictionaryRecordReferenceCacheController<string> stringDictionaryRecordReferenceCacheController,
        DictionaryRecordReferenceCacheController<int> intDictionaryRecordReferenceCacheController,
        // Query Configs
        RecordReferenceQueryConfig recordReferenceQueryConfig,
        RecordAssetReferenceQueryConfig recordAssetReferenceQueryConfig,
        DictionaryAssetReferenceQueryConfig<NifAddonNodeLinkParser, AssetDictionaryReferenceCache<int>, int> nifAddonNodeReferenceQueryConfig,
        DictionaryAssetReferenceQueryConfig<NifSoundLinkParser, AssetDictionaryReferenceCache<string>, string> nifSoundReferenceQueryConfig,
        AssetReferenceCacheQueryConfig<NifTextureParser> nifTextureReferenceQueryConfig,
        AssetReferenceCacheQueryConfig<ScriptFileParser> scriptAssetReferenceQueryConfig) {
        _editorEnvironment = editorEnvironment;
        _mutagenCommonAspectsProvider = mutagenCommonAspectsProvider;

        _recordReferenceSubscriptionManager =
            new ReferenceSubscriptionManager<IFormLinkIdentifier, IFormLinkIdentifier, IReferencedRecord>(
                record => editorEnvironment.LinkCache.ResolveMod(record.Record.FormKey.ModKey) is null,
                (record, change) => record.RecordReferences.Apply(change, FormLinkIdentifierEqualityComparer.Instance),
                (record, newData) => record.RecordReferences.AddRange(newData),
                asset => asset.Record);

        _assetRecordReferenceSubscriptionManager = new ReferenceSubscriptionManager<string, DataRelativePath, IReferencedRecord>(
            record => editorEnvironment.LinkCache.ResolveMod(record.Record.FormKey.ModKey) is null,
            (record, change) => record.AssetReferences.Apply(change),
            (record, newData) => record.AssetReferences.AddRange(newData),
            record => record.Record.EditorID ?? string.Empty);

        _nifAddonNodeReferenceSubscriptionManager = new ReferenceSubscriptionManager<int, DataRelativePath, IReferencedRecord>(
            record => editorEnvironment.LinkCache.ResolveMod(record.Record.FormKey.ModKey) is null,
            (record, change) => record.AssetReferences.Apply(change),
            (record, newData) => record.AssetReferences.AddRange(newData),
            record => mutagenCommonAspectsProvider.GetAddonNodeIndex(record.Record) ?? -1);

        _recordAssetReferenceSubscriptionManager = new ReferenceSubscriptionManager<IAssetLinkGetter, IFormLinkIdentifier, IReferencedAsset>(
            asset => !dataSourceService.FileExists(asset.AssetLink.DataRelativePath),
            (asset, change) => asset.RecordReferences.Apply(change),
            (record, newData) => record.RecordReferences.AddRange(newData),
            asset => asset.AssetLink,
            AssetLinkEqualityComparer.Instance);

        _assetReferenceSubscriptionManager = new ReferenceSubscriptionManager<IAssetLinkGetter, DataRelativePath, IReferencedAsset>(
            asset => !dataSourceService.FileExists(asset.AssetLink.DataRelativePath),
            (asset, change) => asset.AssetReferences.Apply(change),
            (record, newData) => record.AssetReferences.AddRange(newData),
            asset => asset.AssetLink,
            AssetLinkEqualityComparer.Instance);

        _assetReferenceSubscriptionManager = new ReferenceSubscriptionManager<IAssetLinkGetter, DataRelativePath, IReferencedAsset>(
            asset => !dataSourceService.FileExists(asset.AssetLink.DataRelativePath),
            (asset, change) => asset.AssetReferences.Apply(change),
            (record, newData) => record.AssetReferences.AddRange(newData),
            asset => asset.AssetLink,
            AssetLinkEqualityComparer.Instance);

        _recordReferenceController =
            new ReferenceController<IModGetter, RecordModPair, RecordReferenceCache, IFormLinkIdentifier, IFormLinkIdentifier, IReferencedRecord>(
                notificationService,
                modReferenceUpdateTrigger,
                recordReferenceCacheController,
                recordReferenceQueryConfig,
                _recordReferenceSubscriptionManager);

        _recordAssetReferenceController =
            new ReferenceController<IModGetter, RecordModPair, AssetReferenceCache<IFormLinkIdentifier>, IAssetLinkGetter, IFormLinkIdentifier,
                IReferencedAsset>(
                notificationService,
                modAssetReferenceUpdateTrigger,
                recordAssetReferenceCacheController,
                recordAssetReferenceQueryConfig,
                _recordAssetReferenceSubscriptionManager);

        _nifAddonNodeReferenceController =
            new ReferenceController<IDataSource, DataSourceFileLink, AssetDictionaryReferenceCache<int>, int, DataRelativePath, IReferencedRecord>(
                notificationService,
                addonNodeReferenceUpdateTrigger,
                intDictionaryRecordReferenceCacheController,
                nifAddonNodeReferenceQueryConfig,
                _nifAddonNodeReferenceSubscriptionManager);

        _nifTextureReferenceController =
            new ReferenceController<IDataSource, DataSourceFileLink, AssetReferenceCache<DataRelativePath>, IAssetLinkGetter, DataRelativePath,
                IReferencedAsset>(
                notificationService,
                dataSourceReferenceUpdateTrigger,
                assetAssetReferenceCacheController,
                nifTextureReferenceQueryConfig,
                _assetReferenceSubscriptionManager);

        _nifSoundReferenceController =
            new ReferenceController<IDataSource, DataSourceFileLink, AssetDictionaryReferenceCache<string>, string, DataRelativePath, IReferencedRecord>(
                notificationService,
                soundRecordReferenceUpdateTrigger,
                stringDictionaryRecordReferenceCacheController,
                nifSoundReferenceQueryConfig,
                _assetRecordReferenceSubscriptionManager);

        _scriptAssetReferenceController =
            new ReferenceController<IDataSource, DataSourceFileLink, AssetReferenceCache<DataRelativePath>, IAssetLinkGetter, DataRelativePath,
                IReferencedAsset>(
                notificationService,
                dataSourceReferenceUpdateTrigger,
                assetAssetReferenceCacheController,
                scriptAssetReferenceQueryConfig,
                _assetReferenceSubscriptionManager);

        IsLoadingAssetReferences = _recordAssetReferenceController.IsLoading
            .CombineLatest(_nifTextureReferenceController.IsLoading,
                _scriptAssetReferenceController.IsLoading,
                (a, b, c) => a || b || c);

        IsLoadingRecordReferences = _recordReferenceController.IsLoading
            .CombineLatest(_nifSoundReferenceController.IsLoading,
                _nifAddonNodeReferenceController.IsLoading,
                (a, b, c) => a || b || c);

        IsLoading = IsLoadingAssetReferences.CombineLatest(IsLoadingRecordReferences, (a, b) => a || b);
    }

    public IDisposable GetReferencedAsset(IAssetLinkGetter asset, out IReferencedAsset referencedAsset) {
        var recordAssetReferences = _recordAssetReferenceController.GetReferences(asset);
        var nifTextureReferences = _nifTextureReferenceController.GetReferences(asset);
        var scriptReferences = _scriptAssetReferenceController.GetReferences(asset);
        referencedAsset = new ReferencedAsset(asset, recordAssetReferences, nifTextureReferences.Concat(scriptReferences));

        var recordDisposable = _recordAssetReferenceSubscriptionManager.Register(referencedAsset);
        var assetDisposable = _assetReferenceSubscriptionManager.Register(referencedAsset);

        return new CompositeDisposable(recordDisposable, assetDisposable);
    }

    public IDisposable GetReferencedRecord<TMajorRecordGetter>(
        TMajorRecordGetter record,
        out IReferencedRecord<TMajorRecordGetter> outReferencedRecord)
        where TMajorRecordGetter : IMajorRecordGetter {
        var recordReferences = _recordReferenceController.GetReferences(record);
        var editorId = record.EditorID;
        var assetReferences = editorId is not null ? _nifSoundReferenceController.GetReferences(editorId) : [];
        var addonNodeIndex = _mutagenCommonAspectsProvider.GetAddonNodeIndex(record);
        if (addonNodeIndex is not null) {
            assetReferences = assetReferences.Concat(_nifAddonNodeReferenceController.GetReferences(addonNodeIndex.Value));
        }

        outReferencedRecord = new ReferencedRecord<TMajorRecordGetter>(record, recordReferences, assetReferences);

        var nifAddonNodeDisposable = _nifAddonNodeReferenceSubscriptionManager.Register(outReferencedRecord);
        var assetRecordDisposable = _assetRecordReferenceSubscriptionManager.Register(outReferencedRecord);
        var recordDisposable = _recordReferenceSubscriptionManager.Register(outReferencedRecord);
        return new CompositeDisposable(nifAddonNodeDisposable, assetRecordDisposable, recordDisposable);
    }

    public IEnumerable<IFormLinkIdentifier> GetRecordReferences(IFormLinkIdentifier formLink) {
        return _recordReferenceController.GetReferences(formLink);
    }

    public IEnumerable<IFormLinkIdentifier> GetRecordReferences(IAssetLinkGetter assetLink) {
        return _recordAssetReferenceController.GetReferences(assetLink);
    }

    public IEnumerable<DataRelativePath> GetAssetReferences(IFormLinkIdentifier formLink) {
        if (_editorEnvironment.LinkCache.TryResolve(formLink.FormKey, _mutagenCommonAspectsProvider.AddonNodeRecordType, out var addonNode)) {
            // Record is an addon node
            var addonNodeIndex = _mutagenCommonAspectsProvider.GetAddonNodeIndex(addonNode);
            if (addonNodeIndex is not null) {
                return _nifAddonNodeReferenceController.GetReferences(addonNodeIndex.Value);
            }
        } else if (_editorEnvironment.LinkCache.TryResolve(formLink.FormKey, _mutagenCommonAspectsProvider.SoundDescriptorRecordType, out var soundDescriptor)
         && soundDescriptor is { EditorID: {} editorId }) {
            // Record is a sound
            return _nifSoundReferenceController.GetReferences(editorId);
        }

        return [];
    }

    public IEnumerable<DataRelativePath> GetAssetReferences(IAssetLinkGetter assetLink) {
        var nifTextureReferences = _nifTextureReferenceController.GetReferences(assetLink);
        var scriptReferences = _scriptAssetReferenceController.GetReferences(assetLink);

        return nifTextureReferences.Concat(scriptReferences);
    }

    public IEnumerable<string> GetMissingRecordLinks(DataSourceFileLink fileFileLink) {
        var addonNodeIndices = _nifAddonNodeReferenceController.GetLinks(fileFileLink).ToHashSet();

        if (addonNodeIndices.Count > 0) {
            foreach (var addonNode in
                _editorEnvironment.LinkCache.PriorityOrder.WinningOverrides(_mutagenCommonAspectsProvider.AddonNodeRecordType)) {
                var addonNodeIndex = _mutagenCommonAspectsProvider.GetAddonNodeIndex(addonNode);
                if (addonNodeIndex is null) continue;

                addonNodeIndices.Remove(addonNodeIndex.Value);
            }

            foreach (var addonNodeIndex in addonNodeIndices) {
                yield return "Addon Node: " + addonNodeIndex;
            }
        }

        var soundEditorIds = _nifSoundReferenceController.GetLinks(fileFileLink).ToArray();
        if (soundEditorIds.Length > 0) {
            foreach (var soundEditorId in soundEditorIds) {
                if (_editorEnvironment.LinkCache.TryResolve(soundEditorId, _mutagenCommonAspectsProvider.SoundDescriptorRecordType, out _)) continue;

                yield return "Sound: " + soundEditorId;
            }
        }
    }

    public IEnumerable<IAssetLinkGetter> GetAssetLinks(DataSourceFileLink fileFileLink) {
        return _nifTextureReferenceController.GetLinks(fileFileLink)
            .Concat(_scriptAssetReferenceController.GetLinks(fileFileLink));
    }

    public int GetReferenceCount(IAssetLink assetLink) {
        return GetRecordReferences(assetLink).Count() + GetAssetReferences(assetLink).Count();
    }

    public int GetReferenceCount(IFormLinkIdentifier formLink) {
        return GetRecordReferences(formLink).Count() + GetAssetReferences(formLink).Count();
    }
}
