using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Self-balancing AVL tree. Rotations keep the height close to log(n),
    /// so reference-number lookups remain predictable as request volume grows.
    /// </summary>
    public class AvlTree<T> where T : IComparable<T>
    {
        private class Node
        {
            public T Value;
            public Node Left;
            public Node Right;
            public int Height;

            public Node(T value)
            {
                Value = value;
                Height = 1;
            }
        }

        private Node _root;

        public int Count { get; private set; }

        public int Height
        {
            get { return GetHeight(_root); }
        }

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
                return node;
            }

            node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
            return Balance(node);
        }

        private Node Balance(Node node)
        {
            int balance = GetBalance(node);

            if (balance > 1)
            {
                if (GetBalance(node.Left) < 0)
                {
                    node.Left = RotateLeft(node.Left);
                }
                return RotateRight(node);
            }

            if (balance < -1)
            {
                if (GetBalance(node.Right) > 0)
                {
                    node.Right = RotateRight(node.Right);
                }
                return RotateLeft(node);
            }

            return node;
        }

        private Node RotateLeft(Node node)
        {
            var newRoot = node.Right;
            node.Right = newRoot.Left;
            newRoot.Left = node;
            UpdateHeight(node);
            UpdateHeight(newRoot);
            return newRoot;
        }

        private Node RotateRight(Node node)
        {
            var newRoot = node.Left;
            node.Left = newRoot.Right;
            newRoot.Right = node;
            UpdateHeight(node);
            UpdateHeight(newRoot);
            return newRoot;
        }

        private static int GetHeight(Node node)
        {
            return node == null ? 0 : node.Height;
        }

        private static void UpdateHeight(Node node)
        {
            node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
        }

        private static int GetBalance(Node node)
        {
            return node == null ? 0 : GetHeight(node.Left) - GetHeight(node.Right);
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
