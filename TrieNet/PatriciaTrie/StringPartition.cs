// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TrieNet.PatriciaTrie;

[Serializable]
[DebuggerDisplay(
    "{origin.Substring(0,startIndex)} [ {origin.Substring(startIndex,partitionLength)} ] {origin.Substring(startIndex + partitionLength)}"
)]
public struct StringPartition : IEnumerable<char> {
    private readonly string origin;
    private readonly int startIndex;

    public StringPartition(string origin)
        : this(origin, 0, origin?.Length ?? 0) { }

    public StringPartition(string origin, int startIndex)
        : this(origin, startIndex, origin?.Length - startIndex ?? 0) { }

    public StringPartition(string origin, int startIndex, int partitionLength) {
        if (origin == null) throw new ArgumentNullException(nameof(origin));
        if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex), "The value must be non negative.");
        if (partitionLength < 0)
            throw new ArgumentOutOfRangeException(nameof(partitionLength), "The value must be non negative.");
        this.origin = string.Intern(origin);
        this.startIndex = startIndex;
        var availableLength = this.origin.Length - startIndex;
        Length = Math.Min(partitionLength, availableLength);
    }

    public char this[int index] => origin[startIndex + index];

    public int Length { get; }

    #region IEnumerable<char> Members

    public IEnumerator<char> GetEnumerator() {
        for (var i = 0; i < Length; i++) yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    #endregion

    public bool Equals(StringPartition other) {
        return string.Equals(origin, other.origin) && Length == other.Length &&
               startIndex == other.startIndex;
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is StringPartition stringPartition && Equals(stringPartition);
    }

    public override int GetHashCode() {
        unchecked {
            var hashCode = origin != null ? origin.GetHashCode() : 0;
            hashCode = (hashCode * 397) ^ Length;
            hashCode = (hashCode * 397) ^ startIndex;
            return hashCode;
        }
    }

    public bool StartsWith(StringPartition other) {
        if (Length < other.Length) return false;

        for (var i = 0; i < other.Length; i++)
            if (this[i] != other[i])
                return false;
        return true;
    }

    public SplitResult Split(int splitAt) {
        var head = new StringPartition(origin, startIndex, splitAt);
        var rest = new StringPartition(origin, startIndex + splitAt, Length - splitAt);
        return new SplitResult(head, rest);
    }

    public ZipResult ZipWith(StringPartition other) {
        var splitIndex = 0;
        using (var thisEnumerator = GetEnumerator())
        using (var otherEnumerator = other.GetEnumerator()) {
            while (thisEnumerator.MoveNext() && otherEnumerator.MoveNext()) {
                if (thisEnumerator.Current != otherEnumerator.Current) break;
                splitIndex++;
            }
        }

        var thisSplitted = Split(splitIndex);
        var otherSplitted = other.Split(splitIndex);

        var commonHead = thisSplitted.Head;
        var restThis = thisSplitted.Rest;
        var restOther = otherSplitted.Rest;
        return new ZipResult(commonHead, restThis, restOther);
    }

    public override string ToString() {
        var result = new string(this.ToArray());
        return string.Intern(result);
    }

    public static bool operator ==(StringPartition left, StringPartition right) {
        return left.Equals(right);
    }

    public static bool operator !=(StringPartition left, StringPartition right) {
        return !(left == right);
    }
}