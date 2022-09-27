// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System.Linq;
using NUnit.Framework;
using TrieNet.Trie;

namespace TrieNet.Test;

public class TrieTest : BaseTrieTest {
    protected override ITrie<int> CreateTrie() {
        return new Trie<int>();
    }

    [Test]
    //[ExpectedException(typeof(AggregateException))]
    [Explicit]
    public void ExhaustiveParallelAddFails() {
        while (true) {
            var trie = CreateTrie();
            LongPhrases40
                .AsParallel()
                .ForAll(phrase => trie.Add(phrase, phrase.GetHashCode()));
        }
    }
}