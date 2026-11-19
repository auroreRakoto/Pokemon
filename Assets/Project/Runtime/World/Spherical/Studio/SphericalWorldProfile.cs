using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Studio
{
    /// <summary>
    /// Serializable, versioned description of a generated planet and its selected modules.
    /// </summary>
    [Serializable]
    public sealed class SphericalWorldProfile
    {
        public int schemaVersion = 1;
        public int moduleConfigurationVersion = 1;
        public SphericalWorldModuleOptions modules = new SphericalWorldModuleOptions();
        public string id = "custom_world";
        public string displayName = "Custom World";
        public string description = string.Empty;

        [Header("Planet")]
        public float radiusKilometres = 50f;
        public int seed = 481516;
        public int faceResolution = 96;
        public int planisphereWidth = 512;
        public int planisphereHeight = 256;

        [Header("Terrain")]
        public bool layer1Enabled = true;
        public float layer1AmplitudeMeters = 1800f;
        public float layer1Frequency = 2.2f;
        public float continentThreshold = 0.47f;
        public bool layer2Enabled = true;
        public float layer2AmplitudeMeters = 450f;
        public float layer2Frequency = 9f;
        public bool layer3Enabled = true;
        public float layer3AmplitudeMeters = 65f;
        public float layer3Frequency = 34f;

        [Header("Climate and biomes")]
        public float temperatureLatitudeInfluence = 0.75f;
        public float temperatureNoiseInfluence = 0.25f;
        public float oceanCoverageTargetPercent = 65f;
        public float moistureDistanceScaleRadiusMultiplier = 0.18f;
        public float moistureTemperatureEvaporation = 0.35f;
        public float moistureSlopeLoss = 0.2f;
        public float mountainElevationThresholdMeters = 1800f;
        public float mountainSlopeThresholdDegrees = 28f;
        public float desertMoistureThreshold = 0.2f;
        public float desertMinimumTemperature = 0.3f;
        public float forestMoistureThreshold = 0.48f;
        public float forestMinimumTemperature = 0.18f;
        public float forestMaximumTemperature = 0.9f;

        /// <summary>
        /// Creates a profile from the current Planet Studio generation settings.
        /// </summary>
        public static SphericalWorldProfile FromSettings(
            SphericalPlanetStudioSettings settings, string profileId, string name, string profileDescription)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            SphericalWorldProfile profile = new SphericalWorldProfile
            {
                schemaVersion = 1,
                moduleConfigurationVersion = 1,
                modules = settings.modules == null ? new SphericalWorldModuleOptions() : settings.modules.Copy(),
                id = string.IsNullOrWhiteSpace(profileId) ? "custom_world" : profileId,
                displayName = string.IsNullOrWhiteSpace(name) ? "Custom World" : name,
                description = profileDescription ?? string.Empty
            };
            CopyPlanetAndTerrainFromSettings(profile, settings);
            CopyClimateFromSettings(profile, settings);
            return profile;
        }

        /// <summary>
        /// Applies profile generation values while preserving preview-only settings.
        /// </summary>
        public void ApplyTo(SphericalPlanetStudioSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (schemaVersion != 1)
                throw new InvalidOperationException($"Unsupported world profile schema version: {schemaVersion}");
            NormalizeModuleConfiguration();
            settings.modules = modules.Copy();
            CopyPlanetAndTerrainToSettings(this, settings);
            CopyClimateToSettings(this, settings);
            settings.Validate();
        }

        /// <summary>
        /// Migrates profiles created before the optional module section existed.
        /// </summary>
        public void NormalizeModuleConfiguration()
        {
            if (moduleConfigurationVersion == 0)
            {
                modules = new SphericalWorldModuleOptions();
                moduleConfigurationVersion = 1;
                return;
            }
            if (moduleConfigurationVersion != 1)
                throw new InvalidOperationException(
                    $"Unsupported module configuration version: {moduleConfigurationVersion}");
            if (modules == null)
                throw new InvalidOperationException("The world profile module configuration is missing.");
        }

        /// <summary>
        /// Copies planet dimensions and terrain-layer values from Studio settings.
        /// </summary>
        private static void CopyPlanetAndTerrainFromSettings(
            SphericalWorldProfile profile, SphericalPlanetStudioSettings settings)
        {
            profile.radiusKilometres = settings.radiusMeters / 1000f;
            profile.seed = settings.seed;
            profile.faceResolution = settings.faceResolution;
            profile.planisphereWidth = settings.planisphereWidth;
            profile.planisphereHeight = settings.planisphereHeight;
            profile.layer1Enabled = settings.layer1Enabled;
            profile.layer1AmplitudeMeters = settings.layer1AmplitudeMeters;
            profile.layer1Frequency = settings.layer1Frequency;
            profile.continentThreshold = settings.continentThreshold;
            profile.layer2Enabled = settings.layer2Enabled;
            profile.layer2AmplitudeMeters = settings.layer2AmplitudeMeters;
            profile.layer2Frequency = settings.layer2Frequency;
            profile.layer3Enabled = settings.layer3Enabled;
            profile.layer3AmplitudeMeters = settings.layer3AmplitudeMeters;
            profile.layer3Frequency = settings.layer3Frequency;
        }

        /// <summary>
        /// Copies climate and biome-generation values from Studio settings.
        /// </summary>
        private static void CopyClimateFromSettings(
            SphericalWorldProfile profile, SphericalPlanetStudioSettings settings)
        {
            profile.temperatureLatitudeInfluence = settings.temperatureLatitudeInfluence;
            profile.temperatureNoiseInfluence = settings.temperatureNoiseInfluence;
            profile.oceanCoverageTargetPercent = settings.oceanCoverageTargetPercent;
            profile.moistureDistanceScaleRadiusMultiplier = settings.moistureDistanceScaleRadiusMultiplier;
            profile.moistureTemperatureEvaporation = settings.moistureTemperatureEvaporation;
            profile.moistureSlopeLoss = settings.moistureSlopeLoss;
            profile.mountainElevationThresholdMeters = settings.mountainElevationThresholdMeters;
            profile.mountainSlopeThresholdDegrees = settings.mountainSlopeThresholdDegrees;
            profile.desertMoistureThreshold = settings.desertMoistureThreshold;
            profile.desertMinimumTemperature = settings.desertMinimumTemperature;
            profile.forestMoistureThreshold = settings.forestMoistureThreshold;
            profile.forestMinimumTemperature = settings.forestMinimumTemperature;
            profile.forestMaximumTemperature = settings.forestMaximumTemperature;
        }

        /// <summary>
        /// Applies planet dimensions and terrain-layer values to Studio settings.
        /// </summary>
        private static void CopyPlanetAndTerrainToSettings(
            SphericalWorldProfile profile, SphericalPlanetStudioSettings settings)
        {
            settings.radiusMeters = profile.radiusKilometres * 1000f;
            settings.seed = profile.seed;
            settings.faceResolution = profile.faceResolution;
            settings.planisphereWidth = profile.planisphereWidth;
            settings.planisphereHeight = profile.planisphereHeight;
            settings.layer1Enabled = profile.layer1Enabled;
            settings.layer1AmplitudeMeters = profile.layer1AmplitudeMeters;
            settings.layer1Frequency = profile.layer1Frequency;
            settings.continentThreshold = profile.continentThreshold;
            settings.layer2Enabled = profile.layer2Enabled;
            settings.layer2AmplitudeMeters = profile.layer2AmplitudeMeters;
            settings.layer2Frequency = profile.layer2Frequency;
            settings.layer3Enabled = profile.layer3Enabled;
            settings.layer3AmplitudeMeters = profile.layer3AmplitudeMeters;
            settings.layer3Frequency = profile.layer3Frequency;
        }

        /// <summary>
        /// Applies climate and biome-generation values without changing preview controls.
        /// </summary>
        private static void CopyClimateToSettings(
            SphericalWorldProfile profile, SphericalPlanetStudioSettings settings)
        {
            settings.temperatureLatitudeInfluence = profile.temperatureLatitudeInfluence;
            settings.temperatureNoiseInfluence = profile.temperatureNoiseInfluence;
            settings.oceanCoverageTargetPercent = profile.oceanCoverageTargetPercent;
            settings.moistureDistanceScaleRadiusMultiplier = profile.moistureDistanceScaleRadiusMultiplier;
            settings.moistureTemperatureEvaporation = profile.moistureTemperatureEvaporation;
            settings.moistureSlopeLoss = profile.moistureSlopeLoss;
            settings.mountainElevationThresholdMeters = profile.mountainElevationThresholdMeters;
            settings.mountainSlopeThresholdDegrees = profile.mountainSlopeThresholdDegrees;
            settings.desertMoistureThreshold = profile.desertMoistureThreshold;
            settings.desertMinimumTemperature = profile.desertMinimumTemperature;
            settings.forestMoistureThreshold = profile.forestMoistureThreshold;
            settings.forestMinimumTemperature = profile.forestMinimumTemperature;
            settings.forestMaximumTemperature = profile.forestMaximumTemperature;
            settings.geographicSettingsVersion = 1;
        }
    }
}
