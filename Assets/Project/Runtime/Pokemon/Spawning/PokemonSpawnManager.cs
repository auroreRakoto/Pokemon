
using UnityEngine;
using System.Collections.Generic;

public class PokemonSpawnManager : MonoBehaviour
{
    [Header("World")]
    [SerializeField] private WorldBiomeGenerator worldGenerator;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private FruitBushManager fruitBushManager;
    [SerializeField] private GrassVegetationSpawner grassSpawner;
    [SerializeField] private Transform ground;

    [Header("Pokemon")]
    [SerializeField] private PokemonDefinition[] pokemonDefinitions;

    private readonly List<GameObject> spawnedPokemon = new();

    private Vector3 worldCenter;
    private float halfWidth;
    private float halfDepth;

    private void Start()
    {
        GeneratePopulation();
    }

    [ContextMenu("Generate Pokemon Population")]
    public void GeneratePopulation()
    {
        if (worldGenerator == null ||
            resourceManager == null ||
            fruitBushManager == null ||
            grassSpawner == null ||
            ground == null)
        {
            Debug.LogError(
                "Spawn Manager references are missing!"
            );

            return;
        }

        ClearPopulation();

        grassSpawner.Clear();

        worldGenerator.Generate();

        WorldEcologyData worldData =
            worldGenerator.GeneratedData;

        if (worldData == null)
        {
            Debug.LogError(
                "World ecology data is unavailable!"
            );

            return;
        }

        resourceManager.Initialize(worldData, ground);

        fruitBushManager.Initialize(ground);

        if (!fruitBushManager.IsInitialized)
        {
            Debug.LogError(
                "FruitBushManager initialization failed."
            );

            return;
        }

        WorldBiomeMapRenderer mapRenderer =
            worldGenerator.GetComponent<WorldBiomeMapRenderer>();

        grassSpawner.Initialize(mapRenderer, ground);

        worldCenter = ground.position;

        halfWidth = 5f * ground.lossyScale.x;
        halfDepth = 5f * ground.lossyScale.z;

        foreach (PokemonDefinition definition in pokemonDefinitions)
        {
            if (definition == null)
                continue;

            SpawnSpecies(definition);
        }

        Debug.Log(
            $"Pokemon population generated: {spawnedPokemon.Count}"
        );
    }

    private void SpawnSpecies(PokemonDefinition definition)
    {
        if (definition.pokemonPrefab == null)
        {
            Debug.LogError(
                $"No prefab assigned for {definition.pokemonName}!"
            );

            return;
        }

        int spawned = 0;

        int maxAttempts = definition.spawnCount * 100;

        int resolution =
            worldGenerator.GeneratedData.Resolution;

        for (int attempt = 0;
             attempt < maxAttempts &&
             spawned < definition.spawnCount;
             attempt++)
        {
            int x = Random.Range(0, resolution);
            int y = Random.Range(0, resolution);

            int index =
                worldGenerator.GeneratedData.GetIndex(x, y);

            if (!IsSuitableLocation(
                definition,
                worldGenerator.GeneratedData,
                index))
            {
                continue;
            }

            Vector3 localPosition = new Vector3(
                ((x + 0.5f) / resolution - 0.5f) * 10f,
                0f,
                ((y + 0.5f) / resolution - 0.5f) * 10f
            );

            Vector3 spawnPosition =
                ground.TransformPoint(localPosition);

            spawnPosition.y += 0.7f;

            if (CreatePokemon(
                definition,
                spawnPosition,
                spawned + 1))
            {
                spawned++;
            }
        }

        Debug.Log(
            $"{definition.pokemonName}: " +
            $"{spawned}/{definition.spawnCount} spawned"
        );
    }

    private bool IsSuitableLocation(
        PokemonDefinition definition,
        WorldEcologyData data,
        int index
    )
    {
        return
            data.ForestWeight[index] >= definition.minForestWeight &&
            data.ForestWeight[index] <= definition.maxForestWeight &&

            data.Vegetation[index] >= definition.minVegetation &&
            data.Vegetation[index] <= definition.maxVegetation &&

            data.Humidity[index] >= definition.minHumidity &&
            data.Humidity[index] <= definition.maxHumidity &&

            data.Water[index] <= definition.maxWater;
    }

    private bool CreatePokemon(
        PokemonDefinition definition,
        Vector3 position,
        int individualNumber
    )
    {
        GameObject pokemon = Instantiate(
            definition.pokemonPrefab,
            position,
            Quaternion.identity
        );

        PokemonAgent agent =
            pokemon.GetComponent<PokemonAgent>();

        if (agent == null)
        {
            Debug.LogError(
                $"PokemonAgent missing on {definition.pokemonName} prefab!"
            );

            Destroy(pokemon);
            return false;
        }

        agent.Initialize(
            definition,
            worldCenter,
            halfWidth,
            halfDepth,
            fruitBushManager,
            this
        );

        pokemon.name =
            $"{definition.pokemonName}_{individualNumber}";

        spawnedPokemon.Add(pokemon);

        return true;
    }

    public bool TrySpawnOffspring(
        PokemonDefinition definition,
        Vector3 parentPosition
    )
    {
        if (definition == null ||
            definition.pokemonPrefab == null)
        {
            return false;
        }

        spawnedPokemon.RemoveAll(
            pokemon => pokemon == null
        );

        int currentPopulation = 0;

        foreach (GameObject pokemon in spawnedPokemon)
        {
            PokemonAgent agent =
                pokemon.GetComponent<PokemonAgent>();

            if (agent != null &&
                agent.Definition == definition)
            {
                currentPopulation++;
            }
        }

        if (currentPopulation >= definition.maxPopulation)
            return false;

        Vector2 offset =
            Random.insideUnitCircle *
            definition.offspringSpawnRadius;

        Vector3 spawnPosition = parentPosition +
            new Vector3(offset.x, 0f, offset.y);

        spawnPosition.x = Mathf.Clamp(
            spawnPosition.x,
            worldCenter.x - halfWidth,
            worldCenter.x + halfWidth
        );

        spawnPosition.z = Mathf.Clamp(
            spawnPosition.z,
            worldCenter.z - halfDepth,
            worldCenter.z + halfDepth
        );

        return CreatePokemon(
            definition,
            spawnPosition,
            currentPopulation + 1
        );
    }

    private void ClearPopulation()
    {
        foreach (GameObject pokemon in spawnedPokemon)
        {
            if (pokemon != null)
                Destroy(pokemon);
        }

        spawnedPokemon.Clear();
    }
}
