using System;
using Pokemon.Project.World.Spherical.Geography;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Presentation
{
    /// <summary>
    /// Deterministic, tileable stylized albedo detail for WORLD-002 biome surfaces.
    /// The output is a neutral detail multiplier; biome identity remains in material tint/weights.
    /// </summary>
    public static class SphericalBiomeTextureGenerator
    {
        public const int DefaultResolution = 512;
        public const int DefaultSeed = 481516;

        public static Color32[] Generate(SphericalBiome biome, int resolution = DefaultResolution, int seed = DefaultSeed)
        {
            if (resolution < 16 || resolution > 2048)
                throw new ArgumentOutOfRangeException(nameof(resolution), "Texture resolution must be between 16 and 2048.");
            if (biome != SphericalBiome.Plains && biome != SphericalBiome.Forest &&
                biome != SphericalBiome.Desert && biome != SphericalBiome.Mountains)
                throw new ArgumentOutOfRangeException(nameof(biome), "Only the four terrestrial biomes have ground detail textures.");

            Color32[] pixels = new Color32[resolution * resolution];
            int biomeSeed = unchecked(seed + ((int)biome + 1) * 7919);
            for (int y = 0; y < resolution; y++)
            for (int x = 0; x < resolution; x++)
            {
                float u = (float)x / resolution;
                float v = (float)y / resolution;
                float broad = PeriodicNoise(u, v, 4, biomeSeed);
                float medium = PeriodicNoise(u, v, 11, biomeSeed + 101);
                float fine = PeriodicNoise(u, v, 29, biomeSeed + 211);
                float grain = PeriodicNoise(u, v, 67, biomeSeed + 307);
                float value = 0.88f + (broad - 0.5f) * 0.19f + (medium - 0.5f) * 0.13f
                    + (fine - 0.5f) * 0.075f + (grain - 0.5f) * 0.035f;

                // Gentle biome-specific painted marks; keep the texture low-contrast so it reads at distance.
                float mark = PeriodicNoise(u, v, biome == SphericalBiome.Desert ? 7 : 13, biomeSeed + 401);
                if (biome == SphericalBiome.Desert)
                    value += (mark > 0.69f ? 0.045f : 0f) - (mark < 0.27f ? 0.018f : 0f);
                else if (biome == SphericalBiome.Forest)
                    value += mark > 0.73f ? 0.035f : (mark < 0.25f ? -0.025f : 0f);
                else if (biome == SphericalBiome.Mountains)
                    value += (medium > 0.68f ? 0.035f : 0f) - (medium < 0.31f ? 0.035f : 0f);
                else
                    value += mark > 0.76f ? 0.025f : (mark < 0.22f ? -0.015f : 0f);

                value = Mathf.Clamp(value, 0.68f, 1.12f);
                float warm = (broad - 0.5f) * 0.025f;
                float cool = (fine - 0.5f) * 0.018f;
                byte r = ToByte(value + warm);
                byte g = ToByte(value);
                byte b = ToByte(value - warm + cool);
                pixels[y * resolution + x] = new Color32(r, g, b, 255);
            }
            return pixels;
        }

        private static float PeriodicNoise(float u, float v, int cells, int seed)
        {
            float x = u * cells;
            float y = v * cells;
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float tx = Smooth(x - x0);
            float ty = Smooth(y - y0);
            float a = Hash(Wrap(x0, cells), Wrap(y0, cells), seed);
            float b = Hash(Wrap(x0 + 1, cells), Wrap(y0, cells), seed);
            float c = Hash(Wrap(x0, cells), Wrap(y0 + 1, cells), seed);
            float d = Hash(Wrap(x0 + 1, cells), Wrap(y0 + 1, cells), seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        private static int Wrap(int value, int period)
        {
            int result = value % period;
            return result < 0 ? result + period : result;
        }

        private static float Smooth(float value) => value * value * (3f - 2f * value);

        private static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)x * 0x8da6b343u;
                h ^= (uint)y * 0xd8163841u;
                h ^= h >> 13;
                h *= 0x85ebca6bu;
                h ^= h >> 16;
                return (h & 0x00ffffffu) / 16777215f;
            }
        }

        private static byte ToByte(float value) => (byte)Mathf.Clamp(Mathf.RoundToInt(value * 255f), 0, 255);
    }
}
