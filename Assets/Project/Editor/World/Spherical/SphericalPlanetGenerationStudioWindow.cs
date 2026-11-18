#if UNITY_EDITOR
using System;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Studio;
using Pokemon.Project.World.Spherical.Geography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Pokemon.Project.Editor.World.Spherical
{
    /// <summary>Editor-first, non-destructive layered planet generation and inspection studio.</summary>
    public sealed class SphericalPlanetGenerationStudioWindow : EditorWindow
    {
        private const string SettingsPath = "Assets/Project/Content/World/PlanetStudio/SphericalPlanetStudioSettings.asset";
        private const string PreviewRootName = "WORLD-001_PlanetStudioPreview";
        private SphericalPlanetStudioSettings settings;
        private Vector2 scroll;
        private bool showLayer1 = true;
        private bool showLayer2 = true;
        private bool showLayer3 = true;
        private SphericalNeighbourGraph lastDrainageGraph;
        private SphericalDrainageResult lastDrainageResult;

        [MenuItem("Tools/World/Open Planet Generation Studio")]
        public static void Open()
        {
            SphericalPlanetGenerationStudioWindow window = GetWindow<SphericalPlanetGenerationStudioWindow>("Planet Studio");
            window.minSize = new Vector2(360f, 560f);
            window.EnsureSettings();
            window.Show();
        }

        private void OnEnable() => EnsureSettings();

        private void OnGUI()
        {
            EnsureSettings();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("WORLD-001 — Layered Planet Studio", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Build the same spherical data progressively. Layer visibility is non-destructive; switching views only changes presentation.", MessageType.Info);
            EditorGUI.BeginChangeCheck();
            DrawPlanetSettings();
            EditorGUILayout.Space(8);
            DrawTerrainLayers();
            EditorGUILayout.Space(8);
            DrawInspection();
            EditorGUILayout.Space(8);
            DrawDrainageControls();
            bool changed = EditorGUI.EndChangeCheck();
            EditorGUILayout.Space(12);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate / Refresh Preview", GUILayout.Height(32))) GeneratePreview();
                if (GUILayout.Button("Clear Preview", GUILayout.Height(32), GUILayout.Width(105))) ClearPreview();
            }
            if (changed)
            {
                EditorUtility.SetDirty(settings);
                Repaint();
            }
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox("Drainage is an experimental WORLD-002 preview. The sea-level threshold and river accumulation threshold are editable; visual output is not yet hydrographically approved.", MessageType.Warning);
            EditorGUILayout.EndScrollView();
        }

        private void DrawPlanetSettings()
        {
            EditorGUILayout.LabelField("Planet definition", EditorStyles.boldLabel);
            settings.radiusMeters = EditorGUILayout.FloatField("Radius (m)", settings.radiusMeters);
            settings.seed = EditorGUILayout.IntField("Seed", settings.seed);
            settings.faceResolution = EditorGUILayout.IntSlider("Globe resolution", settings.faceResolution, 8, 256);
            settings.planisphereWidth = EditorGUILayout.IntSlider("Planisphere width", settings.planisphereWidth, 64, 1024);
            settings.planisphereHeight = EditorGUILayout.IntSlider("Planisphere height", settings.planisphereHeight, 32, 512);
        }

        private void DrawTerrainLayers()
        {
            EditorGUILayout.LabelField("Terrain generation stack", EditorStyles.boldLabel);
            showLayer1 = EditorGUILayout.Foldout(showLayer1, "Layer 1 — Macro relief", true);
            if (showLayer1)
            {
                settings.layer1Enabled = EditorGUILayout.Toggle("Enabled", settings.layer1Enabled);
                settings.layer1AmplitudeMeters = EditorGUILayout.FloatField("Amplitude (m)", settings.layer1AmplitudeMeters);
                settings.layer1Frequency = EditorGUILayout.FloatField("Frequency", settings.layer1Frequency);
                settings.continentThreshold = EditorGUILayout.Slider("Landform threshold", settings.continentThreshold, 0f, 1f);
            }
            showLayer2 = EditorGUILayout.Foldout(showLayer2, "Layer 2 — Regional relief", true);
            if (showLayer2)
            {
                settings.layer2Enabled = EditorGUILayout.Toggle("Enabled", settings.layer2Enabled);
                settings.layer2AmplitudeMeters = EditorGUILayout.FloatField("Amplitude (m)", settings.layer2AmplitudeMeters);
                settings.layer2Frequency = EditorGUILayout.FloatField("Frequency", settings.layer2Frequency);
            }
            showLayer3 = EditorGUILayout.Foldout(showLayer3, "Layer 3 — Fine details", true);
            if (showLayer3)
            {
                settings.layer3Enabled = EditorGUILayout.Toggle("Enabled", settings.layer3Enabled);
                settings.layer3AmplitudeMeters = EditorGUILayout.FloatField("Amplitude (m)", settings.layer3AmplitudeMeters);
                settings.layer3Frequency = EditorGUILayout.FloatField("Frequency", settings.layer3Frequency);
            }
            EditorGUILayout.HelpBox("Disabled layers keep all their settings. Enable only Layer 1 to return to the macro-relief stage; disable all layers for the smooth base sphere.", MessageType.None);
        }

        private void DrawInspection()
        {
            EditorGUILayout.LabelField("Inspection", EditorStyles.boldLabel);
            settings.previewStage = (SphericalPlanetStudioPreviewStage)EditorGUILayout.EnumPopup("Preview stage", settings.previewStage);
            settings.view = (SphericalPlanetStudioView)EditorGUILayout.EnumPopup("View", settings.view);
            using (new EditorGUI.DisabledScope(false))
            {
                settings.map = (SphericalPlanetStudioMap)EditorGUILayout.EnumPopup("Map", settings.map);
            }
            if (settings.map == SphericalPlanetStudioMap.Moisture || settings.map == SphericalPlanetStudioMap.Water || settings.map == SphericalPlanetStudioMap.Biomes || settings.map == SphericalPlanetStudioMap.Villages || settings.map == SphericalPlanetStudioMap.Slope)
                EditorGUILayout.HelpBox("This map is not generated yet. Choose Terrain, Altitude or Temperature for an implemented preview.", MessageType.Warning);
        }

        private void DrawDrainageControls()
        {
            EditorGUILayout.LabelField("WORLD-002 — Hydrology / Drainage", EditorStyles.boldLabel);
            settings.drainageInputMode = (SphericalDrainageInputMode)EditorGUILayout.EnumPopup("Drainage input", settings.drainageInputMode);
            settings.drainageResolution = EditorGUILayout.IntSlider("Drainage resolution", settings.drainageResolution, 8, 512);
            using (new EditorGUI.DisabledScope(settings.drainageInputMode == SphericalDrainageInputMode.FlatBaseSphereDiagnostic))
                settings.drainageSeaLevelMeters = EditorGUILayout.FloatField("Sea level (m)", settings.drainageSeaLevelMeters);
            settings.riverAccumulationThreshold = EditorGUILayout.FloatField("River accumulation threshold", settings.riverAccumulationThreshold);
            settings.drainagePreviewMode = (SphericalDrainagePreviewMode)EditorGUILayout.EnumPopup("Overlay", settings.drainagePreviewMode);
            if (settings.drainagePreviewMode == SphericalDrainagePreviewMode.FlowDirection)
                settings.flowDirectionStride = EditorGUILayout.IntSlider("Direction sampling stride", settings.flowDirectionStride, 1, 64);
            if (settings.drainageInputMode == SphericalDrainageInputMode.FlatBaseSphereDiagnostic)
                EditorGUILayout.HelpBox("Diagnostic only: uses a perfectly flat base sphere and one deterministic north-pole outlet. This creates an artificial basin and is not a realistic river network. Select Terrain Heightfield to visualize drainage derived from the generated terrain.", MessageType.Warning);
            else
                EditorGUILayout.HelpBox("Terrain Heightfield uses the selected preview stage and provisional sea-level cutoff. This is the mode to inspect the actual terrain-derived drainage network; the sea/ocean model is not yet hydrographically approved.", MessageType.Info);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate Drainage Preview", GUILayout.Height(28))) GenerateDrainagePreview();
                if (GUILayout.Button("Clear Drainage", GUILayout.Height(28), GUILayout.Width(110))) ClearDrainagePreview();
            }
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Isolation tests", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("A — Run Logic Only"))
                    RunDrainageLogicOnlyDiagnostic();
                if (GUILayout.Button("B — Renderer Only"))
                    GenerateRendererOnlyDiagnostic();
            }
            EditorGUILayout.HelpBox(
                "A runs the drainage calculation and logs numeric invariants without creating an overlay. " +
                "B creates three bright test ribbons without running drainage. Run B first to isolate Scene rendering.",
                MessageType.None);
        }

        private void EnsureSettings()
        {
            if (settings != null) return;
            settings = AssetDatabase.LoadAssetAtPath<SphericalPlanetStudioSettings>(SettingsPath);
            if (settings != null) return;
            string folder = "Assets/Project/Content/World/PlanetStudio";
            EnsureFolder("Assets/Project/Content/World");
            EnsureFolder(folder);
            settings = CreateInstance<SphericalPlanetStudioSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private void GeneratePreview()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Planet Studio", "Stop Play Mode before generating an Editor preview.", "OK");
                return;
            }
            if (settings.map != SphericalPlanetStudioMap.Terrain && settings.map != SphericalPlanetStudioMap.Altitude && settings.map != SphericalPlanetStudioMap.Temperature)
            {
                EditorUtility.DisplayDialog("Planet Studio", "This thematic map is not implemented yet. No preview was generated.", "OK");
                return;
            }
            settings.Validate();
            EditorUtility.SetDirty(settings);
            ClearPreview();
            GameObject root = new GameObject(PreviewRootName);
            Undo.RegisterCreatedObjectUndo(root, "Generate Planet Studio Preview");
            Material material = CreatePreviewMaterial();
            if (settings.view == SphericalPlanetStudioView.Spherical)
            {
                Mesh[] meshes = SphericalPlanetStudioGenerator.GenerateGlobeFaces(settings);
                for (int i = 0; i < meshes.Length; i++) CreateMeshObject(root.transform, ((CubeSphereFace)i).ToString(), meshes[i], material);
            }
            else
            {
                Mesh mesh = SphericalPlanetStudioGenerator.GeneratePlanisphere(settings);
                CreateMeshObject(root.transform, "Planisphere_Equirectangular", mesh, material);
            }
            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(root.scene);
            SceneView.RepaintAll();
            Debug.Log($"Planet Studio preview generated. View={settings.view}, Map={settings.map}, Seed={settings.seed}, Stage={settings.previewStage}, Layers=[{settings.layer1Enabled},{settings.layer2Enabled},{settings.layer3Enabled}].", root);
        }


        private void RunDrainageLogicOnlyDiagnostic()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Planet Studio", "Stop Play Mode before running diagnostics.", "OK");
                return;
            }

            try
            {
                settings.Validate();
                EditorUtility.DisplayProgressBar("Drainage Logic Test", "Calculating drainage only (no mesh or renderer)...", 0.25f);
                SphericalNeighbourGraph graph;
                SphericalDrainageResult result = SphericalDrainageStudioPreview.Generate(settings, out graph);

                int riverCount = 0;
                int outletCount = 0;
                int terminalCount = 0;
                double outletAccumulation = 0d;
                float minAccumulation = float.PositiveInfinity;
                float maxAccumulation = float.NegativeInfinity;
                var sorted = new float[result.FlowAccumulation.Length];
                Array.Copy(result.FlowAccumulation, sorted, sorted.Length);
                for (int i = 0; i < sorted.Length; i++)
                {
                    float value = sorted[i];
                    if (value < minAccumulation) minAccumulation = value;
                    if (value > maxAccumulation) maxAccumulation = value;
                    if (result.RiverMask[i]) riverCount++;
                    if (result.OutletMask[i]) outletCount++;
                    if (result.DownstreamNodeIds[i] < 0)
                    {
                        terminalCount++;
                        outletAccumulation += value;
                    }
                }
                Array.Sort(sorted);
                float p50 = sorted[sorted.Length / 2];
                float p95 = sorted[Math.Min(sorted.Length - 1, (int)(sorted.Length * 0.95f))];
                int badDownstream = 0;
                int badRank = 0;
                for (int i = 0; i < graph.NodeCount; i++)
                {
                    int target = result.DownstreamNodeIds[i];
                    if (result.OutletMask[i])
                    {
                        if (target != -1) badDownstream++;
                    }
                    else if (target < 0 || target >= graph.NodeCount)
                    {
                        badDownstream++;
                    }
                    else if (!result.OutletMask[target] && result.FloodRank[target] >= result.FloodRank[i])
                    {
                        badRank++;
                    }
                }

                Debug.Log(
                    $"DRAINAGE LOGIC ONLY — no renderer invoked. Input={settings.drainageInputMode}, " +
                    $"Nodes={graph.NodeCount}, WaterNodes={result.InitialWaterNodeCount}, Outlets={outletCount}, " +
                    $"Terminals={terminalCount}, RiverCandidates={riverCount}, Threshold={settings.riverAccumulationThreshold}, " +
                    $"Accumulation[min={minAccumulation:F2}, p50={p50:F2}, p95={p95:F2}, max={maxAccumulation:F2}], " +
                    $"OutletAccumulationTotal={outletAccumulation:F2}, ExpectedNodeCount={graph.NodeCount}, " +
                    $"BadDownstream={badDownstream}, BadFloodRank={badRank}, " +
                    $"TimingsMs[PriorityFlood={result.PriorityFloodMilliseconds:F1}, " +
                    $"Direction={result.FlowDirectionMilliseconds:F1}, Accumulation={result.AccumulationMilliseconds:F1}].",
                    this);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Drainage logic diagnostic failed", exception.Message, "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void GenerateRendererOnlyDiagnostic()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Planet Studio", "Stop Play Mode before running diagnostics.", "OK");
                return;
            }

            try
            {
                // Deliberately do not call SphericalDrainageStudioPreview.Generate or BuildOverlayMesh.
                ClearPreview();
                GameObject root = new GameObject(PreviewRootName);
                Undo.RegisterCreatedObjectUndo(root, "Create Renderer-Only Diagnostic");
                Mesh[] globeMeshes = SphericalPlanetStudioGenerator.GenerateGlobeFaces(settings);
                Material globeMaterial = CreatePreviewMaterial();
                for (int i = 0; i < globeMeshes.Length; i++)
                    CreateMeshObject(root.transform, ((CubeSphereFace)i).ToString(), globeMeshes[i], globeMaterial);

                Mesh diagnosticMesh = SphericalDrainageStudioPreview.BuildRendererDiagnosticMesh(settings.radiusMeters);
                // Sprites/Default multiplies the mesh vertex colors by the material color.
                // Unlit/Color and URP/Unlit do not reliably consume Mesh.colors, which
                // made all three diagnostic ribbons appear white.
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                    throw new InvalidOperationException(
                        "Shader 'Sprites/Default' is unavailable; renderer diagnostic requires a vertex-color shader.");
                Material diagnosticMaterial = new Material(shader) { name = "WORLD-002_RendererDiagnostic_Magenta" };
                if (diagnosticMaterial.HasProperty("_Color"))
                    diagnosticMaterial.SetColor("_Color", Color.white);
                if (diagnosticMaterial.HasProperty("_BaseColor"))
                    diagnosticMaterial.SetColor("_BaseColor", Color.white);
                CreateMeshObject(root.transform, "WORLD-002_RendererOnlyDiagnostic", diagnosticMesh, diagnosticMaterial);
                Selection.activeGameObject = root;
                EditorSceneManager.MarkSceneDirty(root.scene);
                SceneView.RepaintAll();
                Debug.Log(
                    $"DRAINAGE RENDERER ONLY — drainage algorithm was NOT invoked. " +
                    $"Mesh={diagnosticMesh.name}, Vertices={diagnosticMesh.vertexCount}, " +
                    $"Triangles={diagnosticMesh.triangles.Length / 3}, Bounds={diagnosticMesh.bounds}, " +
                    $"Radius={settings.radiusMeters}, Active={root.activeInHierarchy}.",
                    root);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Renderer diagnostic failed", exception.Message, "OK");
            }
        }

        private void GenerateDrainagePreview()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Planet Studio", "Stop Play Mode before generating an Editor preview.", "OK");
                return;
            }
            settings.Validate();
            EditorUtility.SetDirty(settings);
            try
            {
                EditorUtility.DisplayProgressBar("Planet Studio", "Refreshing terrain preview...", 0.1f);
                SphericalPlanetStudioMap requestedMap = settings.map;
                SphericalPlanetStudioPreviewStage requestedStage = settings.previewStage;
                if (requestedMap != SphericalPlanetStudioMap.Terrain && requestedMap != SphericalPlanetStudioMap.Altitude && requestedMap != SphericalPlanetStudioMap.Temperature)
                    settings.map = SphericalPlanetStudioMap.Terrain;
                if (settings.drainageInputMode == SphericalDrainageInputMode.FlatBaseSphereDiagnostic)
                    settings.previewStage = SphericalPlanetStudioPreviewStage.BaseSphere;
                ClearPreview();
                GeneratePreview();
                settings.map = requestedMap;
                settings.previewStage = requestedStage;
                EditorUtility.DisplayProgressBar("Planet Studio", "Building spherical drainage graph and fields...", 0.3f);
                lastDrainageResult = SphericalDrainageStudioPreview.Generate(settings, out lastDrainageGraph);
                EditorUtility.DisplayProgressBar("Planet Studio", "Building drainage overlay mesh...", 0.8f);
                GameObject root = GameObject.Find(PreviewRootName);
                if (root == null) throw new InvalidOperationException("Planet preview root could not be created.");
                ClearDrainageObjects(root.transform);
                Mesh overlay = SphericalDrainageStudioPreview.BuildOverlayMesh(lastDrainageGraph, lastDrainageResult,
                    settings, settings.view == SphericalPlanetStudioView.Planisphere,
                    out int eligibleSegments, out int renderedSegments, out int samplingStride);
                if (overlay == null)
                    throw new InvalidOperationException(
                        $"No drainage segments matched the current overlay settings. Input={settings.drainageInputMode}, " +
                        $"WaterNodes={lastDrainageResult.InitialWaterNodeCount}, Threshold={settings.riverAccumulationThreshold}, " +
                        $"Candidates={CountTrue(lastDrainageResult.RiverMask)}. For real terrain drainage, use Terrain Heightfield and inspect Sea Level / Accumulation Threshold.");
                Material material = CreateDrainageMaterial();
                CreateMeshObject(root.transform, "WORLD-002_DrainageOverlay", overlay, material);
                EditorSceneManager.MarkSceneDirty(root.scene);
                SceneView.RepaintAll();
                Debug.Log(
                    $"Drainage preview generated. Input={settings.drainageInputMode}, Resolution={settings.drainageResolution}, " +
                    $"Nodes={lastDrainageGraph.NodeCount}, WaterNodes={lastDrainageResult.InitialWaterNodeCount}, " +
                    $"RiverCandidates={CountTrue(lastDrainageResult.RiverMask)}, EligibleSegments={eligibleSegments}, " +
                    $"RenderedSegments={renderedSegments}, SamplingStride={samplingStride}, Vertices={overlay.vertexCount}, " +
                    $"Threshold={settings.riverAccumulationThreshold}, Overlay={settings.drainagePreviewMode}.", root);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Drainage preview failed", exception.Message, "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static int CountTrue(bool[] values)
        {
            int count = 0;
            for (int i = 0; i < values.Length; i++)
                if (values[i]) count++;
            return count;
        }

        private static Material CreateDrainageMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) throw new InvalidOperationException("No compatible shader was found for drainage overlay.");
            var material = new Material(shader) { name = "WORLD-002_DrainageOverlayMaterial" };
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            return material;
        }

        private static void ClearDrainageObjects(Transform root)
        {
            Transform existing = root.Find("WORLD-002_DrainageOverlay");
            if (existing == null) return;
            MeshFilter filter = existing.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null) DestroyImmediate(filter.sharedMesh);
            MeshRenderer renderer = existing.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.sharedMaterial != null) DestroyImmediate(renderer.sharedMaterial);
            DestroyImmediate(existing.gameObject);
        }

        private void ClearDrainagePreview()
        {
            GameObject root = GameObject.Find(PreviewRootName);
            if (root != null) ClearDrainageObjects(root.transform);
            lastDrainageGraph = null;
            lastDrainageResult = null;
            if (root != null) EditorSceneManager.MarkSceneDirty(root.scene);
            SceneView.RepaintAll();
        }

        private static Material CreatePreviewMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) throw new InvalidOperationException("No compatible preview shader was found.");
            return new Material(shader) { name = "WORLD-001_PlanetStudioPreviewMaterial" };
        }

        private static void CreateMeshObject(Transform parent, string name, Mesh mesh, Material material)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            MeshFilter filter = child.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        private static void ClearPreview()
        {
            GameObject existing = GameObject.Find(PreviewRootName);
            if (existing == null) return;
            MeshFilter[] filters = existing.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter filter in filters)
                if (filter.sharedMesh != null) DestroyImmediate(filter.sharedMesh);
            MeshRenderer[] renderers = existing.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in renderers)
                if (renderer.sharedMaterial != null) DestroyImmediate(renderer.sharedMaterial);
            DestroyImmediate(existing);
        }

        private void OnDisable() { }
    }
}
#endif
