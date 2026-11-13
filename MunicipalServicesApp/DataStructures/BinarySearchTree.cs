using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Standard binary search tree. Duplicate values are ignored.
    /// </summary>
    public class BinarySearchTree<T> where T : IComparable<T>
    {
        private class Node
        {
            public T Value;
            public Node Left;
            public Node Right;

            public Node(T value)
            {
                Value = value;
            }
        }

        private Node _root;

        public int Count { get; private set; }

        public void Insert(T value)
        {
            bool inserted;
            _root = Insert(_root, value, out inserted);
            if (inserted) Count++;
        }

        private Node Insert(Node node, T value, out bool inserted)
        {
            if (node == null)
            {
                inserted = true;
                return new Node(value);
            }

            int comparison = value.CompareTo(node.Value);
            if (comparison < 0)
            {
                node.Left = Insert(node.Left, value, out inserted);
            }
            else if (comparison > 0)
            {
                node.Right = Insert(node.Right, value, out inserted);
            }
            else
            {
                inserted = false;
            }
            return node;
        }

        public bool Contains(T value)
        {
            T ignored;
            return TryFind(value, out ignored);
        }

        public bool TryFind(T value, out T found)
        {
            var node = _root;
            while (node != null)
            {
                int comparison = value.CompareTo(node.Value);
                if (comparison == 0)
                {
                    found = node.Value;
                    return true;
                }
                node = comparison < 0 ? node.Left : node.Right;
            }
            found = default(T);
            return false;
        }

        public IEnumerable<T> InOrder()
        {
            var result = new List<T>();
            InOrder(_root, result);
            return result;
        }

        private void InOrder(Node node, IList<T> result)
        {
            if (node == null) return;
            InOrder(node.Left, result);
            result.Add(node.Value);
            InOrder(node.Right, result);
        }
    }
}
