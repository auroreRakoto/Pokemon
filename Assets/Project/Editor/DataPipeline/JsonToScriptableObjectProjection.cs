#if UNITY_EDITOR
using System;
using System.IO;
using Pokemon.Project.Data;
using UnityEditor;
using UnityEngine;

namespace Pokemon.Project.Editor.DataPipeline
{
    /// <summary>
    /// Builds disposable Unity projections from canonical JSON documents.
    /// JSON remains the only editable source of truth.
    /// </summary>
    public static class JsonToScriptableObjectProjection
    {
        private const string OutputRoot = "Assets/Project/Generated/Data";

        [MenuItem("Pokemon/Tools/Data Pipeline/Generate ScriptableObject Projections")]
        public static void GenerateAll()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                Debug.LogError("JSON Projection: could not resolve Unity project root.");
                return;
            }

            string dataRoot = Path.Combine(projectRoot, "Data");
            if (!Directory.Exists(dataRoot))
            {
                Debug.LogError($"JSON Projection: canonical Data folder not found: {dataRoot}");
                return;
            }

            try
            {
                int generated = 0;
                generated += GenerateWorlds(Path.Combine(dataRoot, "worlds"));
                generated += GenerateBiomes(Path.Combine(dataRoot, "biomes"));
                generated += GeneratePokemon(Path.Combine(dataRoot, "pokemon"));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"JSON Projection: generated/updated {generated} ScriptableObject projection(s). Sources remain in Data/.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"JSON Projection failed: {exception}");
            }
        }

        private static int GenerateWorlds(string sourceFolder)
        {
            int count = 0;
            foreach (string file in GetJsonFiles(sourceFolder))
            {
                WorldDefinitionData data = Parse<WorldDefinitionData>(file);
                if (data == null || !IsValidIdentity(data.id, file) || !IsValidWorld(data, file)) continue;
                string path = EnsureOutputPath("Worlds", data.id);
                WorldDefinitionAsset asset = AssetDatabase.LoadAssetAtPath<WorldDefinitionAsset>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<WorldDefinitionAsset>();
                    AssetDatabase.CreateAsset(asset, path);
                }
                asset.data = data;
                EditorUtility.SetDirty(asset);
                count++;
            }
            return count;
        }

        private static int GenerateBiomes(string sourceFolder)
        {
            int count = 0;
            foreach (string file in GetJsonFiles(sourceFolder))
            {
                BiomeDefinitionData data = Parse<BiomeDefinitionData>(file);
                if (data == null || !IsValidIdentity(data.id, file) || !IsValidBiome(data, file)) continue;
                string path = EnsureOutputPath("Biomes", data.id);
                BiomeDefinitionAsset asset = AssetDatabase.LoadAssetAtPath<BiomeDefinitionAsset>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<BiomeDefinitionAsset>();
                    AssetDatabase.CreateAsset(asset, path);
                }
                asset.data = data;
                EditorUtility.SetDirty(asset);
                count++;
            }
            return count;
        }

        private static int GeneratePokemon(string sourceFolder)
        {
            int count = 0;
            foreach (string file in GetJsonFiles(sourceFolder))
            {
                PokemonSpeciesData data = Parse<PokemonSpeciesData>(file);
                if (data == null || !IsValidIdentity(data.id, file) || !IsValidPokemon(data, file)) continue;
                string path = EnsureOutputPath("Pokemon", data.id);
                PokemonSpeciesDefinitionAsset asset = AssetDatabase.LoadAssetAtPath<PokemonSpeciesDefinitionAsset>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<PokemonSpeciesDefinitionAsset>();
                    AssetDatabase.CreateAsset(asset, path);
                }
                asset.data = data;
                EditorUtility.SetDirty(asset);
                count++;
            }
            return count;
        }

        private static string[] GetJsonFiles(string folder)
        {
            if (!Directory.Exists(folder)) return Array.Empty<string>();
            return Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
        }

        private static T Parse<T>(string file) where T : class
        {
            try
            {
                string json = File.ReadAllText(file);
                T data = JsonUtility.FromJson<T>(json);
                if (data == null) Debug.LogError($"JSON Projection: deserialization returned null: {file}");
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogError($"JSON Projection: failed to parse '{file}': {exception.Message}");
                return null;
            }
        }


        private static bool IsValidWorld(WorldDefinitionData data, string sourceFile)
        {
            bool valid = data.schemaVersion == 1 && data.size != null && data.origin != null &&
                         data.mapResolution != null && data.mapResolution.width >= 2 && data.mapResolution.height >= 2 &&
                         data.layers != null && data.biomeIds != null;
            if (!valid) Debug.LogError($"JSON Projection: world definition is incomplete or unsupported: '{sourceFile}'.");
            return valid;
        }

        private static bool IsValidBiome(BiomeDefinitionData data, string sourceFile)
        {
            bool valid = data.schemaVersion == 1 && data.conditions != null && data.presentation != null &&
                         data.conditions.humidity != null && data.conditions.temperature != null &&
                         data.conditions.vegetationDensity != null && data.conditions.height != null;
            if (!valid) Debug.LogError($"JSON Projection: biome definition is incomplete or unsupported: '{sourceFile}'.");
            return valid;
        }

        private static bool IsValidPokemon(PokemonSpeciesData data, string sourceFile)
        {
            bool valid = data.schemaVersion == 1 && data.types != null && data.habitatPreferences != null &&
                         data.habitatPreferences.preferredBiomeIds != null && data.habitatPreferences.humidity != null &&
                         data.habitatPreferences.vegetationDensity != null && data.presentation != null;
            if (!valid) Debug.LogError($"JSON Projection: Pokémon species definition is incomplete or unsupported: '{sourceFile}'.");
            return valid;
        }

        private static bool IsValidIdentity(string id, string sourceFile)
        {
            if (!string.IsNullOrWhiteSpace(id) && IsSafeId(id)) return true;
            Debug.LogError($"JSON Projection: invalid or missing stable id in '{sourceFile}'.");
            return false;
        }

        private static bool IsSafeId(string id)
        {
            foreach (char c in id)
            {
                if (!(char.IsLower(c) || char.IsDigit(c) || c == '_')) return false;
            }
            return true;
        }

        private static string EnsureOutputPath(string category, string id)
        {
            string folder = $"{OutputRoot}/{category}";
            EnsureAssetFolder(folder);
            return $"{folder}/{id}.asset";
        }

        private static void EnsureAssetFolder(string assetFolder)
        {
            string[] parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
