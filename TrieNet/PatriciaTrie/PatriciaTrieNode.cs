// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TrieNet.Trie;

namespace TrieNet.PatriciaTrie;

[Serializable]
[DebuggerDisplay("'{myKey}'")]
public class PatriciaTrieNode<TValue> : TrieNodeBase<TValue> {
    private Dictionary<char, PatriciaTrieNode<TValue>> children;
    private StringPartition myKey;
    private Queue<TValue> values;

    protected PatriciaTrieNode(StringPartition myKey, TValue value)
        : this(myKey, new Queue<TValue>(new[] { value }), new Dictionary<char, PatriciaTrieNode<TValue>>()) { }

    protected PatriciaTrieNode(StringPartition myKey, Queue<TValue> values,
        Dictionary<char, PatriciaTrieNode<TValue>> children) {
        this.values = values;
        this.myKey = myKey;
        this.children = children;
    }

    protected override int KeyLength => myKey.Length;

    protected override IEnumerable<TValue> Values() {
        return values;
    }

    protected override IEnumerable<TrieNodeBase<TValue>> Children() {
        return children.Values;
    }


    protected override void AddValue(TValue value) {
        values.Enqueue(value);
    }

    internal virtual void Add(StringPartition keyRest, TValue value) {
        var zipResult = myKey.ZipWith(keyRest);

        switch (zipResult.MatchKind) {
            case MatchKind.ExactMatch:
                AddValue(value);
                break;

            case MatchKind.IsContained:
                GetOrCreateChild(zipResult.OtherRest, value);
                break;

            case MatchKind.Contains:
                SplitOne(zipResult, value);
                break;

            case MatchKind.Partial:
                SplitTwo(zipResult, value);
                break;
        }
    }


    private void SplitOne(ZipResult zipResult, TValue value) {
        var leftChild = new PatriciaTrieNode<TValue>(zipResult.ThisRest, values, children);

        children = new Dictionary<char, PatriciaTrieNode<TValue>>();
        values = new Queue<TValue>();
        AddValue(value);
        myKey = zipResult.CommonHead;

        children.Add(zipResult.ThisRest[0], leftChild);
    }

    private void SplitTwo(ZipResult zipResult, TValue value) {
        var leftChild = new PatriciaTrieNode<TValue>(zipResult.ThisRest, values, children);
        var rightChild = new PatriciaTrieNode<TValue>(zipResult.OtherRest, value);

        children = new Dictionary<char, PatriciaTrieNode<TValue>>();
        values = new Queue<TValue>();
        myKey = zipResult.CommonHead;

        var leftKey = zipResult.ThisRest[0];
        children.Add(leftKey, leftChild);
        var rightKey = zipResult.OtherRest[0];
        children.Add(rightKey, rightChild);
    }

    protected void GetOrCreateChild(StringPartition key, TValue value) {
        if (!children.TryGetValue(key[0], out var child)) {
            child = new PatriciaTrieNode<TValue>(key, value);
            children.Add(key[0], child);
        }
        else {
            child.Add(key, value);
        }
    }

    protected override TrieNodeBase<TValue> GetOrCreateChild(char key) {
        throw new NotSupportedException("Use alternative signature instead.");
    }

    protected override TrieNodeBase<TValue> GetChildOrNull(string query, int position) {
        if (query == null) throw new ArgumentNullException(nameof(query));
        if (children.TryGetValue(query[position], out var child)) {
            var queryPartition = new StringPartition(query, position, child.myKey.Length);
            if (child.myKey.StartsWith(queryPartition)) return child;
        }

        return null;
    }

    public string Traversal() {
        var result = new StringBuilder();
        result.Append(myKey);

        var subtreeResult = string.Join(" ; ", children.Values.Select(node => node.Traversal()).ToArray());
        if (subtreeResult.Length != 0) {
            result.Append("[");
            result.Append(subtreeResult);
            result.Append("]");
        }

        return result.ToString();
    }

    public override string ToString() {
        return
            string.Format(
                "Key: {0}, Values: {1} Children:{2}, ",
                myKey,
                Values().Count(),
                string.Join(";", children.Keys));
    }
}