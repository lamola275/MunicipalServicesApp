using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Red-black tree implementation using CLRS-style insert fix-up.
    /// It provides guaranteed O(log n) search and insertion for service request keys.
    /// </summary>
    public class RedBlackTree<T> where T : IComparable<T>
    {
        private enum NodeColor
        {
            Red,
            Black
        }

        private class Node
        {
            public T Value;
            public Node Left;
            public Node Right;
            public Node Parent;
            public NodeColor Color;

            public Node(T value)
            {
                Value = value;
                Color = NodeColor.Red;
            }
        }

        private Node _root;

        public int Count { get; private set; }

        public void Insert(T value)
        {
            if (Contains(value)) return;

            var inserted = new Node(value);
            var parent = default(Node);
            var current = _root;

            while (current != null)
            {
                parent = current;
                current = value.CompareTo(current.Value) < 0 ? current.Left : current.Right;
            }

            inserted.Parent = parent;
            if (parent == null)
            {
                _root = inserted;
            }
            else if (value.CompareTo(parent.Value) < 0)
            {
                parent.Left = inserted;
            }
            else
            {
                parent.Right = inserted;
            }

            Count++;
            FixInsert(inserted);
        }

        private void FixInsert(Node node)
        {
            while (node != _root && node.Parent.Color == NodeColor.Red)
            {
                if (node.Parent == node.Parent.Parent.Left)
                {
                    var uncle = node.Parent.Parent.Right;
                    if (IsRed(uncle))
                    {
                        node.Parent.Color = NodeColor.Black;
                        uncle.Color = NodeColor.Black;
                        node.Parent.Parent.Color = NodeColor.Red;
                        node = node.Parent.Parent;
                    }
                    else
                    {
                        if (node == node.Parent.Right)
                        {
                            node = node.Parent;
                            RotateLeft(node);
                        }
                        node.Parent.Color = NodeColor.Black;
                        node.Parent.Parent.Color = NodeColor.Red;
                        RotateRight(node.Parent.Parent);
                    }
                }
                else
                {
                    var uncle = node.Parent.Parent.Left;
                    if (IsRed(uncle))
                    {
                        node.Parent.Color = NodeColor.Black;
                        uncle.Color = NodeColor.Black;
                        node.Parent.Parent.Color = NodeColor.Red;
                        node = node.Parent.Parent;
                    }
                    else
                    {
                        if (node == node.Parent.Left)
                        {
                            node = node.Parent;
                            RotateRight(node);
                        }
                        node.Parent.Color = NodeColor.Black;
                        node.Parent.Parent.Color = NodeColor.Red;
                        RotateLeft(node.Parent.Parent);
                    }
                }
            }
            _root.Color = NodeColor.Black;
        }

        private static bool IsRed(Node node)
        {
            return node != null && node.Color == NodeColor.Red;
        }

        private void RotateLeft(Node node)
        {
            var pivot = node.Right;
            node.Right = pivot.Left;
            if (pivot.Left != null) pivot.Left.Parent = node;

            pivot.Parent = node.Parent;
            if (node.Parent == null)
            {
                _root = pivot;
            }
            else if (node == node.Parent.Left)
            {
                node.Parent.Left = pivot;
            }
            else
            {
                node.Parent.Right = pivot;
            }

            pivot.Left = node;
            node.Parent = pivot;
        }

        private void RotateRight(Node node)
        {
            var pivot = node.Left;
            node.Left = pivot.Right;
            if (pivot.Right != null) pivot.Right.Parent = node;

            pivot.Parent = node.Parent;
            if (node.Parent == null)
            {
                _root = pivot;
            }
            else if (node == node.Parent.Right)
            {
                node.Parent.Right = pivot;
            }
            else
            {
                node.Parent.Left = pivot;
            }

            pivot.Right = node;
            node.Parent = pivot;
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
