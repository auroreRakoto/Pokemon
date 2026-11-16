using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPokemon",
    menuName = "Pokemon/Species/Pokemon Definition"
)]
public class PokemonDefinition : ScriptableObject
{
    [Header("Identity")]
    public string pokemonName;
    public GameObject pokemonPrefab;

    [Header("Population")]
    [Min(0)]
    public int spawnCount = 10;

    [Min(1)]
    public int maxPopulation = 100;

    [Header("Habitat Preferences")]
    [Range(0f, 1f)]
    public float minForestWeight = 0f;

    [Range(0f, 1f)]
    public float maxForestWeight = 1f;

    [Range(0f, 1f)]
    public float minVegetation = 0f;

    [Range(0f, 1f)]
    public float maxVegetation = 1f;

    [Range(0f, 1f)]
    public float minHumidity = 0f;

    [Range(0f, 1f)]
    public float maxHumidity = 1f;

    [Range(0f, 1f)]
    public float maxWater = 0.05f;

    [Header("Diet")]
    public bool eatsVegetation = true;

    public FruitType preferredFood =
        FruitType.Redberry;

    [Range(0f, 1f)]
    public float minFoodDensity = 0.15f;

    [Min(0f)]
    public float foodConsumptionRate = 0.08f;

    [Min(0f)]
    public float hungerRecoveryPerFood = 100f;

    [Header("Needs")]
    [Range(0f, 100f)]
    public float initialSatiety = 100f;

    [Min(0f)]
    public float satietyDrainPerSecond = 0.5f;

    [Range(0f, 100f)]
    public float satietySearchThreshold = 35f;

    [Range(0f, 100f)]
    public float satietyResumeThreshold = 75f;

    [Header("Mortality")]
    public bool canDie = true;

    [Min(0f)]
    public float starvationDeathTime = 120f;

    [Header("Movement")]
    [Min(0f)]
    public float moveSpeed = 2f;

    [Min(1f)]
    public float wanderRadius = 8f;

    [Min(1f)]
    public float foodSearchRadius = 12f;

    [Min(0.1f)]
    public float eatingDistance = 1.5f;

    [Header("Reproduction")]
    public bool canReproduce = true;

    [Range(0f, 100f)]
    public float reproductionSatietyThreshold = 85f;

    [Min(0f)]
    public float reproductionPreparationTime = 60f;

    [Min(0f)]
    public float reproductionCooldown = 300f;

    [Min(0f)]
    public float offspringSpawnRadius = 1.5f;
}