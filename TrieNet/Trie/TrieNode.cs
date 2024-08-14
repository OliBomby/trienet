// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TrieNet.Trie;

[Serializable]
public class TrieNode<TValue> : TrieNodeBase<TValue> {
    private readonly Dictionary<char, TrieNode<TValue>> children;
    private readonly List<TValue> values = new();

    protected TrieNode() {
        children = new Dictionary<char, TrieNode<TValue>>();
    }

    protected override int KeyLength => 1;

    protected override IEnumerable<TrieNodeBase<TValue>> Children() {
        return children.Values;
    }

    protected override IEnumerable<TValue> Values() {
        return values;
    }

    protected override TrieNodeBase<TValue> GetOrCreateChild(char key) {
        if (!children.TryGetValue(key, out var result)) {
            result = new TrieNode<TValue>();
            children.Add(key, result);
        }

        return result;
    }

    [return: MaybeNull]
    protected override TrieNodeBase<TValue> GetChildOrNull([NotNull] string query, int position) {
        if (query == null) throw new ArgumentNullException(nameof(query));
        return
            children.TryGetValue(query[position], out var childNode)
                ? childNode
                : null;
    }

    protected override void AddValue(TValue value) {
        values.Add(value);
    }

    protected override void RemoveAll(TValue[] nodeValues) {
        values.RemoveAll(v => nodeValues.Any(nv => v is not null && v.Equals(nv) || (v is null && nv is null)));
    }
}