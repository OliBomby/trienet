// This code is distributed under MIT license. Copyright (c) 2022 OliBomby
// See license.txt or http://opensource.org/licenses/mit-license.php

using NUnit.Framework;
using TrieNet.Ukkonen;

namespace TrieNet.Test;

[TestFixture]
public class UkkonenTreeTest : SuffixTrieTest {
    protected override ISuffixTrie<int> CreateTrie() {
        return new CharUkkonenTrie<int>(0);
    }

    [Test]
    public void RemoveKey() {
        var trie = CreateTestTrie();
        
        Assert.AreEqual(new [] { 15, 16, 17 }, trie.Retrieve("archi"));
        
        trie.Remove("architecturesque", 17);
        
        Assert.AreEqual(new [] { 15, 16 }, trie.Retrieve("archi"));
    }

    [Test]
    public void RemoveMultipleValues() {
        var trie = CreateTestTrie();
        
        trie.Remove("architis", 15, 16);
        
        Assert.AreEqual(new [] { 17 }, trie.Retrieve("archi"));
    }

    [Test]
    public void RemovePartialMatchedKey_RemoveAllEntries() {
        var trie = CreateTestTrie();
        
        trie.Remove("archi", 15, 16);
        
        Assert.AreEqual(new [] { 17 }, trie.Retrieve("archi"));
    }
    
    [Test]
    public void RemoveLongerKey_HasNoEffect() {
        var trie = CreateTestTrie();
        
        trie.Remove("architissssssss", 15, 16);
        
        Assert.AreEqual(new [] { 15, 16, 17 }, trie.Retrieve("archi"));
    }

    static CharUkkonenTrie<int> CreateTestTrie() {
        var trie = new CharUkkonenTrie<int>();
        for(var i =0; i < Words20.Length; ++i) trie.Add(Words20[i], i);
        return trie;
    }
}