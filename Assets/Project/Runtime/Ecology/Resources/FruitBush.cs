
using UnityEngine;

public class FruitBush : MonoBehaviour
{
    [Header("Fruit Type")]
    [SerializeField]
    private FruitType fruitType = FruitType.Blueberry;

    [Header("Fruit Prefabs")]
    [SerializeField]
    private GameObject blueberryPrefab;

    [SerializeField]
    private GameObject redberryPrefab;

    [Header("Fruit Points")]
    [SerializeField]
    private Transform[] fruitPoints = new Transform[3];

    [Header("Production")]
    [Min(1)]
    [SerializeField]
    private int fruitCapacity = 3;

    [Min(0)]
    [SerializeField]
    private int initialFruit = 3;

    [Min(1f)]
    [SerializeField]
    private float productionTimePerFruit = 100f;

    private int currentFruit;
    private float productionTimer;

    private int totalConsumed;
    private int totalProduced;

    private GameObject[] fruitVisuals;

    private bool initialized;

    // --------------------------------------------------
    // PUBLIC PROPERTIES
    // --------------------------------------------------

    public FruitType Type => fruitType;

    public int CurrentFruit => currentFruit;

    public int Capacity => fruitCapacity;

    public bool HasFruit => currentFruit > 0;

    public int TotalConsumed => totalConsumed;

    public int TotalProduced => totalProduced;

    public float ProductionTimer => productionTimer;

    public bool IsInitialized => initialized;

    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    public void Initialize(
        FruitType type,
        int capacity,
        int startingFruit,
        float productionTime
    )
    {
        fruitType = type;

        fruitCapacity = Mathf.Max(1, capacity);

        initialFruit = Mathf.Clamp(
            startingFruit,
            0,
            fruitCapacity
        );

        productionTimePerFruit = Mathf.Max(
            1f,
            productionTime
        );

        currentFruit = initialFruit;
        productionTimer = 0f;

        totalConsumed = 0;
        totalProduced = 0;

        initialized = true;

        CreateFruitVisuals();
        RefreshVisuals();
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (!initialized)
            return;

        if (currentFruit >= fruitCapacity)
        {
            productionTimer = 0f;
            return;
        }

        productionTimer += Time.deltaTime;

        if (productionTimer >= productionTimePerFruit)
        {
            productionTimer -= productionTimePerFruit;

            currentFruit++;
            totalProduced++;

            RefreshVisuals();
        }
    }

    // --------------------------------------------------
    // FRUIT CONSUMPTION
    // --------------------------------------------------

    public bool TryConsumeFruit(FruitType requestedType)
    {
        if (!initialized)
            return false;

        if (requestedType != fruitType)
            return false;

        if (!HasFruit)
            return false;

        currentFruit--;
        totalConsumed++;

        RefreshVisuals();

        return true;
    }

    // --------------------------------------------------
    // VISUAL CREATION
    // --------------------------------------------------

    private void CreateFruitVisuals()
    {
        ClearFruitVisuals();

        GameObject selectedPrefab = GetFruitPrefab();

        if (selectedPrefab == null)
        {
            Debug.LogError(
                $"Missing fruit prefab for {fruitType} " +
                $"on {gameObject.name}!",
                this
            );

            return;
        }

        if (fruitPoints == null ||
            fruitPoints.Length < fruitCapacity)
        {
            Debug.LogError(
                $"FruitBush {gameObject.name} needs at least " +
                $"{fruitCapacity} fruit points.",
                this
            );

            return;
        }

        fruitVisuals = new GameObject[fruitCapacity];

        for (int i = 0; i < fruitCapacity; i++)
        {
            if (fruitPoints[i] == null)
            {
                Debug.LogError(
                    $"FruitPoint {i} is missing on {gameObject.name}!",
                    this
                );

                continue;
            }

            GameObject fruit = Instantiate(
                selectedPrefab,
                fruitPoints[i],
                false
            );

            fruit.name = $"{fruitType}_{i + 1}";
            fruitVisuals[i] = fruit;

            fruit.SetActive(false);
        }
    }

    private GameObject GetFruitPrefab()
    {
        switch (fruitType)
        {
            case FruitType.Blueberry:
                return blueberryPrefab;

            case FruitType.Redberry:
                return redberryPrefab;

            default:
                return null;
        }
    }

    // --------------------------------------------------
    // VISUAL UPDATE
    // --------------------------------------------------

    private void RefreshVisuals()
    {
        if (fruitVisuals == null)
            return;

        for (int i = 0; i < fruitVisuals.Length; i++)
        {
            if (fruitVisuals[i] == null)
                continue;

            fruitVisuals[i].SetActive(i < currentFruit);
        }
    }

    // --------------------------------------------------
    // CLEANUP
    // --------------------------------------------------

    private void ClearFruitVisuals()
    {
        if (fruitVisuals == null)
            return;

        foreach (GameObject fruit in fruitVisuals)
        {
            if (fruit == null)
                continue;

            Destroy(fruit);
        }

        fruitVisuals = null;
    }
}
