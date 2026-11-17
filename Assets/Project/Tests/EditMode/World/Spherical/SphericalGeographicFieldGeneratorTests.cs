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
        public void DirectionSampling_ReturnsFiniteValuesForAllGeneratedFields()
        {
            var settings = CreateSettings();
            try
            {
                var fields = SphericalGeographicFieldGenerator.Generate(settings);
                Vector3[] directions =
                {
                    Vector3.right, Vector3.left, Vector3.up, Vector3.down,
                    Vector3.forward, Vector3.back, new Vector3(1f, 1f, 1f).normalized
                };
                foreach (Vector3 direction in directions)
                {
                    Assert.IsFalse(float.IsNaN(fields.Sample(SphericalGeographicFieldType.AltitudeMeters, direction)));
                    Assert.IsFalse(float.IsNaN(fields.Sample(SphericalGeographicFieldType.TemperatureNormalized, direction)));
                    Assert.IsFalse(float.IsNaN(fields.Sample(SphericalGeographicFieldType.SlopeDegrees, direction)));
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
            }
            finally { Object.DestroyImmediate(settings); }
        }
    }
}
