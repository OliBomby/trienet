// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;

namespace TrieNet.PatriciaTrie;

[Serializable]
[DebuggerDisplay("Head: '{CommonHead}', This: '{ThisRest}', Other: '{OtherRest}', Kind: {MatchKind}")]
public struct ZipResult {
    private readonly StringPartition otherRest;
    private readonly StringPartition thisRest;

    public ZipResult(StringPartition commonHead, StringPartition thisRest, StringPartition otherRest) {
        CommonHead = commonHead;
        this.thisRest = thisRest;
        this.otherRest = otherRest;
    }

    public MatchKind MatchKind =>
        thisRest.Length == 0
            ? otherRest.Length == 0
                ? MatchKind.ExactMatch
                : MatchKind.IsContained
            : otherRest.Length == 0
                ? MatchKind.Contains
                : MatchKind.Partial;

    public StringPartition OtherRest => otherRest;

    public StringPartition ThisRest => thisRest;

    public StringPartition CommonHead { get; }


    public bool Equals(ZipResult other) {
        return
            CommonHead == other.CommonHead &&
            otherRest == other.otherRest &&
            thisRest == other.thisRest;
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is ZipResult zipResult && Equals(zipResult);
    }

    public override int GetHashCode() {
        unchecked {
            var hashCode = CommonHead.GetHashCode();
            hashCode = (hashCode * 397) ^ otherRest.GetHashCode();
            hashCode = (hashCode * 397) ^ thisRest.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(ZipResult left, ZipResult right) {
        return left.Equals(right);
    }

    public static bool operator !=(ZipResult left, ZipResult right) {
        return !(left == right);
    }
}