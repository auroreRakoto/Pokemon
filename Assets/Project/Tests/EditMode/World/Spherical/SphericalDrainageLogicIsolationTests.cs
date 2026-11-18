using System;
using NUnit.Framework;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Geography;

namespace Pokemon.Project.Tests.EditMode.World.Spherical
{
    /// <summary>
    /// Tests drainage numerics without creating Unity GameObjects, meshes, materials or renderers.
    /// </summary>
    public sealed class SphericalDrainageLogicIsolationTests
    {
        [Test]
        public void MonotonicRadialSlope_AllCellsReachTheKnownPolarOutlet_AndConserveFlow()
        {
            const int resolution = 16;
            const float radius = 50000f;
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(resolution, radius);
            int outlet = graph.GetNodeId(CubeSphereFace.PositiveY, resolution / 2, resolution / 2);
            Vector3NorthPoleAssert(graph.GetDirection(outlet));

            var elevations = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            for (int node = 0; node < graph.NodeCount; node++)
            {
                float dot = graph.GetDirection(node).y;
                elevations[node] = (1f - dot) * 1000f;
            }
            water[outlet] = true;

            SphericalDrainageResult result = SphericalDrainage.Build(graph, elevations, water, 20f);

            Assert.AreEqual(1, result.InitialWaterNodeCount);
            Assert.AreEqual(1, CountTrue(result.OutletMask));
            Assert.AreEqual(-1, result.DownstreamNodeIds[outlet]);
            AssertAllPathsReachOutlet(result, outlet);
            AssertOutletMassConserved(result, graph.NodeCount);
            AssertRiverMaskMatchesThreshold(result);
        }

        [Test]
        public void FlatSingleOutlet_IsAValidAlgorithmStressCaseButNotARealisticRiverModel()
        {
            const int resolution = 16;
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(resolution, 50000f);
            int outlet = graph.GetNodeId(CubeSphereFace.PositiveY, resolution / 2, resolution / 2);
            var elevations = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            water[outlet] = true;

            SphericalDrainageResult result = SphericalDrainage.Build(graph, elevations, water, 100f);

            Assert.AreEqual(1, CountTrue(result.OutletMask));
            AssertAllPathsReachOutlet(result, outlet);
            AssertOutletMassConserved(result, graph.NodeCount);
            AssertRiverMaskMatchesThreshold(result);
        }

        [Test]
        public void RaisingThresholdNeverAddsRiverCandidates()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(16, 50000f);
            var elevations = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            for (int i = 0; i < graph.NodeCount; i++)
            {
                UnityEngine.Vector3 direction = graph.GetDirection(i);
                elevations[i] = (1f - direction.y) * 1000f;
            }
            int outlet = graph.GetNodeId(CubeSphereFace.PositiveY, 8, 8);
            water[outlet] = true;

            SphericalDrainageResult low = SphericalDrainage.Build(graph, elevations, water, 10f);
            SphericalDrainageResult high = SphericalDrainage.Build(graph, elevations, water, 100f);

            for (int i = 0; i < graph.NodeCount; i++)
                if (high.RiverMask[i]) Assert.IsTrue(low.RiverMask[i], $"Threshold monotonicity failed at node {i}.");
        }

        private static void Vector3NorthPoleAssert(UnityEngine.Vector3 direction)
        {
            Assert.That(direction.x, Is.EqualTo(0f).Within(1e-5f));
            Assert.That(direction.y, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(direction.z, Is.EqualTo(0f).Within(1e-5f));
        }

        private static int CountTrue(bool[] values)
        {
            int count = 0;
            for (int i = 0; i < values.Length; i++) if (values[i]) count++;
            return count;
        }

        private static void AssertAllPathsReachOutlet(SphericalDrainageResult result, int expectedOutlet)
        {
            for (int start = 0; start < result.DownstreamNodeIds.Length; start++)
            {
                int current = start;
                int steps = 0;
                while (result.DownstreamNodeIds[current] >= 0 && steps <= result.DownstreamNodeIds.Length)
                {
                    current = result.DownstreamNodeIds[current];
                    steps++;
                }
                Assert.LessOrEqual(steps, result.DownstreamNodeIds.Length, $"Cycle from node {start}.");
                Assert.AreEqual(expectedOutlet, current, $"Node {start} terminates at the wrong outlet.");
            }
        }

        private static void AssertOutletMassConserved(SphericalDrainageResult result, int nodeCount)
        {
            double total = 0d;
            for (int i = 0; i < result.DownstreamNodeIds.Length; i++)
                if (result.DownstreamNodeIds[i] < 0) total += result.FlowAccumulation[i];
            Assert.That(total, Is.EqualTo(nodeCount).Within(0.01d));
        }

        private static void AssertRiverMaskMatchesThreshold(SphericalDrainageResult result)
        {
            for (int i = 0; i < result.RiverMask.Length; i++)
            {
                bool expected = !result.OutletMask[i] &&
                    result.FlowAccumulation[i] >= result.RiverAccumulationThreshold;
                Assert.AreEqual(expected, result.RiverMask[i], $"River mask mismatch at node {i}.");
            }
        }
    }
}
