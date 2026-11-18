using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Studio
{
    /// <summary>
    /// Serializable, versioned description of a generated planet.
    /// JSON profiles contain world-generation characteristics only; editor preview and
    /// experimental drainage controls remain local to the Planet Studio settings asset.
    /// </summary>
    [Serializable]
    public sealed class SphericalWorldProfile
    {
        public int schemaVersion = 1;
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

        public static SphericalWorldProfile FromSettings(
            SphericalPlanetStudioSettings settings, string profileId, string name, string profileDescription)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            return new SphericalWorldProfile
            {
                schemaVersion = 1,
                id = string.IsNullOrWhiteSpace(profileId) ? "custom_world" : profileId,
                displayName = string.IsNullOrWhiteSpace(name) ? "Custom World" : name,
                description = profileDescription ?? string.Empty,
                radiusKilometres = settings.radiusMeters / 1000f,
                seed = settings.seed,
                faceResolution = settings.faceResolution,
                planisphereWidth = settings.planisphereWidth,
                planisphereHeight = settings.planisphereHeight,
                layer1Enabled = settings.layer1Enabled,
                layer1AmplitudeMeters = settings.layer1AmplitudeMeters,
                layer1Frequency = settings.layer1Frequency,
                continentThreshold = settings.continentThreshold,
                layer2Enabled = settings.layer2Enabled,
                layer2AmplitudeMeters = settings.layer2AmplitudeMeters,
                layer2Frequency = settings.layer2Frequency,
                layer3Enabled = settings.layer3Enabled,
                layer3AmplitudeMeters = settings.layer3AmplitudeMeters,
                layer3Frequency = settings.layer3Frequency,
                temperatureLatitudeInfluence = settings.temperatureLatitudeInfluence,
                temperatureNoiseInfluence = settings.temperatureNoiseInfluence,
                oceanCoverageTargetPercent = settings.oceanCoverageTargetPercent,
                moistureDistanceScaleRadiusMultiplier = settings.moistureDistanceScaleRadiusMultiplier,
                moistureTemperatureEvaporation = settings.moistureTemperatureEvaporation,
                moistureSlopeLoss = settings.moistureSlopeLoss,
                mountainElevationThresholdMeters = settings.mountainElevationThresholdMeters,
                mountainSlopeThresholdDegrees = settings.mountainSlopeThresholdDegrees,
                desertMoistureThreshold = settings.desertMoistureThreshold,
                desertMinimumTemperature = settings.desertMinimumTemperature,
                forestMoistureThreshold = settings.forestMoistureThreshold,
                forestMinimumTemperature = settings.forestMinimumTemperature,
                forestMaximumTemperature = settings.forestMaximumTemperature
            };
        }

        public void ApplyTo(SphericalPlanetStudioSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (schemaVersion != 1)
                throw new InvalidOperationException($"Unsupported world profile schema version: {schemaVersion}");

            settings.radiusMeters = radiusKilometres * 1000f;
            settings.seed = seed;
            settings.faceResolution = faceResolution;
            settings.planisphereWidth = planisphereWidth;
            settings.planisphereHeight = planisphereHeight;
            settings.layer1Enabled = layer1Enabled;
            settings.layer1AmplitudeMeters = layer1AmplitudeMeters;
            settings.layer1Frequency = layer1Frequency;
            settings.continentThreshold = continentThreshold;
            settings.layer2Enabled = layer2Enabled;
            settings.layer2AmplitudeMeters = layer2AmplitudeMeters;
            settings.layer2Frequency = layer2Frequency;
            settings.layer3Enabled = layer3Enabled;
            settings.layer3AmplitudeMeters = layer3AmplitudeMeters;
            settings.layer3Frequency = layer3Frequency;
            settings.temperatureLatitudeInfluence = temperatureLatitudeInfluence;
            settings.temperatureNoiseInfluence = temperatureNoiseInfluence;
            settings.oceanCoverageTargetPercent = oceanCoverageTargetPercent;
            settings.moistureDistanceScaleRadiusMultiplier = moistureDistanceScaleRadiusMultiplier;
            settings.moistureTemperatureEvaporation = moistureTemperatureEvaporation;
            settings.moistureSlopeLoss = moistureSlopeLoss;
            settings.mountainElevationThresholdMeters = mountainElevationThresholdMeters;
            settings.mountainSlopeThresholdDegrees = mountainSlopeThresholdDegrees;
            settings.desertMoistureThreshold = desertMoistureThreshold;
            settings.desertMinimumTemperature = desertMinimumTemperature;
            settings.forestMoistureThreshold = forestMoistureThreshold;
            settings.forestMinimumTemperature = forestMinimumTemperature;
            settings.forestMaximumTemperature = forestMaximumTemperature;
            // The profile is the authoritative source for these values. Mark the geographic
            // schema current before validation so a legacy-settings migration cannot overwrite
            // the profile's selected ocean target with the historical default.
            settings.geographicSettingsVersion = 1;
            settings.Validate();
        }
    }
}
