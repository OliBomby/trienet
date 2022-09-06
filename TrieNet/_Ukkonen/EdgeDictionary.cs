using System;
using System.Collections.Generic;

namespace Gma.DataStructures.StringSearch
{
    internal class EdgeDictionary<K, T> : SortedList<K, Edge<K, T>> where K : IComparable<K> { }
}