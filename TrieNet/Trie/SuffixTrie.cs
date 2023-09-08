// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using TrieNet.PatriciaTrie;

namespace TrieNet.Trie;

[Serializable]
public class SuffixTrie<T> : ISuffixTrie<T> {
    private readonly Trie<WordPosition<T>> innerTrie;
    private readonly int minSuffixLength;

    public SuffixTrie(int minSuffixLength)
        : this(new Trie<WordPosition<T>>(), minSuffixLength) { }

    private SuffixTrie(Trie<WordPosition<T>> innerTrie, int minSuffixLength) {
        this.innerTrie = innerTrie;
        this.minSuffixLength = minSuffixLength;
    }

    public long Size => innerTrie.Size();

    public IEnumerable<T> Retrieve(string word) {
        return RetrieveSubstrings(word).Select(o => o.Value).Distinct();
    }

    public IEnumerable<WordPosition<T>> RetrieveSubstrings(string query) {
        return
            innerTrie
                .Retrieve(query)
                .Distinct();
    }

    public void Add(string key, T value) {
        foreach (var (suffix, position) in GetAllSuffixes(minSuffixLength, key))
            innerTrie.Add(suffix, new WordPosition<T>(position, value));
    }

    public void Remove(string key, T value) {
        throw new NotImplementedException();
    }

    public void Remove(string key, params T[] values) {
        throw new NotImplementedException();
    }

    private static IEnumerable<Tuple<string, int>> GetAllSuffixes(int minSuffixLength, string word) {
        for (var i = word.Length - minSuffixLength; i >= 0; i--) {
            var partition = new StringPartition(word, i);
            yield return new Tuple<string, int>(partition.ToString(), i);
        }
    }
}