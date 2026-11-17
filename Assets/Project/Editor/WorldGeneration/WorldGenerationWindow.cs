using Pokemon.Project.World.Generation;
using Pokemon.Project.DataServices.Loading;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Pokemon.Project.Editor.WorldGeneration
{
    public sealed class WorldGenerationWindow : EditorWindow
    {
        private const string GeneratedRootName = WorldTerrainBuilder.DefaultObjectName;
        private const string GeneratedAssetFolder = "Assets/Generated/Worlds";
        private const string GeneratedMaterialPath = GeneratedAssetFolder + "/GeneratedWorldVertexColor.mat";

        private WorldGenerationSettings settings;
        private WorldGenerationResult result;
        private string status = "Configuration loaded from JSON.";

        [MenuItem("Tools/Pokemon World/World Generator")]
        public static void Open() => GetWindow<WorldGenerationWindow>("World Generator");

        private void OnEnable()
        {
            try
            {
                settings = WorldGenerationConfigLoader.LoadForEditor();
                status = $"JSON loaded: {settings.worldWidthMeters:0} x {settings.worldDepthMeters:0} m, " +
                         $"{settings.resolution} x {settings.resolution}, seed {settings.seed}.";
            }
            catch (Exception exception)
            {
                settings = null;
                status = "Could not load world_generation.json: " + exception.Message;
                Debug.LogException(exception);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Procedural World Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "All generation parameters are read from Assets/Project/Data/WorldGeneration/world_generation.json. " +
                "Generate creates and applies the terrain directly to the active Unity scene.",
                MessageType.Info);

            EditorGUILayout.Space(8);
            using (new EditorGUI.DisabledScope(settings == null))
            {
                if (GUILayout.Button("Generate", GUILayout.Height(38)))
                    GenerateAndApply();

                using (new EditorGUI.DisabledScope(settings == null))
                {
                    if (GUILayout.Button("Export", GUILayout.Height(32)))
                        ExportAllMaps();
                }
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(status, MessageType.None);
        }

        private bool Generate(bool applyToScene)
        {
            try
            {
                settings.Validate();
                result = WorldGenerator.Generate(settings);
                if (applyToScene)
                    ApplyToScene();

                status = $"Generated {result.Width} x {result.Height} samples over " +
                         $"{result.WorldWidthMeters:0} x {result.WorldDepthMeters:0} m. " +
                         $"{result.Settlements.Count} settlements, {result.Roads.Count} routes. Seed: {settings.seed}.";
                Repaint();
                return true;
            }
            catch (Exception exception)
            {
                status = "Generation failed: " + exception.Message;
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("World generation failed", exception.Message, "OK");
                return false;
            }
        }

        private void GenerateAndApply()
        {
            if (!Generate(false)) return;
            try
            {
                ApplyToScene();
            }
            catch (Exception exception)
            {
                status = "Could not apply terrain to scene: " + exception.Message;
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Could not apply terrain", exception.Message, "OK");
            }
        }

        private void ApplyToScene()
        {
            EnsureGeneratedAssetFolder();
            Material material = LoadOrCreateGeneratedMaterial();
            Mesh mesh = WorldTerrainBuilder.BuildMesh(result, 1f);
            string meshPath = AssetDatabase.GenerateUniqueAssetPath(
                $"{GeneratedAssetFolder}/GeneratedWorld_{settings.seed}_{result.Width}x{result.Height}.asset");
            AssetDatabase.CreateAsset(mesh, meshPath);

            GameObject previous = FindGeneratedWorld();
            var generated = new GameObject(GeneratedRootName);
            var filter = generated.AddComponent<MeshFilter>();
            var renderer = generated.AddComponent<MeshRenderer>();
            filter.sharedMesh = mesh;
            renderer.sharedMaterial = material;
            var collider = generated.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            generated.transform.position = Vector3.zero;

            Undo.RegisterCreatedObjectUndo(generated, "Generate world terrain");
            if (previous != null)
                Undo.DestroyObjectImmediate(previous);

            Selection.activeGameObject = generated;
            EditorSceneManager.MarkSceneDirty(generated.scene);
            AssetDatabase.SaveAssets();
            status += " Terrain instantiated in the active scene.";
            Debug.Log($"Generated terrain applied to active scene. Mesh asset: {meshPath}");
        }

        private static GameObject FindGeneratedWorld()
        {
            GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
                if (root.name == GeneratedRootName) return root;
            return null;
        }

        private static void EnsureGeneratedAssetFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Generated"))
                AssetDatabase.CreateFolder("Assets", "Generated");
            if (!AssetDatabase.IsValidFolder(GeneratedAssetFolder))
                AssetDatabase.CreateFolder("Assets/Generated", "Worlds");
        }

        private static Material LoadOrCreateGeneratedMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(GeneratedMaterialPath);
            if (material != null) return material;

            Shader shader = Shader.Find(WorldTerrainBuilder.DefaultShaderName);
            if (shader == null)
                throw new InvalidOperationException(
                    $"Shader '{WorldTerrainBuilder.DefaultShaderName}' not found. Ensure WorldVertexColor.shader is imported.");

            material = new Material(shader) { name = "GeneratedWorldVertexColor" };
            AssetDatabase.CreateAsset(material, GeneratedMaterialPath);
            return material;
        }

        private void ExportAllMaps()
        {
            if (result == null && !Generate(false))
                return;

            string folder = EditorUtility.SaveFolderPanel("Export all generated world maps", "", "WorldMaps");
            if (string.IsNullOrEmpty(folder)) return;

            try
            {
                Directory.CreateDirectory(folder);
                WriteFloatLayer(folder, "height_meters", result.HeightMeters);
                WriteFloatLayer(folder, "temperature_0_1", result.Temperature01);
                WriteFloatLayer(folder, "water_mask", result.WaterMask);
                WriteFloatLayer(folder, "distance_to_water_meters", result.DistanceToWaterMeters);
                WriteFloatLayer(folder, "moisture_0_1", result.Moisture01);
                WriteFloatLayer(folder, "vegetation_0_1", result.Vegetation01);
                WriteFloatLayer(folder, "settlement_suitability", result.SettlementSuitability);
                WriteFloatLayer(folder, "traversal_cost", result.TraversalCost);
                WriteFloatLayer(folder, "road_mask", result.RoadMask);
                File.WriteAllBytes(Path.Combine(folder, "biome_u8.bin"), result.Biome);
                File.WriteAllBytes(Path.Combine(folder, "water_type_u8.bin"), result.WaterType);

                var network = new ExportNetwork
                {
                    settlements = result.Settlements.ToArray(),
                    roads = result.Roads.ToArray()
                };
                File.WriteAllText(Path.Combine(folder, "settlements_and_roads.json"),
                    JsonUtility.ToJson(network, true));

                var metadata = new ExportMetadata
                {
                    generatorId = "world_environment_generator",
                    generatorVersion = "0.4.0",
                    seed = settings.seed,
                    width = result.Width,
                    height = result.Height,
                    worldWidthMeters = result.WorldWidthMeters,
                    worldDepthMeters = result.WorldDepthMeters,
                    cellSizeX = settings.CellSizeX,
                    cellSizeZ = settings.CellSizeZ,
                    encoding = "float32 little-endian; biome=u8; water_type=u8",
                    coordinateSystem = "world_xz",
                    rowOrder = "south_to_north"
                };
                File.WriteAllText(Path.Combine(folder, "metadata.json"), JsonUtility.ToJson(metadata, true));
                EditorUtility.RevealInFinder(folder);
                status = "All generated maps exported to: " + folder;
                EditorUtility.DisplayDialog("Export complete", status, "OK");
            }
            catch (Exception exception)
            {
                status = "Export failed: " + exception.Message;
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Export failed", exception.Message, "OK");
            }
        }

        private static void WriteFloatLayer(string folder, string fileName, float[] values)
        {
            using (var stream = new FileStream(Path.Combine(folder, fileName + ".f32"),
                       FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream))
            {
                for (int i = 0; i < values.Length; i++)
                    writer.Write(values[i]);
            }
        }

        [Serializable]
        private sealed class ExportNetwork
        {
            public SettlementSite[] settlements;
            public RoadRoute[] roads;
        }

        [Serializable]
        private sealed class ExportMetadata
        {
            public string generatorId, generatorVersion, encoding, coordinateSystem, rowOrder;
            public int seed, width, height;
            public float worldWidthMeters, worldDepthMeters, cellSizeX, cellSizeZ;
        }
    }
}
