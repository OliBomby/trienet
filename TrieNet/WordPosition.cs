// This code is distributed under MIT license. Copyright (c) 2022 OliBomby
// See license.txt or http://opensource.org/licenses/mit-license.php

namespace TrieNet;

public readonly struct WordPosition<TValue> {
    public WordPosition(int charPosition, TValue value) {
        CharPosition = charPosition;
        Value = value;
    }

    public TValue Value { get; }

    public int CharPosition { get; }

    public override string ToString() {
        return $"( Pos {CharPosition} ) {Value}";
    }
}