
using UnityEngine;
using System;
using System.IO;
using System.Globalization;
using System.Text;

public class EcosystemLogger : MonoBehaviour
{
    private bool initialSnapshotRecorded;

    [Header("References")]
    [SerializeField]
    private EcosystemDataCollector dataCollector;

    [Header("Logging Settings")]
    [Min(0.1f)]
    [SerializeField]
    private float collectionInterval = 5f;

    [Min(0.1f)]
    [SerializeField]
    private float individualCollectionInterval = 10f;

    [SerializeField]
    private bool logOnStart = true;

    private float simulationTime;
    private float collectionTimer;
    private float individualTimer;

    private float sessionStartTime;

    private bool initialized;

    private string sessionDirectory;

    private string worldFile;
    private string populationFile;
    private string resourceFile;
    private string fruitBushFile;
    private string behaviorFile;
    private string individualFile;
    private string eventsFile;

    // --------------------------------------------------
    // EVENTS
    // --------------------------------------------------

    private void OnEnable()
    {
        PokemonAgent.OnEcosystemEvent += HandleEcosystemEvent;
    }

    private void OnDisable()
    {
        PokemonAgent.OnEcosystemEvent -= HandleEcosystemEvent;
    }

    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    private void Start()
    {
        if (dataCollector == null)
        {
            Debug.LogError(
                "EcosystemLogger: DataCollector is missing!"
            );

            return;
        }

        CreateSession();

        sessionStartTime = Time.time;

        initialized = true;
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (!initialized || !dataCollector.IsReady)
            return;

        float deltaTime = Time.deltaTime;

        simulationTime += deltaTime;

        collectionTimer += deltaTime;

        individualTimer += deltaTime;

        if (logOnStart && !initialSnapshotRecorded)
        {
            RecordSnapshot(true);

            initialSnapshotRecorded = true;
        }

        if (collectionTimer >= collectionInterval)
        {
            collectionTimer %= collectionInterval;

            RecordSnapshot(false);
        }

        if (individualTimer >= individualCollectionInterval)
        {
            individualTimer %= individualCollectionInterval;

            RecordIndividualSnapshot();
        }
    }

    // --------------------------------------------------
    // SESSION CREATION
    // --------------------------------------------------

    private void CreateSession()
    {
        string timestamp =
            DateTime.Now.ToString("yyyyMMdd_HHmmss");

        sessionDirectory = Path.Combine(
            Application.persistentDataPath,
            "SimulationLogs",
            $"Simulation_{timestamp}"
        );

        Directory.CreateDirectory(sessionDirectory);

        worldFile = Path.Combine(
            sessionDirectory,
            "world_statistics.csv"
        );

        populationFile = Path.Combine(
            sessionDirectory,
            "population_statistics.csv"
        );

        resourceFile = Path.Combine(
            sessionDirectory,
            "resource_statistics.csv"
        );

        fruitBushFile = Path.Combine(
            sessionDirectory,
            "fruit_bush_statistics.csv"
        );

        behaviorFile = Path.Combine(
            sessionDirectory,
            "behavior_statistics.csv"
        );

        individualFile = Path.Combine(
            sessionDirectory,
            "individual_statistics.csv"
        );

        eventsFile = Path.Combine(
            sessionDirectory,
            "ecosystem_events.csv"
        );

        File.WriteAllText(
            worldFile,
            "SimulationTime,TotalPopulation,AverageSatiety\n"
        );

        File.WriteAllText(
            populationFile,
            "SimulationTime,Species,Population," +
            "AverageSatiety,MinSatiety,MaxSatiety," +
            "StarvingCount\n"
        );

        File.WriteAllText(
            resourceFile,
            "SimulationTime,AvailableFruit,TotalCapacity," +
            "ConsumedFruit,RegeneratedFruit," +
            "FoodAvailabilityPercent\n"
        );

        File.WriteAllText(
            fruitBushFile,
            "SimulationTime,FruitType,BushCount," +
            "AvailableFruit,TotalCapacity," +
            "ConsumedFruit,RegeneratedFruit," +
            "AvailabilityPercent\n"
        );

        File.WriteAllText(
            behaviorFile,
            "SimulationTime,Species,WanderingCount," +
            "SeekingFoodCount,EatingCount,SleepingCount," +
            "FoodSearchAttempts,SuccessfulFoodSearches," +
            "FailedFoodSearches,EatingSessions," +
            "TotalFoodConsumed,TotalDistanceTravelled," +
            "TimeSearching,TimeEating,TimeSleeping\n"
        );

        File.WriteAllText(
            individualFile,
            "SimulationTime,IndividualID,Species," +
            "PositionX,PositionZ,Satiety,State," +
            "TotalFoodConsumed,TotalDistanceTravelled\n"
        );

        File.WriteAllText(
            eventsFile,
            "SimulationTime,IndividualID,Species,Event\n"
        );

        Debug.Log(
            $"Ecosystem logging session created: {sessionDirectory}"
        );
    }

    // --------------------------------------------------
    // SNAPSHOTS
    // --------------------------------------------------

    private void RecordSnapshot(bool includeIndividuals)
    {
        EcosystemSnapshot snapshot =
            dataCollector.Collect(simulationTime);

        WriteWorldStatistics(snapshot);

        WritePopulationStatistics(snapshot);

        WriteResourceStatistics(snapshot);

        WriteFruitBushStatistics(snapshot);

        WriteBehaviorStatistics(snapshot);

        if (includeIndividuals)
            WriteIndividualStatistics(snapshot);
    }

    private void RecordIndividualSnapshot()
    {
        EcosystemSnapshot snapshot =
            dataCollector.Collect(simulationTime);

        WriteIndividualStatistics(snapshot);
    }

    // --------------------------------------------------
    // WORLD STATISTICS
    // --------------------------------------------------

    private void WriteWorldStatistics(
        EcosystemSnapshot snapshot
    )
    {
        string line =
            Format(snapshot.simulationTime) + "," +
            snapshot.totalPopulation + "," +
            Format(snapshot.averageSatiety) + "\n";

        File.AppendAllText(worldFile, line);
    }

    // --------------------------------------------------
    // POPULATION STATISTICS
    // --------------------------------------------------

    private void WritePopulationStatistics(
        EcosystemSnapshot snapshot
    )
    {
        StringBuilder builder = new StringBuilder();

        foreach (SpeciesSnapshot species in snapshot.species)
        {
            builder.Append(Format(snapshot.simulationTime));
            builder.Append(",");

            builder.Append(EscapeCsv(species.speciesName));
            builder.Append(",");

            builder.Append(species.population);
            builder.Append(",");

            builder.Append(Format(species.averageSatiety));
            builder.Append(",");

            builder.Append(Format(species.minSatiety));
            builder.Append(",");

            builder.Append(Format(species.maxSatiety));
            builder.Append(",");

            builder.Append(species.starvingCount);

            builder.AppendLine();
        }

        File.AppendAllText(
            populationFile,
            builder.ToString()
        );
    }

    // --------------------------------------------------
    // GLOBAL RESOURCE STATISTICS
    // --------------------------------------------------

    private void WriteResourceStatistics(
        EcosystemSnapshot snapshot
    )
    {
        float availabilityPercent =
            snapshot.totalFoodCapacity > 0
                ? (float)snapshot.availableFood /
                  snapshot.totalFoodCapacity * 100f
                : 0f;

        string line =
            Format(snapshot.simulationTime) + "," +
            snapshot.availableFood + "," +
            snapshot.totalFoodCapacity + "," +
            snapshot.consumedFood + "," +
            snapshot.regeneratedFood + "," +
            Format(availabilityPercent) + "\n";

        File.AppendAllText(resourceFile, line);
    }

    // --------------------------------------------------
    // FRUIT BUSH STATISTICS
    // --------------------------------------------------

    private void WriteFruitBushStatistics(
        EcosystemSnapshot snapshot
    )
    {
        StringBuilder builder = new StringBuilder();

        foreach (FruitTypeSnapshot fruit in snapshot.fruitTypes)
        {
            builder.Append(Format(snapshot.simulationTime));
            builder.Append(",");

            builder.Append(EscapeCsv(fruit.fruitType));
            builder.Append(",");

            builder.Append(fruit.bushCount);
            builder.Append(",");

            builder.Append(fruit.availableFruit);
            builder.Append(",");

            builder.Append(fruit.totalCapacity);
            builder.Append(",");

            builder.Append(fruit.consumedFruit);
            builder.Append(",");

            builder.Append(fruit.regeneratedFruit);
            builder.Append(",");

            builder.Append(Format(fruit.availabilityPercent));

            builder.AppendLine();
        }

        File.AppendAllText(
            fruitBushFile,
            builder.ToString()
        );
    }

    // --------------------------------------------------
    // BEHAVIOR STATISTICS
    // --------------------------------------------------

    private void WriteBehaviorStatistics(
        EcosystemSnapshot snapshot
    )
    {
        StringBuilder builder = new StringBuilder();

        foreach (SpeciesSnapshot species in snapshot.species)
        {
            builder.Append(Format(snapshot.simulationTime));
            builder.Append(",");

            builder.Append(EscapeCsv(species.speciesName));
            builder.Append(",");

            builder.Append(species.wanderingCount);
            builder.Append(",");

            builder.Append(species.seekingFoodCount);
            builder.Append(",");

            builder.Append(species.eatingCount);
            builder.Append(",");

            builder.Append(species.sleepingCount);
            builder.Append(",");

            builder.Append(species.foodSearchAttempts);
            builder.Append(",");

            builder.Append(species.successfulFoodSearches);
            builder.Append(",");

            builder.Append(species.failedFoodSearches);
            builder.Append(",");

            builder.Append(species.eatingSessions);
            builder.Append(",");

            builder.Append(Format(species.totalFoodConsumed));
            builder.Append(",");

            builder.Append(Format(species.totalDistanceTravelled));
            builder.Append(",");

            builder.Append(Format(species.timeSearching));
            builder.Append(",");

            builder.Append(Format(species.timeEating));
            builder.Append(",");

            builder.Append(Format(species.timeSleeping));

            builder.AppendLine();
        }

        File.AppendAllText(
            behaviorFile,
            builder.ToString()
        );
    }

    // --------------------------------------------------
    // INDIVIDUAL STATISTICS
    // --------------------------------------------------

    private void WriteIndividualStatistics(
        EcosystemSnapshot snapshot
    )
    {
        StringBuilder builder = new StringBuilder();

        foreach (IndividualSnapshot individual in snapshot.individuals)
        {
            builder.Append(Format(individual.simulationTime));
            builder.Append(",");

            builder.Append(individual.individualID);
            builder.Append(",");

            builder.Append(EscapeCsv(individual.speciesName));
            builder.Append(",");

            builder.Append(Format(individual.positionX));
            builder.Append(",");

            builder.Append(Format(individual.positionZ));
            builder.Append(",");

            builder.Append(Format(individual.satiety));
            builder.Append(",");

            builder.Append(EscapeCsv(individual.state));
            builder.Append(",");

            builder.Append(Format(individual.totalFoodConsumed));
            builder.Append(",");

            builder.Append(Format(individual.totalDistanceTravelled));

            builder.AppendLine();
        }

        File.AppendAllText(
            individualFile,
            builder.ToString()
        );
    }

    // --------------------------------------------------
    // ECOSYSTEM EVENTS
    // --------------------------------------------------

    private void HandleEcosystemEvent(
        float eventTime,
        string individualID,
        string speciesName,
        string eventName
    )
    {
        if (!initialized || string.IsNullOrEmpty(eventsFile))
            return;

        float elapsedTime = eventTime - sessionStartTime;

        string line =
            Format(elapsedTime) + "," +
            individualID + "," +
            EscapeCsv(speciesName) + "," +
            EscapeCsv(eventName) + "\n";

        File.AppendAllText(eventsFile, line);
    }

    // --------------------------------------------------
    // CSV UTILITIES
    // --------------------------------------------------

    private string Format(float value)
    {
        return value.ToString(
            "F4",
            CultureInfo.InvariantCulture
        );
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
