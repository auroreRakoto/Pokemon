using Pokemon.Project.World.Spherical.Movement;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Presentation
{
    /// <summary>
    /// Runtime composition root for the dedicated WORLD-001 demo scene.
    /// Rebuilds only objects parented beneath this component's generated root.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SphericalWorldDemoBootstrap : MonoBehaviour
    {
        [Header("Prototype planet settings")]
        [SerializeField] private float planetRadiusMeters = 50000f;
        [SerializeField, Range(2, 256)] private int faceResolution = 256;
        [SerializeField] private int seed = 481516;
        [SerializeField] private float maximumElevationMeters = 350f;
        [SerializeField] private float noiseFrequency = 3.5f;
        [SerializeField, Range(1, 8)] private int octaves = 5;
        [SerializeField] private Material terrainMaterial;

        [Header("Demo controls")]
        [SerializeField] private bool generateOnPlay = true;

        private Transform _generatedRoot;

        private void Start()
        {
            if (generateOnPlay)
                GenerateDemo();
        }

        [ContextMenu("Generate / Rebuild Spherical Demo")]
        public void GenerateDemo()
        {
            ClearDemo();

            SphericalWorldSettings settings = new SphericalWorldSettings
            {
                planetRadiusMeters = planetRadiusMeters,
                faceResolution = faceResolution,
                seed = seed,
                maximumElevationMeters = maximumElevationMeters,
                noiseFrequency = noiseFrequency,
                octaves = octaves
            };
            settings.Validate();

            _generatedRoot = new GameObject("WORLD-001_GeneratedDemo").transform;
            _generatedRoot.SetParent(transform, false);

            GameObject planetObject = new GameObject("SphericalPlanet");
            planetObject.transform.SetParent(_generatedRoot, false);
            SphericalPlanetPresenter presenter = planetObject.AddComponent<SphericalPlanetPresenter>();
            if (terrainMaterial != null)
                presenter.SetTerrainMaterial(terrainMaterial);
            presenter.Build(settings);

            CreateLighting();
            CreatePlayerAndCamera(settings);

            Debug.Log($"WORLD-001 demo generated. Seed={seed}, radius={planetRadiusMeters:N0} m, face resolution={faceResolution}.", this);
        }

        [ContextMenu("Clear Spherical Demo")]
        public void ClearDemo()
        {
            if (_generatedRoot != null)
            {
                DestroyOwnedObject(_generatedRoot.gameObject);
                _generatedRoot = null;
            }
        }

        private void CreatePlayerAndCamera(SphericalWorldSettings settings)
        {
            Vector3 spawnDirection = Vector3.up;
            Vector3 surfacePosition = SphericalSurfaceQuery.GetSurfacePosition(spawnDirection, settings);

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "DemoPlayer_Capsule";
            player.transform.SetParent(_generatedRoot, false);
            player.transform.position = surfacePosition + spawnDirection * 0.9f;
            player.transform.rotation = Quaternion.LookRotation(Vector3.forward, spawnDirection);
            player.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            Collider primitiveCollider = player.GetComponent<Collider>();
            if (primitiveCollider != null)
                DestroyOwnedObject(primitiveCollider);

            GameObject cameraObject = new GameObject("SphericalThirdPersonCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(_generatedRoot, false);
            Camera cameraComponent = cameraObject.AddComponent<Camera>();
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.48f, 0.67f, 0.82f, 1f);
            cameraComponent.nearClipPlane = 0.1f;
            cameraComponent.farClipPlane = 250000f;
            cameraObject.AddComponent<AudioListener>();
            SphericalThirdPersonCamera cameraRig = cameraObject.AddComponent<SphericalThirdPersonCamera>();
            cameraRig.Initialize(player.transform, cameraComponent);

            SphericalWorldDemoOverlay overlay = cameraObject.AddComponent<SphericalWorldDemoOverlay>();
            overlay.Initialize(cameraRig, settings.seed, settings.planetRadiusMeters);
        }

        private void CreateLighting()
        {
            GameObject lightObject = new GameObject("DemoDirectionalLight");
            lightObject.transform.SetParent(_generatedRoot, false);
            lightObject.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
        }

        private void OnDestroy()
        {
            ClearDemo();
        }

        private static void DestroyOwnedObject(UnityEngine.Object ownedObject)
        {
            if (ownedObject == null)
                return;
            if (Application.isPlaying)
                Destroy(ownedObject);
            else
                DestroyImmediate(ownedObject);
        }
    }
}
