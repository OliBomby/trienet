// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TrieNet.Trie;

[Serializable]
public class ConcurrentTrieNode<TValue> : TrieNodeBase<TValue> {
    private readonly ConcurrentDictionary<char, ConcurrentTrieNode<TValue>> children;
    private readonly ConcurrentQueue<TValue> values;

    public ConcurrentTrieNode() {
        children = new ConcurrentDictionary<char, ConcurrentTrieNode<TValue>>();
        values = new ConcurrentQueue<TValue>();
    }


    protected override int KeyLength => 1;

    protected override IEnumerable<TValue> Values() {
        return values;
    }

    protected override IEnumerable<TrieNodeBase<TValue>> Children() {
        return children.Values;
    }

    protected override void AddValue(TValue value) {
        values.Enqueue(value);
    }

    protected override TrieNodeBase<TValue> GetOrCreateChild(char key) {
        return children.GetOrAdd(key, new ConcurrentTrieNode<TValue>());
    }

    protected override TrieNodeBase<TValue> GetChildOrNull(string query, int position) {
        if (query == null) throw new ArgumentNullException(nameof(query));
        return
            children.TryGetValue(query[position], out var childNode)
                ? childNode
                : null;
    }
}