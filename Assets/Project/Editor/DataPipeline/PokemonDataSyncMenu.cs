#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pokemon.Project.Editor.DataPipeline
{
    public static class PokemonDataSyncMenu
    {
        private const string GeneratedFolderName = "PokemonData";

        [MenuItem("Tools/Pokemon Data/Sync Data to StreamingAssets")]
        public static void SyncDataToStreamingAssets()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                Debug.LogError("Pokemon Data Sync: could not resolve Unity project root.");
                return;
            }

            string source = Path.Combine(projectRoot, "Data");
            if (!Directory.Exists(source))
            {
                Debug.LogError($"Pokemon Data Sync: canonical Data folder not found: {source}");
                return;
            }

            string streamingAssets = Path.Combine(Application.dataPath, "StreamingAssets");
            string destination = Path.Combine(streamingAssets, GeneratedFolderName);

            try
            {
                // This destination is reserved exclusively for generated copies of canonical Data/.
                if (Directory.Exists(destination)) Directory.Delete(destination, true);
                Directory.CreateDirectory(destination);
                CopyDirectory(source, destination);
                AssetDatabase.Refresh();
                Debug.Log($"Pokemon Data Sync: copied canonical data from '{source}' to '{destination}'.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Pokemon Data Sync failed: {exception}");
            }
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (string file in Directory.GetFiles(source))
            {
                string target = Path.Combine(destination, Path.GetFileName(file));
                File.Copy(file, target, true);
            }
            foreach (string directory in Directory.GetDirectories(source))
            {
                string target = Path.Combine(destination, Path.GetFileName(directory));
                CopyDirectory(directory, target);
            }
        }
    }
}
#endif
