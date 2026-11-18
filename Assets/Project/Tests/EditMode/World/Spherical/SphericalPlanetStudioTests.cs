using NUnit.Framework;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Studio;
using UnityEngine;

namespace Pokemon.Project.Tests.World.Spherical
{
    public sealed class SphericalPlanetStudioTests
    {
        [TestCase(1000f)]
        [TestCase(5000f)]
        [TestCase(50000f)]
        public void DisabledLayersProducePerfectBaseSphereAtConfiguredRadius(float radiusMeters)
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            settings.radiusMeters = radiusMeters;
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
                            Assert.That(vertex.magnitude, Is.EqualTo(radiusMeters).Within(0.02f));
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
                // The round-trip direction is already constrained to 1e-5 unit-vector distance.
                // Height sampling uses float math, so allow 1e-3 metres for accumulated
                // projection/sampling round-off (observed discrepancy: about 4.9e-4 m).
                Assert.That(planisphereEquivalentValue, Is.EqualTo(globeValue).Within(1e-3f));
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


        [Test]
        public void CubeSphereFaceDirectionsRoundTripToTheirOriginalFaceCoordinates()
        {
            CubeSphereFace[] faces =
            {
                CubeSphereFace.PositiveX, CubeSphereFace.NegativeX,
                CubeSphereFace.PositiveY, CubeSphereFace.NegativeY,
                CubeSphereFace.PositiveZ, CubeSphereFace.NegativeZ
            };
            Vector2[] samples =
            {
                new Vector2(-0.75f, -0.25f),
                new Vector2(0f, 0f),
                new Vector2(0.6f, 0.8f)
            };

            foreach (CubeSphereFace face in faces)
            foreach (Vector2 sample in samples)
            {
                Vector3 direction = CubeSphereTopology.GetDirection(face, sample.x, sample.y);
                CubeSphereTopology.DirectionToFaceCoordinates(direction, out CubeSphereFace recoveredFace, out Vector2 recovered);
                Assert.AreEqual(face, recoveredFace);
                Assert.That(Vector2.Distance(sample, recovered), Is.LessThan(1e-5f));
            }
        }

        [Test]
        public void CubeSphereTopologyRejectsZeroDirection()
        {
            Assert.Throws<System.ArgumentException>(() =>
                CubeSphereTopology.DirectionToFaceCoordinates(Vector3.zero, out _, out _));
        }

        [Test]
        public void SettingsValidationClampsSamplingResolutionsAndRadius()
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                settings.radiusMeters = -10f;
                settings.faceResolution = 1;
                settings.planisphereWidth = 1;
                settings.planisphereHeight = 1;
                settings.Validate();

                Assert.AreEqual(100f, settings.radiusMeters);
                Assert.AreEqual(2, settings.faceResolution);
                Assert.AreEqual(32, settings.planisphereWidth);
                Assert.AreEqual(16, settings.planisphereHeight);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [TestCase(100f)]
        [TestCase(5000f)]
        [TestCase(50000f)]
        [TestCase(1000000f)]
        public void SettingsValidationPreservesFiniteConfigurablePlanetRadii(float radiusMeters)
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                settings.radiusMeters = radiusMeters;
                settings.Validate();
                Assert.AreEqual(radiusMeters, settings.radiusMeters);
                Assert.IsFalse(float.IsNaN(settings.radiusMeters));
                Assert.IsFalse(float.IsInfinity(settings.radiusMeters));
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void SettingsValidationRecoversInvalidPlanetRadius(float invalidRadius)
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                settings.radiusMeters = invalidRadius;
                settings.Validate();
                Assert.AreEqual(50000f, settings.radiusMeters);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [TestCase(1f)]
        [TestCase(25f)]
        [TestCase(49f)]
        [TestCase(50f)]
        [TestCase(65f)]
        [TestCase(80f)]
        [TestCase(99f)]
        public void SettingsValidationPreservesOceanCoverageTargetsInApprovedRange(float targetPercent)
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                settings.oceanCoverageTargetPercent = targetPercent;
                settings.Validate();
                Assert.AreEqual(targetPercent, settings.oceanCoverageTargetPercent);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void SettingsValidationClampsOceanCoverageToOneThroughNinetyNinePercent()
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                settings.oceanCoverageTargetPercent = -10f;
                settings.Validate();
                Assert.AreEqual(1f, settings.oceanCoverageTargetPercent);
                settings.oceanCoverageTargetPercent = 120f;
                settings.Validate();
                Assert.AreEqual(99f, settings.oceanCoverageTargetPercent);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void SettingsValidationRestoresDefaultOceanTargetForNonFiniteValues(float invalidTarget)
        {
            SphericalPlanetStudioSettings settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                settings.oceanCoverageTargetPercent = invalidTarget;
                settings.Validate();
                Assert.AreEqual(65f, settings.oceanCoverageTargetPercent);
            }
            finally { Object.DestroyImmediate(settings); }
        }

    }
}
