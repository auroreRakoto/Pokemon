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

                // Tileable biome-specific surface signatures. Detail is intentionally
                // stronger than the first pass so it remains visible in the playable camera.
                float mark = PeriodicNoise(u, v, biome == SphericalBiome.Desert ? 7 : 13, biomeSeed + 401);
                float redMultiplier = value;
                float greenMultiplier = value;
                float blueMultiplier = value;

                if (biome == SphericalBiome.Plains)
                {
                    // Keep the base albedo fairly clean: the visible grass structure is
                    // rasterized as individual tapered blades below, not buried in noise.
                    float patch = PeriodicNoise(u, v, 5, biomeSeed + 503);
                    float soil = Mathf.Pow(1f - PeriodicNoise(u, v, 19, biomeSeed + 509), 13f);
                    float patchVariation = (patch - 0.5f) * 0.10f;
                    redMultiplier += patchVariation * 0.65f - soil * 0.08f;
                    greenMultiplier += patchVariation + soil * 0.015f;
                    blueMultiplier += patchVariation * 0.35f - soil * 0.10f;
                }
                else if (biome == SphericalBiome.Forest)
                {
                    // Stronger moss patches and leaf-litter speckles for a visibly denser floor.
                    float clusters = PeriodicNoise(u, v, 6, biomeSeed + 601);
                    float leafSpeckle = PeriodicNoise(u, v, 31, biomeSeed + 607);
                    float moss = clusters > 0.61f ? 0.16f : (clusters < 0.34f ? -0.12f : 0f);
                    float litter = leafSpeckle > 0.72f ? 0.10f : (leafSpeckle < 0.2f ? -0.08f : 0f);
                    redMultiplier += moss * 0.55f + litter * 0.8f;
                    greenMultiplier += moss + litter * 0.35f;
                    blueMultiplier += moss * 0.45f - litter * 0.25f;
                }
                else if (biome == SphericalBiome.Desert)
                {
                    float variation = (mark > 0.69f ? 0.07f : 0f) - (mark < 0.27f ? 0.035f : 0f);
                    redMultiplier += variation;
                    greenMultiplier += variation * 0.8f;
                    blueMultiplier += variation * 0.55f;
                }
                else if (biome == SphericalBiome.Mountains)
                {
                    float variation = (medium > 0.68f ? 0.07f : 0f) - (medium < 0.31f ? 0.07f : 0f);
                    redMultiplier += variation;
                    greenMultiplier += variation;
                    blueMultiplier += variation * 1.05f;
                }

                redMultiplier = Mathf.Clamp(redMultiplier + (broad - 0.5f) * 0.035f, 0.48f, 1.30f);
                greenMultiplier = Mathf.Clamp(greenMultiplier, 0.48f, 1.30f);
                blueMultiplier = Mathf.Clamp(blueMultiplier + (fine - 0.5f) * 0.025f, 0.48f, 1.30f);
                byte r = ToByte(redMultiplier);
                byte g = ToByte(greenMultiplier);
                byte b = ToByte(blueMultiplier);
                pixels[y * resolution + x] = new Color32(r, g, b, 255);
            }

            if (biome == SphericalBiome.Plains)
                DrawPlainsGrassBlades(pixels, resolution, biomeSeed);

            return pixels;
        }

        /// <summary>
        /// Draws small tapered, slightly curved grass blades into the tile. Blade placement
        /// is generated from wrapped cell coordinates, so opposite texture edges match.
        /// The 8 m world repeat makes a 512 px tile represent a readable patch of ground.
        /// </summary>
        private static void DrawPlainsGrassBlades(Color32[] pixels, int resolution, int seed)
        {
            const int cells = 32;
            float cellSize = (float)resolution / cells;

            for (int cy = 0; cy < cells; cy++)
            for (int cx = 0; cx < cells; cx++)
            {
                int cellSeed = unchecked(seed + cx * 73856093 + cy * 19349663);
                int bladeCount = 1 + Mathf.FloorToInt(Hash01(cx, cy, cellSeed + 17) * 3f);

                for (int bladeIndex = 0; bladeIndex < bladeCount; bladeIndex++)
                {
                    int bladeSeed = cellSeed + bladeIndex * 83492791;
                    float x0 = (cx + Hash01(cx, cy, bladeSeed + 1)) * cellSize;
                    float y0 = (cy + Hash01(cx, cy, bladeSeed + 2)) * cellSize;
                    float length = Mathf.Lerp(5.5f, 15.5f, Hash01(cx, cy, bladeSeed + 3));
                    float width = Mathf.Lerp(0.65f, 1.55f, Hash01(cx, cy, bladeSeed + 4));
                    float lean = Mathf.Lerp(-0.34f, 0.34f, Hash01(cx, cy, bladeSeed + 5)) * length;
                    float curve = Mathf.Lerp(-0.18f, 0.18f, Hash01(cx, cy, bladeSeed + 6)) * length;
                    bool highlight = Hash01(cx, cy, bladeSeed + 7) > 0.58f;

                    // Rasterize only the blade's local bounds; wrap writes at tile edges.
                    float horizontalReach = Mathf.Abs(lean) + Mathf.Abs(curve) + width + 1f;
                    int minX = Mathf.FloorToInt(x0 - horizontalReach);
                    int maxX = Mathf.CeilToInt(x0 + horizontalReach);
                    int minY = Mathf.FloorToInt(y0 - 1f);
                    int maxY = Mathf.CeilToInt(y0 + length + 1f);

                    for (int py = minY; py <= maxY; py++)
                    {
                        float t = (py + 0.5f - y0) / length;
                        if (t < 0f || t > 1f) continue;

                        float centerX = x0 + lean * t + curve * Mathf.Sin(t * Mathf.PI);
                        float bladeWidth = Mathf.Lerp(width, 0.12f, t);
                        for (int px = minX; px <= maxX; px++)
                        {
                            float distance = Mathf.Abs((px + 0.5f) - centerX);
                            float edge = bladeWidth - distance;
                            if (edge <= -0.35f) continue;

                            float coverage = Mathf.Clamp01((edge + 0.35f) / 0.7f);
                            int wrappedX = Wrap(px, resolution);
                            int wrappedY = Wrap(py, resolution);
                            int index = wrappedY * resolution + wrappedX;
                            Color32 current = pixels[index];

                            // Bright yellow-green highlights and deeper green shadow blades.
                            float blend = coverage * (highlight ? 0.82f : 0.76f);
                            float bladeR = highlight ? 1.02f : 0.68f;
                            float bladeG = highlight ? 1.13f : 0.84f;
                            float bladeB = highlight ? 0.78f : 0.62f;
                            pixels[index] = new Color32(
                                ToByte(Mathf.Lerp(current.r / 255f, bladeR, blend)),
                                ToByte(Mathf.Lerp(current.g / 255f, bladeG, blend)),
                                ToByte(Mathf.Lerp(current.b / 255f, bladeB, blend)),
                                255);
                        }
                    }
                }
            }
        }

        private static float Hash01(int x, int y, int seed) => Hash(Wrap(x, 32), Wrap(y, 32), seed);

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
