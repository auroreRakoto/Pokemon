
using UnityEngine;
using System;

public class GrassVegetationSpawner : MonoBehaviour
{
    [Header("Grass Prefabs")]
    [SerializeField] private GameObject[] grassPrefabs = new GameObject[3];

    [Header("Generation")]
    [Min(1)]
    [SerializeField] private int candidateCount = 4500;

    [Min(0)]
    [SerializeField] private int maxGrassCount = 900;

    [Range(0f, 1f)]
    [SerializeField] private float spawnProbability = 0.30f;

    [Range(0f, 1f)]
    [SerializeField] private float minimumVegetation = 0.20f;

    [Range(0f, 1f)]
    [SerializeField] private float maximumWater = 0.01f;

    [Range(0f, 1f)]
    [SerializeField] private float maximumRockDensity = 0.15f;

    [Header("Forest Influence")]
    [SerializeField] private float meadowDensityMultiplier = 0.9f;
    [SerializeField] private float forestDensityMultiplier = 1.1f;

    [Header("Variation")]
    [SerializeField] private Vector2 randomScale = new Vector2(0.85f, 1.15f);
    [SerializeField] private float groundOffset = 0.02f;

    [Header("Random Seed")]
    [SerializeField] private int seed = 42;

    private Transform grassContainer;
    private int spawnedCount;

    public int SpawnedCount => spawnedCount;

    public void Initialize(WorldEcologyData data, Transform ground)
    {
        Clear();

        if (data == null || ground == null)
        {
            Debug.LogError(
                "GrassVegetationSpawner: missing world data or ground.",
                this
            );
            return;
        }

        if (grassPrefabs == null || grassPrefabs.Length == 0)
        {
            Debug.LogError(
                "GrassVegetationSpawner: no grass prefabs assigned.",
                this
            );
            return;
        }

        bool hasPrefab = false;

        foreach (GameObject prefab in grassPrefabs)
        {
            if (prefab != null)
            {
                hasPrefab = true;
                break;
            }
        }

        if (!hasPrefab)
        {
            Debug.LogError(
                "GrassVegetationSpawner: all grass prefab slots are empty.",
                this
            );
            return;
        }

        CreateContainer(ground);

        System.Random random = new System.Random(seed);

        int resolution = data.Resolution;

        for (int attempt = 0;
             attempt < candidateCount &&
             spawnedCount < maxGrassCount;
             attempt++)
        {
            float localX = RandomRange(random, -5f, 5f);
            float localZ = RandomRange(random, -5f, 5f);

            int x = Mathf.Clamp(
                Mathf.FloorToInt((localX + 5f) / 10f * resolution),
                0,
                resolution - 1
            );

            int y = Mathf.Clamp(
                Mathf.FloorToInt((localZ + 5f) / 10f * resolution),
                0,
                resolution - 1
            );

            int index = data.GetIndex(x, y);

            if (!IsSuitableLocation(data, index))
                continue;

            float vegetation = data.Vegetation[index];
            float forestWeight = data.ForestWeight[index];

            float biomeMultiplier = Mathf.Lerp(
                meadowDensityMultiplier,
                forestDensityMultiplier,
                forestWeight
            );

            float probability = Mathf.Clamp01(
                vegetation * spawnProbability * biomeMultiplier
            );

            if (random.NextDouble() > probability)
                continue;

            SpawnGrass(random, localX, localZ);
        }

        Debug.Log(
            $"Grass generation completed: {spawnedCount} clumps."
        );
    }

    private bool IsSuitableLocation(
        WorldEcologyData data,
        int index
    )
    {
        return
            data.Vegetation[index] >= minimumVegetation &&
            data.Water[index] <= maximumWater &&
            data.Rocks[index] <= maximumRockDensity;
    }

    private void SpawnGrass(
        System.Random random,
        float localX,
        float localZ
    )
    {
        GameObject prefab = GetRandomPrefab(random);

        if (prefab == null)
            return;

        GameObject grass = Instantiate(
            prefab,
            grassContainer
        );

        grass.transform.localPosition = new Vector3(
            localX,
            groundOffset,
            localZ
        );

        grass.transform.localRotation = Quaternion.Euler(
            0f,
            RandomRange(random, 0f, 360f),
            0f
        );

        float scale = RandomRange(
            random,
            randomScale.x,
            randomScale.y
        );

        grass.transform.localScale =
            prefab.transform.localScale * scale;

        grass.name = $"{prefab.name}_{spawnedCount + 1}";

        spawnedCount++;
    }

    private GameObject GetRandomPrefab(System.Random random)
    {
        int validCount = 0;

        foreach (GameObject prefab in grassPrefabs)
        {
            if (prefab != null)
                validCount++;
        }

        if (validCount == 0)
            return null;

        int selectedIndex = random.Next(0, validCount);

        foreach (GameObject prefab in grassPrefabs)
        {
            if (prefab == null)
                continue;

            if (selectedIndex == 0)
                return prefab;

            selectedIndex--;
        }

        return null;
    }

    private void CreateContainer(Transform ground)
    {
        GameObject container = new GameObject("Generated_Grass");

        grassContainer = container.transform;
        grassContainer.SetParent(ground, false);

        grassContainer.localPosition = Vector3.zero;
        grassContainer.localRotation = Quaternion.identity;
        grassContainer.localScale = Vector3.one;
    }

    public void Clear()
    {
        if (grassContainer != null)
        {
            Destroy(grassContainer.gameObject);
            grassContainer = null;
        }

        spawnedCount = 0;
    }

    private float RandomRange(
        System.Random random,
        float min,
        float max
    )
    {
        return min + (float)random.NextDouble() * (max - min);
    }
}
