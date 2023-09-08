// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;

namespace TrieNet.PatriciaTrie;

[Serializable]
public class PatriciaSuffixTrie<TValue> : ISuffixTrie<TValue> {
    private readonly PatriciaTrie<WordPosition<TValue>> innerTrie;

    public PatriciaSuffixTrie(int minQueryLength)
        : this(minQueryLength, new PatriciaTrie<WordPosition<TValue>>()) { }

    internal PatriciaSuffixTrie(int minQueryLength, PatriciaTrie<WordPosition<TValue>> innerTrie) {
        MinQueryLength = minQueryLength;
        this.innerTrie = innerTrie;
    }

    protected int MinQueryLength { get; }

    public long Size => innerTrie.Size();

    public IEnumerable<TValue> Retrieve(string word) {
        return RetrieveSubstrings(word).Select(o => o.Value).Distinct();
    }

    public IEnumerable<WordPosition<TValue>> RetrieveSubstrings(string query) {
        return
            innerTrie
                .Retrieve(query)
                .Distinct();
    }

    public void Add(string key, TValue value) {
        foreach (var (currentSuffix, position) in GetAllSuffixes(MinQueryLength, key))
            innerTrie.Add(currentSuffix, new WordPosition<TValue>(position, value));
    }

    public void Remove(string key, TValue value) {
        throw new NotSupportedException();
    }

    public void Remove(string key, params TValue[] values) {
        throw new NotImplementedException();
    }

    private static IEnumerable<Tuple<StringPartition, int>> GetAllSuffixes(int minSuffixLength, string word) {
        for (var i = word.Length - minSuffixLength; i >= 0; i--)
            yield return new Tuple<StringPartition, int>(new StringPartition(word, i), i);
    }
}