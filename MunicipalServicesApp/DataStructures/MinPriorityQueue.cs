using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{

    public class MinPriorityQueue<T>
    {
        private readonly List<KeyValuePair<int, T>> _heap = new List<KeyValuePair<int, T>>();

        public int Count
        {
            get { return _heap.Count; }
        }

        public void Enqueue(T item, int priority)
        {
            _heap.Add(new KeyValuePair<int, T>(priority, item));
            SiftUp(_heap.Count - 1);
        }

        public T Dequeue()
        {
            if (_heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            T top = _heap[0].Value;

            _heap[0] = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);
            if (_heap.Count > 0)
            {
                SiftDown(0);
            }
            return top;
        }

        public T Peek()
        {
            if (_heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }
            return _heap[0].Value;
        }

        public List<T> OrderedItems()
        {
            var copy = new List<KeyValuePair<int, T>>(_heap);
            var result = new List<T>();

            while (copy.Count > 0)
            {
                result.Add(copy[0].Value);
                copy[0] = copy[copy.Count - 1];
                copy.RemoveAt(copy.Count - 1);


                int i = 0;
                while (true)
                {
                    int left = 2 * i + 1;
                    int right = 2 * i + 2;
                    int smallest = i;

                    if (left < copy.Count && copy[left].Key < copy[smallest].Key) smallest = left;
                    if (right < copy.Count && copy[right].Key < copy[smallest].Key) smallest = right;
                    if (smallest == i) break;

                    var tmp = copy[i];
                    copy[i] = copy[smallest];
                    copy[smallest] = tmp;
                    i = smallest;
                }
            }
            return result;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_heap[index].Key >= _heap[parent].Key) break;

                var tmp = _heap[index];
                _heap[index] = _heap[parent];
                _heap[parent] = tmp;
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;
                int smallest = index;

                if (left < _heap.Count && _heap[left].Key < _heap[smallest].Key) smallest = left;
                if (right < _heap.Count && _heap[right].Key < _heap[smallest].Key) smallest = right;
                if (smallest == index) break;

                var tmp = _heap[index];
                _heap[index] = _heap[smallest];
                _heap[smallest] = tmp;
                index = smallest;
            }
        }
    }
}
