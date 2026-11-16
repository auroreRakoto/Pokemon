using UnityEngine;
using System.Collections.Generic;

public class FruitBushManager : MonoBehaviour
{
    [Header("World")]
    [SerializeField] private Transform ground;

    [Header("Bush Prefabs")]
    [SerializeField] private FruitBush redBushPrefab;
    [SerializeField] private FruitBush blueBushPrefab;

    [Header("Population")]
    [Min(0)]
    [SerializeField] private int redBushCount = 75;

    [Min(0)]
    [SerializeField] private int blueBushCount = 75;

    [Header("Bush Settings")]
    [Min(1)]
    [SerializeField] private int fruitCapacity = 3;

    [Min(0)]
    [SerializeField] private int initialFruit = 3;

    [Min(0.01f)]
    [SerializeField] private float productionTimePerFruit = 100f;

    [Header("Placement")]
    [Min(0f)]
    [SerializeField] private float groundOffset = 0.05f;

    private readonly List<FruitBush> bushes = new();

    public bool IsInitialized { get; private set; }

    public IReadOnlyList<FruitBush> Bushes => bushes;

    public float TotalAvailableFruit
    {
        get
        {
            float total = 0f;

            foreach (FruitBush bush in bushes)
            {
                if (bush != null)
                    total += bush.CurrentFruit;
            }

            return total;
        }
    }

    public float TotalConsumedFruit
    {
        get
        {
            float total = 0f;

            foreach (FruitBush bush in bushes)
            {
                if (bush != null)
                    total += bush.TotalConsumed;
            }

            return total;
        }
    }

    public float TotalProducedFruit
    {
        get
        {
            float total = 0f;

            foreach (FruitBush bush in bushes)
            {
                if (bush != null)
                    total += bush.TotalProduced;
            }

            return total;
        }
    }

    public int RedBushCount
    {
        get
        {
            int count = 0;

            foreach (FruitBush bush in bushes)
            {
                if (bush != null &&
                    bush.Type == FruitType.Redberry)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public int BlueBushCount
    {
        get
        {
            int count = 0;

            foreach (FruitBush bush in bushes)
            {
                if (bush != null &&
                    bush.Type == FruitType.Blueberry)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public void Initialize(Transform groundTransform)
    {
        if (groundTransform == null)
        {
            Debug.LogError(
                "FruitBushManager: Ground reference is missing."
            );

            return;
        }

        ground = groundTransform;

        ClearBushes();

        if (redBushPrefab == null ||
            blueBushPrefab == null)
        {
            Debug.LogError(
                "FruitBushManager: Bush prefabs are missing."
            );

            return;
        }

        GenerateBushes(
            redBushPrefab,
            FruitType.Redberry,
            redBushCount
        );

        GenerateBushes(
            blueBushPrefab,
            FruitType.Blueberry,
            blueBushCount
        );

        IsInitialized = true;

        Debug.Log(
            $"Fruit bushes initialized: " +
            $"{RedBushCount} Redberry bushes, " +
            $"{BlueBushCount} Blueberry bushes."
        );
    }

    private void GenerateBushes(
        FruitBush prefab,
        FruitType type,
        int count
    )
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 localPosition = new Vector3(
                Random.Range(-5f, 5f),
                0f,
                Random.Range(-5f, 5f)
            );

            Vector3 position =
                ground.TransformPoint(localPosition);

            position.y += groundOffset;

            FruitBush bush = Instantiate(
                prefab,
                position,
                Quaternion.identity,
                transform
            );

            bush.Initialize(
                type,
                fruitCapacity,
                initialFruit,
                productionTimePerFruit
            );

            bush.name = $"{type}_Bush_{i + 1}";

            bushes.Add(bush);
        }
    }

    public bool FindNearestBush(
        Vector3 position,
        FruitType requestedType,
        out FruitBush nearestBush
    )
    {
        nearestBush = null;

        if (!IsInitialized)
            return false;

        float nearestDistance = float.MaxValue;

        foreach (FruitBush bush in bushes)
        {
            if (bush == null ||
                !bush.HasFruit ||
                bush.Type != requestedType)
            {
                continue;
            }

            float distance = (
                bush.transform.position - position
            ).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestBush = bush;
            }
        }

        return nearestBush != null;
    }

    public bool ConsumeFruit(
        FruitBush bush,
        FruitType requestedType
    )
    {
        if (!IsInitialized || bush == null)
            return false;

        return bush.TryConsumeFruit(requestedType);
    }

    private void ClearBushes()
    {
        foreach (FruitBush bush in bushes)
        {
            if (bush != null)
                Destroy(bush.gameObject);
        }

        bushes.Clear();
        IsInitialized = false;
    }
}