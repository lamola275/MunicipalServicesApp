using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Node for a general (basic) tree in which every node may have any number of children.
    /// </summary>
    public class TreeNode<T>
    {
        public T Value { get; set; }
        public List<TreeNode<T>> Children { get; private set; }

        public TreeNode(T value)
        {
            Value = value;
            Children = new List<TreeNode<T>>();
        }

        public TreeNode<T> AddChild(T value)
        {
            var child = new TreeNode<T>(value);
            Children.Add(child);
            return child;
        }
    }

    /// <summary>
    /// Basic tree implementation used to model the municipal status hierarchy.
    /// It deliberately does not impose binary or ordering rules.
    /// </summary>
    public class BasicTree<T>
    {
        public TreeNode<T> Root { get; private set; }

        public BasicTree(T rootValue)
        {
            Root = new TreeNode<T>(rootValue);
        }

        public int Count
        {
            get { return CountNodes(Root); }
        }

        private int CountNodes(TreeNode<T> node)
        {
            if (node == null) return 0;
            int total = 1;
            foreach (var child in node.Children)
            {
                total += CountNodes(child);
            }
            return total;
        }

        public IEnumerable<T> DepthFirst()
        {
            var results = new List<T>();
            DepthFirst(Root, results);
            return results;
        }

        private void DepthFirst(TreeNode<T> node, IList<T> results)
        {
            if (node == null) return;
            results.Add(node.Value);
            foreach (var child in node.Children)
            {
                DepthFirst(child, results);
            }
        }

        public IEnumerable<T> BreadthFirst()
        {
            var results = new List<T>();
            if (Root == null) return results;

            var queue = new Queue<TreeNode<T>>();
            queue.Enqueue(Root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                results.Add(node.Value);
                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }
            return results;
        }

        public IEnumerable<string> ToIndentedLines()
        {
            var lines = new List<string>();
            BuildIndentedLines(Root, 0, lines);
            return lines;
        }

        private void BuildIndentedLines(TreeNode<T> node, int depth, IList<string> lines)
        {
            if (node == null) return;
            lines.Add(new string(' ', depth * 4) + "- " + node.Value);
            foreach (var child in node.Children)
            {
                BuildIndentedLines(child, depth + 1, lines);
            }
        }
    }
}
