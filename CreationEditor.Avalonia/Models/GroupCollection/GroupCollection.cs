﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using DynamicData.Binding;
using Noggog;
using ReactiveUI;
namespace CreationEditor.Avalonia.Models.GroupCollection;

/// <summary>
/// Groups items by their properties in a <see cref="ObservableCollection{T}"/>
/// </summary>
/// <typeparam name="T">Type of items</typeparam>
public sealed class GroupCollection<T> {
    private readonly List<Group<T>> _activeGroups = new();
    private readonly IObservableCollection<T> _source;
    private readonly GroupInstance _topLevelGroup;

    public IObservableCollection<object> Items => _topLevelGroup.Items;

    public GroupCollection(IObservableCollection<T> source, params Group<T>[] groups) {
        _source = source;
        _topLevelGroup = new GroupInstance(null!, new ObservableCollectionExtended<object>(_source.OfType<object>()));

        foreach (var group in groups) {
            group.WhenAnyValue(x => x.IsGrouped)
                .DistinctUntilChanged()
                .Subscribe(grouped => {
                    if (grouped) {
                        _activeGroups.Add(group);

                        _topLevelGroup.GroupBy(group);
                    } else {
                        var level = _activeGroups.IndexOf(group);

                        _activeGroups.Remove(group);

                        _topLevelGroup.Ungroup<T>(level);
                    }
                })
                .DisposeWith(group);
        }

        _source.CollectionChanged += (_, args) => {
            switch (args.Action) {
                case NotifyCollectionChangedAction.Add:
                    if (args.NewItems is null) break;

                    _topLevelGroup.Add(args.NewItems.OfType<T>(), _activeGroups);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (args.OldItems is null) break;

                    _topLevelGroup.Remove(args.OldItems.OfType<T>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (args.OldItems is null || args.NewItems is null) break;

                    _topLevelGroup.Remove(args.OldItems.OfType<T>());
                    _topLevelGroup.Add(args.NewItems.OfType<T>(), _activeGroups);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
            }
        };
    }
}
