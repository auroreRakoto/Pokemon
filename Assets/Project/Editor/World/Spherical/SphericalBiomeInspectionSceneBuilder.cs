#if UNITY_EDITOR
using System.IO;
using Pokemon.Project.World.Spherical.Inspection;
using Pokemon.Project.World.Spherical.Studio;
using Pokemon.Project.World.Spherical.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pokemon.Project.Editor.World.Spherical
{
    /// <summary>Creates a separate, reproducible Play Mode scene for inspecting WORLD-002 biomes on foot.</summary>
    public static class SphericalBiomeInspectionSceneBuilder
    {
        private const string SettingsPath = "Assets/Project/Content/World/PlanetStudio/SphericalPlanetStudioSettings.asset";
        private const string SceneFolder = "Assets/Project/Content/Scenes/Worlds";
        private const string ScenePath = SceneFolder + "/WORLD-002_BiomeInspection.unity";
        private const string BiomeMappingPath = "Assets/Project/Content/World/PlanetStudio/CFG_BiomeMaterialMapping.asset";

        [MenuItem("Tools/World/Create Playable Biome Inspection Scene")]
        public static void CreateOrOpenScene()
        {
            SphericalPlanetStudioSettings settings = EnsureSettings();
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Biome Inspection", "Planet Studio settings could not be created.", "OK");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("WORLD-002_PlayableBiomeInspection");
            SphericalBiomeInspectionBootstrap bootstrap = root.AddComponent<SphericalBiomeInspectionBootstrap>();
            SerializedObject serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("settings").objectReferenceValue = settings;
            SphericalBiomeMaterialMapping mapping =
                AssetDatabase.LoadAssetAtPath<SphericalBiomeMaterialMapping>(BiomeMappingPath);
            SerializedProperty mappingProperty = serialized.FindProperty("biomeMaterialMapping");
            if (mappingProperty != null)
                mappingProperty.objectReferenceValue = mapping;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (mapping == null)
                Debug.LogWarning(
                    "Biome material mapping was not found. Run Tools/World/Generate or Refresh Biome Materials (URP), " +
                    "then assign it to the inspection scene.", root);

            if (!Directory.Exists(SceneFolder))
                Directory.CreateDirectory(SceneFolder);
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
            Debug.Log($"Created playable biome inspection scene at {ScenePath}. Enter Play Mode to generate the current WORLD-002 fields and spawn on plains.", root);
        }


        [MenuItem("Tools/World/Assign Biome Mapping to Current Inspection Scene")]
        public static void AssignMappingToCurrentInspectionScene()
        {
            SphericalBiomeMaterialMapping mapping =
                AssetDatabase.LoadAssetAtPath<SphericalBiomeMaterialMapping>(BiomeMappingPath);
            if (mapping == null)
            {
                EditorUtility.DisplayDialog(
                    "Biome Texturing",
                    "CFG_BiomeMaterialMapping.asset was not found. First run Tools/World/Generate or Refresh Biome Materials (URP).",
                    "OK");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Biome Texturing", "There is no valid active scene.", "OK");
                return;
            }

            int assignedCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                SphericalBiomeInspectionBootstrap[] bootstraps =
                    root.GetComponentsInChildren<SphericalBiomeInspectionBootstrap>(true);
                foreach (SphericalBiomeInspectionBootstrap bootstrap in bootstraps)
                {
                    SerializedObject serialized = new SerializedObject(bootstrap);
                    SerializedProperty property = serialized.FindProperty("biomeMaterialMapping");
                    if (property == null) continue;
                    property.objectReferenceValue = mapping;
                    serialized.ApplyModifiedProperties();
                    EditorUtility.SetDirty(bootstrap);
                    assignedCount++;
                }
            }

            if (assignedCount == 0)
            {
                EditorUtility.DisplayDialog(
                    "Biome Texturing",
                    "No SphericalBiomeInspectionBootstrap was found in the active scene.",
                    "OK");
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Assigned CFG_BiomeMaterialMapping to {assignedCount} playable biome inspection bootstrap(s) in '{scene.name}'.");
        }

        private static SphericalPlanetStudioSettings EnsureSettings()
        {
            SphericalPlanetStudioSettings settings = AssetDatabase.LoadAssetAtPath<SphericalPlanetStudioSettings>(SettingsPath);
            if (settings != null)
                return settings;

            string folder = "Assets/Project/Content/World/PlanetStudio";
            EnsureFolder("Assets/Project/Content/World");
            EnsureFolder(folder);
            settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            settings.layer1Enabled = true;
            settings.layer2Enabled = true;
            settings.layer3Enabled = true;
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
