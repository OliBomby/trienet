// This code is distributed under MIT license. Copyright (c) 2022 OliBomby
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrieNet.Ukkonen;

[Serializable]
[DebuggerDisplay(
    "{origin[startIndex..endIndex]}"
)]
public struct StringSlice<TKey> : IEnumerable<TKey> where TKey : IEquatable<TKey> {
    private readonly ReadOnlyMemory<TKey> origin;
    private int startIndex;
    private int endIndex;

    public StringSlice(ReadOnlyMemory<TKey> origin)
        : this(origin, 0, origin.Length) { }

    public StringSlice(ReadOnlyMemory<TKey> origin, int startIndex)
        : this(origin, startIndex, origin.Length - startIndex) { }

    public StringSlice(ReadOnlyMemory<TKey> origin, int startIndex, int partitionLength) {
        if (startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex), "The value must be non negative.");
        if (partitionLength < 0)
            throw new ArgumentOutOfRangeException(nameof(partitionLength), "The value must be non negative.");
        this.origin = origin;
        this.startIndex = startIndex;
        endIndex = Math.Min(origin.Length, startIndex + partitionLength);
    }

    public TKey this[int index] => origin.Span[startIndex + index];

    public int StartIndex {
        set => startIndex = Math.Max(0, Math.Min(value, endIndex));
        get => startIndex;
    }

    public int EndIndex {
        set => endIndex = Math.Max(StartIndex, Math.Min(value, origin.Length));
        get => endIndex;
    }

    public int Length => endIndex - startIndex;

    #region IEnumerable<char> Members

    public IEnumerator<TKey> GetEnumerator() {
        for (var i = 0; i < Length; i++) yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    #endregion

    public bool Equals(StringSlice<TKey> other) {
        return origin.Equals(other.origin) &&
               endIndex == other.endIndex &&
               startIndex == other.startIndex;
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is StringSlice<TKey> sli && Equals(sli);
    }

    public override int GetHashCode() {
        unchecked {
            var hashCode = origin.GetHashCode();
            hashCode = (hashCode * 397) ^ endIndex;
            hashCode = (hashCode * 397) ^ startIndex;
            return hashCode;
        }
    }

    public StringSlice<TKey> Slice(int newStartIndex) {
        return Slice(newStartIndex, Length - newStartIndex);
    }

    public StringSlice<TKey> Slice(int newStartIndex, int count) {
        return new StringSlice<TKey>(origin, this.startIndex + newStartIndex, Math.Min(count, Length - newStartIndex));
    }

    public bool StartsWith(StringSlice<TKey> other) {
        return StartsWith(other.AsSpan());
    }

    public bool StartsWith(ReadOnlySpan<TKey> other) {
        if (Length < other.Length) return false;

        for (var i = 0; i < other.Length; i++)
            if (!this[i].Equals(other[i]))
                return false;
        return true;
    }

    public ReadOnlyMemory<TKey> AsMemory() {
        return origin.Slice(startIndex, Length);
    }

    public ReadOnlySpan<TKey> AsSpan() {
        return origin.Span.Slice(startIndex, Length);
    }

    public (StringSlice<TKey>, StringSlice<TKey>) Split(int splitAt) {
        var head = new StringSlice<TKey>(origin, startIndex, splitAt);
        var rest = new StringSlice<TKey>(origin, startIndex + splitAt, Length - splitAt);
        return (head, rest);
    }

    public static bool operator ==(StringSlice<TKey> left, StringSlice<TKey> right) {
        return left.Equals(right);
    }

    public static bool operator !=(StringSlice<TKey> left, StringSlice<TKey> right) {
        return !(left == right);
    }
}