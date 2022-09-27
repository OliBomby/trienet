// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace TrieNet.Test.TestCaseGeneration;

[TestFixture]
public class TestCaseGenerator {
    [TestCase(40, Explicit = true)]
    public void GenerateParallelAddTestCases(int count) {
        var vocabulary = NonsenseGeneration.GetVocabulary();
        var phrases = new string[40];
        var random = new Random();
        for (var i = 0; i < count; i++) {
            var words = NonsenseGeneration.GetRandomWords(vocabulary, 30, random);
            phrases[i] = string.Join(string.Empty, words);
        }

        using var output = File.CreateText($"ParallelAddTestCases{count}.txt");
        WriteArrayDeclaration(count.ToString(), phrases, output);
    }


    [TestCase(10, Explicit = true)]
    [TestCase(40, Explicit = true)]
    [TestCase(100, Explicit = true)]
    public void GeneratePrefixSearchTestCases(int count) {
        var vocabulary = NonsenseGeneration.GetVocabulary();
        var words = NonsenseGeneration.GetRandomNeighbourWordGroups(vocabulary, count).ToArray();
        var idsAndWords = new KeyValuePair<int, string>[words.Length];
        for (var i = 0; i < words.Length; i++) idsAndWords[i] = new KeyValuePair<int, string>(i, words[i]);

        using var output = File.CreateText($"PrefixSearchTestCases{count}.txt");
        WriteArrayDeclaration(count.ToString(), words, output);

        foreach (var keyValuePair in idsAndWords) {
            var word = keyValuePair.Value;
            foreach (var query in GetAllPrefixes(word)) {
                var query1 = query;
                var actual =
                    idsAndWords
                        .Where(idWordPair => idWordPair.Value.StartsWith(query1))
                        .Select(idWordPair => idWordPair.Key);

                var array = string.Join(",", actual.Select(id => id.ToString()));
                output.WriteLine("[TestCase(\"{0}\", new[] {{{1}}})]", query, array);
            }
        }
    }

    private static IEnumerable<string> GetAllPrefixes(string word) {
        for (var i = 1; i <= word.Length; i++) yield return word.Substring(0, i);
    }


    [TestCase(10, Explicit = true)]
    [TestCase(20, Explicit = true)]
    [TestCase(100, Explicit = true)]
    public void GenerateSuffixSearchTestCases(int count) {
        var vocabulary = NonsenseGeneration.GetVocabulary();
        var words = NonsenseGeneration.GetRandomNeighbourWordGroups(vocabulary, count).ToArray();
        var idsAndWords = new KeyValuePair<int, string>[words.Length];
        for (var i = 0; i < words.Length; i++) idsAndWords[i] = new KeyValuePair<int, string>(i, words[i]);

        using var output = File.CreateText($"SuffixSearchTestCases{count}.txt");
        WriteArrayDeclaration(count.ToString(), words, output);

        foreach (var keyValuePair in idsAndWords) {
            var word = keyValuePair.Value;
            foreach (var query in GetAllSubstrings(word)) {
                var query1 = query;
                var actual =
                    idsAndWords
                        .Where(idWordPair => idWordPair.Value.Contains(query1))
                        .Select(idWordPair => idWordPair.Key);

                var array = string.Join(",", actual.Select(id => id.ToString()));
                output.WriteLine("[TestCase(\"{0}\", new[] {{{1}}})]", query, array);
            }
        }
    }

    private static IEnumerable<string> GetAllSubstrings(string word) {
        for (var i = 0; i < word.Length - 1; i++)
        for (var j = i + 1; j < word.Length; j++)
            yield return word.Substring(i, j - i);
    }


    private static void WriteArrayDeclaration(string name, string[] words, StreamWriter output) {
        output.WriteLine("public string[] Words{0} = new[] {{", name);
        for (var index = 0; index < words.Length - 1; index++) {
            var word = words[index];
            output.WriteLine("\"{0}\",", word);
        }

        output.WriteLine("\"{0}\"", words[^1]);
        output.WriteLine("}};");
        output.WriteLine();
    }
}