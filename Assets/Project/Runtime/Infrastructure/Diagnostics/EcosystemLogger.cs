using System;
using System.IO;
using UnityEngine;

/// <summary>Coordinates periodic ecosystem snapshots and session event logging.</summary>
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
    private EcosystemCsvWriter csvWriter;

    /// <summary>Subscribes to ecosystem events while this component is enabled.</summary>
    private void OnEnable()
    {
        PokemonAgent.OnEcosystemEvent += HandleEcosystemEvent;
    }

    /// <summary>Stops receiving ecosystem events when this component is disabled.</summary>
    private void OnDisable()
    {
        PokemonAgent.OnEcosystemEvent -= HandleEcosystemEvent;
    }

    /// <summary>Creates the CSV session before enabling periodic collection.</summary>
    private void Start()
    {
        if (dataCollector == null)
        {
            Debug.LogError("EcosystemLogger: DataCollector is missing!");
            return;
        }

        if (!CreateSession())
            return;

        sessionStartTime = Time.time;
        initialized = true;
    }

    /// <summary>Advances collection timers and records due ecosystem snapshots.</summary>
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

    /// <summary>Creates a timestamped directory and initializes its CSV files.</summary>
    private bool CreateSession()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        sessionDirectory = Path.Combine(
            Application.dataPath,
            "Project",
            "Data",
            "Simulation",
            $"Simulation_{timestamp}"
        );

        try
        {
            csvWriter = new EcosystemCsvWriter(sessionDirectory);
        }
        catch (IOException exception)
        {
            Debug.LogError(
                $"EcosystemLogger: unable to create simulation files in " +
                $"{sessionDirectory}. {exception.Message}"
            );
            return false;
        }
        catch (UnauthorizedAccessException exception)
        {
            Debug.LogError(
                $"EcosystemLogger: access denied while creating simulation " +
                $"files in {sessionDirectory}. {exception.Message}"
            );
            return false;
        }

        Debug.Log($"Ecosystem logging session created: {sessionDirectory}");
        return true;
    }

    /// <summary>Collects and writes aggregate statistics, optionally including individuals.</summary>
    private void RecordSnapshot(bool includeIndividuals)
    {
        EcosystemSnapshot snapshot = dataCollector.Collect(simulationTime);
        csvWriter.WriteSnapshot(snapshot, true, includeIndividuals);
    }

    /// <summary>Collects and writes only the individual statistics for the current interval.</summary>
    private void RecordIndividualSnapshot()
    {
        EcosystemSnapshot snapshot = dataCollector.Collect(simulationTime);
        csvWriter.WriteSnapshot(snapshot, false, true);
    }

    /// <summary>Writes an ecosystem event using time elapsed since the session began.</summary>
    private void HandleEcosystemEvent(
        float eventTime,
        string individualID,
        string speciesName,
        string eventName
    )
    {
        if (!initialized || csvWriter == null)
            return;

        float elapsedTime = eventTime - sessionStartTime;
        csvWriter.WriteEvent(elapsedTime, individualID, speciesName, eventName);
    }
}
