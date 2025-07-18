﻿namespace CreationEditor;

public static class TreeExtension {
    public static IEnumerable<TLimit> GetAllChildren<T, TLimit>(
        this T root,
        Func<T, IEnumerable<T>?> childSelector,
        bool includeRoot = false)
        where TLimit : T {
        return root.GetAllChildren(childSelector, includeRoot).OfType<TLimit>();
    }

    public static IEnumerable<T> GetAllChildren<T>(this T root, Func<T, IEnumerable<T>?> childSelector, bool includeRoot = false) {
        if (includeRoot) yield return root;

        var stack = new Stack<T>();
        stack.Push(root);

        while (stack.Count > 0) {
            var current = stack.Pop();

            var childEnumerable = childSelector(current);
            if (childEnumerable is null) continue;

            foreach (var child in childEnumerable) {
                stack.Push(child);
                yield return child;
            }
        }
    }

    public static IEnumerable<T> GetAllChildren<T>(
        this IEnumerable<T> rootEnumerable,
        Func<T, IEnumerable<T>?> childSelector,
        bool includeRoot = false) {
        return rootEnumerable.SelectMany(rootItem => GetAllChildren(rootItem, childSelector, includeRoot));
    }

    public static IEnumerable<TLimit> GetChildren<T, TLimit>(
        this T root,
        Predicate<T> childPredicate,
        Func<T, IEnumerable<T>?> childSelector,
        bool includeRoot = false)
        where TLimit : T {
        return root.GetChildren(childPredicate, childSelector, includeRoot).OfType<TLimit>();
    }

    public static IEnumerable<T> GetChildren<T>(
        this T root,
        Predicate<T> childPredicate,
        Func<T, IEnumerable<T>?> childSelector,
        bool includeRoot = false) {
        if (includeRoot) yield return root;

        var stack = new Stack<T>();
        stack.Push(root);

        while (stack.Count > 0) {
            var current = stack.Pop();

            var childEnumerable = childSelector(current);
            if (childEnumerable is null) continue;

            foreach (var child in childEnumerable) {
                if (childPredicate(child)) {
                    stack.Push(child);
                    yield return child;
                }
            }
        }
    }

    public static IEnumerable<T> GetChildren<T>(
        this IEnumerable<T> rootEnumerable,
        Predicate<T> childPredicate,
        Func<T, IEnumerable<T>?> childSelector,
        bool includeRoot = false) {
        return rootEnumerable.SelectMany(rootItem => GetChildren(rootItem, childPredicate, childSelector, includeRoot));
    }

    public static IEnumerable<T> GetTreeLeaves<T>(
        this T root,
        Func<T, IEnumerable<T>?> childSelector) {
        if (childSelector(root)?.Any() is false) {
            yield return root;
            yield break;
        }

        var stack = new Stack<T>();
        stack.Push(root);

        while (stack.Count > 0) {
            var current = stack.Pop();

            var childEnumerable = childSelector(current);
            if (childEnumerable is null || !childEnumerable.Any()) {
                yield return current;
                continue;
            }

            foreach (var child in childEnumerable) {
                stack.Push(child);
            }
        }
    }
}
