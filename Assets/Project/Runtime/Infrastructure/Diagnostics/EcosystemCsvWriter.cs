using System;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>Owns CSV file creation and serialization for ecosystem snapshots.</summary>
internal sealed class EcosystemCsvWriter
{
    private readonly string worldFile;
    private readonly string populationFile;
    private readonly string resourceFile;
    private readonly string fruitBushFile;
    private readonly string behaviorFile;
    private readonly string individualFile;
    private readonly string eventsFile;

    /// <summary>Creates the session directory and writes all CSV headers.</summary>
    public EcosystemCsvWriter(string sessionDirectory)
    {
        Directory.CreateDirectory(sessionDirectory);

        worldFile = Path.Combine(sessionDirectory, "world_statistics.csv");
        populationFile = Path.Combine(sessionDirectory, "population_statistics.csv");
        resourceFile = Path.Combine(sessionDirectory, "resource_statistics.csv");
        fruitBushFile = Path.Combine(sessionDirectory, "fruit_bush_statistics.csv");
        behaviorFile = Path.Combine(sessionDirectory, "behavior_statistics.csv");
        individualFile = Path.Combine(sessionDirectory, "individual_statistics.csv");
        eventsFile = Path.Combine(sessionDirectory, "ecosystem_events.csv");

        string[,] headers =
        {
            { worldFile, "SimulationTime,TotalPopulation,AverageSatiety\n" },
            { populationFile, "SimulationTime,Species,Population," +
                "AverageSatiety,MinSatiety,MaxSatiety,StarvingCount\n" },
            { resourceFile, "SimulationTime,AvailableFruit,TotalCapacity," +
                "ConsumedFruit,RegeneratedFruit,FoodAvailabilityPercent\n" },
            { fruitBushFile, "SimulationTime,FruitType,BushCount," +
                "AvailableFruit,TotalCapacity,ConsumedFruit,RegeneratedFruit," +
                "AvailabilityPercent\n" },
            { behaviorFile, "SimulationTime,Species,WanderingCount," +
                "SeekingFoodCount,EatingCount,SleepingCount,FoodSearchAttempts," +
                "SuccessfulFoodSearches,FailedFoodSearches,EatingSessions," +
                "TotalFoodConsumed,TotalDistanceTravelled,TimeSearching," +
                "TimeEating,TimeSleeping\n" },
            { individualFile, "SimulationTime,IndividualID,Species," +
                "PositionX,PositionZ,Satiety,State,TotalFoodConsumed," +
                "TotalDistanceTravelled\n" },
            { eventsFile, "SimulationTime,IndividualID,Species,Event\n" }
        };

        for (int index = 0; index < headers.GetLength(0); index++)
            File.WriteAllText(headers[index, 0], headers[index, 1]);
    }

    /// <summary>Writes aggregate and individual sections selected by the caller.</summary>
    public void WriteSnapshot(
        EcosystemSnapshot snapshot,
        bool includeAggregates,
        bool includeIndividuals
    )
    {
        if (includeAggregates)
        {
            WriteWorldStatistics(snapshot);
            WritePopulationStatistics(snapshot);
            WriteResourceStatistics(snapshot);
            WriteFruitBushStatistics(snapshot);
            WriteBehaviorStatistics(snapshot);
        }

        if (includeIndividuals)
            WriteIndividualStatistics(snapshot);
    }

    /// <summary>Appends one event row to the session event file.</summary>
    public void WriteEvent(
        float elapsedTime,
        string individualID,
        string speciesName,
        string eventName
    )
    {
        string line =
            FormatCsvValue(elapsedTime) + "," +
            individualID + "," +
            FormatCsvValue(speciesName) + "," +
            FormatCsvValue(eventName) + "\n";

        File.AppendAllText(eventsFile, line);
    }

    /// <summary>Appends the global population and satiety row.</summary>
    private void WriteWorldStatistics(EcosystemSnapshot snapshot)
    {
        string line =
            FormatCsvValue(snapshot.simulationTime) + "," +
            FormatCsvValue(snapshot.totalPopulation) + "," +
            FormatCsvValue(snapshot.averageSatiety) + "\n";

        File.AppendAllText(worldFile, line);
    }

    /// <summary>Serializes per-species population and behaviour counters.</summary>
    private void WritePopulationStatistics(EcosystemSnapshot snapshot)
    {
        StringBuilder builder = new StringBuilder();

        foreach (SpeciesSnapshot species in snapshot.species)
        {
            builder.Append(FormatCsvValue(snapshot.simulationTime)).Append(",");
            builder.Append(FormatCsvValue(species.speciesName)).Append(",");
            builder.Append(FormatCsvValue(species.population)).Append(",");
            builder.Append(FormatCsvValue(species.averageSatiety)).Append(",");
            builder.Append(FormatCsvValue(species.minSatiety)).Append(",");
            builder.Append(FormatCsvValue(species.maxSatiety)).Append(",");
            builder.Append(FormatCsvValue(species.starvingCount));
            builder.AppendLine();
        }

        File.AppendAllText(populationFile, builder.ToString());
    }

    /// <summary>Serializes total available, consumed and regenerated food.</summary>
    private void WriteResourceStatistics(EcosystemSnapshot snapshot)
    {
        float availabilityPercent = snapshot.totalFoodCapacity > 0
            ? (float)snapshot.availableFood / snapshot.totalFoodCapacity * 100f
            : 0f;

        string line =
            FormatCsvValue(snapshot.simulationTime) + "," +
            FormatCsvValue(snapshot.availableFood) + "," +
            FormatCsvValue(snapshot.totalFoodCapacity) + "," +
            FormatCsvValue(snapshot.consumedFood) + "," +
            FormatCsvValue(snapshot.regeneratedFood) + "," +
            FormatCsvValue(availabilityPercent) + "\n";

        File.AppendAllText(resourceFile, line);
    }

    /// <summary>Serializes per-fruit-type bush counts and availability.</summary>
    private void WriteFruitBushStatistics(EcosystemSnapshot snapshot)
    {
        StringBuilder builder = new StringBuilder();

        foreach (FruitTypeSnapshot fruit in snapshot.fruitTypes)
        {
            builder.Append(FormatCsvValue(snapshot.simulationTime)).Append(",");
            builder.Append(FormatCsvValue(fruit.fruitType)).Append(",");
            builder.Append(FormatCsvValue(fruit.bushCount)).Append(",");
            builder.Append(FormatCsvValue(fruit.availableFruit)).Append(",");
            builder.Append(FormatCsvValue(fruit.totalCapacity)).Append(",");
            builder.Append(FormatCsvValue(fruit.consumedFruit)).Append(",");
            builder.Append(FormatCsvValue(fruit.regeneratedFruit)).Append(",");
            builder.Append(FormatCsvValue(fruit.availabilityPercent));
            builder.AppendLine();
        }

        File.AppendAllText(fruitBushFile, builder.ToString());
    }

    /// <summary>Serializes the per-species behaviour and movement counters.</summary>
    private void WriteBehaviorStatistics(EcosystemSnapshot snapshot)
    {
        StringBuilder builder = new StringBuilder();

        foreach (SpeciesSnapshot species in snapshot.species)
        {
            builder.Append(FormatCsvValue(snapshot.simulationTime)).Append(",");
            builder.Append(FormatCsvValue(species.speciesName)).Append(",");
            builder.Append(FormatCsvValue(species.wanderingCount)).Append(",");
            builder.Append(FormatCsvValue(species.seekingFoodCount)).Append(",");
            builder.Append(FormatCsvValue(species.eatingCount)).Append(",");
            builder.Append(FormatCsvValue(species.sleepingCount)).Append(",");
            builder.Append(FormatCsvValue(species.foodSearchAttempts)).Append(",");
            builder.Append(FormatCsvValue(species.successfulFoodSearches)).Append(",");
            builder.Append(FormatCsvValue(species.failedFoodSearches)).Append(",");
            builder.Append(FormatCsvValue(species.eatingSessions)).Append(",");
            builder.Append(FormatCsvValue(species.totalFoodConsumed)).Append(",");
            builder.Append(FormatCsvValue(species.totalDistanceTravelled)).Append(",");
            builder.Append(FormatCsvValue(species.timeSearching)).Append(",");
            builder.Append(FormatCsvValue(species.timeEating)).Append(",");
            builder.Append(FormatCsvValue(species.timeSleeping));
            builder.AppendLine();
        }

        File.AppendAllText(behaviorFile, builder.ToString());
    }

    /// <summary>Serializes the collected state of each individual Pokémon.</summary>
    private void WriteIndividualStatistics(EcosystemSnapshot snapshot)
    {
        StringBuilder builder = new StringBuilder();

        foreach (IndividualSnapshot individual in snapshot.individuals)
        {
            builder.Append(FormatCsvValue(individual.simulationTime)).Append(",");
            builder.Append(individual.individualID).Append(",");
            builder.Append(FormatCsvValue(individual.speciesName)).Append(",");
            builder.Append(FormatCsvValue(individual.positionX)).Append(",");
            builder.Append(FormatCsvValue(individual.positionZ)).Append(",");
            builder.Append(FormatCsvValue(individual.satiety)).Append(",");
            builder.Append(FormatCsvValue(individual.state)).Append(",");
            builder.Append(FormatCsvValue(individual.totalFoodConsumed)).Append(",");
            builder.Append(FormatCsvValue(individual.totalDistanceTravelled));
            builder.AppendLine();
        }

        File.AppendAllText(individualFile, builder.ToString());
    }

    /// <summary>Formats invariant numeric values and safely quotes CSV text values.</summary>
    private static string FormatCsvValue(object value)
    {
        if (value == null)
            return string.Empty;

        if (value is float floatValue)
            return floatValue.ToString("F4", CultureInfo.InvariantCulture);

        if (value is string textValue)
        {
            if (string.IsNullOrEmpty(textValue))
                return string.Empty;

            return "\"" + textValue.Replace("\"", "\"\"") + "\"";
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
