
using UnityEngine;
using System.Collections.Generic;

public class GrassVegetationSpawner : MonoBehaviour
{
    [Header("Grass Prefabs")]
    [SerializeField] private GameObject[] grassPrefabs = new GameObject[3];

    [Header("Placement")]
    [Min(0.1f)]
    [SerializeField] private float placementSpacing = 2f;

    [Range(0f, 1f)]
    [SerializeField] private float vegetationDensityMultiplier = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float minimumVegetation = 0.01f;

    [Header("Variation")]
    [SerializeField] private Vector2 randomScale = new Vector2(0.9f, 1.1f);
    [SerializeField] private float groundOffset = 0.02f;

    private Transform grassContainer;
    private int spawnedCount;

    public int SpawnedCount => spawnedCount;

    public void Initialize(
        WorldBiomeMapRenderer mapRenderer,
        Transform ground
    )
    {
        Clear();

        if (mapRenderer == null || ground == null)
        {
            Debug.LogError(
                "GrassVegetationSpawner: missing renderer or ground.",
                this
            );
            return;
        }

        Texture2D vegetationMap = mapRenderer.VegetationTexture;

        if (vegetationMap == null)
        {
            Debug.LogError(
                "GrassVegetationSpawner: vegetation texture is unavailable.",
                this
            );
            return;
        }

        if (!HasValidPrefabs())
        {
            Debug.LogError(
                "GrassVegetationSpawner: no valid grass prefabs assigned.",
                this
            );
            return;
        }

        int resolutionX = vegetationMap.width;
        int resolutionY = vegetationMap.height;

        CreateContainer(ground);

        float worldWidth = 10f * ground.lossyScale.x;
        float worldDepth = 10f * ground.lossyScale.z;

        int stepsX = Mathf.Max(
            1,
            Mathf.CeilToInt(worldWidth / placementSpacing)
        );

        int stepsZ = Mathf.Max(
            1,
            Mathf.CeilToInt(worldDepth / placementSpacing)
        );

        float spacingX = 10f / stepsX;
        float spacingZ = 10f / stepsZ;

        for (int z = 0; z < stepsZ; z++)
        {
            for (int x = 0; x < stepsX; x++)
            {
                float localX = -5f + (x + 0.5f) * spacingX;
                float localZ = -5f + (z + 0.5f) * spacingZ;

                float u = (localX + 5f) / 10f;
                float v = (localZ + 5f) / 10f;

                // Read directly from the dedicated vegetation texture.
                float vegetation = vegetationMap.GetPixelBilinear(u, v).r;

                vegetation = Mathf.Clamp01(
                    vegetation * vegetationDensityMultiplier
                );

                if (vegetation < minimumVegetation)
                    continue;

                float occupancy = GetDeterministicValue(x, z, 0);

                if (occupancy >= vegetation)
                    continue;

                SpawnGrass(x, z, ground, localX, localZ);
            }
        }

        Debug.Log(
            $"Grass generation completed: {spawnedCount} clumps."
        );
    }

    private void SpawnGrass(
        int gridX,
        int gridZ,
        Transform ground,
        float localX,
        float localZ
    )
    {
        GameObject prefab = GetPrefabForPosition(gridX, gridZ);

        if (prefab == null)
            return;

        Vector3 worldPosition = ground.TransformPoint(
            new Vector3(localX, groundOffset, localZ)
        );

        float rotation = GetDeterministicValue(
            gridX,
            gridZ,
            1
        ) * 360f;

        Quaternion worldRotation =
            ground.rotation * Quaternion.Euler(0f, rotation, 0f);

        GameObject grass = Instantiate(
            prefab,
            worldPosition,
            worldRotation
        );

        float scaleVariation = Mathf.Lerp(
            randomScale.x,
            randomScale.y,
            GetDeterministicValue(gridX, gridZ, 2)
        );

        grass.transform.localScale =
            prefab.transform.localScale * scaleVariation;

        grass.transform.SetParent(grassContainer, true);

        grass.name = $"{prefab.name}_{spawnedCount + 1}";

        spawnedCount++;
    }

    private GameObject GetPrefabForPosition(int x, int z)
    {
        List<GameObject> validPrefabs = new List<GameObject>();

        foreach (GameObject prefab in grassPrefabs)
        {
            if (prefab != null)
                validPrefabs.Add(prefab);
        }

        if (validPrefabs.Count == 0)
            return null;

        int index = Mathf.FloorToInt(
            GetDeterministicValue(x, z, 3) * validPrefabs.Count
        );

        index = Mathf.Clamp(
            index,
            0,
            validPrefabs.Count - 1
        );

        return validPrefabs[index];
    }

    private bool HasValidPrefabs()
    {
        if (grassPrefabs == null)
            return false;

        foreach (GameObject prefab in grassPrefabs)
        {
            if (prefab != null)
                return true;
        }

        return false;
    }

    private float GetDeterministicValue(int x, int z, int offset)
    {
        unchecked
        {
            int hash =
                x * 73856093 ^
                z * 19349663 ^
                offset * 83492791;

            hash ^= hash >> 13;
            hash *= 1274126177;
            hash ^= hash >> 16;

            return (hash & 0x7fffffff) / 2147483647f;
        }
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
}
