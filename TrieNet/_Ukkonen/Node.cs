using System;
using System.Collections.Generic;
using System.Linq;

namespace Gma.DataStructures.StringSearch
{
    internal class Node<K, T> where K : IComparable<K>
    {
        private readonly List<WordPosition<T>> _data;

        public List<(K, Edge<K, T>)> Edges { get; }

        public Node<K, T> Suffix { get; set; }

        public Node()
        {
            Edges = new List<(K, Edge<K, T>)>();
            _data = new List<WordPosition<T>>();
            Suffix = null;
        }

        public long Size() {
            long sum = 1;
            foreach (var key in Edges) {
                sum += key.Item2.Target.Size();
            }

            return sum;
        }

        public IEnumerable<WordPosition<T>> GetData()
        {
            var childData = Edges.SelectMany((e) => e.Item2.Target.GetData());
            return _data.Concat(childData);
        }

        public void AddRef(WordPosition<T> value)
        {
            if (_data.Contains(value))
                return;

            _data.Add(value);
            //  add this reference to all the suffixes as well
            var iter = Suffix;
            var i = 0;
            while (iter != null)
            {
                iter._data.Add(new WordPosition<T>(value.CharPosition + ++i, value.Value));
                iter = iter.Suffix;
            }
        }

        public void AddEdge(K ch, Edge<K, T> e) {
            var index = IndexOf(ch);
            if (index < 0) {
                Edges.Insert(~index, (ch, e));
            } else {
                Edges[index] = (ch, e);
            }
        }

        public Edge<K, T> GetEdge(K ch) {
            var index = IndexOf(ch);
            return index < 0 ? null : Edges[index].Item2;
        }

        private int IndexOf(K ch) {
            // Perform binary search to find the place of this character
            var n = Edges.Count;
            var min = 0;
            var max = n - 1;
            var comparer = Comparer<K>.Default;
            while (min <= max) {
                var mid = min + (max - min) / 2;
                K midTerm = Edges[mid].Item1;

                switch (comparer.Compare(midTerm, ch)) {
                    case 0:
                        return mid;
                    case < 0:
                        max = mid - 1;
                        break;
                    case > 0:
                        min = mid + 1;
                        break;
                }
            }

            return ~min;
        }
    }
}