using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Geography
{
    public sealed partial class SphericalGeographicFieldSet
    {
        /// <summary>
        /// Returns a biome or throws an explicit error when this field set has no biome classification.
        /// </summary>
        public SphericalBiome GetBiome(CubeSphereFace face, int x, int y)
        {
            if (!HasBiomeClassification)
                throw new InvalidOperationException("Biome classification is unavailable in this geographic field set.");
            int index = GetIndex(face, x, y);
            return _biomes[(int)face][index];
        }

        /// <summary>
        /// Tries to read a biome at a lattice coordinate without inventing a fallback biome.
        /// </summary>
        public bool TryGetBiome(CubeSphereFace face, int x, int y, out SphericalBiome biome)
        {
            int index = GetIndex(face, x, y);
            if (!HasBiomeClassification)
            {
                biome = SphericalBiome.None;
                return false;
            }
            biome = _biomes[(int)face][index];
            return true;
        }

        /// <summary>
        /// Samples a biome or throws an explicit error when classification was not generated.
        /// </summary>
        public SphericalBiome SampleBiome(Vector3 direction)
        {
            if (!TrySampleBiome(direction, out SphericalBiome biome))
                throw new InvalidOperationException("Biome classification is unavailable in this geographic field set.");
            return biome;
        }

        /// <summary>
        /// Tries to sample a biome; returns false and SphericalBiome.None when classification is absent.
        /// </summary>
        public bool TrySampleBiome(Vector3 direction, out SphericalBiome biome)
        {
            GetNearestCell(direction, out CubeSphereFace face, out int x, out int y);
            return TryGetBiome(face, x, y, out biome);
        }
    }
}
