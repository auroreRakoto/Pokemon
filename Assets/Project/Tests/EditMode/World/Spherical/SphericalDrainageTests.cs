using System;
using NUnit.Framework;
using Pokemon.Project.World.Spherical.Geography;

namespace Pokemon.Project.Tests.EditMode.World.Spherical
{
    public sealed class SphericalDrainageTests
    {
        [Test]
        public void PriorityFlood_FillsClosedDepressionAndRoutesToWater()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(8, 50000f);
            var elevation = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            for (int i = 0; i < elevation.Length; i++) elevation[i] = 100f;
            int outlet = graph.GetNodeId(Pokemon.Project.World.Spherical.CubeSphereFace.PositiveX, 0, 4);
            int basin = graph.GetNodeId(Pokemon.Project.World.Spherical.CubeSphereFace.PositiveX, 4, 4);
            elevation[basin] = 10f;
            water[outlet] = true;

            SphericalDrainageResult result = SphericalDrainage.Build(graph, elevation, water, 3f);
            Assert.AreEqual(100f, result.FilledElevationMeters[basin], 1e-4f);
            Assert.GreaterOrEqual(result.FloodRank[basin], 0);
            Assert.AreEqual(-1, result.DownstreamNodeIds[outlet]);
            Assert.Less(result.FloodRank[result.DownstreamNodeIds[basin]], result.FloodRank[basin]);
        }

        [Test]
        public void PriorityFlood_IsDeterministicAndProducesAcyclicFlow()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(8, 50000f);
            var elevation = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            for (int i = 0; i < elevation.Length; i++)
            {
                var d = graph.GetDirection(i);
                elevation[i] = 100f + 40f * d.y + 8f * d.x * d.z;
                water[i] = d.y < -0.65f;
            }

            SphericalDrainageResult a = SphericalDrainage.Build(graph, elevation, water, 12f);
            SphericalDrainageResult b = SphericalDrainage.Build(graph, elevation, water, 12f);
            Assert.AreEqual(a.DownstreamNodeIds.Length, graph.NodeCount);
            for (int i = 0; i < graph.NodeCount; i++)
            {
                Assert.AreEqual(a.DownstreamNodeIds[i], b.DownstreamNodeIds[i]);
                Assert.AreEqual(a.FlowAccumulation[i], b.FlowAccumulation[i]);
                Assert.IsFalse(float.IsNaN(a.FlowAccumulation[i]) || float.IsInfinity(a.FlowAccumulation[i]));
                int target = a.DownstreamNodeIds[i];
                if (target >= 0 && !a.OutletMask[target])
                    Assert.Less(a.FloodRank[target], a.FloodRank[i], "Every non-outlet flow edge must move toward an earlier flood rank.");
                if (a.OutletMask[i]) Assert.AreEqual(-1, target, "Water cells and fallback outlet are sinks.");
            }
        }

        [Test]
        public void FlowAccumulation_ConservesAllContributingCellsAtOutlets()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(8, 50000f);
            var elevation = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            for (int i = 0; i < elevation.Length; i++)
            {
                elevation[i] = 100f + graph.GetDirection(i).y * 20f;
                water[i] = graph.GetDirection(i).y < -0.7f;
            }
            SphericalDrainageResult result = SphericalDrainage.Build(graph, elevation, water, 10f);
            double outletTotal = 0d;
            for (int i = 0; i < graph.NodeCount; i++)
                if (result.DownstreamNodeIds[i] < 0) outletTotal += result.FlowAccumulation[i];
            Assert.That(outletTotal, Is.EqualTo(graph.NodeCount).Within(0.01d));
        }

        [Test]
        public void InvalidInputsAreRejected()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(4, 50000f);
            Assert.Throws<ArgumentException>(() =>
                SphericalDrainage.Build(graph, new float[1], new bool[1], 5f));
        }

        [Test]
        public void CubeSphereCorners_AreDeduplicatedIntoEightCanonicalNodes()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(8, 50000f);
            var cornerIds = new System.Collections.Generic.HashSet<int>();
            for (int face = 0; face < 6; face++)
            {
                var cubeFace = (Pokemon.Project.World.Spherical.CubeSphereFace)face;
                cornerIds.Add(graph.GetNodeId(cubeFace, 0, 0));
                cornerIds.Add(graph.GetNodeId(cubeFace, 0, 8));
                cornerIds.Add(graph.GetNodeId(cubeFace, 8, 0));
                cornerIds.Add(graph.GetNodeId(cubeFace, 8, 8));
            }
            Assert.AreEqual(8, cornerIds.Count, "The six face grids must share exactly eight canonical cube corners.");
            foreach (int corner in cornerIds)
                Assert.GreaterOrEqual(graph.GetNeighbourCount(corner), 3);
        }

        [Test]
        public void AllSixFaceBoundaries_AreConnectedThroughCanonicalSeamNodes()
        {
            const int resolution = 8;
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(resolution, 50000f);
            for (int face = 0; face < 6; face++)
            {
                var f = (Pokemon.Project.World.Spherical.CubeSphereFace)face;
                for (int i = 0; i <= resolution; i++)
                {
                    int left = graph.GetNodeId(f, 0, i);
                    int right = graph.GetNodeId(f, resolution, i);
                    int bottom = graph.GetNodeId(f, i, 0);
                    int top = graph.GetNodeId(f, i, resolution);
                    Assert.GreaterOrEqual(graph.GetNeighbourCount(left), 3);
                    Assert.GreaterOrEqual(graph.GetNeighbourCount(right), 3);
                    Assert.GreaterOrEqual(graph.GetNeighbourCount(bottom), 3);
                    Assert.GreaterOrEqual(graph.GetNeighbourCount(top), 3);
                }
            }
        }

        [Test]
        public void NorthAndSouthPoles_AreRepresentedByFiniteConnectedNodes()
        {
            const int resolution = 8;
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(resolution, 50000f);
            int north = graph.GetNodeId(Pokemon.Project.World.Spherical.CubeSphereFace.PositiveY, 4, 4);
            int south = graph.GetNodeId(Pokemon.Project.World.Spherical.CubeSphereFace.NegativeY, 4, 4);
            Assert.That(graph.GetDirection(north).y, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(graph.GetDirection(south).y, Is.EqualTo(-1f).Within(1e-5f));
            Assert.Greater(graph.GetNeighbourCount(north), 0);
            Assert.Greater(graph.GetNeighbourCount(south), 0);
        }

        [Test]
        public void DrainageFromNegativeXFace_ReachesOutletOnPositiveXFace()
        {
            const int resolution = 8;
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(resolution, 50000f);
            var elevation = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            for (int i = 0; i < elevation.Length; i++) elevation[i] = 100f;
            int outlet = graph.GetNodeId(Pokemon.Project.World.Spherical.CubeSphereFace.PositiveX, 8, 4);
            int source = graph.GetNodeId(Pokemon.Project.World.Spherical.CubeSphereFace.NegativeX, 0, 4);
            water[outlet] = true;

            SphericalDrainageResult result = SphericalDrainage.Build(graph, elevation, water, 4f);
            int current = source;
            int steps = 0;
            while (current != outlet && steps <= graph.NodeCount)
            {
                current = result.DownstreamNodeIds[current];
                Assert.GreaterOrEqual(current, 0, "Drainage chain terminated before reaching the selected outlet.");
                steps++;
            }
            Assert.AreEqual(outlet, current);
            Assert.Less(steps, graph.NodeCount);
        }

        [Test]
        public void FlatPlateau_DrainsDeterministicallyWithoutCycles()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(8, 50000f);
            var elevation = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            for (int i = 0; i < elevation.Length; i++) elevation[i] = 100f;
            int outlet = graph.GetNodeId(Pokemon.Project.World.Spherical.CubeSphereFace.PositiveZ, 8, 4);
            water[outlet] = true;
            SphericalDrainageResult result = SphericalDrainage.Build(graph, elevation, water, 8f);
            AssertAllChainsTerminateAtOutlet(result, graph.NodeCount);
        }

        [Test]
        public void MultipleOutlets_ProduceDeterministicAcyclicDrainage()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(8, 50000f);
            var elevation = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            for (int i = 0; i < elevation.Length; i++)
                elevation[i] = 100f + 15f * graph.GetDirection(i).y;
            water[graph.GetNodeId(Pokemon.Project.World.Spherical.CubeSphereFace.PositiveX, 8, 4)] = true;
            water[graph.GetNodeId(Pokemon.Project.World.Spherical.CubeSphereFace.NegativeZ, 0, 4)] = true;
            SphericalDrainageResult result = SphericalDrainage.Build(graph, elevation, water, 8f);
            AssertAllChainsTerminateAtOutlet(result, graph.NodeCount);
            for (int i = 0; i < graph.NodeCount; i++)
                if (water[i]) Assert.AreEqual(-1, result.DownstreamNodeIds[i]);
        }

        private static void AssertAllChainsTerminateAtOutlet(SphericalDrainageResult result, int nodeCount)
        {
            for (int start = 0; start < nodeCount; start++)
            {
                int current = start;
                int steps = 0;
                while (result.DownstreamNodeIds[current] >= 0 && steps <= nodeCount)
                {
                    current = result.DownstreamNodeIds[current];
                    steps++;
                }
                Assert.LessOrEqual(steps, nodeCount, $"Flow chain from node {start} contains a cycle.");
                Assert.IsTrue(result.OutletMask[current], $"Flow chain from node {start} did not reach an outlet.");
            }
        }

        [Test]
        [Category("Performance")]
        [Explicit("Run manually on target PC. Three 512-resolution drainage passes.")]
        public void Benchmark_512_DrainageThreeRuns_ReportsStageTimesAndPeakWorkingSet()
        {
            const int resolution = 512;
            const float radiusMeters = 50000f;
            const float riverThreshold = 100f;
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(resolution, radiusMeters);
            var elevation = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            for (int i = 0; i < graph.NodeCount; i++)
            {
                UnityEngine.Vector3 d = graph.GetDirection(i);
                elevation[i] = 1200f + 700f * d.y + 180f * d.x * d.z;
                water[i] = d.y < -0.82f;
            }

            var results = new System.Collections.Generic.List<SphericalDrainageResult>(3);
            var workingSetSampler = new WorkingSetPeakSampler(5);
            workingSetSampler.Start();
            try
            {
                for (int run = 0; run < 3; run++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    results.Add(SphericalDrainage.Build(graph, elevation, water, riverThreshold));
                    TestContext.WriteLine(
                        $"run={run + 1}, priorityFloodMs={results[run].PriorityFloodMilliseconds:F1}, " +
                        $"flowDirectionMs={results[run].FlowDirectionMilliseconds:F1}, " +
                        $"accumulationMs={results[run].AccumulationMilliseconds:F1}");
                }
            }
            finally
            {
                workingSetSampler.Stop();
            }

            TestContext.WriteLine(
                $"512 drainage benchmark: nodes={graph.NodeCount}, runs=3, " +
                $"peakWorkingSetBytes={workingSetSampler.PeakBytes}, " +
                $"peakWorkingSetMiB={workingSetSampler.PeakBytes / 1048576.0:F1}, " +
                $"samplingIntervalMs=5, note=process-wide sampled peak including Unity Editor baseline.");

            Assert.AreEqual(3, results.Count);
            for (int run = 1; run < results.Count; run++)
            {
                Assert.AreEqual(results[0].DownstreamNodeIds.Length, results[run].DownstreamNodeIds.Length);
                for (int i = 0; i < graph.NodeCount; i++)
                    Assert.AreEqual(results[0].DownstreamNodeIds[i], results[run].DownstreamNodeIds[i],
                        $"Non-deterministic downstream result at node {i}.");
            }

            WriteTimingSummary("Priority-Flood", results, r => r.PriorityFloodMilliseconds);
            WriteTimingSummary("Flow direction", results, r => r.FlowDirectionMilliseconds);
            WriteTimingSummary("Accumulation", results, r => r.AccumulationMilliseconds);
        }

        private static void WriteTimingSummary(string label,
            System.Collections.Generic.List<SphericalDrainageResult> results,
            Func<SphericalDrainageResult, double> selector)
        {
            var values = new double[results.Count];
            for (int i = 0; i < results.Count; i++) values[i] = selector(results[i]);
            Array.Sort(values);
            TestContext.WriteLine(
                $"{label}: medianMs={values[values.Length / 2]:F1}, maxMs={values[values.Length - 1]:F1}");
        }

        private sealed class WorkingSetPeakSampler
        {
            private readonly int _intervalMilliseconds;
            private System.Threading.Thread _thread;
            private volatile bool _running;
            private long _peakBytes;

            public long PeakBytes => _peakBytes;

            public WorkingSetPeakSampler(int intervalMilliseconds)
            {
                _intervalMilliseconds = intervalMilliseconds;
            }

            public void Start()
            {
                _peakBytes = 0;
                _running = true;
                _thread = new System.Threading.Thread(Sample)
                {
                    IsBackground = true,
                    Name = "SphericalDrainageMemorySampler"
                };
                _thread.Start();
            }

            public void Stop()
            {
                _running = false;
                if (_thread != null) _thread.Join();
            }

            private void Sample()
            {
                using (var process = System.Diagnostics.Process.GetCurrentProcess())
                {
                    while (_running)
                    {
                        long workingSet = process.WorkingSet64;
                        if (workingSet > _peakBytes) _peakBytes = workingSet;
                        System.Threading.Thread.Sleep(_intervalMilliseconds);
                    }
                    long finalWorkingSet = process.WorkingSet64;
                    if (finalWorkingSet > _peakBytes) _peakBytes = finalWorkingSet;
                }
            }
        }


        [Test]
        public void DryWorldFallbackOutlet_IsExcludedFromRiverMask()
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(4, 50000f);
            var elevation = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            for (int i = 0; i < elevation.Length; i++) elevation[i] = 100f + i;
            SphericalDrainageResult result = SphericalDrainage.Build(graph, elevation, water, 1f);
            int outletCount = 0;
            for (int i = 0; i < graph.NodeCount; i++)
            {
                if (result.OutletMask[i])
                {
                    outletCount++;
                    Assert.IsFalse(result.RiverMask[i]);
                    Assert.AreEqual(-1, result.DownstreamNodeIds[i]);
                }
            }
            Assert.AreEqual(1, outletCount);
        }

    }
}
