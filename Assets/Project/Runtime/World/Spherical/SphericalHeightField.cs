using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical
{
    /// <summary>
    /// Deterministic 3D value-noise height field sampled from a unit-sphere direction.
    /// Sampling directions rather than per-face UVs guarantees a continuous field across face seams.
    /// </summary>
    public static class SphericalHeightField
    {
        public static float SampleHeight(Vector3 direction, SphericalWorldSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            settings.Validate();
            return SampleHeightValidated(direction, settings);
        }

        internal static float SampleHeightValidated(Vector3 direction, SphericalWorldSettings settings)
        {
            if (direction.sqrMagnitude < 1e-12f)
                throw new ArgumentException("Direction must be non-zero.", nameof(direction));

            Vector3 unitDirection = direction.normalized;
            float frequency = settings.noiseFrequency;
            float amplitude = 1f;
            float amplitudeSum = 0f;
            float valueSum = 0f;

            for (int octave = 0; octave < settings.octaves; octave++)
            {
                Vector3 samplePoint = unitDirection * frequency;
                float octaveValue = ValueNoise3D(samplePoint, settings.seed + octave * 1013);
                valueSum += octaveValue * amplitude;
                amplitudeSum += amplitude;

                frequency *= settings.lacunarity;
                amplitude *= settings.persistence;
            }

            if (amplitudeSum <= 1e-8f)
                return 0f;

            float normalizedNoise = Mathf.Clamp01((valueSum / amplitudeSum + 1f) * 0.5f);
            return normalizedNoise * settings.maximumElevationMeters;
        }

        private static float ValueNoise3D(Vector3 point, int seed)
        {
            int x0 = Mathf.FloorToInt(point.x);
            int y0 = Mathf.FloorToInt(point.y);
            int z0 = Mathf.FloorToInt(point.z);

            float tx = Fade(point.x - x0);
            float ty = Fade(point.y - y0);
            float tz = Fade(point.z - z0);

            float c000 = HashToValue(x0, y0, z0, seed);
            float c100 = HashToValue(x0 + 1, y0, z0, seed);
            float c010 = HashToValue(x0, y0 + 1, z0, seed);
            float c110 = HashToValue(x0 + 1, y0 + 1, z0, seed);
            float c001 = HashToValue(x0, y0, z0 + 1, seed);
            float c101 = HashToValue(x0 + 1, y0, z0 + 1, seed);
            float c011 = HashToValue(x0, y0 + 1, z0 + 1, seed);
            float c111 = HashToValue(x0 + 1, y0 + 1, z0 + 1, seed);

            float x00 = Mathf.Lerp(c000, c100, tx);
            float x10 = Mathf.Lerp(c010, c110, tx);
            float x01 = Mathf.Lerp(c001, c101, tx);
            float x11 = Mathf.Lerp(c011, c111, tx);
            float y0Value = Mathf.Lerp(x00, x10, ty);
            float y1Value = Mathf.Lerp(x01, x11, ty);
            return Mathf.Lerp(y0Value, y1Value, tz);
        }

        private static float Fade(float value)
        {
            return value * value * value * (value * (value * 6f - 15f) + 10f);
        }

        private static float HashToValue(int x, int y, int z, int seed)
        {
            unchecked
            {
                uint hash = (uint)seed;
                hash ^= (uint)x * 0x8DA6B343u;
                hash ^= (uint)y * 0xD8163841u;
                hash ^= (uint)z * 0xCB1AB31Fu;
                hash ^= hash >> 16;
                hash *= 0x7FEB352Du;
                hash ^= hash >> 15;
                hash *= 0x846CA68Bu;
                hash ^= hash >> 16;
                return (hash & 0x00FFFFFFu) / 8388607.5f - 1f;
            }
        }
    }
}
