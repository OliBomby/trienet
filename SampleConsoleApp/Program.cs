// This code is distributed under MIT license. Copyright (c) 2022 OliBomby
// See license.txt or http://opensource.org/licenses/mit-license.php

using System.Diagnostics;
using System.Text;
using TrieNet;
using TrieNet.Ukkonen;

namespace SampleConsoleApp;

internal class Program {
    private static void Main(string[] _) {
        var trie = new UkkonenTrie<char, int>(0);
        //You can replace it with other trie data structures too
        //ITrie<int> trie = new Trie<int>();
        //ITrie<int> trie = new PatriciaSuffixTrie<int>(3);


        try {
            //Build-up
            BuildUp("sample.txt", trie);
            //Look-up
            LookUp("a", trie);
            LookUp("e", trie);
            LookUp("u", trie);
            LookUp("i", trie);
            LookUp("o", trie);
            LookUp("fox", trie);
            LookUp("overs", trie);
            LookUp("porta", trie);
            LookUp("supercalifragilisticexpialidocious", trie);
        }
        catch (IOException ioException) {
            Console.WriteLine("Error: {0}", ioException.Message);
        }
        catch (UnauthorizedAccessException unauthorizedAccessException) {
            Console.WriteLine("Error: {0}", unauthorizedAccessException.Message);
        }

        Console.WriteLine("-------------Press any key to quit--------------");
        Console.ReadKey();
    }

    private static void BuildUp(string fileName, IGenericSuffixTrie<char, int> trie) {
        var allWordsInFile = GetWordsFromFile(fileName);
        var i = 0;
        foreach (var word in allWordsInFile) trie.Add(word.AsMemory(), ++i);
    }

    private static void LookUp(string searchString, IGenericSuffixTrie<char, int> trie) {
        Console.WriteLine("----------------------------------------");
        Console.WriteLine("Look-up for string '{0}'", searchString);
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var result = trie.RetrieveSubstrings(searchString.AsSpan()).ToArray();
        stopWatch.Stop();

        var matchesText = string.Join(",", result);
        var matchesCount = result.Count();

        if (matchesCount == 0)
            Console.WriteLine("No matches found.\tTime: {0}", stopWatch.Elapsed);
        else
            Console.WriteLine(" {0} matches found. \tTime: {1}\tLines: {2}", matchesCount, stopWatch.Elapsed,
                matchesText);
    }


    private static IEnumerable<string> GetWordsFromFile(string file) {
        using var reader = File.OpenText(file);
        Console.WriteLine("Processing file {0} ...", file);
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var lineNo = 0;
        while (!reader.EndOfStream) {
            var line = reader.ReadLine();
            lineNo++;
            var words = GetWordsFromLine(line!);
            foreach (var word in words) yield return word;
        }

        stopWatch.Stop();
        Console.WriteLine("Lines:{0}\tTime:{1}", lineNo, stopWatch.Elapsed);
    }

    private static IEnumerable<string> GetWordsFromLine(string line) {
        var word = new StringBuilder();
        foreach (var ch in line)
            if (char.IsLetter(ch)) {
                word.Append(ch);
            }
            else {
                if (word.Length == 0) continue;
                yield return word.ToString();
                word.Clear();
            }

        if (word.Length == 0) yield break;
        yield return word.ToString();
    }
}