using NUnit.Framework;
using Pokemon.Project.World.Spherical.Studio;
using UnityEngine;

namespace Pokemon.Project.Tests.EditMode.World.Spherical
{
    public sealed class SphericalWorldProfileTests
    {
        [Test]
        public void ProfileRoundTrip_PreservesWorldCharacteristics()
        {
            var settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                settings.radiusMeters = 12500f;
                settings.seed = 123456;
                settings.faceResolution = 64;
                settings.layer1AmplitudeMeters = 2400f;
                settings.layer2AmplitudeMeters = 600f;
                settings.oceanCoverageTargetPercent = 35f;
                settings.desertMoistureThreshold = 0.31f;
                settings.Validate();

                SphericalWorldProfile profile = SphericalWorldProfile.FromSettings(
                    settings, "test_profile", "Test Profile", "Test profile.");
                string json = JsonUtility.ToJson(profile);
                SphericalWorldProfile restored = JsonUtility.FromJson<SphericalWorldProfile>(json);

                var target = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
                try
                {
                    restored.ApplyTo(target);
                    Assert.AreEqual(12500f, target.radiusMeters);
                    Assert.AreEqual(123456, target.seed);
                    Assert.AreEqual(64, target.faceResolution);
                    Assert.AreEqual(2400f, target.layer1AmplitudeMeters);
                    Assert.AreEqual(600f, target.layer2AmplitudeMeters);
                    Assert.AreEqual(35f, target.oceanCoverageTargetPercent);
                    Assert.AreEqual(0.31f, target.desertMoistureThreshold);
                }
                finally { Object.DestroyImmediate(target); }
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void ApplyTo_DoesNotModifyPlayerOrPreviewOnlySettings()
        {
            var source = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            var target = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                target.radiusMeters = 5000f;
                target.oceanCoverageTargetPercent = 42f;
                target.view = SphericalPlanetStudioView.Planisphere;
                target.map = SphericalPlanetStudioMap.Moisture;
                target.drainageResolution = 128;
                SphericalWorldProfile profile = SphericalWorldProfile.FromSettings(
                    source, "source", "Source", "Source.");
                profile.ApplyTo(target);

                Assert.AreEqual(SphericalPlanetStudioView.Planisphere, target.view);
                Assert.AreEqual(SphericalPlanetStudioMap.Moisture, target.map);
                Assert.AreEqual(128, target.drainageResolution);
            }
            finally
            {
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(target);
            }
        }


        [Test]
        public void ApplyTo_LegacySettingsKeepsProfileOceanTarget()
        {
            var settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                settings.geographicSettingsVersion = 0;
                var profile = new SphericalWorldProfile { oceanCoverageTargetPercent = 35f };
                profile.ApplyTo(settings);
                Assert.AreEqual(1, settings.geographicSettingsVersion);
                Assert.AreEqual(35f, settings.oceanCoverageTargetPercent);
            }
            finally { Object.DestroyImmediate(settings); }
        }


        [Test]
        public void TinyWorldProfile_UsesOneKilometreRadius()
        {
            var settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                var profile = new SphericalWorldProfile
                {
                    schemaVersion = 1,
                    radiusKilometres = 1f,
                    faceResolution = 128,
                    layer1AmplitudeMeters = 35f,
                    layer2AmplitudeMeters = 8f,
                    layer3AmplitudeMeters = 1.5f,
                    mountainElevationThresholdMeters = 20f,
                    mountainSlopeThresholdDegrees = 32f,
                    oceanCoverageTargetPercent = 65f
                };
                profile.ApplyTo(settings);
                Assert.AreEqual(1000f, settings.radiusMeters);
                Assert.AreEqual(128, settings.faceResolution);
                Assert.AreEqual(35f, settings.layer1AmplitudeMeters);
                Assert.AreEqual(8f, settings.layer2AmplitudeMeters);
                Assert.AreEqual(1.5f, settings.layer3AmplitudeMeters);
                Assert.AreEqual(20f, settings.mountainElevationThresholdMeters);
                Assert.AreEqual(32f, settings.mountainSlopeThresholdDegrees);
                Assert.AreEqual(65f, settings.oceanCoverageTargetPercent);
            }
            finally { Object.DestroyImmediate(settings); }
        }

        [Test]
        public void ApplyTo_RejectsUnsupportedSchemaVersion()
        {
            var settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                var profile = new SphericalWorldProfile { schemaVersion = 999 };
                Assert.Throws<System.InvalidOperationException>(() => profile.ApplyTo(settings));
            }
            finally { Object.DestroyImmediate(settings); }
        }
    }
}
