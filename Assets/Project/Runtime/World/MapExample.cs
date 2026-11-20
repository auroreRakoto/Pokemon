
using System;
using System.Collections.Generic;

namespace Pokemon.World
{
    // // Temporary hardcoded map generator.
    public static class MapExample
    {
        public static MapData Generate()
        {
            const int width = 64;
            const int height = 64;

            float[] heightValues = new float[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);
                    float ny = (float)y / (height - 1);

                    // Rolling terrain
                    float terrain =
                        0.35f
                        + 0.07f * MathF.Sin(nx * 12f)
                                * MathF.Cos(ny * 10f)
                        + 0.04f * MathF.Sin(nx * 25f + ny * 13f)
                        + 0.025f * MathF.Cos(ny * 30f - nx * 8f);

                    // Mountain ranges
                    float mountains =
                        Gaussian(nx, ny, 0.35f, 0.35f, 0.10f, 0.14f) * 0.55f
                        + Gaussian(nx, ny, 0.72f, 0.55f, 0.12f, 0.10f) * 0.48f
                        + Gaussian(nx, ny, 0.50f, 0.80f, 0.16f, 0.08f) * 0.35f;

                    // Valley
                    float valley =
                        Gaussian(nx, ny, 0.25f, 0.72f, 0.13f, 0.10f) * 0.16f;

                    float value = terrain + mountains - valley;

                    heightValues[y * width + x] =
                        Math.Clamp(value, 0f, 1f);
                }
            }

            var layers = new List<MapLayerData>
            {
                new MapLayerData("height", heightValues)
            };

            // Map start and end positions.
            var spawnPoints = new MapSpawnPoints(
                new MapPoint(4, 4),
                new MapPoint(59, 59)
            );

            return new MapData(
                width,
                height,
                layers,
                spawnPoints
            );
        }

        private static float Gaussian(
            float x,
            float y,
            float centerX,
            float centerY,
            float spreadX,
            float spreadY)
        {
            float dx = (x - centerX) / spreadX;
            float dy = (y - centerY) / spreadY;

            return MathF.Exp(-0.5f * (dx * dx + dy * dy));
        }
    }
}
