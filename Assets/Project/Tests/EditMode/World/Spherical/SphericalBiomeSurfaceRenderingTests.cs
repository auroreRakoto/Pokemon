using NUnit.Framework;
using Pokemon.Project.World.Spherical.Geography;
using Pokemon.Project.World.Spherical.Presentation;
using Pokemon.Project.World.Spherical.Studio;
using UnityEngine;

namespace Pokemon.Project.Tests.EditMode.World.Spherical
{
    public sealed class SphericalBiomeSurfaceRenderingTests
    {
        private static SphericalPlanetStudioSettings CreateSettings()
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            settings.radiusMeters = 50000f;
            settings.faceResolution = 12;
            settings.seed = 481516;
            settings.layer1Enabled = true;
            settings.layer2Enabled = true;
            settings.layer3Enabled = true;
            settings.Validate();
            return settings;
        }

        [Test]
        public void GenerateAllFaces_ProducesNormalizedBiomeWeightsAndOceanMask()
        {
            SphericalPlanetStudioSettings settings = CreateSettings();
            Mesh[] meshes = null;
            try
            {
                SphericalGeographicFieldSet fields = SphericalGeographicFieldGenerator.Generate(settings);
                meshes = SphericalBiomeSurfaceMeshGenerator.GenerateAllFaces(settings, fields, 2f);
                Assert.AreEqual(6, meshes.Length);

                foreach (Mesh mesh in meshes)
                {
                    Assert.IsNotNull(mesh);
                    Assert.AreEqual(13 * 13, mesh.vertexCount);
                    Assert.AreEqual(mesh.vertexCount, mesh.colors.Length);
                    Assert.AreEqual(mesh.vertexCount, mesh.uv2.Length);
                    Color[] weights = mesh.colors;
                    Vector2[] water = mesh.uv2;
                    for (int i = 0; i < mesh.vertexCount; i++)
                    {
                        float total = weights[i].r + weights[i].g + weights[i].b + weights[i].a;
                        Assert.That(Mathf.Abs(total) < 1e-5f || Mathf.Abs(total - 1f) < 1e-5f, Is.True);
                        Assert.That(water[i].x, Is.InRange(0f, 1f));
                        Assert.That(float.IsNaN(total), Is.False);
                    }
                }
            }
            finally
            {
                if (meshes != null)
                    foreach (Mesh mesh in meshes)
                        if (mesh != null) Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void CalculateBiomeWeights_IsDeterministicAndUsesNormalizedLandWeights()
        {
            SphericalPlanetStudioSettings settings = CreateSettings();
            try
            {
                SphericalGeographicFieldSet fields = SphericalGeographicFieldGenerator.Generate(settings);
                Vector3 direction = new Vector3(0.31f, 0.72f, -0.61f).normalized;
                Color first = SphericalBiomeSurfaceMeshGenerator.CalculateBiomeWeights(fields, direction, 2f, out float firstWater);
                Color second = SphericalBiomeSurfaceMeshGenerator.CalculateBiomeWeights(fields, direction, 2f, out float secondWater);
                Assert.That(first.r, Is.EqualTo(second.r).Within(1e-6f));
                Assert.That(first.g, Is.EqualTo(second.g).Within(1e-6f));
                Assert.That(first.b, Is.EqualTo(second.b).Within(1e-6f));
                Assert.That(first.a, Is.EqualTo(second.a).Within(1e-6f));
                Assert.That(firstWater, Is.EqualTo(secondWater).Within(1e-6f));
                Assert.That(firstWater, Is.InRange(0f, 1f));
                float landTotal = first.r + first.g + first.b + first.a;
                Assert.That(Mathf.Abs(landTotal) < 1e-5f || Mathf.Abs(landTotal - 1f) < 1e-5f, Is.True);
            }
            finally { Object.DestroyImmediate(settings); }
        }
    }
}
