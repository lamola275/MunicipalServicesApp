using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Array-backed binary min-heap. The smallest item according to the supplied
    /// comparer is always available at the root in O(1) time.
    /// </summary>
    public class BinaryHeap<T>
    {
        private readonly List<T> _items;
        private readonly IComparer<T> _comparer;

        public BinaryHeap() : this(Comparer<T>.Default)
        {
        }

        public BinaryHeap(IComparer<T> comparer)
        {
            _items = new List<T>();
            _comparer = comparer ?? Comparer<T>.Default;
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public void Enqueue(T item)
        {
            _items.Add(item);
            SiftUp(_items.Count - 1);
        }

        public T Peek()
        {
            if (_items.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }
            return _items[0];
        }

        public T Dequeue()
        {
            if (_items.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }

            T result = _items[0];
            int lastIndex = _items.Count - 1;
            _items[0] = _items[lastIndex];
            _items.RemoveAt(lastIndex);
            if (_items.Count > 0)
            {
                SiftDown(0);
            }
            return result;
        }

        public List<T> DequeueAll()
        {
            var copy = new BinaryHeap<T>(_comparer);
            foreach (var item in _items)
            {
                copy.Enqueue(item);
            }

            var result = new List<T>();
            while (copy.Count > 0)
            {
                result.Add(copy.Dequeue());
            }
            return result;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_comparer.Compare(_items[index], _items[parent]) >= 0)
                {
                    break;
                }
                Swap(index, parent);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            while (true)
            {
                int left = index * 2 + 1;
                int right = left + 1;
                int smallest = index;

                if (left < _items.Count && _comparer.Compare(_items[left], _items[smallest]) < 0)
                {
                    smallest = left;
                }
                if (right < _items.Count && _comparer.Compare(_items[right], _items[smallest]) < 0)
                {
                    smallest = right;
                }
                if (smallest == index)
                {
                    return;
                }
                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int first, int second)
        {
            T temporary = _items[first];
            _items[first] = _items[second];
            _items[second] = temporary;
        }
    }
}
