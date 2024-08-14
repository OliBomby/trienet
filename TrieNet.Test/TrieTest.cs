// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
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

    [Test]
    public void RemoveKey() {
        var trie = CreateDefaultTrie();

        Assert.AreEqual(new [] { 21, 22, 23}, trie.Retrieve("capo"));
        Assert.AreEqual(new [] { 22, 23}, trie.Retrieve("capoc"));
        
        trie.Remove("capoc", 22);
        
        Assert.AreEqual(new [] { 21, 23}, trie.Retrieve("capo"));
        Assert.AreEqual(new [] { 23}, trie.Retrieve("capoc"));
    }

    [Test]
    public void RemoveMultipleKeys() {
        var trie = CreateDefaultTrie();
        
        trie.Remove("capoc", 22, 23);
        
        Assert.AreEqual(new [] { 21}, trie.Retrieve("capo"));
        Assert.AreEqual(Enumerable.Empty<int>(), trie.Retrieve("capoc"));
    }

    [Test]
    public void RemovePartialMatchedKey_RemoveAllEntries() {
        var trie = CreateDefaultTrie();
        
        trie.Remove("capo", 22, 23);
        
        Assert.AreEqual(new [] { 21 }, trie.Retrieve("capo"));
        Assert.AreEqual(Array.Empty<int>(), trie.Retrieve("capoc"));
    }

    [Test]
    public void RemoveLongerKey_HasNoEffect() {
        var trie = CreateDefaultTrie();
        
        trie.Remove("capocissss", 22);
        
        Assert.AreEqual(new [] { 21, 22, 23}, trie.Retrieve("capo"));
        Assert.AreEqual(new [] { 22, 23}, trie.Retrieve("capoc"));
    }
}