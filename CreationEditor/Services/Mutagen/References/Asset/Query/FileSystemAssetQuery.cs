﻿using System.Collections.Concurrent;
using System.IO.Abstractions;
using CreationEditor.Services.Cache.Validation;
using CreationEditor.Services.FileSystem.Validation;
using CreationEditor.Services.Mutagen.References.Asset.Cache;
using CreationEditor.Services.Mutagen.References.Asset.Cache.Serialization;
using CreationEditor.Services.Mutagen.References.Asset.Parser;
using Mutagen.Bethesda.Assets;
using Mutagen.Bethesda.Environments.DI;
using Serilog;
namespace CreationEditor.Services.Mutagen.References.Asset.Query;

public sealed class FileSystemAssetQuery(
    Func<string, IFileSystemValidation> fileSystemValidationFactory,
    ILogger logger,
    IFileSystem fileSystem,
    IDataDirectoryProvider dataDirectoryProvider,
    IFileAssetParser fileAssetParser,
    IAssetReferenceSerialization<string, DataRelativePath> serialization)
    : IAssetReferenceCacheableQuery<string, DataRelativePath>, IAssetReferenceCacheableValidatableQuery<string, DataRelativePath> {
    private const string WildcardSearchPattern = "*";

    public Version CacheVersion { get; } = new(1, 0);
    public IAssetReferenceSerialization<string, DataRelativePath> Serialization { get; } = serialization;
    public IInternalCacheValidation<string, DataRelativePath> CacheValidation { get; } = fileSystemValidationFactory(fileAssetParser.FilterPattern);
    public string QueryName => fileAssetParser.Name;
    public IDictionary<string, AssetReferenceCache<string, DataRelativePath>> AssetCaches { get; }
        = new ConcurrentDictionary<string, AssetReferenceCache<string, DataRelativePath>>();

    public string ReferenceToSource(DataRelativePath reference) => reference.Path;
    public IEnumerable<AssetQueryResult<DataRelativePath>> ParseAssets(string path) {
        var isFilePath = fileSystem.Path.HasExtension(path);

        // If the path is not rooted, combine it with the data directory
        if (!fileSystem.Path.IsPathRooted(path)) {
            path = fileSystem.Path.Combine(dataDirectoryProvider.Path, path);
        }

        // For files, just parse the file
        if (isFilePath) {
            foreach (var result in fileAssetParser.ParseFile(path)) yield return result;

            yield break;
        }

        // For directories, parse all files in the directory and all subdirectories
        foreach (var file in fileSystem.Directory.EnumerateFiles(path, WildcardSearchPattern, SearchOption.AllDirectories)) {
            foreach (var result in fileAssetParser.ParseFile(file)) {
                yield return result;
            }
        }
    }

    public void WriteCacheValidation(BinaryWriter writer, string source) {}

    public void WriteContext(BinaryWriter writer, string source) => writer.Write(source);

    public void WriteReferences(BinaryWriter writer, IEnumerable<DataRelativePath> references) {
        foreach (var usage in references) {
            writer.Write(usage.Path);
        }
    }

    public bool IsCacheUpToDate(BinaryReader reader, string source) => true;

    public string ReadContextString(BinaryReader reader) => reader.ReadString();

    public IEnumerable<DataRelativePath> ReadReferences(BinaryReader reader, string contextString, int assetReferenceCount) {
        for (var i = 0; i < assetReferenceCount; i++) {
            yield return reader.ReadString();
        }
    }
}
