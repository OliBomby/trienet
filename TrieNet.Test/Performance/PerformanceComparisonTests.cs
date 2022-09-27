// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TrieNet.PatriciaTrie;
using TrieNet.Test.TestCaseGeneration;
using TrieNet.Trie;
using TrieNet.Ukkonen;

namespace TrieNet.Test.Performance;

[TestFixture]
[Explicit]
public class PerformanceComparisonTests {
    [OneTimeSetUp]
    public void SetUp() {
        result = new StringBuilder();
        writer = new StringWriter(result);
        vocabualry = NonsenseGeneration.GetVocabulary();
    }

    [OneTimeTearDown]
    public void TearDown() {
        writer?.Close();
        Console.WriteLine(result);
    }

    private string[] vocabualry;
    private StringWriter writer;
    private StringBuilder result;

    private enum TrieType {
        List,
        Simple,
        Patricia,
        Ukkonen
    }

    [TestCase("List", 10000, 1000)]
    [TestCase("List", 100000, 1000)]
    [TestCase("List", 1000000, 1000)]
    [TestCase("List", 10000000, 1000)]
    [TestCase("Simple", 10000, 1000)]
    [TestCase("Simple", 100000, 1000)]
    [TestCase("Simple", 1000000, 1000)]
    [TestCase("Simple", 10000000, 1000)]
    [TestCase("Patricia", 10000, 1000)]
    [TestCase("Patricia", 100000, 1000)]
    [TestCase("Patricia", 1000000, 1000)]
    [TestCase("Patricia", 10000000, 1000)]
    [TestCase("Ukkonen", 10000, 1000)]
    [TestCase("Ukkonen", 100000, 1000)]
    [TestCase("Ukkonen", 1000000, 1000)]
    [TestCase("Ukkonen", 10000000, 1000)]
    public void TestX(string trieTypeName, int wordCount, int lookupCount) {
        var randomText = NonsenseGeneration.GetRandomWords(vocabualry, wordCount).ToArray();
        var lookupWords = NonsenseGeneration.GetRandomWords(vocabualry, lookupCount).ToArray();
        var trie = CreateTrie<string>(trieTypeName);
        Measure(trie, randomText, lookupWords, out var buildUp, out var avgLookUp);
        Console.WriteLine("Build-up time: {0}", buildUp);
        Console.WriteLine("Avg. look-up time: {0}", avgLookUp);
        writer.WriteLine("{0};{1};{2};{3}", trieTypeName, wordCount, buildUp, avgLookUp);
    }

    private ITrie<T> CreateTrie<T>(string trieTypeName) {
        var trieType = (TrieType)Enum.Parse(typeof(TrieType), trieTypeName);
        switch (trieType) {
            case TrieType.List:
                return new FakeTrie<T>();

            case TrieType.Patricia:
                return new PatriciaSuffixTrie<T>(3);

            case TrieType.Simple:
                return new SuffixTrie<T>(3);

            case TrieType.Ukkonen:
                return new CharUkkonenTrie<T>(3);

            default:
                throw new NotSupportedException();
        }
    }

    private void Measure(ITrie<string> trie, IEnumerable<string> randomText, IEnumerable<string> lookupWords, out TimeSpan buildUp, out TimeSpan avgLookUp) {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        foreach (var word in randomText) trie.Add(word, word);
        stopwatch.Stop();
        buildUp = stopwatch.Elapsed;


        var lookupCount = 0;
        stopwatch.Reset();
        foreach (var lookupWord in lookupWords) {
            lookupCount++;
            stopwatch.Start();
            var _ = trie.Retrieve(lookupWord).ToArray();
            stopwatch.Stop();
        }

        avgLookUp = lookupCount == 0 ? TimeSpan.Zero : new TimeSpan(stopwatch.ElapsedTicks / lookupCount);
    }
}