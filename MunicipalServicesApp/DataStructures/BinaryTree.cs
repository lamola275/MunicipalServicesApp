using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    public class BinaryTreeNode<T>
    {
        public T Value { get; set; }
        public BinaryTreeNode<T> Left { get; set; }
        public BinaryTreeNode<T> Right { get; set; }

        public BinaryTreeNode(T value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// General binary tree with the three classic traversal orders.
    /// The application builds a balanced tree from sorted service request references.
    /// </summary>
    public class BinaryTree<T>
    {
        public BinaryTreeNode<T> Root { get; set; }

        public BinaryTree()
        {
        }

        public BinaryTree(BinaryTreeNode<T> root)
        {
            Root = root;
        }

        public int Count
        {
            get { return CountNodes(Root); }
        }

        public int Height
        {
            get { return GetHeight(Root); }
        }

        private int CountNodes(BinaryTreeNode<T> node)
        {
            if (node == null) return 0;
            return 1 + CountNodes(node.Left) + CountNodes(node.Right);
        }

        private int GetHeight(BinaryTreeNode<T> node)
        {
            if (node == null) return 0;
            int left = GetHeight(node.Left);
            int right = GetHeight(node.Right);
            return 1 + (left > right ? left : right);
        }

        public IEnumerable<T> PreOrder()
        {
            var result = new List<T>();
            PreOrder(Root, result);
            return result;
        }

        private void PreOrder(BinaryTreeNode<T> node, IList<T> result)
        {
            if (node == null) return;
            result.Add(node.Value);
            PreOrder(node.Left, result);
            PreOrder(node.Right, result);
        }

        public IEnumerable<T> InOrder()
        {
            var result = new List<T>();
            InOrder(Root, result);
            return result;
        }

        private void InOrder(BinaryTreeNode<T> node, IList<T> result)
        {
            if (node == null) return;
            InOrder(node.Left, result);
            result.Add(node.Value);
            InOrder(node.Right, result);
        }

        public IEnumerable<T> PostOrder()
        {
            var result = new List<T>();
            PostOrder(Root, result);
            return result;
        }

        private void PostOrder(BinaryTreeNode<T> node, IList<T> result)
        {
            if (node == null) return;
            PostOrder(node.Left, result);
            PostOrder(node.Right, result);
            result.Add(node.Value);
        }
    }
}
