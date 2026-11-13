using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MunicipalServicesApp.DataStructures;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Services
{
    /// <summary>
    /// Builds and exposes every advanced data structure used by the Service
    /// Request Status feature. The raw repository remains the source of truth;
    /// this service creates efficient indexes and relationship views over it.
    /// </summary>
    public class ServiceRequestAnalytics
    {
        public BasicTree<string> StatusHierarchy { get; private set; }
        public BinaryTree<string> ReferenceBinaryTree { get; private set; }
        public BinarySearchTree<RequestKey> ReferenceBst { get; private set; }
        public AvlTree<RequestKey> ReferenceAvl { get; private set; }
        public RedBlackTree<RequestKey> ReferenceRedBlack { get; private set; }
        public BinaryHeap<RequestPriorityItem> PriorityHeap { get; private set; }
        public ServiceRequestGraph RelationshipGraph { get; private set; }

        public ServiceRequestAnalytics()
        {
            Rebuild(new List<IssueReport>());
        }

        public void Rebuild(IEnumerable<IssueReport> requests)
        {
            var ordered = requests
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.ReferenceNumber))
                .OrderBy(r => r.ReferenceNumber, StringComparer.OrdinalIgnoreCase)
                .ToList();

            StatusHierarchy = BuildStatusHierarchy(ordered);
            ReferenceBinaryTree = BuildBalancedBinaryTree(ordered);

            ReferenceBst = new BinarySearchTree<RequestKey>();
            ReferenceAvl = new AvlTree<RequestKey>();
            ReferenceRedBlack = new RedBlackTree<RequestKey>();
            PriorityHeap = new BinaryHeap<RequestPriorityItem>();

            foreach (var request in ordered)
            {
                var key = new RequestKey(request.ReferenceNumber, request);
                ReferenceBst.Insert(key);
                ReferenceAvl.Insert(key);
                ReferenceRedBlack.Insert(key);
                PriorityHeap.Enqueue(new RequestPriorityItem(request));
            }

            RelationshipGraph = ServiceRequestGraph.Build(ordered);
        }

        public IssueReport FindByReference(string referenceNumber)
        {
            RequestKey found;
            var probe = new RequestKey((referenceNumber ?? string.Empty).Trim(), null);
            if (ReferenceAvl.TryFind(probe, out found)) return found.Request;
            if (ReferenceBst.TryFind(probe, out found)) return found.Request;
            if (ReferenceRedBlack.TryFind(probe, out found)) return found.Request;
            return null;
        }

        public string GetLookupDiagnostics(string referenceNumber)
        {
            var probe = new RequestKey((referenceNumber ?? string.Empty).Trim(), null);
            RequestKey found;
            bool bst = ReferenceBst.TryFind(probe, out found);
            bool avl = ReferenceAvl.TryFind(probe, out found);
            bool redBlack = ReferenceRedBlack.TryFind(probe, out found);

            return "BST lookup: " + (bst ? "found" : "not found") + Environment.NewLine +
                   "AVL lookup: " + (avl ? "found" : "not found") + Environment.NewLine +
                   "Red-black lookup: " + (redBlack ? "found" : "not found");
        }

        public List<IssueReport> GetRequestsByReferenceOrder()
        {
            return ReferenceRedBlack.InOrder().Select(k => k.Request).ToList();
        }

        public List<IssueReport> GetRequestsByPriority()
        {
            return PriorityHeap.DequeueAll().Select(item => item.Request).ToList();
        }

        public string BuildDataStructureSummary()
        {
            var builder = new StringBuilder();
            builder.AppendLine("ADVANCED DATA STRUCTURE VIEW");
            builder.AppendLine("================================");
            builder.AppendLine();

            builder.AppendLine("1. BASIC TREE - status workflow hierarchy");
            builder.AppendLine("   Nodes: " + StatusHierarchy.Count);
            foreach (var line in StatusHierarchy.ToIndentedLines())
            {
                builder.AppendLine("   " + line);
            }
            builder.AppendLine();

            builder.AppendLine("2. BINARY TREE - balanced reference tree");
            builder.AppendLine("   Nodes: " + ReferenceBinaryTree.Count + " | Height: " + ReferenceBinaryTree.Height);
            builder.AppendLine("   Pre-order:  " + Format(ReferenceBinaryTree.PreOrder()));
            builder.AppendLine("   In-order:   " + Format(ReferenceBinaryTree.InOrder()));
            builder.AppendLine("   Post-order: " + Format(ReferenceBinaryTree.PostOrder()));
            builder.AppendLine();

            builder.AppendLine("3. BINARY SEARCH TREE");
            builder.AppendLine("   Nodes: " + ReferenceBst.Count);
            builder.AppendLine("   Sorted references: " + Format(ReferenceBst.InOrder().Select(k => k.ReferenceNumber)));
            builder.AppendLine();

            builder.AppendLine("4. AVL TREE");
            builder.AppendLine("   Nodes: " + ReferenceAvl.Count + " | Guaranteed-balanced height: " + ReferenceAvl.Height);
            builder.AppendLine("   Sorted references: " + Format(ReferenceAvl.InOrder().Select(k => k.ReferenceNumber)));
            builder.AppendLine();

            builder.AppendLine("5. RED-BLACK TREE");
            builder.AppendLine("   Nodes: " + ReferenceRedBlack.Count);
            builder.AppendLine("   Sorted references: " + Format(ReferenceRedBlack.InOrder().Select(k => k.ReferenceNumber)));
            builder.AppendLine();

            builder.AppendLine("6. BINARY HEAP - municipal priority queue");
            foreach (var item in PriorityHeap.DequeueAll())
            {
                builder.AppendLine("   " + item);
            }

            return builder.ToString();
        }

        public string BuildGraphSummary(string startReference)
        {
            var builder = new StringBuilder();
            builder.AppendLine("GRAPH, TRAVERSAL AND MINIMUM SPANNING TREE");
            builder.AppendLine("==========================================");
            builder.AppendLine("Vertices: " + RelationshipGraph.VertexCount + " | Unique weighted edges: " + RelationshipGraph.EdgeCount);
            builder.AppendLine();

            if (string.IsNullOrWhiteSpace(startReference))
            {
                startReference = RelationshipGraph.Vertices.OrderBy(v => v).FirstOrDefault();
            }

            if (!string.IsNullOrWhiteSpace(startReference))
            {
                builder.AppendLine("Traversal start: " + startReference);
                builder.AppendLine("Breadth-first: " + Format(RelationshipGraph.BreadthFirst(startReference)));
                builder.AppendLine("Depth-first:   " + Format(RelationshipGraph.DepthFirst(startReference)));
                builder.AppendLine();
            }

            var minimumEdges = RelationshipGraph.MinimumSpanningTree();
            builder.AppendLine("Minimum spanning tree / forest (Prim's algorithm)");
            builder.AppendLine("Edges selected: " + minimumEdges.Count +
                               " | Total relationship cost: " + minimumEdges.Sum(e => e.Weight));
            foreach (var edge in minimumEdges)
            {
                builder.AppendLine("   " + edge);
            }

            return builder.ToString();
        }

        private static BasicTree<string> BuildStatusHierarchy(IEnumerable<IssueReport> requests)
        {
            var tree = new BasicTree<string>("Municipal Service Requests");
            var statusNodes = new Dictionary<string, TreeNode<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var request in requests)
            {
                TreeNode<string> statusNode;
                if (!statusNodes.TryGetValue(request.Status, out statusNode))
                {
                    statusNode = tree.Root.AddChild(request.Status);
                    statusNodes.Add(request.Status, statusNode);
                }
                statusNode.AddChild(request.ReferenceNumber + " - " + request.Category);
            }
            return tree;
        }

        private static BinaryTree<string> BuildBalancedBinaryTree(IList<IssueReport> ordered)
        {
            return new BinaryTree<string>(BuildBalancedNode(ordered, 0, ordered.Count - 1));
        }

        private static BinaryTreeNode<string> BuildBalancedNode(IList<IssueReport> ordered, int start, int end)
        {
            if (start > end) return null;
            int middle = start + (end - start) / 2;
            var node = new BinaryTreeNode<string>(ordered[middle].ReferenceNumber)
            {
                Left = BuildBalancedNode(ordered, start, middle - 1),
                Right = BuildBalancedNode(ordered, middle + 1, end)
            };
            return node;
        }

        private static string Format(IEnumerable<string> values)
        {
            var list = values.ToList();
            return list.Count == 0 ? "(none)" : string.Join(", ", list);
        }
    }
}
