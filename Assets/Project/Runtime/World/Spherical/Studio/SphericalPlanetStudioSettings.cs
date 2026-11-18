using UnityEngine;

namespace Pokemon.Project.World.Spherical.Studio
{
    public enum SphericalPlanetStudioView
    {
        Spherical,
        Planisphere
    }

    public enum SphericalPlanetStudioPreviewStage
    {
        BaseSphere,
        Layer1,
        Layer2,
        Layer3,
        Final
    }

    public enum SphericalPlanetStudioMap
    {
        Terrain,
        Altitude,
        Temperature,
        Moisture,
        Water,
        Biomes,
        Villages,
        Slope
    }

    public enum SphericalDrainagePreviewMode
    {
        RiverCandidates,
        FlowAccumulation,
        FlowDirection
    }

    public enum SphericalDrainageInputMode
    {
        TerrainHeightfield,
        FlatBaseSphereDiagnostic
    }

    [CreateAssetMenu(menuName = "Pokemon/World/Spherical Planet Studio Settings", fileName = "SphericalPlanetStudioSettings")]
    public sealed class SphericalPlanetStudioSettings : ScriptableObject
    {
        [Header("Planet")]
        [Min(100f)] public float radiusMeters = 50000f;
        [Min(2)] public int faceResolution = 96;
        [Min(32)] public int planisphereWidth = 512;
        [Min(16)] public int planisphereHeight = 256;
        public int seed = 481516;

        [Header("Terrain layer visibility")]
        public bool layer1Enabled;
        public bool layer2Enabled;
        public bool layer3Enabled;

        [Header("Layer 1 — macro relief")]
        [Min(0f)] public float layer1AmplitudeMeters = 1800f;
        [Min(0.01f)] public float layer1Frequency = 2.2f;
        [Range(0f, 1f)] public float continentThreshold = 0.47f;

        [Header("Layer 2 — regional relief")]
        [Min(0f)] public float layer2AmplitudeMeters = 450f;
        [Min(0.01f)] public float layer2Frequency = 9f;

        [Header("Layer 3 — fine details")]
        [Min(0f)] public float layer3AmplitudeMeters = 65f;
        [Min(0.01f)] public float layer3Frequency = 34f;

        [Header("Preview")]
        public SphericalPlanetStudioPreviewStage previewStage = SphericalPlanetStudioPreviewStage.Final;
        public SphericalPlanetStudioView view = SphericalPlanetStudioView.Spherical;
        public SphericalPlanetStudioMap map = SphericalPlanetStudioMap.Terrain;
        [Min(0f)] public float temperatureLatitudeInfluence = 0.75f;
        [Min(0f)] public float temperatureNoiseInfluence = 0.25f;

        [Header("Drainage preview (experimental)")]
        public SphericalDrainageInputMode drainageInputMode = SphericalDrainageInputMode.TerrainHeightfield;
        [Range(8, 512)] public int drainageResolution = 512;
        public float drainageSeaLevelMeters = 0f;
        [Min(1f)] public float riverAccumulationThreshold = 100f;
        public SphericalDrainagePreviewMode drainagePreviewMode = SphericalDrainagePreviewMode.RiverCandidates;
        [Range(1, 64)] public int flowDirectionStride = 12;

        public void Validate()
        {
            radiusMeters = Mathf.Max(100f, radiusMeters);
            faceResolution = Mathf.Clamp(faceResolution, 2, 256);
            planisphereWidth = Mathf.Clamp(planisphereWidth, 32, 2048);
            planisphereHeight = Mathf.Clamp(planisphereHeight, 16, 1024);
            drainageResolution = Mathf.Clamp(drainageResolution, 8, 512);
            riverAccumulationThreshold = Mathf.Max(1f, riverAccumulationThreshold);
            flowDirectionStride = Mathf.Clamp(flowDirectionStride, 1, 64);
            layer1AmplitudeMeters = Mathf.Max(0f, layer1AmplitudeMeters);
            layer2AmplitudeMeters = Mathf.Max(0f, layer2AmplitudeMeters);
            layer3AmplitudeMeters = Mathf.Max(0f, layer3AmplitudeMeters);
        }
    }
}
