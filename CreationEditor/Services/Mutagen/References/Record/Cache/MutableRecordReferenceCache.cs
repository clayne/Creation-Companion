﻿using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
namespace CreationEditor.Services.Mutagen.References.Record.Cache;

public sealed class MutableRecordReferenceCache : IRecordReferenceCache {
    private readonly IModGetter _mutableMod;
    private readonly ImmutableRecordReferenceCache? _immutableReferenceCache;
    private readonly ModReferenceCache _mutableModReferenceCache;

    internal MutableRecordReferenceCache(IModGetter mutableMod, ModReferenceCache mutableModReferenceCache, ImmutableRecordReferenceCache? immutableReferenceCache = null) {
        _mutableMod = mutableMod;
        _mutableModReferenceCache = mutableModReferenceCache;
        _immutableReferenceCache = immutableReferenceCache;
    }

    public bool AddRecord(IMajorRecordGetter record) {
        return _mutableModReferenceCache.FormKeys.Add(record.FormKey);
    }

    public bool RemoveReference(FormKey formKey, IFormLinkIdentifier oldReference) {
        // when the record was not part of the MUTABLE MOD before we need to reevaluate all old form from the other one too  

        return _mutableModReferenceCache.Cache.TryGetValue(formKey, out var references)
         && references.Remove(oldReference);
    }

    public void RemoveReferences(FormKey formKey, IEnumerable<IFormLinkIdentifier> oldReferences) {
        if (!_mutableModReferenceCache.Cache.TryGetValue(formKey, out var references)) return;

        foreach (var oldReference in oldReferences) {
            references.Remove(oldReference);
        }
    }

    public bool AddReference(FormKey formKey, IFormLinkIdentifier newReference) {
        var references = _mutableModReferenceCache.Cache.GetOrAdd(formKey);

        return references.Add(newReference);
    }

    public void AddReferences(FormKey formKey, IEnumerable<IFormLinkIdentifier> newReferences) {
        var references = _mutableModReferenceCache.Cache.GetOrAdd(formKey);
        foreach (var newReference in newReferences) {
            references.Add(newReference);
        }
    }

    public IEnumerable<IFormLinkIdentifier> GetReferences(FormKey formKey, IReadOnlyList<IModGetter> modOrder) {
        if (_mutableModReferenceCache.Cache.TryGetValue(formKey, out var references)) {
            foreach (var reference in references) {
                yield return reference;
            }
        } else if (_immutableReferenceCache is not null) {
            foreach (var reference in _immutableReferenceCache.GetReferences(formKey, modOrder)) {
                yield return reference;
            }
        }
    }
}
