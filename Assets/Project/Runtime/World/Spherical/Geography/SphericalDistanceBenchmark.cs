using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Geography
{
    public sealed class SphericalDistanceBenchmarkResult
    {
        public int NodeCount { get; internal set; }
        public int EdgeCount { get; internal set; }
        public int SourceCount { get; internal set; }
        public double DijkstraMilliseconds { get; internal set; }
        public double GreatCircleReferenceMilliseconds { get; internal set; }
        public float MeanAbsoluteErrorMeters { get; internal set; }
        public float MaximumAbsoluteErrorMeters { get; internal set; }
        public int ComparedNodes { get; internal set; }
        public long EstimatedGraphStorageBytes { get; internal set; }
    }

    /// <summary>
    /// Small-scale comparison harness. The all-pairs-to-sources great-circle reference
    /// is intentionally O(nodes × sources) and must not be used for production resolution.
    /// </summary>
    public static class SphericalDistanceBenchmark
    {
        public static SphericalDistanceBenchmarkResult Compare(
            SphericalNeighbourGraph graph, IList<int> sourceNodeIds, int maximumComparedNodes = 512)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (sourceNodeIds == null || sourceNodeIds.Count == 0)
                throw new ArgumentException("At least one source is required.", nameof(sourceNodeIds));
            if (maximumComparedNodes < 1) throw new ArgumentOutOfRangeException(nameof(maximumComparedNodes));

            var timer = Stopwatch.StartNew();
            float[] graphDistances = SphericalGraphDistance.CalculateToNearestSource(graph, sourceNodeIds);
            timer.Stop();
            double dijkstraMs = timer.Elapsed.TotalMilliseconds;

            int compared = Math.Min(graph.NodeCount, maximumComparedNodes);
            timer.Restart();
            float errorSum = 0f;
            float maxError = 0f;
            for (int sample = 0; sample < compared; sample++)
            {
                // Evenly distributed deterministic sampling covers the full sphere,
                // unlike comparing only the first contiguous block of node IDs.
                int node = compared == graph.NodeCount
                    ? sample
                    : (int)((long)sample * (graph.NodeCount - 1) / Math.Max(1, compared - 1));
                float reference = float.PositiveInfinity;
                Vector3 direction = graph.GetDirection(node);
                for (int sourceIndex = 0; sourceIndex < sourceNodeIds.Count; sourceIndex++)
                {
                    Vector3 sourceDirection = graph.GetDirection(sourceNodeIds[sourceIndex]);
                    float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(direction, sourceDirection), -1f, 1f));
                    float distance = angle * graph.PlanetRadiusMeters;
                    if (distance < reference) reference = distance;
                }
                float error = Math.Abs(graphDistances[node] - reference);
                errorSum += error;
                if (error > maxError) maxError = error;
            }
            timer.Stop();

            return new SphericalDistanceBenchmarkResult
            {
                NodeCount = graph.NodeCount,
                EdgeCount = graph.EdgeCount,
                SourceCount = sourceNodeIds.Count,
                DijkstraMilliseconds = dijkstraMs,
                GreatCircleReferenceMilliseconds = timer.Elapsed.TotalMilliseconds,
                MeanAbsoluteErrorMeters = errorSum / compared,
                MaximumAbsoluteErrorMeters = maxError,
                ComparedNodes = compared,
                EstimatedGraphStorageBytes = graph.EstimatedStorageBytes
            };
        }
    }
}
