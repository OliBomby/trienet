// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;

namespace TrieNet.Test.Performance;

public class FakeTrie<T> : ITrie<T> {
    private readonly Stack<KeyValuePair<string, T>> stack;

    public FakeTrie() {
        stack = new Stack<KeyValuePair<string, T>>();
    }

    public IEnumerable<T> Retrieve(string query) {
        foreach (var keyValuePair in stack) {
            var key = keyValuePair.Key;
            var value = keyValuePair.Value;
            if (key.Contains(query)) yield return value;
        }
    }

    public void Add(string key, T value) {
        var keyValPair = new KeyValuePair<string, T>(key, value);
        stack.Push(keyValPair);
    }
}