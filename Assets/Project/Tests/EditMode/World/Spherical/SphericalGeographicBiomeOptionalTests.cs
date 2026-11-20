using NUnit.Framework;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Geography;
using Pokemon.Project.World.Spherical.Movement;
using Pokemon.Project.World.Spherical.Studio;
using UnityEngine;

namespace Pokemon.Project.Tests.EditMode.World.Spherical
{
    /// <summary>
    /// Verifies World Core generation and surface queries when biome classification is absent.
    /// </summary>
    public sealed class SphericalGeographicBiomeOptionalTests
    {
        /// <summary>
        /// Creates a deterministic low-resolution spherical fixture with non-flat relief.
        /// </summary>
        private static SphericalPlanetStudioSettings CreateSettings()
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            settings.radiusMeters = 50000f;
            settings.faceResolution = 16;
            settings.seed = 481516;
            settings.layer1Enabled = true;
            settings.layer2Enabled = true;
            settings.layer3Enabled = true;
            settings.Validate();
            return settings;
        }

        /// <summary>
        /// Generates World Core fields without allocating biome classification arrays.
        /// </summary>
        [Test]
        public void Generate_WithoutBiomeClassification_ReportsUnavailable()
        {
            SphericalPlanetStudioSettings settings = CreateSettings();
            try
            {
                SphericalGeographicFieldSet fields = SphericalGeographicFieldGenerator.Generate(settings, false);
                Assert.That(fields.HasBiomeClassification, Is.False);
                Assert.That(fields.Resolution, Is.EqualTo(settings.faceResolution));
                Assert.That(fields.ActualOceanCoveragePercent, Is.GreaterThan(0f));
            }
            finally { Object.DestroyImmediate(settings); }
        }

        /// <summary>
        /// Returns false rather than inventing Plains when biome classification was not generated.
        /// </summary>
        [Test]
        public void TrySampleBiome_WithoutClassification_ReturnsFalseAndNone()
        {
            SphericalPlanetStudioSettings settings = CreateSettings();
            try
            {
                SphericalGeographicFieldSet fields = SphericalGeographicFieldGenerator.Generate(settings, false);
                bool sampled = fields.TrySampleBiome(Vector3.up, out SphericalBiome biome);
                Assert.That(sampled, Is.False);
                Assert.That(biome, Is.EqualTo(SphericalBiome.None));
                Assert.That(fields.TryGetBiome(CubeSphereFace.PositiveY, 8, 8, out biome), Is.False);
                Assert.That(biome, Is.EqualTo(SphericalBiome.None));
            }
            finally { Object.DestroyImmediate(settings); }
        }

        /// <summary>
        /// Makes legacy biome access fail explicitly when classification is unavailable.
        /// </summary>
        [Test]
        public void LegacyBiomeAccess_WithoutClassification_ThrowsDiagnostic()
        {
            SphericalPlanetStudioSettings settings = CreateSettings();
            try
            {
                SphericalGeographicFieldSet fields = SphericalGeographicFieldGenerator.Generate(settings, false);
                Assert.Throws<System.InvalidOperationException>(() => fields.SampleBiome(Vector3.up));
                Assert.Throws<System.InvalidOperationException>(() => fields.GetBiome(CubeSphereFace.PositiveY, 8, 8));
            }
            finally { Object.DestroyImmediate(settings); }
        }

        /// <summary>
        /// Preserves every authoritative World Core field for the same settings and seed.
        /// </summary>
        [Test]
        public void Generate_WithAndWithoutBiomes_PreservesWorldCoreFields()
        {
            SphericalPlanetStudioSettings settings = CreateSettings();
            try
            {
                SphericalGeographicFieldSet withBiomes = SphericalGeographicFieldGenerator.Generate(settings, true);
                SphericalGeographicFieldSet withoutBiomes = SphericalGeographicFieldGenerator.Generate(settings, false);
                Assert.That(withoutBiomes.DerivedSeaLevelMeters, Is.EqualTo(withBiomes.DerivedSeaLevelMeters));
                Assert.That(withoutBiomes.ActualOceanCoveragePercent, Is.EqualTo(withBiomes.ActualOceanCoveragePercent));
                SphericalGeographicFieldType[] fields =
                {
                    SphericalGeographicFieldType.AltitudeMeters,
                    SphericalGeographicFieldType.TemperatureNormalized,
                    SphericalGeographicFieldType.SlopeDegrees,
                    SphericalGeographicFieldType.DistanceToWaterMeters,
                    SphericalGeographicFieldType.MoistureNormalized
                };
                for (int face = 0; face < 6; face++)
                for (int y = 0; y <= withBiomes.Resolution; y++)
                for (int x = 0; x <= withBiomes.Resolution; x++)
                {
                    CubeSphereFace cubeFace = (CubeSphereFace)face;
                    Assert.That(withoutBiomes.GetWaterClass(cubeFace, x, y),
                        Is.EqualTo(withBiomes.GetWaterClass(cubeFace, x, y)));
                    for (int field = 0; field < fields.Length; field++)
                        Assert.That(withoutBiomes.GetValue(fields[field], cubeFace, x, y),
                            Is.EqualTo(withBiomes.GetValue(fields[field], cubeFace, x, y)));
                }
            }
            finally { Object.DestroyImmediate(settings); }
        }

        /// <summary>
        /// Keeps the legacy generation overload biome-enabled for existing callers and profiles.
        /// </summary>
        [Test]
        public void Generate_LegacyOverload_StillIncludesBiomeClassification()
        {
            SphericalPlanetStudioSettings settings = CreateSettings();
            try
            {
                SphericalGeographicFieldSet fields = SphericalGeographicFieldGenerator.Generate(settings);
                Assert.That(fields.HasBiomeClassification, Is.True);
                bool foundLandBiome = false;
                for (int face = 0; face < 6 && !foundLandBiome; face++)
                for (int y = 0; y <= fields.Resolution && !foundLandBiome; y++)
                for (int x = 0; x <= fields.Resolution; x++)
                {
                    CubeSphereFace cubeFace = (CubeSphereFace)face;
                    if (fields.GetWaterClass(cubeFace, x, y) != SphericalWaterClass.Land) continue;
                    Assert.That(fields.TryGetBiome(cubeFace, x, y, out SphericalBiome biome), Is.True);
                    Assert.That(biome, Is.Not.EqualTo(SphericalBiome.None));
                    foundLandBiome = true;
                    break;
                }
                Assert.That(foundLandBiome, Is.True);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        /// <summary>
        /// Allows the ground probe to return altitude, water and normal data without biome classification.
        /// </summary>
        [Test]
        public void GroundProbe_WithoutBiomeClassification_RemainsAvailable()
        {
            SphericalPlanetStudioSettings settings = CreateSettings();
            try
            {
                SphericalGeographicFieldSet fields = SphericalGeographicFieldGenerator.Generate(settings, false);
                bool sampled = SphericalGroundProbe.TrySample(fields, Vector3.zero,
                    settings.radiusMeters, Vector3.up, out SphericalGroundProbe.Sample sample);
                Assert.That(sampled, Is.True);
                Assert.That(sample.HasBiomeClassification, Is.False);
                Assert.That(sample.Biome, Is.EqualTo(SphericalBiome.None));
                Assert.That(sample.SurfacePosition.magnitude, Is.GreaterThan(0f));
            }
            finally { Object.DestroyImmediate(settings); }
        }
    }
}
