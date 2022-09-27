[![Build status](https://ci.appveyor.com/api/projects/status/4ruj2ijq2uc0pu2m/branch/master?svg=true)](https://ci.appveyor.com/project/gmamaladze/trienet/branch/master) [![NuGet version](https://badge.fury.io/nu/TrieNet.svg)](https://badge.fury.io/nu/TrieNet)

![TrieNet - The library provides .NET Data Structures for Prefix String Search and Substring (Infix) Search to Implement Auto-completion and Intelli-sense.](/img/trienet.png)

# Usage

<pre>
  nuget install TODO
</pre>

```csharp
using TrieNet.Ukkonen;
	
...

var trie = new CharUkkonenTrie<int>(3);
//var trie = new GenerixSuffixTrie<char, int>(3);

trie.Add("hello", 1);
trie.Add("world", 2);
trie.Add("hell", 3);

var result = trie.Retrieve("hel");
// result = { new WordPosition(0, 1), new WordPosition(0, 3) };
```

## Implementation

This small library contains a bunch of trie data structures all having the same interface:

```csharp
public interface ITrie {
  IEnumerable Retrieve(string query);
  void Add(string key, TValue value);
}
```

| Class                | Description                                                                                                                                         |
|----------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
 | `Trie`               | The simple trie, allows only prefix search, like `.Where(s => s.StartsWith(searchString))`                                                          |
 | `SuffixTrie`         | Also allows infix search, like `.Where(s => s.Contains(searchString))`                                                                              |
 | `PatriciaTrie`       | Compressed trie, more compact, a bit more efficient during look-up, but a quite slower during build-up.                                             |
 | `SuffixPatriciaTrie` | The same as `PatriciaTrie`, also enabling infix search.                                                                                             |
 | `ParallelTrie`       | Very primitively implemented parallel data structure which allows adding data and retrieving results from different threads simultaneusly.          |
 | `CharUkkonenTrie`    | The same as `SuffixPatriciaTrie`, but more compact, more efficient during look-up, and quite a lot faster during build-up. This is the best choice. |

There is also a generic interface which lets you search for more than just strings:

```csharp
public interface IGenericTrie<TKey, TValue> where TKey : IEquatable<TKey> {
    IEnumerable<TValue> Retrieve(ReadOnlySpan<TKey> query);
    void Add(ReadOnlyMemory<TKey> key, TValue value);
}
```

At the moment only `UkkonenTrie` implements this interface.

## Performance

I don't have charts to show at the moment, but trust me that the `UkkonenTrie` is the fastest and most optimised.

## Demo app

The app demonstrates indexing of large text files and look-up inside them. Indexing usually takes only a few seconds and the look-up delay will be unnoticeable for
the user.

![](/img/trie-demo-app.png)

