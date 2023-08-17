﻿using System.Reactive;
using CreationEditor.Core;
using CreationEditor.Services.Query.From;
using CreationEditor.Services.Query.Select;
using CreationEditor.Services.Query.Where;
using DynamicData.Binding;
namespace CreationEditor.Services.Query;

public sealed record QueryRunnerMemento(
    QueryFromMemento QueryFrom,
    List<QueryConditionEntryMemento> QueryConditions,
    FieldSelectorMemento OrderBySelector,
    FieldSelectorMemento FieldSelector);

public interface IQueryRunner : IMementoProvider<QueryRunnerMemento> {
    public Guid Id { get; }
    IQueryFrom QueryFrom { get; }
    IObservableCollection<IQueryConditionEntry> QueryConditions { get; }
    IFieldSelector FieldSelector { get; }
    public IFieldSelector OrderBySelector { get; }

    IObservable<Unit> SettingsChanged { get; }
    IObservable<string> Summary { get; }

    IEnumerable<object?> RunQuery();
}