
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("Regeneration")]
    [Min(0f)]
    [SerializeField] private float vegetationRegrowthPerSecond = 0.001f;

    [Min(0.1f)]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("Statistics")]
    [Min(0f)]
    [SerializeField] private float foodCellThreshold = 0.01f;

    private WorldEcologyData worldData;
    private Transform ground;

    private float[] availableFood;
    private float[] initialFood;

    private float updateTimer;

    private float totalConsumed;
    private float totalRegenerated;

    // --------------------------------------------------
    // CONFIGURATION ACCESSORS
    // --------------------------------------------------

    public float VegetationRegrowthPerSecond =>
        vegetationRegrowthPerSecond;

    public float UpdateInterval => updateInterval;

    public float FoodCellThreshold => foodCellThreshold;

    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    public bool IsInitialized => worldData != null;

    public int Resolution =>
        worldData != null ? worldData.Resolution : 0;

    // --------------------------------------------------
    // RESOURCE STATISTICS
    // --------------------------------------------------

    public float TotalAvailableFood
    {
        get
        {
            float total = 0f;

            if (availableFood == null)
                return 0f;

            foreach (float food in availableFood)
                total += food;

            return total;
        }
    }

    public float TotalInitialFood
    {
        get
        {
            float total = 0f;

            if (initialFood == null)
                return 0f;

            foreach (float food in initialFood)
                total += food;

            return total;
        }
    }

    public float TotalConsumed => totalConsumed;

    public float TotalRegenerated => totalRegenerated;

    public int TotalFoodCells =>
        availableFood != null ? availableFood.Length : 0;

    public int EmptyFoodCells
    {
        get
        {
            if (availableFood == null)
                return 0;

            int count = 0;

            foreach (float food in availableFood)
            {
                if (food <= foodCellThreshold)
                    count++;
            }

            return count;
        }
    }

    public int FoodCellsAboveThreshold
    {
        get
        {
            if (availableFood == null)
                return 0;

            int count = 0;

            foreach (float food in availableFood)
            {
                if (food > foodCellThreshold)
                    count++;
            }

            return count;
        }
    }

    public float AverageFoodDensity
    {
        get
        {
            if (availableFood == null ||
                availableFood.Length == 0)
                return 0f;

            return TotalAvailableFood / availableFood.Length;
        }
    }

    // --------------------------------------------------
    // MINIMAP ACCESSORS
    // --------------------------------------------------

    public float GetAvailableFoodAtIndex(int index)
    {
        if (availableFood == null ||
            index < 0 ||
            index >= availableFood.Length)
            return 0f;

        return availableFood[index];
    }

    public float GetInitialFoodAtIndex(int index)
    {
        if (initialFood == null ||
            index < 0 ||
            index >= initialFood.Length)
            return 0f;

        return initialFood[index];
    }

    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    public void Initialize(
        WorldEcologyData data,
        Transform groundTransform
    )
    {
        if (data == null || groundTransform == null)
        {
            Debug.LogError(
                "ResourceManager: missing world data or ground."
            );
            return;
        }

        worldData = data;
        ground = groundTransform;

        int size = data.Resolution * data.Resolution;

        availableFood = new float[size];
        initialFood = new float[size];

        totalConsumed = 0f;
        totalRegenerated = 0f;
        updateTimer = 0f;

        for (int i = 0; i < size; i++)
        {
            initialFood[i] = data.Vegetation[i];
            availableFood[i] = initialFood[i];
        }

        Debug.Log("ResourceManager initialized.");
    }

    // --------------------------------------------------
    // REGENERATION
    // --------------------------------------------------

    private void Update()
    {
        if (!IsInitialized)
            return;

        updateTimer += Time.deltaTime;

        if (updateTimer < updateInterval)
            return;

        float deltaTime = updateTimer;
        updateTimer = 0f;

        RegenerateFood(deltaTime);
    }

    private void RegenerateFood(float deltaTime)
    {
        for (int i = 0; i < availableFood.Length; i++)
        {
            float previousAmount = availableFood[i];

            availableFood[i] = Mathf.MoveTowards(
                availableFood[i],
                initialFood[i],
                vegetationRegrowthPerSecond * deltaTime
            );

            totalRegenerated +=
                availableFood[i] - previousAmount;
        }
    }

    // --------------------------------------------------
    // FOOD SEARCH
    // --------------------------------------------------

    public bool FindNearestFood(
        Vector3 worldPosition,
        float searchRadius,
        float minimumFood,
        out Vector3 foodPosition
    )
    {
        foodPosition = worldPosition;

        if (!IsInitialized || searchRadius <= 0f)
            return false;

        Vector3 localPosition =
            ground.InverseTransformPoint(worldPosition);

        int resolution = worldData.Resolution;

        float u = (localPosition.x + 5f) / 10f;
        float v = (localPosition.z + 5f) / 10f;

        int centerX = Mathf.Clamp(
            Mathf.FloorToInt(u * resolution),
            0,
            resolution - 1
        );

        int centerY = Mathf.Clamp(
            Mathf.FloorToInt(v * resolution),
            0,
            resolution - 1
        );

        float cellSize =
            10f * ground.lossyScale.x / resolution;

        int cellRadius = Mathf.CeilToInt(
            searchRadius / Mathf.Max(0.001f, cellSize)
        );

        float bestScore = float.MinValue;
        bool found = false;

        for (int y = centerY - cellRadius;
             y <= centerY + cellRadius;
             y++)
        {
            for (int x = centerX - cellRadius;
                 x <= centerX + cellRadius;
                 x++)
            {
                if (x < 0 || x >= resolution ||
                    y < 0 || y >= resolution)
                    continue;

                int index = worldData.GetIndex(x, y);

                float food = availableFood[index];

                if (food < minimumFood)
                    continue;

                Vector3 candidate =
                    GetCellWorldPosition(x, y);

                float distance = Vector3.Distance(
                    worldPosition,
                    candidate
                );

                if (distance > searchRadius)
                    continue;

                float score =
                    food - distance / searchRadius * 0.2f;

                if (score > bestScore)
                {
                    bestScore = score;
                    foodPosition = candidate;
                    found = true;
                }
            }
        }

        return found;
    }

    // --------------------------------------------------
    // FOOD CONSUMPTION
    // --------------------------------------------------

    public float ConsumeFood(
        Vector3 worldPosition,
        float requestedAmount
    )
    {
        if (!IsInitialized || requestedAmount <= 0f)
            return 0f;

        int index = GetWorldIndex(worldPosition);

        if (index < 0)
            return 0f;

        float consumed = Mathf.Min(
            availableFood[index],
            requestedAmount
        );

        availableFood[index] -= consumed;

        totalConsumed += consumed;

        return consumed;
    }

    public float GetAvailableFood(Vector3 worldPosition)
    {
        if (!IsInitialized)
            return 0f;

        int index = GetWorldIndex(worldPosition);

        return index >= 0 ? availableFood[index] : 0f;
    }

    // --------------------------------------------------
    // WORLD COORDINATES
    // --------------------------------------------------

    private int GetWorldIndex(Vector3 worldPosition)
    {
        Vector3 local =
            ground.InverseTransformPoint(worldPosition);

        float u = (local.x + 5f) / 10f;
        float v = (local.z + 5f) / 10f;

        if (u < 0f || u > 1f ||
            v < 0f || v > 1f)
            return -1;

        int resolution = worldData.Resolution;

        int x = Mathf.Clamp(
            Mathf.FloorToInt(u * resolution),
            0,
            resolution - 1
        );

        int y = Mathf.Clamp(
            Mathf.FloorToInt(v * resolution),
            0,
            resolution - 1
        );

        return worldData.GetIndex(x, y);
    }

    private Vector3 GetCellWorldPosition(int x, int y)
    {
        float u = (x + 0.5f) / worldData.Resolution;
        float v = (y + 0.5f) / worldData.Resolution;

        Vector3 localPosition = new Vector3(
            u * 10f - 5f,
            0f,
            v * 10f - 5f
        );

        return ground.TransformPoint(localPosition);
    }
}
