// This code is distributed under MIT license. Copyright (c) 2022 OliBomby
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;

namespace TrieNet;

/// <summary>
///     Interface to be implemented by a data structure
///     which allows adding values <see cref="TValue" /> associated with generic keys.
///     The interface allows retrieval of multiple values.
/// </summary>
/// <typeparam name="TValue">The data type of the values associates to the strings.</typeparam>
/// <typeparam name="TKey">The data type of the characters to search for.</typeparam>
public interface IGenericTrie<TKey, TValue> where TKey : IEquatable<TKey> {
    IEnumerable<TValue> Retrieve(ReadOnlySpan<TKey> query);
    void Add(ReadOnlyMemory<TKey> key, TValue value);
}