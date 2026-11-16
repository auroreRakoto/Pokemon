
using UnityEngine;

[RequireComponent(typeof(WorldBiomeMapRenderer))]
public class WorldBiomeGenerator : MonoBehaviour
{
    [Header("World Configuration")]
    [SerializeField] private WorldDefinition worldDefinition;

    private WorldBiomeMapRenderer mapRenderer;

    public WorldEcologyData GeneratedData { get; private set; }

    private void Awake()
    {
        mapRenderer = GetComponent<WorldBiomeMapRenderer>();
    }

    [ContextMenu("Generate World")]
    public void Generate()
    {
        GeneratedData = null;

        if (worldDefinition == null ||
            worldDefinition.meadowBiome == null ||
            worldDefinition.forestBiome == null)
        {
            Debug.LogError("World or biome definitions are missing!");
            return;
        }

        mapRenderer = GetComponent<WorldBiomeMapRenderer>();

        int resolution = Mathf.Max(32, worldDefinition.resolution);

        WorldEcologyData worldData = new WorldEcologyData(resolution);

        System.Random random = new System.Random(worldDefinition.seed);

        float offsetX = (float)random.NextDouble() * 10000f;
        float offsetY = (float)random.NextDouble() * 10000f;

        float lowerThreshold = Mathf.Clamp01(
            worldDefinition.forestThreshold - worldDefinition.transitionWidth
        );

        float upperThreshold = Mathf.Clamp01(
            worldDefinition.forestThreshold + worldDefinition.transitionWidth
        );

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (float)x / resolution;
                float v = (float)y / resolution;

                float noise = Mathf.PerlinNoise(
                    u * worldDefinition.biomeNoiseScale + offsetX,
                    v * worldDefinition.biomeNoiseScale + offsetY
                );

                float weight = Mathf.InverseLerp(
                    lowerThreshold,
                    upperThreshold,
                    noise
                );

                weight = Mathf.SmoothStep(0f, 1f, weight);

                worldData.ForestWeight[worldData.GetIndex(x, y)] = weight;
            }
        }

        MeadowData meadowData = BiomeEcologyGenerator.Generate(
            worldDefinition.meadowBiome,
            resolution
        );

        MeadowData forestData = BiomeEcologyGenerator.Generate(
            worldDefinition.forestBiome,
            resolution
        );

        for (int i = 0; i < resolution * resolution; i++)
        {
            float forestWeight = worldData.ForestWeight[i];

            worldData.Vegetation[i] = Mathf.Lerp(
                meadowData.Vegetation[i],
                forestData.Vegetation[i],
                forestWeight
            );

            worldData.Flora[i] = Mathf.Lerp(
                meadowData.Flora[i],
                forestData.Flora[i],
                forestWeight
            );

            worldData.Rocks[i] = Mathf.Lerp(
                meadowData.Rocks[i],
                forestData.Rocks[i],
                forestWeight
            );

            worldData.Bushes[i] = Mathf.Lerp(
                meadowData.Bushes[i],
                forestData.Bushes[i],
                forestWeight
            );

            worldData.Water[i] = Mathf.Lerp(
                meadowData.Water[i],
                forestData.Water[i],
                forestWeight
            );

            worldData.Humidity[i] = Mathf.Lerp(
                meadowData.Humidity[i],
                forestData.Humidity[i],
                forestWeight
            );
        }

        GeneratedData = worldData;

        mapRenderer.Render(worldData, worldDefinition);

        Debug.Log($"World ecology generated: {worldDefinition.worldName}");
    }
}
