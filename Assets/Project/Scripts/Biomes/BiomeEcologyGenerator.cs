
using UnityEngine;

public static class BiomeEcologyGenerator
{
    public static MeadowData Generate(
        BiomeDefinition definition,
        int resolution
    )
    {
        MeadowData data = new MeadowData(resolution);

        System.Random random = new System.Random(definition.seed);

        float offsetX = (float)random.NextDouble() * 10000f;
        float offsetY = (float)random.NextDouble() * 10000f;

        GenerateWater(data, definition, offsetX, offsetY);
        GenerateRocks(data, definition, random);
        GenerateHumidity(data, definition);
        GenerateVegetation(data, definition, offsetX, offsetY);
        GenerateFlora(data, definition, random);
        GenerateBushes(data, definition, offsetX, offsetY, random);

        return data;
    }

    private static void GenerateWater(
        MeadowData data,
        BiomeDefinition definition,
        float offsetX,
        float offsetY
    )
    {
        int resolution = data.Resolution;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (float)x / resolution;
                float v = (float)y / resolution;

                float noise = Mathf.PerlinNoise(
                    u * definition.waterScale + offsetX + 500f,
                    v * definition.waterScale + offsetY + 500f
                );

                data.Water[data.GetIndex(x, y)] =
                    noise > definition.waterThreshold ? 1f : 0f;
            }
        }
    }

    private static void GenerateRocks(
        MeadowData data,
        BiomeDefinition definition,
        System.Random random
    )
    {
        GeneratePoints(
            data,
            data.Rocks,
            definition.rockCount,
            definition.rockRadius,
            random
        );
    }

    private static void GenerateHumidity(
        MeadowData data,
        BiomeDefinition definition
    )
    {
        int resolution = data.Resolution;
        int size = resolution * resolution;

        float[] distances = new float[size];

        const float infinity = 100000000f;
        const float diagonalCost = 1.41421356f;

        for (int i = 0; i < size; i++)
            distances[i] = data.Water[i] > 0f ? 0f : infinity;

        // Forward pass
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int index = data.GetIndex(x, y);
                float distance = distances[index];

                if (x > 0)
                    distance = Mathf.Min(distance, distances[index - 1] + 1f);

                if (y > 0)
                    distance = Mathf.Min(distance, distances[index - resolution] + 1f);

                if (x > 0 && y > 0)
                    distance = Mathf.Min(
                        distance,
                        distances[index - resolution - 1] + diagonalCost
                    );

                if (x < resolution - 1 && y > 0)
                    distance = Mathf.Min(
                        distance,
                        distances[index - resolution + 1] + diagonalCost
                    );

                distances[index] = distance;
            }
        }

        // Backward pass
        for (int y = resolution - 1; y >= 0; y--)
        {
            for (int x = resolution - 1; x >= 0; x--)
            {
                int index = data.GetIndex(x, y);
                float distance = distances[index];

                if (x < resolution - 1)
                    distance = Mathf.Min(distance, distances[index + 1] + 1f);

                if (y < resolution - 1)
                    distance = Mathf.Min(distance, distances[index + resolution] + 1f);

                if (x < resolution - 1 && y < resolution - 1)
                    distance = Mathf.Min(
                        distance,
                        distances[index + resolution + 1] + diagonalCost
                    );

                if (x > 0 && y < resolution - 1)
                    distance = Mathf.Min(
                        distance,
                        distances[index + resolution - 1] + diagonalCost
                    );

                distances[index] = distance;
            }
        }

        for (int i = 0; i < size; i++)
        {
            float distance = distances[i];

            data.Humidity[i] =
                distance > definition.humidityRadius
                ? 0f
                : Mathf.Exp(-distance / definition.humidityDecayDistance);
        }
    }

    private static void GenerateVegetation(
        MeadowData data,
        BiomeDefinition definition,
        float offsetX,
        float offsetY
    )
    {
        int resolution = data.Resolution;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int index = data.GetIndex(x, y);

                if (data.Water[index] > 0f)
                {
                    data.Vegetation[index] = 0f;
                    continue;
                }

                float u = (float)x / resolution;
                float v = (float)y / resolution;

                float noise = FractalNoise(
                    u * definition.vegetationScale + offsetX,
                    v * definition.vegetationScale + offsetY,
                    definition
                );

                float humidityBonus =
                    data.Humidity[index] * definition.humidityInfluence;

                float adjustedNoise = Mathf.Clamp01(noise + humidityBonus);

                float rockFactor = Mathf.Lerp(
                    1f,
                    1f - definition.rockVegetationExclusion,
                    data.Rocks[index]
                );

                adjustedNoise *= rockFactor;

                float density = 0f;

                if (adjustedNoise > definition.pastureThreshold)
                    density = 0.5f;

                if (adjustedNoise > definition.denseVegetationThreshold)
                    density = 1f;

                data.Vegetation[index] = density;
            }
        }
    }

    private static void GenerateFlora(
        MeadowData data,
        BiomeDefinition definition,
        System.Random random
    )
    {
        int count = definition.flowerCount;
        int radius = definition.flowerRadius;

        int placed = 0;
        int maxAttempts = count * 20;

        for (int attempt = 0;
             attempt < maxAttempts && placed < count;
             attempt++)
        {
            int x = random.Next(0, data.Resolution);
            int y = random.Next(0, data.Resolution);

            int index = data.GetIndex(x, y);

            if (data.Vegetation[index] < definition.floraVegetationThreshold)
                continue;

            if (data.Water[index] > definition.floraWaterThreshold)
                continue;

            if (data.Rocks[index] > 0f)
                continue;

            int actualRadius = random.Next(
                Mathf.Max(1, radius - 1),
                radius + 2
            );

            DrawCircle(data, data.Flora, x, y, actualRadius);
            placed++;
        }
    }

    private static void GenerateBushes(
        MeadowData data,
        BiomeDefinition definition,
        float offsetX,
        float offsetY,
        System.Random random
    )
    {
        int placed = 0;
        int attempts = definition.bushCount * 30;

        int resolution = data.Resolution;

        for (int attempt = 0;
             attempt < attempts && placed < definition.bushCount;
             attempt++)
        {
            int x = random.Next(0, resolution);
            int y = random.Next(0, resolution);

            int index = data.GetIndex(x, y);

            if (data.Water[index] > definition.floraWaterThreshold)
                continue;

            if (data.Rocks[index] > 0f)
                continue;

            if (data.Vegetation[index] < definition.floraVegetationThreshold)
                continue;

            float u = (float)x / resolution;
            float v = (float)y / resolution;

            float noise = FractalNoise(
                u * definition.bushNoiseScale + offsetX + 2500f,
                v * definition.bushNoiseScale + offsetY + 2500f,
                definition
            );

            if (noise < definition.bushNoiseThreshold)
                continue;

            int radius = random.Next(
                Mathf.Max(1, definition.bushRadius - 1),
                definition.bushRadius + 2
            );

            DrawCircle(data, data.Bushes, x, y, radius);
            placed++;
        }
    }

    private static void GeneratePoints(
        MeadowData data,
        float[] layer,
        int count,
        int radius,
        System.Random random
    )
    {
        for (int i = 0; i < count; i++)
        {
            int x = random.Next(0, data.Resolution);
            int y = random.Next(0, data.Resolution);

            int actualRadius = random.Next(
                Mathf.Max(1, radius - 1),
                radius + 2
            );

            DrawCircle(data, layer, x, y, actualRadius);
        }
    }

    private static void DrawCircle(
        MeadowData data,
        float[] layer,
        int centerX,
        int centerY,
        int radius
    )
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y > radius * radius)
                    continue;

                int px = centerX + x;
                int py = centerY + y;

                if (px < 0 || px >= data.Resolution ||
                    py < 0 || py >= data.Resolution)
                    continue;

                layer[data.GetIndex(px, py)] = 1f;
            }
        }
    }

    private static float FractalNoise(
        float x,
        float y,
        BiomeDefinition definition
    )
    {
        float amplitude = 1f;
        float frequency = 1f;

        float total = 0f;
        float maxValue = 0f;

        for (int i = 0; i < definition.octaves; i++)
        {
            float noise = Mathf.PerlinNoise(
                x * frequency,
                y * frequency
            );

            total += noise * amplitude;
            maxValue += amplitude;

            amplitude *= definition.persistence;
            frequency *= definition.lacunarity;
        }

        return maxValue > 0f ? total / maxValue : 0f;
    }
}
