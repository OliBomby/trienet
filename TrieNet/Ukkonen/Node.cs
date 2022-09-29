// This code is distributed under MIT license. Copyright (c) 2022 OliBomby
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;

namespace TrieNet.Ukkonen;

public class Node<TKey, TValue> where TKey : IComparable<TKey> {
    public List<WordPosition<TValue>> Data { get; }

    public Node() {
        Edges = new List<(TKey, Edge<TKey, TValue>)>();
        Data = new List<WordPosition<TValue>>();
        Suffix = null;
    }

    public List<(TKey, Edge<TKey, TValue>)> Edges { get; }

    public Node<TKey, TValue> Suffix { get; set; }

    public long Size() {
        long sum = 1;
        foreach (var key in Edges) sum += key.Item2.Target.Size();

        return sum;
    }

    /// <summary>
    /// Gets the data of the whole sub-tree rooted on this node.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<WordPosition<TValue>> GetData() {
        var childData = Edges.SelectMany(e => e.Item2.Target.GetData());
        return Data.Concat(childData);
    }

    public void AddRef(WordPosition<TValue> value) {
        if (Data.Contains(value))
            return;

        Data.Add(value);
        //  add this reference to all the suffixes as well
        var iter = Suffix;
        var i = 0;
        while (iter != null) {
            iter.Data.Add(new WordPosition<TValue>(value.CharPosition + ++i, value.Value));
            iter = iter.Suffix;
        }
    }

    public void AddEdge(TKey ch, Edge<TKey, TValue> e) {
        var index = IndexOf(ch);
        if (index < 0)
            Edges.Insert(~index, (ch, e));
        else
            Edges[index] = (ch, e);
    }

    public Edge<TKey, TValue> GetEdge(TKey ch) {
        var index = IndexOf(ch);
        return index < 0 ? null : Edges[index].Item2;
    }

    private int IndexOf(TKey ch) {
        // Perform binary search to find the place of this character
        var n = Edges.Count;
        var min = 0;
        var max = n - 1;
        var comparer = Comparer<TKey>.Default;
        while (min <= max) {
            var mid = min + (max - min) / 2;
            var midTerm = Edges[mid].Item1;

            switch (comparer.Compare(midTerm, ch)) {
                case 0:
                    return mid;
                case < 0:
                    max = mid - 1;
                    break;
                case > 0:
                    min = mid + 1;
                    break;
            }
        }

        return ~min;
    }
}