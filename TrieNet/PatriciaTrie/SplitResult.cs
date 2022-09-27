// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;

namespace TrieNet.PatriciaTrie;

[Serializable]
public struct SplitResult {
    public SplitResult(StringPartition head, StringPartition rest) {
        Head = head;
        Rest = rest;
    }

    public StringPartition Rest { get; }

    public StringPartition Head { get; }

    public bool Equals(SplitResult other) {
        return Head == other.Head && Rest == other.Rest;
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is SplitResult splitResult && Equals(splitResult);
    }

    public override int GetHashCode() {
        unchecked {
            return (Head.GetHashCode() * 397) ^ Rest.GetHashCode();
        }
    }

    public static bool operator ==(SplitResult left, SplitResult right) {
        return left.Equals(right);
    }

    public static bool operator !=(SplitResult left, SplitResult right) {
        return !(left == right);
    }
}