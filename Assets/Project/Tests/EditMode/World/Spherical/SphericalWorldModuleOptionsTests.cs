using NUnit.Framework;
using Pokemon.Project.World.Spherical.Studio;
using UnityEngine;

namespace Pokemon.Project.Tests.EditMode.World.Spherical
{
    /// <summary>
    /// Verifies serialization and legacy migration for world module options.
    /// </summary>
    public sealed class SphericalWorldModuleOptionsTests
    {
        /// <summary>
        /// Confirms defaults preserve the current playable inspection composition.
        /// </summary>
        [Test]
        public void DefaultOptions_PreserveCurrentWorldProfileBehaviour()
        {
            SphericalWorldModuleOptions options = new SphericalWorldModuleOptions();

            Assert.That(options.terrainPresentationEnabled, Is.True);
            Assert.That(options.biomeClassificationEnabled, Is.True);
            Assert.That(options.biomeGroundPresentationEnabled, Is.True);
            Assert.That(options.environmentAssetsEnabled, Is.False);
            Assert.That(options.playerEnabled, Is.True);
            Assert.That(options.pokemonEnabled, Is.False);
        }

        /// <summary>
        /// Confirms all optional module choices survive JSON serialization.
        /// </summary>
        [Test]
        public void ProfileRoundTrip_PreservesModuleOptions()
        {
            SphericalWorldProfile profile = new SphericalWorldProfile();
            profile.modules.terrainPresentationEnabled = false;
            profile.modules.biomeClassificationEnabled = false;
            profile.modules.biomeGroundPresentationEnabled = false;
            profile.modules.environmentAssetsEnabled = true;
            profile.modules.playerEnabled = false;
            profile.modules.pokemonEnabled = true;

            SphericalWorldProfile restored =
                JsonUtility.FromJson<SphericalWorldProfile>(JsonUtility.ToJson(profile));
            restored.NormalizeModuleConfiguration();

            Assert.That(restored.modules.terrainPresentationEnabled, Is.False);
            Assert.That(restored.modules.biomeClassificationEnabled, Is.False);
            Assert.That(restored.modules.biomeGroundPresentationEnabled, Is.False);
            Assert.That(restored.modules.environmentAssetsEnabled, Is.True);
            Assert.That(restored.modules.playerEnabled, Is.False);
            Assert.That(restored.modules.pokemonEnabled, Is.True);
        }

        /// <summary>
        /// Migrates version-one world profiles without changing their existing gameplay defaults.
        /// </summary>
        [Test]
        public void LegacyProfile_MigratesMissingModuleOptions()
        {
            SphericalWorldProfile profile = JsonUtility.FromJson<SphericalWorldProfile>(
                "{\"schemaVersion\":1,\"id\":\"legacy\",\"radiusKilometres\":50}");

            profile.NormalizeModuleConfiguration();

            Assert.That(profile.moduleConfigurationVersion, Is.EqualTo(1));
            Assert.That(profile.modules.playerEnabled, Is.True);
            Assert.That(profile.modules.biomeClassificationEnabled, Is.True);
            Assert.That(profile.modules.environmentAssetsEnabled, Is.False);
            Assert.That(profile.modules.pokemonEnabled, Is.False);
        }


        /// <summary>
        /// Applies legacy profile data and confirms migration occurs before profile use.
        /// </summary>
        [Test]
        public void ApplyTo_MigratesLegacyModuleConfiguration()
        {
            SphericalWorldProfile profile = JsonUtility.FromJson<SphericalWorldProfile>(
                "{\"schemaVersion\":1,\"radiusKilometres\":12}");
            SphericalPlanetStudioSettings settings =
                ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                profile.ApplyTo(settings);
                Assert.That(profile.modules.playerEnabled, Is.True);
                Assert.That(profile.modules.environmentAssetsEnabled, Is.False);
                Assert.That(settings.radiusMeters, Is.EqualTo(12000f));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }


        /// <summary>
        /// Copies module selections between Studio settings and profiles without shared mutable references.
        /// </summary>
        [Test]
        public void ProfileSettingsTransfer_CopiesModuleOptions()
        {
            SphericalPlanetStudioSettings source =
                ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            SphericalPlanetStudioSettings target =
                ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            try
            {
                source.modules.playerEnabled = false;
                source.modules.environmentAssetsEnabled = true;
                SphericalWorldProfile profile = SphericalWorldProfile.FromSettings(
                    source, "modules", "Modules", "Module copy test.");
                source.modules.playerEnabled = true;
                profile.ApplyTo(target);

                Assert.That(profile.modules.playerEnabled, Is.False);
                Assert.That(target.modules.playerEnabled, Is.False);
                Assert.That(target.modules.environmentAssetsEnabled, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(target);
            }
        }

        /// <summary>
        /// Rejects module configuration versions that this runtime cannot interpret.
        /// </summary>
        [Test]
        public void NormalizeModuleConfiguration_RejectsUnsupportedVersion()
        {
            SphericalWorldProfile profile = new SphericalWorldProfile
            {
                moduleConfigurationVersion = 99
            };

            Assert.Throws<System.InvalidOperationException>(
                () => profile.NormalizeModuleConfiguration());
        }

        /// <summary>
        /// Rejects a current module configuration whose serialized options are missing.
        /// </summary>
        [Test]
        public void NormalizeModuleConfiguration_RejectsMissingCurrentOptions()
        {
            SphericalWorldProfile profile = new SphericalWorldProfile
            {
                moduleConfigurationVersion = 1,
                modules = null
            };

            Assert.Throws<System.InvalidOperationException>(
                () => profile.NormalizeModuleConfiguration());
        }
    }
}
