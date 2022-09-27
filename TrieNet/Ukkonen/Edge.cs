// This code is distributed under MIT license. Copyright (c) 2022 OliBomby
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;

namespace TrieNet.Ukkonen;

public class Edge<TKey, TValue> where TKey : IComparable<TKey> {
    public Edge(ReadOnlyMemory<TKey> label, Node<TKey, TValue> target) {
        Label = label;
        Target = target;
    }

    public ReadOnlyMemory<TKey> Label { get; set; }

    public Node<TKey, TValue> Target { get; }
}