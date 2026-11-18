using NUnit.Framework;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Geography;
using Pokemon.Project.World.Spherical.Studio;
using UnityEngine;

namespace Pokemon.Project.Tests.EditMode.World.Spherical
{
    public sealed class SphericalGeographicFieldGeneratorTests
    {
        private static SphericalPlanetStudioSettings CreateSettings()
        {
            var settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            settings.radiusMeters = 50000f;
            settings.faceResolution = 16;
            settings.seed = 481516;
            settings.layer1Enabled = true;
            settings.layer2Enabled = true;
            settings.layer3Enabled = true;
            settings.Validate();
            return settings;
        }

        [Test]
        public void Generate_ProducesExpectedResolutionAndMetadata()
        {
            var settings = CreateSettings();
            try
            {
                SphericalGeographicFieldSet fields = SphericalGeographicFieldGenerator.Generate(settings);
                Assert.AreEqual(16, fields.Resolution);
                Assert.AreEqual(settings.seed, fields.Seed);
                Assert.AreEqual(settings.radiusMeters, fields.PlanetRadiusMeters);
                Assert.AreEqual(6 * 17 * 17, fields.SamplesPerFace * 6);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void Generate_IsDeterministicForSameSettings()
        {
            var settings = CreateSettings();
            try
            {
                var a = SphericalGeographicFieldGenerator.Generate(settings);
                var b = SphericalGeographicFieldGenerator.Generate(settings);
                for (int face = 0; face < 6; face++)
                for (int y = 0; y <= a.Resolution; y++)
                for (int x = 0; x <= a.Resolution; x++)
                {
                    var f = (CubeSphereFace)face;
                    Assert.AreEqual(a.GetValue(SphericalGeographicFieldType.AltitudeMeters, f, x, y),
                        b.GetValue(SphericalGeographicFieldType.AltitudeMeters, f, x, y));
                    Assert.AreEqual(a.GetValue(SphericalGeographicFieldType.TemperatureNormalized, f, x, y),
                        b.GetValue(SphericalGeographicFieldType.TemperatureNormalized, f, x, y));
                    Assert.AreEqual(a.GetValue(SphericalGeographicFieldType.SlopeDegrees, f, x, y),
                        b.GetValue(SphericalGeographicFieldType.SlopeDegrees, f, x, y));
                }
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void Generate_FieldsStayWithinDeclaredRanges()
        {
            var settings = CreateSettings();
            try
            {
                var fields = SphericalGeographicFieldGenerator.Generate(settings);
                for (int face = 0; face < 6; face++)
                for (int y = 0; y <= fields.Resolution; y++)
                for (int x = 0; x <= fields.Resolution; x++)
                {
                    var f = (CubeSphereFace)face;
                    float temperature = fields.GetValue(SphericalGeographicFieldType.TemperatureNormalized, f, x, y);
                    float slope = fields.GetValue(SphericalGeographicFieldType.SlopeDegrees, f, x, y);
                    Assert.That(temperature, Is.InRange(0f, 1f));
                    Assert.That(slope, Is.InRange(0f, 90f));
                }
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void SharedFaceEdgeSamples_HaveMatchingAltitudeAndTemperature()
        {
            var settings = CreateSettings();
            try
            {
                var fields = SphericalGeographicFieldGenerator.Generate(settings);
                // +X face right edge and +Z face right/left edge share directions according
                // to CubeSphereTopology's basis. Compare by direction first, then field sample.
                Vector3 directionA = fields.GetDirection(CubeSphereFace.PositiveX, fields.Resolution, fields.Resolution / 2);
                CubeSphereTopology.DirectionToFaceCoordinates(directionA, out CubeSphereFace faceB, out Vector2 uvB);
                int xB = Mathf.RoundToInt((uvB.x + 1f) * 0.5f * fields.Resolution);
                int yB = Mathf.RoundToInt((uvB.y + 1f) * 0.5f * fields.Resolution);
                Assert.That(Vector3.Distance(directionA, fields.GetDirection(faceB, xB, yB)), Is.LessThan(0.0001f));
                Assert.That(Mathf.Abs(fields.GetValue(SphericalGeographicFieldType.AltitudeMeters,
                    CubeSphereFace.PositiveX, fields.Resolution, fields.Resolution / 2) -
                    fields.GetValue(SphericalGeographicFieldType.AltitudeMeters, faceB, xB, yB)), Is.LessThan(0.001f));
                Assert.That(Mathf.Abs(fields.GetValue(SphericalGeographicFieldType.TemperatureNormalized,
                    CubeSphereFace.PositiveX, fields.Resolution, fields.Resolution / 2) -
                    fields.GetValue(SphericalGeographicFieldType.TemperatureNormalized, faceB, xB, yB)), Is.LessThan(0.0001f));
                Assert.AreEqual(fields.GetWaterClass(CubeSphereFace.PositiveX, fields.Resolution, fields.Resolution / 2),
                    fields.GetWaterClass(faceB, xB, yB));
                Assert.AreEqual(fields.GetBiome(CubeSphereFace.PositiveX, fields.Resolution, fields.Resolution / 2),
                    fields.GetBiome(faceB, xB, yB));
                Assert.That(Mathf.Abs(fields.GetValue(SphericalGeographicFieldType.MoistureNormalized,
                    CubeSphereFace.PositiveX, fields.Resolution, fields.Resolution / 2) -
                    fields.GetValue(SphericalGeographicFieldType.MoistureNormalized, faceB, xB, yB)), Is.LessThan(0.0001f));
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void Generate_OceanCoverageMatchesAreaWeightedTargetWithinCellQuantization()
        {
            var settings = CreateSettings();
            try
            {
                settings.oceanCoverageTargetPercent = 65f;
                var fields = SphericalGeographicFieldGenerator.Generate(settings);
                Assert.That(Mathf.Abs(fields.ActualOceanCoveragePercent - 65f),
                    Is.LessThanOrEqualTo(fields.OceanCoverageQuantizationTolerancePercent + 0.001f));
                Assert.AreEqual(65f, fields.OceanCoverageTargetPercent);
                Assert.IsFalse(float.IsNaN(fields.DerivedSeaLevelMeters));
                Assert.IsFalse(float.IsInfinity(fields.DerivedSeaLevelMeters));
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void Generate_WaterAndBiomeClassesAreMutuallyExclusiveAndComplete()
        {
            var settings = CreateSettings();
            try
            {
                var fields = SphericalGeographicFieldGenerator.Generate(settings);
                int oceanCount = 0;
                int landCount = 0;
                for (int face = 0; face < 6; face++)
                for (int y = 0; y <= fields.Resolution; y++)
                for (int x = 0; x <= fields.Resolution; x++)
                {
                    CubeSphereFace f = (CubeSphereFace)face;
                    SphericalWaterClass water = fields.GetWaterClass(f, x, y);
                    SphericalBiome biome = fields.GetBiome(f, x, y);
                    if (water == SphericalWaterClass.Ocean)
                    {
                        oceanCount++;
                        Assert.AreEqual(SphericalBiome.None, biome);
                    }
                    else
                    {
                        landCount++;
                        Assert.Contains(biome, new[]
                        {
                            SphericalBiome.Plains,
                            SphericalBiome.Forest,
                            SphericalBiome.Desert,
                            SphericalBiome.Mountains
                        });
                    }
                    Assert.That(fields.GetValue(SphericalGeographicFieldType.MoistureNormalized, f, x, y), Is.InRange(0f, 1f));
                }
                Assert.Greater(oceanCount, 0);
                Assert.Greater(landCount, 0);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void Generate_OceanAndBiomeFieldsAreDeterministic()
        {
            var settings = CreateSettings();
            try
            {
                var a = SphericalGeographicFieldGenerator.Generate(settings);
                var b = SphericalGeographicFieldGenerator.Generate(settings);
                Assert.AreEqual(a.DerivedSeaLevelMeters, b.DerivedSeaLevelMeters);
                Assert.AreEqual(a.ActualOceanCoveragePercent, b.ActualOceanCoveragePercent);
                for (int face = 0; face < 6; face++)
                for (int y = 0; y <= a.Resolution; y++)
                for (int x = 0; x <= a.Resolution; x++)
                {
                    CubeSphereFace f = (CubeSphereFace)face;
                    Assert.AreEqual(a.GetWaterClass(f, x, y), b.GetWaterClass(f, x, y));
                    Assert.AreEqual(a.GetBiome(f, x, y), b.GetBiome(f, x, y));
                    Assert.AreEqual(a.GetValue(SphericalGeographicFieldType.MoistureNormalized, f, x, y),
                        b.GetValue(SphericalGeographicFieldType.MoistureNormalized, f, x, y));
                }
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void Generate_HigherOceanTargetProducesHigherOrEqualSeaLevel()
        {
            var settings = CreateSettings();
            try
            {
                settings.oceanCoverageTargetPercent = 60f;
                var lowerTarget = SphericalGeographicFieldGenerator.Generate(settings);
                settings.oceanCoverageTargetPercent = 70f;
                var higherTarget = SphericalGeographicFieldGenerator.Generate(settings);
                Assert.That(higherTarget.DerivedSeaLevelMeters, Is.GreaterThanOrEqualTo(lowerTarget.DerivedSeaLevelMeters));
                Assert.That(higherTarget.ActualOceanCoveragePercent,
                    Is.GreaterThanOrEqualTo(lowerTarget.ActualOceanCoveragePercent));
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void Generate_DistanceToOceanIsZeroOnOceanAndPositiveOnLand()
        {
            var settings = CreateSettings();
            try
            {
                var fields = SphericalGeographicFieldGenerator.Generate(settings);
                bool foundLand = false;
                for (int face = 0; face < 6; face++)
                for (int y = 0; y <= fields.Resolution; y++)
                for (int x = 0; x <= fields.Resolution; x++)
                {
                    CubeSphereFace f = (CubeSphereFace)face;
                    float distance = fields.GetValue(SphericalGeographicFieldType.DistanceToWaterMeters, f, x, y);
                    Assert.That(distance, Is.GreaterThanOrEqualTo(0f));
                    if (fields.GetWaterClass(f, x, y) == SphericalWaterClass.Ocean)
                        Assert.AreEqual(0f, distance);
                    else if (distance > 0f)
                        foundLand = true;
                }
                Assert.IsTrue(foundLand);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void Generate_GeodesicDistanceApproximatesNearestOceanGreatCircle()
        {
            var settings = CreateSettings();
            try
            {
                var fields = SphericalGeographicFieldGenerator.Generate(settings);
                var graph = SphericalNeighbourGraph.Build(fields.Resolution, fields.PlanetRadiusMeters);
                var sourceDirections = new System.Collections.Generic.List<Vector3>();
                var sourceMask = new bool[graph.NodeCount];
                for (int face = 0; face < 6; face++)
                for (int y = 0; y <= fields.Resolution; y++)
                for (int x = 0; x <= fields.Resolution; x++)
                {
                    CubeSphereFace f = (CubeSphereFace)face;
                    int node = graph.GetNodeId(f, x, y);
                    if (sourceMask[node]) continue;
                    if (fields.GetWaterClass(f, x, y) == SphericalWaterClass.Ocean)
                    {
                        sourceMask[node] = true;
                        sourceDirections.Add(graph.GetDirection(node));
                    }
                }

                int checkedLandNodes = 0;
                for (int node = 0; node < graph.NodeCount && checkedLandNodes < 24; node += 7)
                {
                    if (sourceMask[node]) continue;
                    Vector3 direction = graph.GetDirection(node);
                    float direct = float.PositiveInfinity;
                    for (int source = 0; source < sourceDirections.Count; source++)
                    {
                        float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(direction, sourceDirections[source]), -1f, 1f));
                        direct = Mathf.Min(direct, angle * fields.PlanetRadiusMeters);
                    }
                    float graphDistance = FindDistanceAtNode(fields, graph, node);
                    float edgeAllowance = fields.PlanetRadiusMeters * 2f / fields.Resolution;
                    Assert.That(graphDistance, Is.GreaterThanOrEqualTo(direct - 1f));
                    Assert.That(graphDistance, Is.LessThanOrEqualTo(direct * 1.5f + edgeAllowance));
                    checkedLandNodes++;
                }
                Assert.AreEqual(24, checkedLandNodes);
            }
            finally { Object.DestroyImmediate(settings); }
        }


        [TestCase(5000f)]
        [TestCase(50000f)]
        [TestCase(1000000f)]
        public void Generate_UsesConfiguredPlanetRadiusAcrossWorldSizes(float radiusMeters)
        {
            var settings = CreateSettings();
            try
            {
                settings.radiusMeters = radiusMeters;
                var fields = SphericalGeographicFieldGenerator.Generate(settings);
                Assert.AreEqual(radiusMeters, fields.PlanetRadiusMeters);
                Assert.IsFalse(float.IsNaN(fields.DerivedSeaLevelMeters));
                Assert.IsFalse(float.IsInfinity(fields.DerivedSeaLevelMeters));
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [TestCase(1f)]
        [TestCase(35f)]
        [TestCase(65f)]
        [TestCase(99f)]
        public void Generate_OceanCoverageSupportsLowDefaultAndHighTargets(float targetPercent)
        {
            var settings = CreateSettings();
            try
            {
                settings.oceanCoverageTargetPercent = targetPercent;
                var fields = SphericalGeographicFieldGenerator.Generate(settings);
                Assert.AreEqual(targetPercent, fields.OceanCoverageTargetPercent);
                Assert.That(Mathf.Abs(fields.ActualOceanCoveragePercent - targetPercent),
                    Is.LessThanOrEqualTo(fields.OceanCoverageQuantizationTolerancePercent + 0.001f));
                Assert.IsFalse(float.IsNaN(fields.DerivedSeaLevelMeters));
                Assert.IsFalse(float.IsInfinity(fields.DerivedSeaLevelMeters));
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void Generate_ChangingPlanetRadiusDoesNotRescaleTerrainReliefSettings()
        {
            var settings = CreateSettings();
            try
            {
                settings.layer1AmplitudeMeters = 1800f;
                settings.layer2AmplitudeMeters = 450f;
                settings.layer3AmplitudeMeters = 65f;
                Vector3 direction = new Vector3(0.31f, 0.72f, -0.44f).normalized;
                settings.radiusMeters = 5000f;
                float smallWorldHeight = SphericalPlanetStudioGenerator.SamplePreviewHeight(
                    direction, settings, SphericalPlanetStudioPreviewStage.Final);
                settings.radiusMeters = 50000f;
                float largeWorldHeight = SphericalPlanetStudioGenerator.SamplePreviewHeight(
                    direction, settings, SphericalPlanetStudioPreviewStage.Final);
                Assert.AreEqual(smallWorldHeight, largeWorldHeight);
                Assert.AreEqual(1800f, settings.layer1AmplitudeMeters);
                Assert.AreEqual(450f, settings.layer2AmplitudeMeters);
                Assert.AreEqual(65f, settings.layer3AmplitudeMeters);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        private static float FindDistanceAtNode(SphericalGeographicFieldSet fields,
            SphericalNeighbourGraph graph, int targetNode)
        {
            for (int face = 0; face < 6; face++)
            for (int y = 0; y <= fields.Resolution; y++)
            for (int x = 0; x <= fields.Resolution; x++)
            {
                CubeSphereFace f = (CubeSphereFace)face;
                if (graph.GetNodeId(f, x, y) == targetNode)
                    return fields.GetValue(SphericalGeographicFieldType.DistanceToWaterMeters, f, x, y);
            }
            Assert.Fail("Canonical graph node was not represented on a face lattice.");
            return -1f;
        }

        [Test]
        public void SettingsMigration_RestoresApprovedOceanTargetForPreWorld002Assets()
        {
            var settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                settings.geographicSettingsVersion = 0;
                settings.oceanCoverageTargetPercent = 0f;
                settings.Validate();
                Assert.AreEqual(1, settings.geographicSettingsVersion);
                Assert.AreEqual(65f, settings.oceanCoverageTargetPercent);
            }
            finally { Object.DestroyImmediate(settings); }
        }
    }
}
