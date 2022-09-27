// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

namespace TrieNet.PatriciaTrie;

public enum MatchKind {
    ExactMatch,
    Contains,
    IsContained,
    Partial
}