
using UnityEngine;
using System.Collections.Generic;

public class EcosystemDataCollector : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private FruitBushManager fruitBushManager;

    [SerializeField]
    private PokemonDefinition[] trackedSpecies;

    [Header("Statistics")]
    [Min(0f)]
    [SerializeField]
    private float starvingSatietyThreshold = 10f;

    public bool IsReady =>
        fruitBushManager != null &&
        fruitBushManager.IsInitialized;

    public void Initialize(FruitBushManager bushes)
    {
        fruitBushManager = bushes;
    }

    // --------------------------------------------------
    // MAIN COLLECTION
    // --------------------------------------------------

    public EcosystemSnapshot Collect(float simulationTime)
    {
        EcosystemSnapshot snapshot =
            new EcosystemSnapshot();

        snapshot.simulationTime = simulationTime;

        if (!IsReady)
            return snapshot;

        CollectPokemonStatistics(snapshot, simulationTime);

        CollectFruitStatistics(snapshot);

        return snapshot;
    }

    // --------------------------------------------------
    // POKEMON STATISTICS
    // --------------------------------------------------

    private void CollectPokemonStatistics(
        EcosystemSnapshot snapshot,
        float simulationTime
    )
    {
        PokemonAgent[] agents =
            FindObjectsByType<PokemonAgent>(
                FindObjectsSortMode.None
            );

        Dictionary<PokemonDefinition, List<PokemonAgent>>
            populations =
            new Dictionary<PokemonDefinition, List<PokemonAgent>>();

        if (trackedSpecies != null)
        {
            foreach (PokemonDefinition definition in trackedSpecies)
            {
                if (definition != null &&
                    !populations.ContainsKey(definition))
                {
                    populations.Add(
                        definition,
                        new List<PokemonAgent>()
                    );
                }
            }
        }

        float totalSatiety = 0f;
        int validPopulation = 0;

        foreach (PokemonAgent agent in agents)
        {
            if (agent == null ||
                !agent.IsAlive ||
                agent.Definition == null)
            {
                continue;
            }

            validPopulation++;

            totalSatiety += agent.Satiety;

            PokemonDefinition definition = agent.Definition;

            if (!populations.ContainsKey(definition))
            {
                populations.Add(
                    definition,
                    new List<PokemonAgent>()
                );
            }

            populations[definition].Add(agent);

            IndividualSnapshot individual =
                new IndividualSnapshot();

            individual.simulationTime = simulationTime;

            individual.individualID =
                agent.IndividualID.ToString();

            individual.speciesName =
                definition.pokemonName;

            individual.positionX =
                agent.transform.position.x;

            individual.positionZ =
                agent.transform.position.z;

            individual.satiety = agent.Satiety;

            individual.state = agent.CurrentState;

            individual.totalFoodConsumed =
                agent.TotalFoodConsumed;

            individual.totalDistanceTravelled =
                agent.TotalDistanceTravelled;

            snapshot.individuals.Add(individual);
        }

        snapshot.totalPopulation = validPopulation;

        snapshot.averageSatiety = validPopulation > 0
            ? totalSatiety / validPopulation
            : 0f;

        foreach (var entry in populations)
        {
            CollectSpeciesStatistics(
                entry.Key,
                entry.Value,
                snapshot
            );
        }
    }

    private void CollectSpeciesStatistics(
        PokemonDefinition definition,
        List<PokemonAgent> agents,
        EcosystemSnapshot snapshot
    )
    {
        SpeciesSnapshot species =
            new SpeciesSnapshot();

        species.speciesName = definition.pokemonName;

        species.population = agents.Count;

        species.minSatiety = 100f;
        species.maxSatiety = 0f;

        float satietyTotal = 0f;

        foreach (PokemonAgent agent in agents)
        {
            float satiety = agent.Satiety;

            satietyTotal += satiety;

            species.minSatiety =
                Mathf.Min(species.minSatiety, satiety);

            species.maxSatiety =
                Mathf.Max(species.maxSatiety, satiety);

            if (satiety <= starvingSatietyThreshold)
                species.starvingCount++;

            switch (agent.CurrentState)
            {
                case "Wandering":
                    species.wanderingCount++;
                    break;

                case "SeekingFood":
                    species.seekingFoodCount++;
                    break;

                case "Eating":
                    species.eatingCount++;
                    break;

                case "Sleeping":
                    species.sleepingCount++;
                    break;
            }

            species.foodSearchAttempts +=
                agent.FoodSearchAttempts;

            species.successfulFoodSearches +=
                agent.SuccessfulFoodSearches;

            species.failedFoodSearches +=
                agent.FailedFoodSearches;

            species.eatingSessions +=
                agent.EatingSessions;

            species.totalFoodConsumed +=
                agent.TotalFoodConsumed;

            species.totalDistanceTravelled +=
                agent.TotalDistanceTravelled;

            species.timeSearching +=
                agent.TimeSearching;

            species.timeEating +=
                agent.TimeEating;

            species.timeSleeping +=
                agent.TimeSleeping;
        }

        species.averageSatiety = agents.Count > 0
            ? satietyTotal / agents.Count
            : 0f;

        if (agents.Count == 0)
            species.minSatiety = 0f;

        snapshot.species.Add(species);
    }

    // --------------------------------------------------
    // FRUIT STATISTICS
    // --------------------------------------------------

    private void CollectFruitStatistics(
        EcosystemSnapshot snapshot
    )
    {
        Dictionary<FruitType, FruitTypeSnapshot> statistics =
            new Dictionary<FruitType, FruitTypeSnapshot>();

        foreach (FruitType type in
                 System.Enum.GetValues(typeof(FruitType)))
        {
            FruitTypeSnapshot fruit =
                new FruitTypeSnapshot();

            fruit.fruitType = type.ToString();

            statistics.Add(type, fruit);
        }

        foreach (FruitBush bush in fruitBushManager.Bushes)
        {
            if (bush == null)
                continue;

            FruitTypeSnapshot fruit;

            if (!statistics.TryGetValue(
                bush.Type,
                out fruit))
            {
                continue;
            }

            fruit.bushCount++;

            fruit.availableFruit += bush.CurrentFruit;

            fruit.totalCapacity += bush.Capacity;

            fruit.consumedFruit += bush.TotalConsumed;

            fruit.regeneratedFruit += bush.TotalProduced;
        }

        foreach (FruitTypeSnapshot fruit in statistics.Values)
        {
            fruit.availabilityPercent =
                fruit.totalCapacity > 0
                    ? (float)fruit.availableFruit /
                      fruit.totalCapacity * 100f
                    : 0f;

            snapshot.fruitTypes.Add(fruit);

            snapshot.availableFood += fruit.availableFruit;

            snapshot.totalFoodCapacity += fruit.totalCapacity;

            snapshot.consumedFood += fruit.consumedFruit;

            snapshot.regeneratedFood += fruit.regeneratedFruit;
        }
    }
}
