#if UNITY_EDITOR
using System.IO;
using Pokemon.Project.World.Spherical.Inspection;
using Pokemon.Project.World.Spherical.Studio;
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
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (!Directory.Exists(SceneFolder))
                Directory.CreateDirectory(SceneFolder);
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
            Debug.Log($"Created playable biome inspection scene at {ScenePath}. Enter Play Mode to generate the current WORLD-002 fields and spawn on plains.", root);
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
