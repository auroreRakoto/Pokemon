using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>
    /// Deterministic priority-flood drainage prototype on the canonical spherical
    /// neighbour graph. Elevations are metres; water cells are drainage outlets.
    /// </summary>
    public static class SphericalDrainage
    {
        public static SphericalDrainageResult Build(
            SphericalNeighbourGraph graph,
            float[] elevationMeters,
            bool[] waterMask,
            float riverAccumulationThreshold)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (elevationMeters == null) throw new ArgumentNullException(nameof(elevationMeters));
            if (waterMask == null) throw new ArgumentNullException(nameof(waterMask));
            if (elevationMeters.Length != graph.NodeCount || waterMask.Length != graph.NodeCount)
                throw new ArgumentException("Elevation and water arrays must match the graph node count.");
            if (riverAccumulationThreshold < 1f || float.IsNaN(riverAccumulationThreshold) ||
                float.IsInfinity(riverAccumulationThreshold))
                throw new ArgumentOutOfRangeException(nameof(riverAccumulationThreshold));

            int count = graph.NodeCount;
            var phaseTimer = Stopwatch.StartNew();
            double priorityFloodMilliseconds = 0d;
            double flowDirectionMilliseconds = 0d;
            double accumulationMilliseconds = 0d;
            var filled = new float[count];
            var downstream = new int[count];
            var floodRank = new int[count];
            var visited = new bool[count];
            var outlets = new bool[count];
            var parent = new int[count];
            var heap = new NodeMinHeap();

            int waterCount = 0;
            for (int i = 0; i < count; i++)
            {
                float h = elevationMeters[i];
                if (float.IsNaN(h) || float.IsInfinity(h))
                    throw new ArgumentException("Elevation values must be finite.", nameof(elevationMeters));
                filled[i] = h;
                downstream[i] = -1;
                parent[i] = -1;
                if (waterMask[i])
                {
                    outlets[i] = true;
                    visited[i] = true;
                    floodRank[i] = -1;
                    heap.Push(i, h);
                    waterCount++;
                }
            }

            // A fully dry artificial test globe still needs a deterministic outlet.
            // Production requests should provide at least one ocean/lake water cell.
            if (waterCount == 0)
            {
                int lowest = 0;
                for (int i = 1; i < count; i++)
                    if (elevationMeters[i] < elevationMeters[lowest]) lowest = i;
                outlets[lowest] = true;
                visited[lowest] = true;
                heap.Push(lowest, elevationMeters[lowest]);
            }

            int nextRank = 0;
            while (heap.Count > 0)
            {
                int current = heap.Pop();
                floodRank[current] = nextRank++;
                int neighbourCount = graph.GetNeighbourCount(current);
                for (int edge = 0; edge < neighbourCount; edge++)
                {
                    int neighbour = graph.GetNeighbour(current, edge);
                    if (visited[neighbour]) continue;
                    visited[neighbour] = true;
                    parent[neighbour] = current;
                    filled[neighbour] = Math.Max(elevationMeters[neighbour], filled[current]);
                    heap.Push(neighbour, filled[neighbour]);
                }
            }

            priorityFloodMilliseconds = phaseTimer.Elapsed.TotalMilliseconds;
            phaseTimer.Restart();

            // Route each land cell to a strictly earlier flood-rank node.
            // Prefer the steepest filled-surface descent; use the flood parent
            // as the deterministic flat/depression fallback.
            for (int node = 0; node < count; node++)
            {
                if (outlets[node]) continue;
                int best = -1;
                float bestSlope = float.NegativeInfinity;
                int neighbours = graph.GetNeighbourCount(node);
                for (int edge = 0; edge < neighbours; edge++)
                {
                    int candidate = graph.GetNeighbour(node, edge);
                    if (!outlets[candidate] && floodRank[candidate] >= floodRank[node]) continue;
                    float drop = filled[node] - filled[candidate];
                    if (!outlets[candidate] && drop < -1e-4f) continue;
                    float slope = drop / graph.GetEdgeDistanceMeters(node, edge);
                    if (best < 0 || slope > bestSlope ||
                        (Math.Abs(slope - bestSlope) <= 1e-7f && candidate < best))
                    {
                        best = candidate;
                        bestSlope = slope;
                    }
                }
                if (best < 0) best = parent[node];
                if (best < 0 || (!outlets[best] && floodRank[best] >= floodRank[node]))
                    throw new InvalidOperationException("Drainage routing failed to find an acyclic downstream node.");
                downstream[node] = best;
            }

            flowDirectionMilliseconds = phaseTimer.Elapsed.TotalMilliseconds;
            phaseTimer.Restart();

            var accumulation = new float[count];
            for (int i = 0; i < count; i++) accumulation[i] = 1f;
            var order = new int[count];
            for (int i = 0; i < count; i++) order[floodRank[i]] = i;
            for (int rank = count - 1; rank >= 0; rank--)
            {
                int node = order[rank];
                int target = downstream[node];
                if (target >= 0) accumulation[target] += accumulation[node];
            }

            var riverMask = new bool[count];
            for (int i = 0; i < count; i++)
                riverMask[i] = !outlets[i] && accumulation[i] >= riverAccumulationThreshold;

            accumulationMilliseconds = phaseTimer.Elapsed.TotalMilliseconds;
            return new SphericalDrainageResult(filled, downstream, accumulation, riverMask,
                floodRank, outlets, waterCount, riverAccumulationThreshold,
                priorityFloodMilliseconds, flowDirectionMilliseconds, accumulationMilliseconds);
        }

        private sealed class NodeMinHeap
        {
            private readonly List<Entry> _items = new List<Entry>();
            public int Count => _items.Count;
            public void Push(int node, float priority)
            {
                var item = new Entry(node, priority);
                _items.Add(item);
                int index = _items.Count - 1;
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (Compare(_items[parent], item) <= 0) break;
                    _items[index] = _items[parent];
                    index = parent;
                }
                _items[index] = item;
            }
            public int Pop()
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
                return root.Node;
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

    public sealed class SphericalDrainageResult
    {
        public float[] FilledElevationMeters { get; }
        public int[] DownstreamNodeIds { get; }
        public float[] FlowAccumulation { get; }
        public bool[] RiverMask { get; }
        public int[] FloodRank { get; }
        public bool[] OutletMask { get; }
        public int InitialWaterNodeCount { get; }
        public float RiverAccumulationThreshold { get; }
        public double PriorityFloodMilliseconds { get; }
        public double FlowDirectionMilliseconds { get; }
        public double AccumulationMilliseconds { get; }

        internal SphericalDrainageResult(float[] filled, int[] downstream, float[] accumulation,
            bool[] riverMask, int[] floodRank, bool[] outlets, int waterCount, float threshold,
            double priorityFloodMilliseconds, double flowDirectionMilliseconds, double accumulationMilliseconds)
        {
            FilledElevationMeters = filled;
            DownstreamNodeIds = downstream;
            FlowAccumulation = accumulation;
            RiverMask = riverMask;
            FloodRank = floodRank;
            OutletMask = outlets;
            InitialWaterNodeCount = waterCount;
            RiverAccumulationThreshold = threshold;
            PriorityFloodMilliseconds = priorityFloodMilliseconds;
            FlowDirectionMilliseconds = flowDirectionMilliseconds;
            AccumulationMilliseconds = accumulationMilliseconds;
        }
    }
}
