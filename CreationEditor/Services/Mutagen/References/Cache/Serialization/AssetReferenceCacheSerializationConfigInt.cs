﻿using CreationEditor.Services.DataSource;
using Mutagen.Bethesda.Assets;
namespace CreationEditor.Services.Mutagen.References.Cache.Serialization;

public class AssetReferenceCacheSerializationConfigInt<TSource> : IReferenceCacheSerializationConfigLink<TSource, int, DataRelativePath>
    where TSource : IDataSource {
    public Version CacheVersion { get; } = new(1, 0);
    public bool IsCacheUpToDate(BinaryReader reader, TSource source) => true;
    public string ReadSource(BinaryReader reader) => reader.ReadString();
    public IEnumerable<DataRelativePath> ReadReferences(BinaryReader reader, string sourceContextString, int referenceCount) {
        for (var i = 0; i < referenceCount; i++) {
            var referencePath = new DataRelativePath(reader.ReadString());
            yield return referencePath;
        }
    }
    public void WriteCacheValidation(BinaryWriter writer, TSource source) {}
    public void WriteSource(BinaryWriter writer, TSource source) {
        writer.Write(source.Path);
    }
    public void WriteReferences(BinaryWriter writer, IEnumerable<DataRelativePath> references) {
        foreach (var reference in references) {
            writer.Write(reference.ToString());
        }
    }
    public int ReadLink(BinaryReader reader) => reader.ReadInt32();
    public void WriteLink(BinaryWriter writer, int link) => writer.Write(link);
}
