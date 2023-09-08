// This code is distributed under MIT license. Copyright (c) 2022 OliBomby
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;

namespace TrieNet.Ukkonen;

public class CharUkkonenTrie<TValue> : UkkonenTrie<char, TValue>, ISuffixTrie<TValue> {
    public CharUkkonenTrie() : base(0) { }

    public CharUkkonenTrie(int minSuffixLength) : base(minSuffixLength) { }

    public void Add(string key, TValue value) {
        Add(key.AsMemory(), value);
    }

    public void Remove(string key, TValue value) {
        throw new NotImplementedException();
    }

    public void Remove(string key, params TValue[] values) {
        throw new NotImplementedException();
    }

    public IEnumerable<TValue> Retrieve(string query) {
        return Retrieve(query.AsSpan());
    }

    public IEnumerable<WordPosition<TValue>> RetrieveSubstrings(string query) {
        return RetrieveSubstrings(query.AsSpan());
    }
}