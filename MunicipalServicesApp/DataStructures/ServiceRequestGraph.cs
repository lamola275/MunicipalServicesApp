using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// Weighted undirected edge between two service request references.
    /// </summary>
    public class GraphEdge
    {
        public string From { get; private set; }
        public string To { get; private set; }
        public int Weight { get; private set; }
        public string Reason { get; private set; }

        public GraphEdge(string from, string to, int weight, string reason)
        {
            From = from;
            To = to;
            Weight = weight;
            Reason = reason;
        }

        public bool Connects(string first, string second)
        {
            return (From == first && To == second) || (From == second && To == first);
        }

        public GraphEdge Reverse()
        {
            return new GraphEdge(To, From, Weight, Reason);
        }

        public override string ToString()
        {
            return From + " --(" + Weight + ", " + Reason + ")--> " + To;
        }
    }

    /// <summary>
    /// Adjacency-list graph used to model relationships between service requests.
    /// Lower edge weights represent stronger operational relationships.
    /// </summary>
    public class ServiceRequestGraph
    {
        private readonly Dictionary<string, List<GraphEdge>> _adjacency;
        private readonly Dictionary<string, IssueReport> _requests;

        private ServiceRequestGraph()
        {
            _adjacency = new Dictionary<string, List<GraphEdge>>(StringComparer.OrdinalIgnoreCase);
            _requests = new Dictionary<string, IssueReport>(StringComparer.OrdinalIgnoreCase);
        }

        public int VertexCount
        {
            get { return _adjacency.Count; }
        }

        public int EdgeCount
        {
            get { return GetUniqueEdges().Count; }
        }

        public IEnumerable<string> Vertices
        {
            get { return _adjacency.Keys; }
        }

        public static ServiceRequestGraph Build(IEnumerable<IssueReport> requests)
        {
            var graph = new ServiceRequestGraph();
            var ordered = requests.OrderBy(r => r.DateSubmitted).ThenBy(r => r.ReferenceNumber).ToList();

            foreach (var request in ordered)
            {
                graph.AddVertex(request);
            }

            // A chronological chain guarantees that every vertex is reachable.
            for (int i = 1; i < ordered.Count; i++)
            {
                graph.AddEdge(ordered[i - 1], ordered[i], 4, "submission sequence");
            }

            AddGroupedEdges(graph, ordered.GroupBy(r => Normalize(r.Category)), 2, "same category");
            AddGroupedEdges(graph, ordered.GroupBy(r => Normalize(r.Location)), 1, "same location");
            AddGroupedEdges(graph, ordered.GroupBy(r => Normalize(r.AssignedDepartment)), 3, "same department");
            return graph;
        }

        private static void AddGroupedEdges(
            ServiceRequestGraph graph,
            IEnumerable<IGrouping<string, IssueReport>> groups,
            int weight,
            string reason)
        {
            foreach (var group in groups)
            {
                var items = group.OrderBy(r => r.DateSubmitted).ToList();
                for (int i = 1; i < items.Count; i++)
                {
                    graph.AddEdge(items[i - 1], items[i], weight, reason);
                }
            }
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }

        private void AddVertex(IssueReport request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ReferenceNumber)) return;
            if (!_adjacency.ContainsKey(request.ReferenceNumber))
            {
                _adjacency.Add(request.ReferenceNumber, new List<GraphEdge>());
                _requests.Add(request.ReferenceNumber, request);
            }
        }

        private void AddEdge(IssueReport first, IssueReport second, int weight, string reason)
        {
            if (first == null || second == null) return;
            if (string.Equals(first.ReferenceNumber, second.ReferenceNumber, StringComparison.OrdinalIgnoreCase)) return;

            AddEdge(first.ReferenceNumber, second.ReferenceNumber, weight, reason);
        }

        private void AddEdge(string first, string second, int weight, string reason)
        {
            if (!_adjacency.ContainsKey(first) || !_adjacency.ContainsKey(second)) return;

            var existing = _adjacency[first].FirstOrDefault(e => e.To.Equals(second, StringComparison.OrdinalIgnoreCase));
            if (existing != null && existing.Weight <= weight) return;

            if (existing != null)
            {
                _adjacency[first].Remove(existing);
                var reverse = _adjacency[second].FirstOrDefault(e => e.To.Equals(first, StringComparison.OrdinalIgnoreCase));
                if (reverse != null) _adjacency[second].Remove(reverse);
            }

            _adjacency[first].Add(new GraphEdge(first, second, weight, reason));
            _adjacency[second].Add(new GraphEdge(second, first, weight, reason));
        }

        public List<string> BreadthFirst(string startReference)
        {
            var result = new List<string>();
            if (!_adjacency.ContainsKey(startReference)) return result;

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>();
            queue.Enqueue(startReference);
            visited.Add(startReference);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                result.Add(current);
                foreach (var edge in _adjacency[current].OrderBy(e => e.Weight).ThenBy(e => e.To))
                {
                    if (visited.Add(edge.To))
                    {
                        queue.Enqueue(edge.To);
                    }
                }
            }
            return result;
        }

        public List<string> DepthFirst(string startReference)
        {
            var result = new List<string>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DepthFirst(startReference, visited, result);
            return result;
        }

        private void DepthFirst(string current, ISet<string> visited, IList<string> result)
        {
            if (string.IsNullOrWhiteSpace(current) || !_adjacency.ContainsKey(current) || !visited.Add(current))
            {
                return;
            }

            result.Add(current);
            foreach (var edge in _adjacency[current].OrderBy(e => e.Weight).ThenBy(e => e.To))
            {
                DepthFirst(edge.To, visited, result);
            }
        }

        /// <summary>
        /// Prim's algorithm. If the graph is disconnected, this returns a minimum spanning forest.
        /// </summary>
        public List<GraphEdge> MinimumSpanningTree()
        {
            var result = new List<GraphEdge>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var uniqueEdges = GetUniqueEdges();

            foreach (var start in _adjacency.Keys.OrderBy(k => k))
            {
                if (visited.Contains(start)) continue;
                visited.Add(start);

                while (true)
                {
                    GraphEdge best = null;
                    foreach (var edge in uniqueEdges)
                    {
                        bool fromVisited = visited.Contains(edge.From);
                        bool toVisited = visited.Contains(edge.To);
                        if (fromVisited == toVisited) continue;
                        if (best == null || edge.Weight < best.Weight ||
                            (edge.Weight == best.Weight && string.CompareOrdinal(edge.To, best.To) < 0))
                        {
                            best = edge;
                        }
                    }

                    if (best == null) break;
                    result.Add(best);
                    visited.Add(visited.Contains(best.From) ? best.To : best.From);
                }
            }
            return result;
        }

        public List<GraphEdge> GetUniqueEdges()
        {
            var result = new List<GraphEdge>();
            foreach (var pair in _adjacency)
            {
                foreach (var edge in pair.Value)
                {
                    if (string.CompareOrdinal(edge.From, edge.To) < 0)
                    {
                        result.Add(edge);
                    }
                }
            }
            return result.OrderBy(e => e.Weight).ThenBy(e => e.From).ThenBy(e => e.To).ToList();
        }

        public IssueReport FindRequest(string reference)
        {
            IssueReport request;
            return _requests.TryGetValue(reference, out request) ? request : null;
        }
    }
}
