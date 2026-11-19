using System;
using Pokemon.Project.World.Spherical.Geography;
using Pokemon.Project.World.Spherical.Movement;
using Pokemon.Project.World.Spherical.Presentation;
using Pokemon.Project.World.Spherical.Studio;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Inspection
{
    /// <summary>
    /// Runtime composition root for the temporary playable inspection of the exact
    /// WORLD-002 geographic fields generated from the Planet Studio settings asset.
    /// This is a visualisation/locomotion fixture, not the production world bootstrap.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SphericalBiomeInspectionBootstrap : MonoBehaviour
    {
        [SerializeField] private SphericalPlanetStudioSettings settings;
        [SerializeField, Min(0.1f)] private float capsuleHeightMeters = 1.8f;

        [Header("Spawn")]
        [SerializeField] private SphericalBiome spawnBiome = SphericalBiome.Plains;
        [SerializeField, Range(0f, 45f)] private float preferredMaximumSpawnSlope = 18f;

        [Header("Forest Decoration")]
        [Tooltip("Assign the imported SM_Tree_Deciduous and SM_Tree_Birch FBX model assets here.")]
        [SerializeField] private GameObject[] forestTreePrefabs = new GameObject[2];
        [SerializeField, Min(0)] private int forestTreeCount = 140;
        [SerializeField, Min(0.1f)] private float forestTreeMinimumSpacingMeters = 8f;
        [SerializeField, Range(0f, 60f)] private float forestTreeMaximumSlopeDegrees = 32f;
        [SerializeField, Min(0f)] private float forestTreeSpawnExclusionRadiusMeters = 14f;
        [SerializeField] private Vector2 forestTreeScaleRange = new Vector2(0.85f, 1.2f);

        [SerializeField] private Material biomeMaterial;
        [SerializeField] private SphericalBiomeMaterialMapping biomeMaterialMapping;
        [SerializeField] private bool generateOnPlay = true;

        private Transform _generatedRoot;
        private SphericalGeographicFieldSet _fields;
        private Material _ownedMaterial;
        private Mesh[] _generatedMeshes;

        private void Start()
        {
            DisableOverlappingPlanetStudioPreview();
            if (generateOnPlay)
                GenerateInspectionWorld();
        }

        /// <summary>
        /// The legacy Planet Studio preview is a separate full-size sphere. If it remains
        /// active in the inspection scene it can cover the playable textured planet.
        /// Disable only that known preview root for this Play Mode session.
        /// Unity restores the scene object's authored active state when Play Mode stops.
        /// </summary>
        private void DisableOverlappingPlanetStudioPreview()
        {
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == gameObject || roots[i].name != "WORLD-001_PlanetStudioPreview")
                    continue;

                if (roots[i].activeSelf)
                {
                    roots[i].SetActive(false);
                    Debug.Log(
                        "[WORLD-010] Disabled WORLD-001_PlanetStudioPreview during playable inspection " +
                        "to prevent it from covering the textured WORLD-002 planet.", this);
                }
                return;
            }
        }

        [ContextMenu("Generate / Refresh Playable Biome Inspection")]
        public void GenerateInspectionWorld()
        {
            ClearInspectionWorld();
            if (settings == null)
                throw new InvalidOperationException("Planet Studio settings are not assigned to the biome inspection scene.");

            settings.Validate();
            if (!settings.layer1Enabled && !settings.layer2Enabled && !settings.layer3Enabled)
                throw new InvalidOperationException("Enable at least the macro-relief layer before generating the playable biome inspection.");

            _fields = SphericalGeographicFieldGenerator.Generate(settings);
            _generatedRoot = new GameObject("WORLD-002_GeneratedPlayableInspection").transform;
            _generatedRoot.SetParent(transform, false);

            BuildPlanet();
            Vector3 spawnDirection = FindSpawnDirection(spawnBiome);
            CreatePlayerAndCamera(spawnDirection);
            SpawnForestTrees(spawnDirection);
            CreateLighting();

            Debug.Log(
                $"Playable biome inspection generated. Seed={settings.seed}, Resolution={settings.faceResolution}, " +
                $"Ocean={_fields.ActualOceanCoveragePercent:F2}%, RequestedSpawnBiome={spawnBiome}, " +
                $"ActualSpawnBiome={_fields.SampleBiome(spawnDirection)}.", this);
        }

        [ContextMenu("Clear Playable Biome Inspection")]
        public void ClearInspectionWorld()
        {
            if (_generatedRoot != null)
            {
                DestroyOwnedObject(_generatedRoot.gameObject);
                _generatedRoot = null;
            }
            if (_generatedMeshes != null)
            {
                foreach (Mesh mesh in _generatedMeshes)
                    if (mesh != null) DestroyOwnedObject(mesh);
                _generatedMeshes = null;
            }
            if (_ownedMaterial != null)
            {
                DestroyOwnedObject(_ownedMaterial);
                _ownedMaterial = null;
            }
            _fields = null;
        }

        private void BuildPlanet()
        {
            // Vertex RGBA stores normalized biome weights; UV2.x stores ocean weight.
            // The shader samples all biome textures in world-space triplanar coordinates,
            // so texture scale remains physical and independent of planet radius.
            Mesh[] meshes = SphericalBiomeSurfaceMeshGenerator.GenerateAllFaces(
                settings, _fields, transitionWidthCells: 2f);
            _generatedMeshes = meshes;
            Material material = ResolveBiomeMaterial();
            try
            {
                for (int i = 0; i < meshes.Length; i++)
                {
                    GameObject face = new GameObject(((CubeSphereFace)i).ToString());
                    face.transform.SetParent(_generatedRoot, false);
                    face.AddComponent<MeshFilter>().sharedMesh = meshes[i];
                    face.AddComponent<MeshRenderer>().sharedMaterial = material;
                    // Field-aware movement queries the shared WORLD-002 data directly.
                    // Planet-scale MeshColliders are intentionally not created.
                }
            }
            catch
            {
                foreach (Mesh mesh in meshes)
                    if (mesh != null) DestroyOwnedObject(mesh);
                throw;
            }
        }

        private Material ResolveBiomeMaterial()
        {
            if (biomeMaterialMapping == null)
                throw new InvalidOperationException(
                    "The biome material mapping is not assigned. Run Tools/World/Generate or Refresh Biome Materials (URP), " +
                    "then Tools/World/Assign Biome Mapping to Current Inspection Scene.");

            Shader shader = Shader.Find("Pokemon/World/SphericalBiomeBlend");
            if (shader == null)
                throw new InvalidOperationException(
                    "The opaque URP shader 'Pokemon/World/SphericalBiomeBlend' could not be found or compiled.");

            if (biomeMaterial != null)
                Debug.LogWarning(
                    $"Ignoring legacy single biome material '{biomeMaterial.name}'. The playable surface now uses " +
                    "CFG_BiomeMaterialMapping and the four-biome blend shader.", this);

            _ownedMaterial = new Material(shader)
            {
                name = "WORLD-002_BiomeBlend_Opaque",
                renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry
            };
            biomeMaterialMapping.ConfigureBlendMaterial(_ownedMaterial);
            return _ownedMaterial;
        }

        private void CreatePlayerAndCamera(Vector3 spawnDirection)
        {
            float groundRadius = settings.radiusMeters + _fields.Sample(SphericalGeographicFieldType.AltitudeMeters, spawnDirection);

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "InspectionPlayer_Capsule";
            player.transform.SetParent(_generatedRoot, false);
            player.transform.localScale = new Vector3(0.7f, capsuleHeightMeters * 0.5f, 0.7f);
            Collider primitiveCollider = player.GetComponent<Collider>();
            if (primitiveCollider != null) DestroyOwnedObject(primitiveCollider);
            player.transform.position = spawnDirection * (groundRadius + capsuleHeightMeters * 0.5f);
            SphericalSurfaceQuery.GetTangentFrame(spawnDirection, out Vector3 north, out _);
            player.transform.rotation = Quaternion.LookRotation(north, spawnDirection);

            GameObject cameraObject = new GameObject("InspectionThirdPersonCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(_generatedRoot, false);
            Camera camera = cameraObject.AddComponent<Camera>();
            // The camera follows the planet-local up vector. A global skybox horizon
            // therefore appears tilted while traversing the sphere.
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.48f, 0.67f, 0.82f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = Mathf.Max(10000f, settings.radiusMeters * 2.5f);
            cameraObject.AddComponent<AudioListener>();
            SphericalThirdPersonCamera cameraRig = cameraObject.AddComponent<SphericalThirdPersonCamera>();
            cameraRig.Initialize(player.transform, camera);

            SphericalPlayerController controller = player.AddComponent<SphericalPlayerController>();
            controller.Initialize(_fields, Vector3.zero, settings.radiusMeters, cameraRig, capsuleHeightMeters);

            SphericalWorldDemoOverlay overlay = cameraObject.AddComponent<SphericalWorldDemoOverlay>();
            overlay.Initialize(cameraRig, settings.seed, settings.radiusMeters);
        }

        private Vector3 FindSpawnDirection(SphericalBiome requestedBiome)
        {
            if (requestedBiome == SphericalBiome.None)
                throw new InvalidOperationException("Spawn biome must be one of the four terrestrial biomes.");

            Vector3 bestPreferredDirection = Vector3.zero;
            float bestPreferredScore = float.PositiveInfinity;
            Vector3 bestFallbackDirection = Vector3.zero;
            float bestFallbackSlope = float.PositiveInfinity;

            int resolution = _fields.Resolution;
            float angularOffset = 1.5f / resolution;

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            for (int y = 1; y < resolution; y++)
            for (int x = 1; x < resolution; x++)
            {
                CubeSphereFace face = (CubeSphereFace)faceIndex;
                if (_fields.GetBiome(face, x, y) != requestedBiome ||
                    _fields.GetWaterClass(face, x, y) != SphericalWaterClass.Land)
                    continue;

                Vector3 direction = _fields.GetDirection(face, x, y);
                float slope = _fields.GetValue(SphericalGeographicFieldType.SlopeDegrees, face, x, y);

                // Always retain a fallback on the requested biome. This allows spawning
                // in mountainous/desert worlds even when no cell meets the preferred slope.
                if (slope < bestFallbackSlope)
                {
                    bestFallbackSlope = slope;
                    bestFallbackDirection = direction;
                }

                if (slope > preferredMaximumSpawnSlope)
                    continue;

                SphericalSurfaceQuery.GetTangentFrame(direction, out Vector3 tangentA, out Vector3 tangentB);
                bool nearBiomeTransition = false;
                for (int a = -1; a <= 1 && !nearBiomeTransition; a++)
                for (int b = -1; b <= 1 && !nearBiomeTransition; b++)
                {
                    if (a == 0 && b == 0) continue;
                    Vector3 neighbour = (direction + (tangentA * a + tangentB * b) * angularOffset).normalized;
                    SphericalBiome neighbourBiome = _fields.SampleBiome(neighbour);
                    if (neighbourBiome != requestedBiome && neighbourBiome != SphericalBiome.None)
                        nearBiomeTransition = true;
                }

                float score = slope + (nearBiomeTransition ? 10f : 0f);
                if (score < bestPreferredScore)
                {
                    bestPreferredScore = score;
                    bestPreferredDirection = direction;
                }
            }

            if (bestPreferredDirection != Vector3.zero)
                return bestPreferredDirection;

            if (bestFallbackDirection != Vector3.zero)
            {
                Debug.LogWarning(
                    $"No {requestedBiome} spawn cell met the preferred slope limit of " +
                    $"{preferredMaximumSpawnSlope:F1}°. Spawning at the flattest available {requestedBiome} land cell " +
                    $"(slope {bestFallbackSlope:F1}°).", this);
                return bestFallbackDirection;
            }

            throw new InvalidOperationException(
                $"No dry-land spawn location exists in the requested biome '{requestedBiome}'. " +
                "Choose another Spawn Biome or regenerate the planet with different biome/ocean settings.");
        }

        private void SpawnForestTrees(Vector3 spawnDirection)
        {
            if (forestTreePrefabs == null || forestTreePrefabs.Length == 0 || forestTreeCount <= 0)
                return;

            var validPrefabs = new System.Collections.Generic.List<GameObject>(forestTreePrefabs.Length);
            for (int i = 0; i < forestTreePrefabs.Length; i++)
                if (forestTreePrefabs[i] != null) validPrefabs.Add(forestTreePrefabs[i]);

            if (validPrefabs.Count == 0)
            {
                Debug.LogWarning(
                    "[WORLD-010] No forest tree prefabs assigned. Import the Blender tree FBXs and assign " +
                    "SM_Tree_Deciduous / SM_Tree_Birch to Forest Tree Prefabs.", this);
                return;
            }

            int seed = unchecked(settings.seed * 486187739 + 7919);
            System.Random random = new System.Random(seed);
            var placedDirections = new System.Collections.Generic.List<Vector3>(forestTreeCount);
            int attempts = Mathf.Max(forestTreeCount * 35, 100);
            int spawned = 0;
            float minimumSpacingSquared = forestTreeMinimumSpacingMeters * forestTreeMinimumSpacingMeters;
            float exclusionAngle = forestTreeSpawnExclusionRadiusMeters / Mathf.Max(1f, settings.radiusMeters);
            float exclusionDot = Mathf.Cos(exclusionAngle);

            for (int attempt = 0; attempt < attempts && spawned < forestTreeCount; attempt++)
            {
                // Uniform random point on the sphere, independent of cube-face seams.
                float y = (float)(random.NextDouble() * 2.0 - 1.0);
                float azimuth = (float)(random.NextDouble() * Math.PI * 2.0);
                float horizontal = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
                Vector3 direction = new Vector3(
                    horizontal * Mathf.Cos(azimuth), y, horizontal * Mathf.Sin(azimuth));

                if (_fields.SampleWaterClass(direction) != SphericalWaterClass.Land ||
                    _fields.SampleBiome(direction) != SphericalBiome.Forest)
                    continue;

                if (_fields.Sample(SphericalGeographicFieldType.SlopeDegrees, direction) >
                    forestTreeMaximumSlopeDegrees)
                    continue;

                // Keep the immediate player start area clear.
                if (Vector3.Dot(direction, spawnDirection) > exclusionDot)
                    continue;

                bool tooClose = false;
                for (int i = 0; i < placedDirections.Count; i++)
                {
                    float chordDistance = (direction - placedDirections[i]).magnitude * settings.radiusMeters;
                    if (chordDistance * chordDistance < minimumSpacingSquared)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                GameObject prefab = validPrefabs[random.Next(validPrefabs.Count)];

                float altitude = _fields.Sample(SphericalGeographicFieldType.AltitudeMeters, direction);
                Vector3 localPosition = direction * (settings.radiusMeters + altitude + 0.03f);
                Quaternion alignToSurface = Quaternion.FromToRotation(Vector3.up, direction);
                Quaternion randomYaw = Quaternion.AngleAxis((float)random.NextDouble() * 360f, Vector3.up);

                GameObject tree = Instantiate(prefab, _generatedRoot);
                tree.name = $"{prefab.name}_{spawned + 1:000}";
                tree.transform.localPosition = localPosition;
                tree.transform.localRotation = alignToSurface * randomYaw;
                float scale = Mathf.Lerp(
                    forestTreeScaleRange.x,
                    forestTreeScaleRange.y,
                    (float)random.NextDouble());
                tree.transform.localScale = Vector3.one * scale;

                placedDirections.Add(direction);
                spawned++;
            }

            Debug.Log(
                $"[WORLD-010] Forest decoration: {spawned}/{forestTreeCount} trees placed " +
                $"(seed={seed}, slope<={forestTreeMaximumSlopeDegrees:F1}°, " +
                $"spacing>={forestTreeMinimumSpacingMeters:F1}m).", this);
        }

        private void CreateLighting()
        {
            GameObject lightObject = new GameObject("InspectionDirectionalLight");
            lightObject.transform.SetParent(_generatedRoot, false);
            lightObject.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
        }

        private void OnDestroy() => ClearInspectionWorld();

        private static void DestroyOwnedObject(UnityEngine.Object ownedObject)
        {
            if (ownedObject == null) return;
            if (Application.isPlaying) Destroy(ownedObject);
            else DestroyImmediate(ownedObject);
        }
    }
}
