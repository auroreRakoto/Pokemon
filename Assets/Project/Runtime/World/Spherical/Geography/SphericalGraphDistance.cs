using System;
using System.Collections.Generic;

namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>Multi-source Dijkstra over the CSR-backed spherical neighbour graph.</summary>
    public static class SphericalGraphDistance
    {
        public static float[] CalculateToNearestSource(SphericalNeighbourGraph graph, IList<int> sourceNodeIds)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (sourceNodeIds == null) throw new ArgumentNullException(nameof(sourceNodeIds));
            if (sourceNodeIds.Count == 0) throw new ArgumentException("At least one source node is required.", nameof(sourceNodeIds));

            var distances = new float[graph.NodeCount];
            for (int i = 0; i < distances.Length; i++) distances[i] = float.PositiveInfinity;
            var heap = new MinHeap();

            for (int i = 0; i < sourceNodeIds.Count; i++)
            {
                int source = sourceNodeIds[i];
                if (source < 0 || source >= graph.NodeCount)
                    throw new ArgumentOutOfRangeException(nameof(sourceNodeIds));
                if (distances[source] == 0f) continue;
                distances[source] = 0f;
                heap.Push(source, 0f);
            }

            while (heap.Count > 0)
            {
                heap.Pop(out int node, out float distance);
                if (distance > distances[node]) continue;
                int count = graph.GetNeighbourCount(node);
                for (int i = 0; i < count; i++)
                {
                    int neighbour = graph.GetNeighbour(node, i);
                    float candidate = distance + graph.GetEdgeDistanceMeters(node, i);
                    if (candidate >= distances[neighbour]) continue;
                    distances[neighbour] = candidate;
                    heap.Push(neighbour, candidate);
                }
            }
            return distances;
        }

        private sealed class MinHeap
        {
            private readonly List<Entry> _items = new List<Entry>();
            public int Count => _items.Count;
            public void Push(int node, float priority)
            {
                var entry = new Entry(node, priority);
                _items.Add(entry);
                int index = _items.Count - 1;
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (Compare(_items[parent], entry) <= 0) break;
                    _items[index] = _items[parent];
                    index = parent;
                }
                _items[index] = entry;
            }
            public void Pop(out int node, out float priority)
            {
                Entry root = _items[0];
                Entry last = _items[_items.Count - 1];
                _items.RemoveAt(_items.Count - 1);
                if (_items.Count > 0)
                {
                    int index = 0;
                    while (true)
                    {
                        int left = index * 2 + 1;
                        if (left >= _items.Count) break;
                        int right = left + 1;
                        int child = right < _items.Count && Compare(_items[right], _items[left]) < 0 ? right : left;
                        if (Compare(last, _items[child]) <= 0) break;
                        _items[index] = _items[child];
                        index = child;
                    }
                    _items[index] = last;
                }
                node = root.Node;
                priority = root.Priority;
            }
            private static int Compare(Entry a, Entry b)
            {
                int c = a.Priority.CompareTo(b.Priority);
                return c != 0 ? c : a.Node.CompareTo(b.Node);
            }
            private struct Entry
            {
                public readonly int Node;
                public readonly float Priority;
                public Entry(int node, float priority) { Node = node; Priority = priority; }
            }
        }
    }
}
