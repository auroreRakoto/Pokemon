#if UNITY_EDITOR
using Pokemon.Project.World.Spherical.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pokemon.Project.Editor.World.Spherical
{
    /// <summary>Creates the dedicated, reproducible WORLD-001 demo scene.</summary>
    public static class SphericalWorldDemoSceneTool
    {
        private const string ScenePath = "Assets/Project/Content/Scenes/Worlds/SphericalWorldDemo.unity";

        [MenuItem("Tools/World/Create Spherical World Demo Scene")]
        public static void CreateDemoScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Spherical World Demo", "Stop Play Mode before creating the demo scene.", "OK");
                return;
            }

            bool proceed = !SceneManager.GetActiveScene().isDirty ||
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            if (!proceed)
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject bootstrapObject = new GameObject("WORLD-001_SphericalWorldDemo");
            bootstrapObject.AddComponent<SphericalWorldDemoBootstrap>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = bootstrapObject;
            EditorGUIUtility.PingObject(bootstrapObject);

            Debug.Log($"Created WORLD-001 demo scene at {ScenePath}. Press Play to generate the planet.");
        }
    }
}
#endif
