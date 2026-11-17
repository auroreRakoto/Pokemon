using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Pokemon.Project.DataServices.Loading
{
    /// <summary>
    /// Loads the canonical world-generation JSON without coupling the generation core
    /// to file paths or UnityEditor. Editor tools load the source file; player builds
    /// load its build-time copy from StreamingAssets.
    /// </summary>
    public static class WorldGenerationConfigLoader
    {
        public const string RelativeSourcePath = "Project/Data/WorldGeneration/world_generation.json";
        public const string StreamingAssetsFileName = "world_generation.json";

#if UNITY_EDITOR
        public static Pokemon.Project.World.Generation.WorldGenerationSettings LoadForEditor()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string path = Path.Combine(projectRoot, "Assets", RelativeSourcePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
                throw new FileNotFoundException("Canonical world-generation JSON was not found.", path);

            return ParseAndValidate(File.ReadAllText(path));
        }
#endif

        /// <summary>
        /// Runtime/player loader. Invoke from a MonoBehaviour coroutine.
        /// The build pipeline copies the canonical JSON into StreamingAssets.
        /// </summary>
        public static System.Collections.IEnumerator LoadForRuntime(Action<Pokemon.Project.World.Generation.WorldGenerationSettings> onSuccess,
            Action<string> onError)
        {
            string path = Path.Combine(Application.streamingAssetsPath, StreamingAssetsFileName);
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke("Could not load world-generation JSON from StreamingAssets: " + request.error);
                    yield break;
                }

                try { onSuccess?.Invoke(ParseAndValidate(request.downloadHandler.text)); }
                catch (Exception exception) { onError?.Invoke(exception.Message); }
            }
        }

        private static Pokemon.Project.World.Generation.WorldGenerationSettings ParseAndValidate(string json)
        {
            var settings = JsonUtility.FromJson<Pokemon.Project.World.Generation.WorldGenerationSettings>(json);
            if (settings == null)
                throw new InvalidOperationException("World-generation JSON could not be deserialized.");
            settings.Validate();
            return settings;
        }
    }
}
