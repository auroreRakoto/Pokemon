
using System;
using System.Collections.Generic;

[Serializable]
public class SpeciesSnapshot
{
    public string speciesName;

    public int population;

    public float averageSatiety;
    public float minSatiety;
    public float maxSatiety;

    public int starvingCount;

    public int wanderingCount;
    public int seekingFoodCount;
    public int eatingCount;
    public int sleepingCount;

    public int foodSearchAttempts;
    public int successfulFoodSearches;
    public int failedFoodSearches;
    public int eatingSessions;

    public float totalFoodConsumed;
    public float totalDistanceTravelled;

    public float timeSearching;
    public float timeEating;
    public float timeSleeping;
}

[Serializable]
public class IndividualSnapshot
{
    public float simulationTime;

    public string individualID;
    public string speciesName;

    public float positionX;
    public float positionZ;

    public float satiety;
    public string state;

    public float totalFoodConsumed;
    public float totalDistanceTravelled;
}

[Serializable]
public class FruitTypeSnapshot
{
    public string fruitType;

    public int bushCount;

    public int availableFruit;
    public int totalCapacity;

    public int consumedFruit;
    public int regeneratedFruit;

    public float availabilityPercent;
}

[Serializable]
public class EcosystemSnapshot
{
    public float simulationTime;

    public int totalPopulation;
    public float averageSatiety;

    public int availableFood;
    public int totalFoodCapacity;
    public int consumedFood;
    public int regeneratedFood;

    public List<SpeciesSnapshot> species =
        new List<SpeciesSnapshot>();

    public List<IndividualSnapshot> individuals =
        new List<IndividualSnapshot>();

    public List<FruitTypeSnapshot> fruitTypes =
        new List<FruitTypeSnapshot>();
}
