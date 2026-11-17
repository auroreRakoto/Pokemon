#if UNITY_EDITOR
using System;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Studio;
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
            EditorGUILayout.HelpBox("Available maps in this first implementation: Terrain, Altitude and Temperature. Moisture, Water, Biomes, Villages and Slope remain unavailable until their data-generation stages are implemented.", MessageType.Warning);
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
