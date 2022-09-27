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
}