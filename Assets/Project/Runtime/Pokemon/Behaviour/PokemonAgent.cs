
using UnityEngine;
using System;

public class PokemonAgent : MonoBehaviour
{
    private enum AgentState
    {
        Wandering,
        SeekingFood,
        Eating,
        Sleeping
    }

    public enum ActivityPattern
    {
        Diurnal,
        Nocturnal
    }

    public static event Action<float, string, string, string>
        OnEcosystemEvent;

    [Header("Needs")]
    [SerializeField] private PokemonNeeds needs =
        new PokemonNeeds();

    private PokemonDefinition definition;
    private FruitBushManager fruitBushManager;
    private PokemonSpawnManager spawnManager;

    private AgentState currentState;

    private Vector3 targetPosition;
    private Vector3 worldCenter;

    private float halfWidth;
    private float halfDepth;

    private float foodSearchTimer;
    private float foodCooldownTimer;

    [Header("Food Consumption")]
    [Tooltip("Minimum time between two fruit consumptions, in seconds.")]
    [Min(0f)]
    [SerializeField] private float foodCooldownDuration = 300f;

    private bool initialized;
    private bool isAlive = true;

    // Statistics
    private float totalFoodConsumed;
    private float totalDistanceTravelled;

    private float timeSearching;
    private float timeEating;
    private float timeSleeping;

    private int foodSearchAttempts;
    private int successfulFoodSearches;
    private int failedFoodSearches;
    private int eatingSessions;

    private Vector3 previousPosition;

    private FruitBush targetBush;

    // --------------------------------------------------
    // DAILY ROUTINE
    // --------------------------------------------------

    [Header("Daily Routine")]

    [SerializeField]
    private ActivityPattern activityPattern =
        ActivityPattern.Diurnal;

    [Tooltip("Duration of one complete simulated day, in seconds.")]
    [Min(10f)]
    [SerializeField]
    private float secondsPerDay = 240f;

    [Tooltip("Hour at which daytime activity starts.")]
    [Range(0f, 23f)]
    [SerializeField]
    private float dayStartHour = 6f;

    [Tooltip("Hour at which daytime activity ends.")]
    [Range(0f, 23f)]
    [SerializeField]
    private float nightStartHour = 18f;

    [Tooltip("Random individual variation in activity schedule.")]
    [Range(0f, 3f)]
    [SerializeField]
    private float scheduleVariationHours = 1.5f;

    [Tooltip("A sleeping Pokemon wakes if satiety falls below this value.")]
    [Range(0f, 30f)]
    [SerializeField]
    private float emergencyWakeSatiety = 8f;

    [Tooltip("Hunger drain multiplier while sleeping.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float sleepMetabolismMultiplier = 0.5f;

    private float individualScheduleOffset;

    private float currentHour;

    public float CurrentHour => currentHour;

    public float TimeSleeping => timeSleeping;

    public bool IsSleeping =>
        currentState == AgentState.Sleeping;

    public ActivityPattern CurrentActivityPattern =>
        activityPattern;

    // --------------------------------------------------
    // REPRODUCTION
    // --------------------------------------------------

    private float reproductionPreparationTimer;
    private float reproductionCooldownTimer;
    private float reproductionRetryTimer;

    private bool preparingForReproduction;
    private bool reproductionReadyEventSent;

    private int offspringCount;

    // --------------------------------------------------
    // MORTALITY
    // --------------------------------------------------

    private float timeAtZeroSatiety;
    private bool starvationEventSent;

    // --------------------------------------------------
    // PUBLIC PROPERTIES
    // --------------------------------------------------

    public PokemonDefinition Definition => definition;

    public float Satiety => needs.Satiety;

    public EntityId IndividualID => GetEntityId();

    public string CurrentState => currentState.ToString();

    public float TotalFoodConsumed => totalFoodConsumed;

    public float TotalDistanceTravelled =>
        totalDistanceTravelled;

    public float TimeSearching => timeSearching;

    public float TimeEating => timeEating;

    public int FoodSearchAttempts => foodSearchAttempts;

    public int SuccessfulFoodSearches =>
        successfulFoodSearches;

    public int FailedFoodSearches => failedFoodSearches;

    public int EatingSessions => eatingSessions;

    public int OffspringCount => offspringCount;

    public float ReproductionPreparationTimer =>
        reproductionPreparationTimer;

    public bool IsPreparingForReproduction =>
        preparingForReproduction;

    public float TimeAtZeroSatiety => timeAtZeroSatiety;

    public bool IsAlive => isAlive;

    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    public void Initialize(
        PokemonDefinition pokemonDefinition,
        Vector3 center,
        float width,
        float depth,
        FruitBushManager bushes,
        PokemonSpawnManager spawner
    )
    {
        definition = pokemonDefinition;
        fruitBushManager = bushes;
        spawnManager = spawner;

        worldCenter = center;
        halfWidth = width;
        halfDepth = depth;

        needs.Initialize(definition.initialSatiety);

        currentState = AgentState.Wandering;

        // Statistics
        totalFoodConsumed = 0f;
        totalDistanceTravelled = 0f;

        timeSearching = 0f;
        timeEating = 0f;
        timeSleeping = 0f;

        foodSearchAttempts = 0;
        successfulFoodSearches = 0;
        failedFoodSearches = 0;
        eatingSessions = 0;

        // Daily routine
        individualScheduleOffset = UnityEngine.Random.Range(
            -scheduleVariationHours,
            scheduleVariationHours
        );

        // Reproduction
        reproductionPreparationTimer = 0f;
        reproductionCooldownTimer = 0f;
        reproductionRetryTimer = 0f;

        preparingForReproduction = false;
        reproductionReadyEventSent = false;

        offspringCount = 0;

        // Mortality
        timeAtZeroSatiety = 0f;
        starvationEventSent = false;

        targetBush = null;
        foodSearchTimer = 0f;
        foodCooldownTimer = 0f;

        isAlive = true;
        initialized = true;

        previousPosition = transform.position;

        UpdateSimulationClock();

        ChooseNewDestination();

        EmitEvent("Initialized");

        // Apply the initial day/night state.
        UpdateDailyRoutine();
    }

    // --------------------------------------------------
    // MAIN UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (!initialized || !isAlive)
            return;

        float deltaTime = Time.deltaTime;

        // Food cooldown decreases even while sleeping.
        foodCooldownTimer = Mathf.Max(
            0f,
            foodCooldownTimer - deltaTime
        );

        UpdateSimulationClock();

        UpdateDailyRoutine();

        // Sleeping Pokemon consume less energy.
        float metabolismMultiplier =
            IsSleeping
                ? sleepMetabolismMultiplier
                : 1f;

        needs.UpdateSatiety(
            deltaTime,
            definition.satietyDrainPerSecond *
            metabolismMultiplier
        );

        UpdateMortality(deltaTime);

        if (!isAlive)
            return;

        // Behaviour statistics
        if (currentState == AgentState.SeekingFood)
            timeSearching += deltaTime;

        if (currentState == AgentState.Eating)
            timeEating += deltaTime;

        if (currentState == AgentState.Sleeping)
            timeSleeping += deltaTime;

        UpdateBehaviour();

        UpdateReproduction(deltaTime);

        totalDistanceTravelled += Vector3.Distance(
            previousPosition,
            transform.position
        );

        previousPosition = transform.position;
    }

    // --------------------------------------------------
    // SIMULATION CLOCK
    // --------------------------------------------------

    private void UpdateSimulationClock()
    {
        if (secondsPerDay <= 0f)
            return;

        float elapsedDayFraction =
            (Time.time / secondsPerDay) % 1f;

        currentHour =
            (dayStartHour +
             elapsedDayFraction * 24f +
             individualScheduleOffset + 24f) % 24f;
    }

    private bool IsActivePeriod()
    {
        if (activityPattern == ActivityPattern.Diurnal)
        {
            return IsWithinHours(
                currentHour,
                dayStartHour,
                nightStartHour
            );
        }

        return IsWithinHours(
            currentHour,
            nightStartHour,
            dayStartHour
        );
    }

    private bool IsWithinHours(
        float hour,
        float startHour,
        float endHour
    )
    {
        if (startHour < endHour)
        {
            return hour >= startHour &&
                   hour < endHour;
        }

        return hour >= startHour ||
               hour < endHour;
    }

    // --------------------------------------------------
    // DAILY ROUTINE
    // --------------------------------------------------

    private void UpdateDailyRoutine()
    {
        bool activePeriod = IsActivePeriod();

        if (activePeriod)
        {
            if (currentState == AgentState.Sleeping)
            {
                WakeUp();
            }

            return;
        }

        // Critical hunger can interrupt sleep.
        if (needs.Satiety <= emergencyWakeSatiety)
        {
            if (currentState == AgentState.Sleeping)
            {
                WakeUp();
            }

            return;
        }

        // Start sleeping.
        if (currentState != AgentState.Sleeping)
        {
            targetBush = null;
            foodSearchTimer = 0f;

            ChangeState(AgentState.Sleeping);

            EmitEvent("SleepStarted");
        }
    }

    private void WakeUp()
    {
        ChangeState(AgentState.Wandering);

        EmitEvent("WakeUp");

        ChooseNewDestination();
    }

    // --------------------------------------------------
    // MORTALITY
    // --------------------------------------------------

    private void UpdateMortality(float deltaTime)
    {
        if (!definition.canDie)
            return;

        if (needs.Satiety > 0f)
        {
            timeAtZeroSatiety = 0f;
            starvationEventSent = false;
            return;
        }

        timeAtZeroSatiety += deltaTime;

        if (!starvationEventSent)
        {
            starvationEventSent = true;
            EmitEvent("StarvationStarted");
        }

        if (timeAtZeroSatiety >= definition.starvationDeathTime)
            DieFromStarvation();
    }

    private void DieFromStarvation()
    {
        if (!isAlive)
            return;

        isAlive = false;
        initialized = false;

        preparingForReproduction = false;
        targetBush = null;

        EmitEvent("StarvationDeath");

        Debug.Log(
            $"{definition.pokemonName} died from starvation. " +
            $"Individual ID: {IndividualID}"
        );

        Destroy(gameObject);
    }

    // --------------------------------------------------
    // BEHAVIOUR
    // --------------------------------------------------

    private void UpdateBehaviour()
    {
        // Sleeping Pokemon do not perform other activities.
        if (currentState == AgentState.Sleeping)
            return;

        if (!definition.eatsVegetation ||
            fruitBushManager == null ||
            !fruitBushManager.IsInitialized)
        {
            UpdateWandering();
            return;
        }

        switch (currentState)
        {
            case AgentState.Wandering:

                // Only search for food when the cooldown has expired.
                if (foodCooldownTimer <= 0f &&
                    needs.Satiety <=
                    definition.satietySearchThreshold)
                {
                    ChangeState(AgentState.SeekingFood);
                    foodSearchTimer = 0f;
                    return;
                }

                UpdateWandering();
                break;

            case AgentState.SeekingFood:
                UpdateSeekingFood();
                break;

            case AgentState.Eating:
                UpdateEating();
                break;
        }
    }

    // --------------------------------------------------
    // WANDERING
    // --------------------------------------------------

    private void UpdateWandering()
    {
        MoveTowardsTarget();

        if (Vector3.Distance(
            transform.position,
            targetPosition) < 0.2f)
        {
            ChooseNewDestination();
        }
    }

    // --------------------------------------------------
    // FOOD SEARCH
    // --------------------------------------------------

    private void UpdateSeekingFood()
    {
        if (needs.Satiety >= definition.satietyResumeThreshold)
        {
            targetBush = null;

            ChangeState(AgentState.Wandering);
            ChooseNewDestination();
            return;
        }

        foodSearchTimer -= Time.deltaTime;

        if (foodSearchTimer <= 0f)
        {
            foodSearchTimer = 1f;
            foodSearchAttempts++;

            bool foundFood = fruitBushManager.FindNearestBush(
                transform.position,
                definition.preferredFood,
                out FruitBush nearestBush
            );

            if (foundFood)
            {
                successfulFoodSearches++;

                targetBush = nearestBush;
                targetPosition = nearestBush.transform.position;

                EmitEvent("FoundFruitBush");
            }
            else
            {
                failedFoodSearches++;

                targetBush = null;

                EmitEvent("FailedFoodSearch");

                ChooseNewDestination();
                return;
            }
        }

        if (targetBush == null)
        {
            foodSearchTimer = 0f;
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            targetBush.transform.position
        );

        if (distance <= definition.eatingDistance)
        {
            eatingSessions++;

            ChangeState(AgentState.Eating);
            return;
        }

        targetPosition = targetBush.transform.position;

        MoveTowardsTarget();
    }

    // --------------------------------------------------
    // EATING
    // --------------------------------------------------

    private void UpdateEating()
    {
        if (needs.Satiety >= definition.satietyResumeThreshold)
        {
            targetBush = null;

            ChangeState(AgentState.Wandering);
            ChooseNewDestination();
            return;
        }

        if (targetBush == null)
        {
            ChangeState(AgentState.SeekingFood);
            foodSearchTimer = 0f;
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            targetBush.transform.position
        );

        if (distance > definition.eatingDistance * 1.2f)
        {
            ChangeState(AgentState.SeekingFood);
            return;
        }

        bool consumed = fruitBushManager.ConsumeFruit(
            targetBush,
            definition.preferredFood
        );

        if (!consumed)
        {
            targetBush = null;

            ChangeState(AgentState.SeekingFood);
            foodSearchTimer = 0f;
            return;
        }

        totalFoodConsumed += 1f;

        // One fruit fully restores satiety.
        needs.RestoreSatiety(1f, 100f);

        // Prevent another fruit consumption for the configured interval.
        foodCooldownTimer = foodCooldownDuration;

        EmitEvent("FruitConsumed");

        // One fruit per consumption.
        targetBush = null;

        if (needs.Satiety >= definition.satietyResumeThreshold)
        {
            ChangeState(AgentState.Wandering);
            ChooseNewDestination();
        }
        else
        {
            ChangeState(AgentState.SeekingFood);
            foodSearchTimer = 0f;
        }
    }

    // --------------------------------------------------
    // REPRODUCTION
    // --------------------------------------------------

    private void UpdateReproduction(float deltaTime)
    {
        if (!definition.canReproduce || spawnManager == null)
            return;

        bool isWellFed =
            totalFoodConsumed > 0f &&
            needs.Satiety >=
            definition.reproductionSatietyThreshold;

        if (!isWellFed)
        {
            CancelReproduction();
            return;
        }

        if (reproductionCooldownTimer > 0f)
        {
            reproductionCooldownTimer = Mathf.Max(
                0f,
                reproductionCooldownTimer - deltaTime
            );

            return;
        }

        if (!preparingForReproduction)
        {
            preparingForReproduction = true;
            reproductionPreparationTimer = 0f;
            reproductionReadyEventSent = false;

            EmitEvent("ReproductionStarted");
        }

        reproductionPreparationTimer += deltaTime;

        if (reproductionPreparationTimer <
            definition.reproductionPreparationTime)
        {
            return;
        }

        if (needs.Satiety <
            definition.reproductionSatietyThreshold)
        {
            CancelReproduction();
            return;
        }

        if (!reproductionReadyEventSent)
        {
            reproductionReadyEventSent = true;
            EmitEvent("ReproductionReady");
        }

        reproductionRetryTimer -= deltaTime;

        if (reproductionRetryTimer > 0f)
            return;

        if (needs.Satiety <
            definition.reproductionSatietyThreshold)
        {
            CancelReproduction();
            return;
        }

        bool success = spawnManager.TrySpawnOffspring(
            definition,
            transform.position
        );

        if (success)
        {
            offspringCount++;

            EmitEvent("Birth");

            preparingForReproduction = false;
            reproductionPreparationTimer = 0f;
            reproductionReadyEventSent = false;
            reproductionRetryTimer = 0f;

            reproductionCooldownTimer =
                definition.reproductionCooldown;
        }
        else
        {
            reproductionRetryTimer = 5f;

            EmitEvent("ReproductionBlocked");
        }
    }

    private void CancelReproduction()
    {
        if (!preparingForReproduction)
            return;

        preparingForReproduction = false;
        reproductionPreparationTimer = 0f;
        reproductionReadyEventSent = false;
        reproductionRetryTimer = 0f;

        EmitEvent("ReproductionCancelled");
    }

    // --------------------------------------------------
    // MOVEMENT AND EVENTS
    // --------------------------------------------------

    private void ChangeState(AgentState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        EmitEvent("StateChanged:" + newState);
    }

    private void EmitEvent(string eventName)
    {
        if (definition == null)
            return;

        OnEcosystemEvent?.Invoke(
            Time.time,
            IndividualID.ToString(),
            definition.pokemonName,
            eventName
        );
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction =
            targetPosition - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Vector3 movement =
            direction.normalized *
            definition.moveSpeed *
            Time.deltaTime;

        Vector3 nextPosition =
            transform.position + movement;

        nextPosition.x = Mathf.Clamp(
            nextPosition.x,
            worldCenter.x - halfWidth,
            worldCenter.x + halfWidth
        );

        nextPosition.z = Mathf.Clamp(
            nextPosition.z,
            worldCenter.z - halfDepth,
            worldCenter.z + halfDepth
        );

        transform.position = nextPosition;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            5f * Time.deltaTime
        );
    }

    private void ChooseNewDestination()
    {
        Vector2 randomDirection =
            UnityEngine.Random.insideUnitCircle *
            definition.wanderRadius;

        float targetX = Mathf.Clamp(
            transform.position.x + randomDirection.x,
            worldCenter.x - halfWidth,
            worldCenter.x + halfWidth
        );

        float targetZ = Mathf.Clamp(
            transform.position.z + randomDirection.y,
            worldCenter.z - halfDepth,
            worldCenter.z + halfDepth
        );

        targetPosition = new Vector3(
            targetX,
            transform.position.y,
            targetZ
        );
    }
}
