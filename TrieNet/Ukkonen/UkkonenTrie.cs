// This code is distributed under MIT license. Copyright (c) 2022 OliBomby
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;

namespace TrieNet.Ukkonen;

public class UkkonenTrie<TKey, TValue> : IGenericSuffixTrie<TKey, TValue>
    where TKey : IEquatable<TKey>, IComparable<TKey> {
    protected readonly int MinSuffixLength;

    //The root of the suffix tree
    protected readonly Node<TKey, TValue> Root;

    //The last leaf that was added during the update operation
    private Node<TKey, TValue> activeLeaf;

    public UkkonenTrie() : this(0) { }

    public UkkonenTrie(int minSuffixLength) {
        MinSuffixLength = minSuffixLength;
        Root = new Node<TKey, TValue>();
        activeLeaf = Root;
    }

    public long Size => Root.Size();

    public IEnumerable<TValue> Retrieve(ReadOnlySpan<TKey> word) {
        return RetrieveSubstrings(word).Select(o => o.Value).Distinct();
    }

    public IEnumerable<WordPosition<TValue>> RetrieveSubstrings(ReadOnlySpan<TKey> word) {
        if (word.Length < MinSuffixLength) return Enumerable.Empty<WordPosition<TValue>>();
        var tmpNode = SearchNode(word);
        return tmpNode == null
            ? Enumerable.Empty<WordPosition<TValue>>()
            : tmpNode.GetData();
    }

    public void Add(ReadOnlyMemory<TKey> key, TValue value) {
        // reset activeLeaf
        activeLeaf = Root;

        var remainder = key;
        var s = Root;
        var size = key.Length;
        var offset = 0;

        // proceed with tree construction (closely related to procedure in
        // Ukkonen's paper)
        var text = new StringSlice<TKey>(key, 0, 0);
        // iterate over the string, one char at a time
        for (var i = 0; i < remainder.Length; i++) {
            // line 6
            text.EndIndex += 1;

            // line 7: update the tree with the new transitions due to this new char
            (s, text, offset) = Update(s, text, remainder.Slice(i), value, size, offset);
            // line 8: make sure the active Tuple is canonical
            (s, text, offset) = Canonize(s, text, offset);
        }

        // add leaf suffix link, is necessary
        if (null == activeLeaf.Suffix && activeLeaf != Root && activeLeaf != s) activeLeaf.Suffix = s;
    }

    #region Remove node

    protected void RemoveAll(ReadOnlyMemory<TKey> key, TValue[] values) {
        if (key.Length < MinSuffixLength) return;
        var tmpNode = SearchNode(key.Span);
        tmpNode?.RemoveAll(values);
    }
    
    #endregion

    public IEnumerable<WordPosition<TValue>>
        RetrieveSubstringsRange(ReadOnlyMemory<TKey> min, ReadOnlyMemory<TKey> max) {
        if (min.Length != max.Length) throw new ArgumentException("Lengths of min and max must be the same.");
        if (min.Length < MinSuffixLength) return Enumerable.Empty<WordPosition<TValue>>();
        var nodes = SearchNodeRange(Root, min, max);
        return nodes.SelectMany(o => o.GetData());
    }


    private static bool RegionMatchesRange(ReadOnlySpan<TKey> min, ReadOnlySpan<TKey> max, int toffset,
        ReadOnlySpan<TKey> second, int ooffset, int len) {
        for (var i = 0; i < len; i++) {
            var chMin = min[toffset + i];
            var chMax = max[toffset + i];
            var two = second[ooffset + i];
            if (two.CompareTo(chMin) < 0 || two.CompareTo(chMax) > 0) return false;
        }

        return true;
    }


    private static bool RegionMatches(ReadOnlySpan<TKey> first, int toffset, ReadOnlySpan<TKey> second, int ooffset,
        int len) {
        for (var i = 0; i < len; i++) {
            var one = first[toffset + i];
            var two = second[ooffset + i];
            if (!one.Equals(two)) return false;
        }

        return true;
    }

    /**
     * Returns all tree NodeA (if present) that corresponds to the given range of strings.
     */
    private static IEnumerable<Node<TKey, TValue>> SearchNodeRange(Node<TKey, TValue> startNode,
        ReadOnlyMemory<TKey> min,
        ReadOnlyMemory<TKey> max) {
        /*
         * Verifies if exists a path from the root to a Node such that the concatenation
         * of all the labels on the path is a superstring of the given word.
         * If such a path is found, the last Node on it is returned.
         */

        // Perform a breadth-first search
        var nodesToSearch = new Queue<(Node<TKey, TValue>, int)>();
        nodesToSearch.Enqueue((startNode, 0));
        while (nodesToSearch.Count > 0) {
            var (currentNode, i) = nodesToSearch.Dequeue();
            var chMin = min.Span[i];
            var chMax = max.Span[i];

            // follow all the EdgeA<T> which are between min and max
            var edges = currentNode.Edges;
            foreach (var (ch, edge) in edges) {
                if (edge is null || ch.CompareTo(chMin) < 0 || ch.CompareTo(chMax) > 0) continue;

                var label = edge.Label.Span;
                var lenToMatch = Math.Min(min.Length - i, label.Length);

                if (!RegionMatchesRange(min.Span, max.Span, i, label, 0, lenToMatch))
                    // the label on the EdgeA<T> does not correspond to the one in the string to search
                    continue;

                if (label.Length >= min.Length - i)
                    yield return edge.Target;
                else
                    // advance to next Node
                    nodesToSearch.Enqueue((edge.Target, i + lenToMatch));
            }
        }
    }

    /**
     * Returns the tree NodeA (if present) that corresponds to the given string.
     */
    private Node<TKey, TValue> SearchNode(ReadOnlySpan<TKey> word) {
        /*
         * Verifies if exists a path from the root to a Node such that the concatenation
         * of all the labels on the path is a superstring of the given word.
         * If such a path is found, the last Node on it is returned.
         */
        var currentNode = Root;

        for (var i = 0; i < word.Length; ++i) {
            var ch = word[i];
            // follow the EdgeA<T> corresponding to this char
            var currentEdge = currentNode.GetEdge(ch);
            if (null == currentEdge)
                // there is no EdgeA<T> starting with this char
                return null;
            var label = currentEdge.Label.Span;
            var lenToMatch = Math.Min(word.Length - i, label.Length);

            if (!RegionMatches(word, i, label, 0, lenToMatch))
                // the label on the EdgeA<T> does not correspond to the one in the string to search
                return null;

            if (label.Length >= word.Length - i) return currentEdge.Target;
            // advance to next Node
            currentNode = currentEdge.Target;
            i += lenToMatch - 1;
        }

        return null;
    }

    /**
     * Tests whether the string stringPart + t is contained in the subtree that has inputs as root.
     * If that's not the case, and there exists a path of edges e1, e2, ... such that
     * e1.label + e2.label + ... + $end = stringPart
     * and there is an EdgeA g such that g.label = stringPart + rest
     * Then g will be split in two different edges, one having $end as label, and the other one
     * having rest as label.
     * @param inputs the starting NodeA
     * @param stringPart the string to search
     * @param t the following character
     * @param remainder the remainder of the string to add to the index
     * @param value the value to add to the index
     * @return a Tuple containing
     * true/false depending on whether (stringPart + t) is contained in the subtree starting in inputs
     * the last Node that can be reached by following the path denoted by stringPart starting from inputs
     */
    private static (bool, Node<TKey, TValue>, int) TestAndSplit(Node<TKey, TValue> inputs, StringSlice<TKey> stringPart,
        TKey t,
        ReadOnlyMemory<TKey> remainder, TValue value, int size, int offset) {
        // descend the tree as far as possible
        (var s, var str, offset) = Canonize(inputs, stringPart, offset);

        if (str.Length > 0) {
            var g = s.GetEdge(str[0]);

            var label = g.Label;
            // must see whether "str" is substring of the label of an EdgeA<T>
            if (label.Length > str.Length && label.Span[str.Length].Equals(t)) return (true, s, offset);
            // need to split the EdgeA<T>
            var newlabel = label.Slice(str.Length);
            //assert (label.startsWith(str));

            // build a new Node
            var r = new Node<TKey, TValue>();
            // build a new EdgeA<T>
            var newedge = new Edge<TKey, TValue>(str.AsMemory(), r);

            g.Label = newlabel;

            // link s -> r
            r.AddEdge(newlabel.Span[0], g);
            s.AddEdge(str[0], newedge);

            return (false, r, offset + str.Length);
        }

        var e = s.GetEdge(t);
        if (null == e)
            // if there is no t-transtion from s
            return (false, s, offset);
        var eLabelSpan = e.Label.Span;
        var remainderSpan = remainder.Span;
        if (remainderSpan.SequenceEqual(eLabelSpan)) {
            // update payload of destination Node
            e.Target.AddRef(new WordPosition<TValue>(size - offset - remainder.Length, value));
            return (true, s, offset);
        }

        if (remainderSpan.StartsWith(eLabelSpan)) return (true, s, offset);
        if (!eLabelSpan.StartsWith(remainderSpan)) return (true, s, offset);
        // need to split as above
        var newNode = new Node<TKey, TValue>();
        newNode.AddRef(new WordPosition<TValue>(size - offset - remainder.Length, value));

        var newEdge = new Edge<TKey, TValue>(remainder, newNode);
        e.Label = e.Label.Slice(remainder.Length);
        newNode.AddEdge(e.Label.Span[0], e);
        s.AddEdge(t, newEdge);
        return (false, s, offset);
        // they are different words. No prefix. but they may still share some common substr
    }

    /**
     * Return a (Node, string) (n, remainder) Tuple such that n is a farthest descendant of
     * s (the input Node) that can be reached by following a path of edges denoting
     * a prefix of inputstr and remainder will be string that must be
     * appended to the concatenation of labels from s to n to get inpustr.
     */
    private static (Node<TKey, TValue>, StringSlice<TKey>, int) Canonize(Node<TKey, TValue> s,
        StringSlice<TKey> inputstr, int offset) {
        if (inputstr.Length == 0) return (s, inputstr, offset);
        var currentNode = s;
        var g = s.GetEdge(inputstr[0]);
        // descend the tree as long as a proper label is found
        while (g != null && inputstr.Length >= g.Label.Length && inputstr.StartsWith(g.Label.Span)) {
            inputstr.StartIndex += g.Label.Length;
            offset += g.Label.Length;
            currentNode = g.Target;
            if (inputstr.Length > 0) g = currentNode.GetEdge(inputstr[0]);
        }

        return (currentNode, inputstr, offset);
    }

    /**
     * Updates the tree starting from inputNode and by adding stringPart.
     *
     * Returns a reference (Node, string) Tuple for the string that has been added so far.
     * This means:
     * - the Node will be the Node that can be reached by the longest path string (S1)
     * that can be obtained by concatenating consecutive edges in the tree and
     * that is a substring of the string added so far to the tree.
     * - the string will be the remainder that must be added to S1 to get the string
     * added so far.
     * @param inputNode the Node to start from
     * @param stringPart the string to add to the tree
     * @param rest the rest of the string
     * @param value the value to add to the index
     */
    private (Node<TKey, TValue>, StringSlice<TKey>, int) Update(Node<TKey, TValue> inputNode,
        StringSlice<TKey> stringPart,
        ReadOnlyMemory<TKey> rest, TValue value, int size, int offset) {
        var s = inputNode;
        var tempstr = stringPart;
        var newChar = stringPart[^1];

        // line 1
        var oldroot = Root;

        // line 1b
        var (endpoint, r, offset2) =
            TestAndSplit(s, tempstr.Slice(0, tempstr.Length - 1), newChar, rest, value, size, offset);

        // line 2
        while (!endpoint) {
            // line 3
            var tempEdge = r.GetEdge(newChar);
            Node<TKey, TValue> leaf;
            if (null != tempEdge) {
                // such a Node is already present. This is one of the main differences from Ukkonen's case:
                // the tree can contain deeper nodes at this stage because different strings were added by previous iterations.
                leaf = tempEdge.Target;
            }
            else {
                // must build a new leaf
                leaf = new Node<TKey, TValue>();
                leaf.AddRef(new WordPosition<TValue>(size - offset2 - rest.Length, value));
                var newedge = new Edge<TKey, TValue>(rest, leaf);
                r.AddEdge(newChar, newedge);
            }

            // update suffix link for newly created leaf
            if (activeLeaf != Root) activeLeaf.Suffix = leaf;
            activeLeaf = leaf;

            // line 4
            if (oldroot != Root) oldroot.Suffix = r;

            // line 5
            oldroot = r;

            // line 6
            if (null == s.Suffix) {
                // root Node
                // this is a special case to handle what is referred to as Node _|_ on the paper
                tempstr = tempstr.Slice(1, tempstr.Length - 1);
            }
            else {
                tempstr.EndIndex -= 1;
                (s, tempstr, offset) = Canonize(s.Suffix, tempstr, offset - 1);
                tempstr.EndIndex += 1;
            }

            // line 7
            (endpoint, r, offset2) = TestAndSplit(s, SafeCutLastChar(tempstr), newChar, rest, value, size, offset);
        }

        // line 8
        if (oldroot != Root) oldroot.Suffix = r;

        return (s, tempstr, offset);
    }

    private static StringSlice<TKey> SafeCutLastChar(StringSlice<TKey> seq) {
        return seq.Length == 0 ? new StringSlice<TKey>() : seq.Slice(0, seq.Length - 1);
    }
}