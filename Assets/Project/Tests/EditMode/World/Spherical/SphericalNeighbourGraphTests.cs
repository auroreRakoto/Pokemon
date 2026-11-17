using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Geography;
using UnityEngine;

namespace Pokemon.Project.Tests.EditMode.World.Spherical
{
    public sealed class SphericalNeighbourGraphTests
    {
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        public void Build_MergesSharedFaceEdgesAndCorners(int resolution)
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(resolution, 50000f);
            int expectedUniqueNodes = 6 * resolution * resolution + 2;
            Assert.AreEqual(expectedUniqueNodes, graph.NodeCount,
                "A six-face cube surface has 6N² + 2 unique lattice vertices.");
        }

        [Test]
        public void Build_ProducesSymmetricConnectedPositiveEdges()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(8, 50000f);
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(0);
            visited.Add(0);
            while (queue.Count > 0)
            {
                int node = queue.Dequeue();
                var neighbours = graph.GetNeighbours(node);
                Assert.GreaterOrEqual(neighbours.Count, 3);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    int other = neighbours[i];
                    Assert.Greater(graph.GetEdgeDistanceMeters(node, i), 0f);
                    var reverse = graph.GetNeighbours(other);
                    bool found = false;
                    for (int j = 0; j < reverse.Count; j++)
                        if (reverse[j] == node) { found = true; break; }
                    Assert.IsTrue(found, "Every edge must be bidirectional.");
                    if (visited.Add(other)) queue.Enqueue(other);
                }
            }
            Assert.AreEqual(graph.NodeCount, visited.Count, "All sphere samples must belong to one connected graph.");
        }

        [Test]
        public void FaceSeams_MapToTheSameCanonicalNode()
        {
            const int resolution = 12;
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(resolution, 50000f);
            // +X at u=-1 and +Z at u=+1 describe the same geometric edge.
            int first = graph.GetNodeId(CubeSphereFace.PositiveX, 0, resolution / 2);
            int second = graph.GetNodeId(CubeSphereFace.PositiveZ, resolution, resolution / 2);
            Assert.AreEqual(first, second);
        }

        [Test]
        public void MultiSourceDijkstra_IsDeterministicAndZeroAtSources()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(8, 50000f);
            var sources = new List<int>
            {
                graph.GetNodeId(CubeSphereFace.PositiveX, 8, 8),
                graph.GetNodeId(CubeSphereFace.NegativeZ, 0, 4)
            };
            float[] a = SphericalGraphDistance.CalculateToNearestSource(graph, sources);
            float[] b = SphericalGraphDistance.CalculateToNearestSource(graph, sources);
            Assert.AreEqual(graph.NodeCount, a.Length);
            for (int i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i], b[i]);
                Assert.IsFalse(float.IsNaN(a[i]));
                Assert.IsFalse(float.IsInfinity(a[i]));
            }
            Assert.AreEqual(0f, a[sources[0]]);
            Assert.AreEqual(0f, a[sources[1]]);
        }

        [Test]
        public void Benchmark_ReportsGraphAndGreatCircleError()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(4, 50000f);
            var sources = new List<int>
            {
                graph.GetNodeId(CubeSphereFace.PositiveX, 4, 4),
                graph.GetNodeId(CubeSphereFace.NegativeY, 0, 2)
            };
            SphericalDistanceBenchmarkResult result = SphericalDistanceBenchmark.Compare(graph, sources, 64);
            Assert.AreEqual(graph.NodeCount, result.NodeCount);
            Assert.AreEqual(graph.EdgeCount, result.EdgeCount);
            Assert.Greater(result.ComparedNodes, 0);
            Assert.GreaterOrEqual(result.MeanAbsoluteErrorMeters, 0f);
            Assert.GreaterOrEqual(result.MaximumAbsoluteErrorMeters, result.MeanAbsoluteErrorMeters);
            TestContext.WriteLine(
                $"Spherical distance benchmark: nodes={result.NodeCount}, edges={result.EdgeCount}, sources={result.SourceCount}, " +
                $"Dijkstra={result.DijkstraMilliseconds:F3} ms, great-circle reference={result.GreatCircleReferenceMilliseconds:F3} ms, " +
                $"mean error={result.MeanAbsoluteErrorMeters:F3} m, max error={result.MaximumAbsoluteErrorMeters:F3} m, " +
                $"compared nodes={result.ComparedNodes}.");
        }

        [Test]
        [Category("Performance")]
        [Explicit("Heavy 512-resolution benchmark. Run manually on the target PC.")]
        public void Benchmark_Resolution512_BuildsAndRunsDijkstra()
        {
            const int resolution = 512;
            const float radius = 50000f;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(resolution, radius);
            stopwatch.Stop();

            var sources = new List<int>
            {
                graph.GetNodeId(CubeSphereFace.PositiveX, resolution / 2, resolution / 2),
                graph.GetNodeId(CubeSphereFace.NegativeY, resolution, resolution / 2),
                graph.GetNodeId(CubeSphereFace.PositiveZ, 0, resolution / 2)
            };
            var distanceTimer = System.Diagnostics.Stopwatch.StartNew();
            float[] distances = SphericalGraphDistance.CalculateToNearestSource(graph, sources);
            distanceTimer.Stop();

            Assert.AreEqual(6 * resolution * resolution + 2, graph.NodeCount);
            Assert.AreEqual(graph.NodeCount, distances.Length);
            Assert.AreEqual(0f, distances[sources[0]]);
            Assert.AreEqual(0f, distances[sources[1]]);
            Assert.AreEqual(0f, distances[sources[2]]);
            for (int i = 0; i < distances.Length; i++)
                Assert.IsFalse(float.IsNaN(distances[i]) || float.IsInfinity(distances[i]));

            TestContext.WriteLine(
                $"512 benchmark: nodes={graph.NodeCount}, edges={graph.EdgeCount}, " +
                $"graphBuildMs={stopwatch.Elapsed.TotalMilliseconds:F1}, " +
                $"dijkstraMs={distanceTimer.Elapsed.TotalMilliseconds:F1}, " +
                $"estimatedGraphBytes={graph.EstimatedStorageBytes}, " +
                $"estimatedGraphMiB={graph.EstimatedStorageBytes / 1048576.0:F1}");
        }
    }
}
