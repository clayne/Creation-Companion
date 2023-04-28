﻿using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;
namespace SearchPlugin.Models;

public interface ITextSearcherDefinition {
    public string SearcherName { get; }
}

public interface ITextSearcher<TMod, TModGetter> : ITextSearcherDefinition
    where TModGetter : class, IModGetter
    where TMod : class, TModGetter, IMod {
    public IEnumerable<RecordReferences<TMod, TModGetter>> GetTextReference(IMajorRecordGetterEnumerable mod, string reference, StringComparison comparison);
    public void ReplaceTextReference(IMajorRecordQueryableGetter record, ILinkCache<TMod, TModGetter> linkCache, TMod mod, string oldText, string newText, StringComparison comparison);
}

public abstract class TextSearcher<TMod, TModGetter, TMajor, TMajorGetter> : ITextSearcher<TMod, TModGetter>
    where TMajor : class, TMajorGetter, IMajorRecordQueryable
    where TMajorGetter : class, IMajorRecordQueryableGetter
    where TModGetter : class, IModGetter
    where TMod : class, TModGetter, IMod {
    public abstract string SearcherName { get; }

    public IEnumerable<RecordReferences<TMod, TModGetter>> GetTextReference(IMajorRecordGetterEnumerable mod, string reference, StringComparison comparison) {
        foreach (var record in mod.EnumerateMajorRecords<TMajorGetter>()) {
            var references = GetText(record)
                .NotNull()
                .Where(text => text.Contains(reference, comparison))
                .Select(text => new TextDiff(text, text))
                .ToArray();

            if (references.Length == 0) continue;

            foreach (var textDiff in references) {
                yield return new RecordReferences<TMod, TModGetter>(this, record, textDiff);
            }
        }
    }

    public void ReplaceTextReference(IMajorRecordQueryableGetter record, ILinkCache<TMod, TModGetter> linkCache, TMod mod, string oldText, string newText, StringComparison comparison) {
        if (record is not TMajorGetter) return;
        if (!linkCache.TryResolveContext<TMajor, TMajorGetter>(((IMajorRecordIdentifier) record).FormKey, out var context)) return;

        var overrideRecord = context.GetOrAddAsOverride(mod);
        if (record is IDialogTopicGetter topic) {
            foreach (var response in topic.Responses) {
                var responseContext = linkCache.ResolveContext<IDialogResponses, IDialogResponsesGetter>(response.FormKey);
                responseContext.GetOrAddAsOverride(mod);
            }
        }

        ReplaceText(overrideRecord, oldText, newText, comparison);
    }

    protected abstract IEnumerable<string?> GetText(TMajorGetter record);
    protected abstract void ReplaceText(TMajor record, string oldText, string newText, StringComparison comparison);

    protected bool Equals(TextSearcher<TMod, TModGetter, TMajor, TMajorGetter> other) {
        return SearcherName == other.SearcherName;
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((TextSearcher<TMod, TModGetter, TMajor, TMajorGetter>) obj);
    }

    public override int GetHashCode() {
        return SearcherName.GetHashCode();
    }

    public override string ToString() {
        return SearcherName;
    }
}