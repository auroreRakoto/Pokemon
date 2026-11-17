using Pokemon.Project.World.Spherical;
using NUnit.Framework;
using Pokemon.Project.World.Spherical.Studio;
using UnityEngine;

namespace Pokemon.Project.Tests.World.Spherical
{
    public sealed class SphericalPlanetStudioTests
    {
        [Test]
        public void DisabledLayersProducePerfectBaseSphere()
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            settings.radiusMeters = 1000f;
            settings.layer1Enabled = false;
            settings.layer2Enabled = false;
            settings.layer3Enabled = false;
            try
            {
                Mesh[] meshes = SphericalPlanetStudioGenerator.GenerateGlobeFaces(settings);
                try
                {
                    foreach (Mesh mesh in meshes)
                        foreach (Vector3 vertex in mesh.vertices)
                            Assert.That(vertex.magnitude, Is.EqualTo(settings.radiusMeters).Within(0.02f));
                }
                finally { foreach (Mesh mesh in meshes) Object.DestroyImmediate(mesh); }
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void LayerContributionsAreDeterministicAndIndependent()
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            settings.radiusMeters = 1000f;
            settings.seed = 12345;
            settings.layer1Enabled = true;
            settings.layer2Enabled = false;
            settings.layer3Enabled = false;
            Vector3 direction = new Vector3(0.3f, 0.7f, -0.2f).normalized;
            try
            {
                float layer1Only = SphericalPlanetStudioGenerator.SampleTotalHeight(direction, settings);
                settings.layer2Enabled = true;
                float layer1And2 = SphericalPlanetStudioGenerator.SampleTotalHeight(direction, settings);
                settings.layer2Enabled = false;
                float layer1Again = SphericalPlanetStudioGenerator.SampleTotalHeight(direction, settings);
                Assert.That(layer1Again, Is.EqualTo(layer1Only).Within(1e-5f));
                Assert.That(layer1And2, Is.Not.EqualTo(layer1Only));
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void PlanisphereAndGlobeUseSameDirectionBasedHeightField()
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            settings.layer1Enabled = true;
            settings.faceResolution = 12;
            settings.planisphereWidth = 32;
            settings.planisphereHeight = 16;
            try
            {
                Vector3 direction = CubeSphereTopology.GetDirection(CubeSphereFace.PositiveZ, 0.25f, -0.4f);
                Vector2 mapCoordinates = SphericalPlanetStudioGenerator.DirectionToPlanisphereCoordinates(direction);
                Vector3 roundTripDirection = SphericalPlanetStudioGenerator.PlanisphereCoordinatesToDirection(mapCoordinates.x, mapCoordinates.y);
                Assert.That(Vector3.Distance(direction, roundTripDirection), Is.LessThan(1e-5f));
                float globeValue = SphericalPlanetStudioGenerator.SampleTotalHeight(direction, settings);
                float planisphereEquivalentValue = SphericalPlanetStudioGenerator.SampleTotalHeight(roundTripDirection, settings);
                Assert.That(planisphereEquivalentValue, Is.EqualTo(globeValue).Within(1e-4f));
                Mesh planisphere = SphericalPlanetStudioGenerator.GeneratePlanisphere(settings);
                Assert.That(planisphere.vertexCount, Is.EqualTo((settings.planisphereWidth + 1) * (settings.planisphereHeight + 1)));
                Object.DestroyImmediate(planisphere);
            }
            finally { Object.DestroyImmediate(settings); }
        }



        [Test]
        public void PreviewStageCanReturnToEarlierLayerWithoutLosingLaterLayerSettings()
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            settings.layer1Enabled = true;
            settings.layer2Enabled = true;
            settings.layer3Enabled = true;
            Vector3 direction = new Vector3(0.23f, -0.64f, 0.51f).normalized;
            try
            {
                float layer1Stage = SphericalPlanetStudioGenerator.SamplePreviewHeight(direction, settings, SphericalPlanetStudioPreviewStage.Layer1);
                float finalStage = SphericalPlanetStudioGenerator.SamplePreviewHeight(direction, settings, SphericalPlanetStudioPreviewStage.Final);
                float layer1Again = SphericalPlanetStudioGenerator.SamplePreviewHeight(direction, settings, SphericalPlanetStudioPreviewStage.Layer1);
                Assert.That(layer1Again, Is.EqualTo(layer1Stage).Within(1e-5f));
                Assert.That(finalStage, Is.Not.EqualTo(layer1Stage));
                Assert.IsTrue(settings.layer2Enabled);
                Assert.IsTrue(settings.layer3Enabled);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void ChangingThematicMapDoesNotChangeTerrainGeometry()
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            settings.faceResolution = 8;
            settings.layer1Enabled = true;
            try
            {
                settings.map = SphericalPlanetStudioMap.Terrain;
                Mesh terrain = SphericalPlanetStudioGenerator.GenerateCubeFace(CubeSphereFace.PositiveX, settings);
                settings.map = SphericalPlanetStudioMap.Temperature;
                Mesh temperature = SphericalPlanetStudioGenerator.GenerateCubeFace(CubeSphereFace.PositiveX, settings);
                try
                {
                    CollectionAssert.AreEqual(terrain.vertices, temperature.vertices);
                    Assert.That(terrain.colors, Is.Not.EqualTo(temperature.colors));
                }
                finally
                {
                    Object.DestroyImmediate(terrain);
                    Object.DestroyImmediate(temperature);
                }
            }
            finally { Object.DestroyImmediate(settings); }
        }


        [Test]
        public void PlanisphereWrapBoundaryMapsToSameSphericalDirection()
        {
            Vector3 left = SphericalPlanetStudioGenerator.PlanisphereCoordinatesToDirection(0f, 0.37f);
            Vector3 right = SphericalPlanetStudioGenerator.PlanisphereCoordinatesToDirection(1f, 0.37f);
            Assert.That(Vector3.Distance(left, right), Is.LessThan(1e-5f));
        }

        [Test]
        public void TemperatureMapIsBounded()
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                float value = SphericalPlanetStudioGenerator.SampleTemperature(new Vector3(0.1f, 0.8f, 0.2f), settings);
                Assert.That(value, Is.InRange(0f, 1f));
            }
            finally { Object.DestroyImmediate(settings); }
        }
    }
}
