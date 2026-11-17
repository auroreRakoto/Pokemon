using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical
{
    /// <summary>
    /// Runtime settings for the first spherical-world prototype.
    /// Values are prototype tuning parameters, not production world configuration.
    /// </summary>
    [Serializable]
    public sealed class SphericalWorldSettings
    {
        [Header("Planet")]
        [Min(1f)] public float planetRadiusMeters = 50000f;
        [Range(2, 256)] public int faceResolution = 48;

        [Header("Terrain")]
        public int seed = 481516;
        [Min(0f)] public float maximumElevationMeters = 350f;
        [Min(0.0001f)] public float noiseFrequency = 3.5f;
        [Range(1, 8)] public int octaves = 5;
        [Min(1.01f)] public float lacunarity = 2f;
        [Range(0f, 1f)] public float persistence = 0.5f;

        public void Validate()
        {
            if (planetRadiusMeters < 1f)
                throw new ArgumentOutOfRangeException(nameof(planetRadiusMeters), "Planet radius must be at least one metre.");

            if (faceResolution < 2 || faceResolution > 256)
                throw new ArgumentOutOfRangeException(nameof(faceResolution), "Face resolution must be between 2 and 256.");

            if (maximumElevationMeters < 0f)
                throw new ArgumentOutOfRangeException(nameof(maximumElevationMeters));

            if (noiseFrequency <= 0f)
                throw new ArgumentOutOfRangeException(nameof(noiseFrequency));

            if (octaves < 1 || octaves > 8)
                throw new ArgumentOutOfRangeException(nameof(octaves));

            if (lacunarity <= 1f)
                throw new ArgumentOutOfRangeException(nameof(lacunarity), "Lacunarity must be greater than one.");

            if (persistence < 0f || persistence > 1f)
                throw new ArgumentOutOfRangeException(nameof(persistence));
        }
    }
}
