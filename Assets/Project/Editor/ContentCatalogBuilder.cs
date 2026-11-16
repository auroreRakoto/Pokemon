#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Finds project content and updates the central catalog.
/// Safe to run repeatedly: it updates one catalog asset in place.
/// </summary>
public static class ContentCatalogBuilder
{
    private const string ContentRoot = "Assets/Project/Content";
    private const string CatalogFolder = "Assets/Project/Content/Definitions";
    private const string CatalogPath =
        CatalogFolder + "/ProjectContentCatalog.asset";

    [MenuItem("Pokemon/Content/Build or Update Catalog")]
    public static void BuildOrUpdateCatalog()
    {
        EnsureFolderExists(CatalogFolder);

        ProjectContentCatalog catalog =
            AssetDatabase.LoadAssetAtPath<ProjectContentCatalog>(CatalogPath);

        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<ProjectContentCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            Debug.Log("Created ProjectContentCatalog at " + CatalogPath);
        }

        List<string> report = new List<string>();

        PokemonDefinition[] pokemonDefinitions =
            FindAssets<PokemonDefinition>("t:PokemonDefinition", report);

        BiomeDefinition[] biomeDefinitions =
            FindAssets<BiomeDefinition>("t:BiomeDefinition", report);

        WorldDefinition[] worldDefinitions =
            FindAssets<WorldDefinition>("t:WorldDefinition", report);

        List<GameObject> pokemonPrefabs = new List<GameObject>();

        foreach (PokemonDefinition definition in pokemonDefinitions)
        {
            if (definition == null)
                continue;

            if (definition.pokemonPrefab == null)
            {
                report.Add(
                    "ERROR: Pokémon definition '" + definition.name +
                    "' has no pokemonPrefab reference.");
                continue;
            }

            if (!pokemonPrefabs.Contains(definition.pokemonPrefab))
                pokemonPrefabs.Add(definition.pokemonPrefab);
        }

        FruitBush fruitBushPrefab =
            FindUniquePrefabWithComponent<FruitBush>(report);

        GameObject blueberryPrefab =
            FindUniquePrefabByName("Blueberry", report);

        GameObject redberryPrefab =
            FindUniquePrefabByName("Redberry", report);

        GameObject[] grassPrefabs =
        {
            FindPreferredGrassPrefab("ShortGrass", report),
            FindPreferredGrassPrefab("TallGrass", report),
            FindPreferredGrassPrefab("WildGrass", report)
        };

        // SerializedObject avoids exposing mutable authoring fields publicly.
        SerializedObject serializedCatalog = new SerializedObject(catalog);

        SetObjectArray(
            serializedCatalog, "pokemonDefinitions", pokemonDefinitions);
        SetObjectArray(
            serializedCatalog, "biomeDefinitions", biomeDefinitions);
        SetObjectArray(
            serializedCatalog, "worldDefinitions", worldDefinitions);
        SetObjectArray(
            serializedCatalog, "pokemonPrefabs", pokemonPrefabs.ToArray());
        SetObject(
            serializedCatalog, "fruitBushPrefab", fruitBushPrefab);
        SetObject(
            serializedCatalog, "blueberryPrefab", blueberryPrefab);
        SetObject(
            serializedCatalog, "redberryPrefab", redberryPrefab);
        SetObjectArray(
            serializedCatalog, "grassPrefabs", grassPrefabs);

        serializedCatalog.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        report.Insert(0, "Catalog updated: " + CatalogPath);
        report.Add("Pokémon definitions: " + pokemonDefinitions.Length);
        report.Add("Pokémon prefabs referenced by definitions: " +
                   pokemonPrefabs.Count);
        report.Add("Biome definitions: " + biomeDefinitions.Length);
        report.Add("World definitions: " + worldDefinitions.Length);
        report.Add("Grass prefab slots populated: " +
                   grassPrefabs.Count(prefab => prefab != null));

        Debug.Log(string.Join("\n", report));
        EditorUtility.DisplayDialog(
            "Project Content Catalog",
            "Catalog updated.\n\n" +
            "Pokémon: " + pokemonDefinitions.Length +
            "\nBiomes: " + biomeDefinitions.Length +
            "\nWorlds: " + worldDefinitions.Length +
            "\nGrass prefabs: " +
            grassPrefabs.Count(prefab => prefab != null) +
            "\n\nOpen the Console for warnings and details.",
            "OK");
    }

    [MenuItem("Pokemon/Content/Validate Catalog")]
    public static void ValidateCatalog()
    {
        ProjectContentCatalog catalog =
            AssetDatabase.LoadAssetAtPath<ProjectContentCatalog>(CatalogPath);

        if (catalog == null)
        {
            EditorUtility.DisplayDialog(
                "Validate Catalog",
                "Catalog not found. Run 'Build or Update Catalog' first.",
                "OK");
            return;
        }

        List<string> errors = new List<string>();

        if (catalog.PokemonDefinitions == null ||
            catalog.PokemonDefinitions.Length == 0)
            errors.Add("No Pokémon definitions found.");

        if (catalog.BiomeDefinitions == null ||
            catalog.BiomeDefinitions.Length == 0)
            errors.Add("No biome definitions found.");

        if (catalog.WorldDefinitions == null ||
            catalog.WorldDefinitions.Length == 0)
            errors.Add("No world definitions found.");

        if (catalog.FruitBushPrefab == null)
            errors.Add("FruitBush prefab is missing or ambiguous.");

        if (catalog.BlueberryPrefab == null)
            errors.Add("Blueberry prefab is missing or ambiguous.");

        if (catalog.RedberryPrefab == null)
            errors.Add("Redberry prefab is missing or ambiguous.");

        if (catalog.GrassPrefabs == null ||
            catalog.GrassPrefabs.Length != 3 ||
            catalog.GrassPrefabs.Any(prefab => prefab == null))
            errors.Add("One or more grass prefab slots are missing.");

        if (catalog.PokemonDefinitions != null)
        {
            foreach (PokemonDefinition definition in catalog.PokemonDefinitions)
            {
                if (definition == null)
                {
                    errors.Add("Catalog contains a null Pokémon definition.");
                    continue;
                }

                if (definition.pokemonPrefab == null)
                    errors.Add(
                        "Missing prefab on Pokémon definition: " +
                        definition.name);
            }
        }

        if (errors.Count == 0)
        {
            Debug.Log("Project Content Catalog validation PASSED.");
            EditorUtility.DisplayDialog(
                "Validate Catalog",
                "Validation passed. No missing required references found.",
                "OK");
        }
        else
        {
            string message = string.Join("\n", errors);
            Debug.LogError("Project Content Catalog validation FAILED:\n" +
                           message);
            EditorUtility.DisplayDialog(
                "Validate Catalog",
                "Validation found " + errors.Count +
                " issue(s). See the Console for details.",
                "OK");
        }
    }

    private static T[] FindAssets<T>(
        string filter,
        List<string> report) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets(
            filter, new[] { ContentRoot });

        List<T> assets = new List<T>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null)
                assets.Add(asset);
        }

        return assets
            .OrderBy(asset => asset.name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static FruitBush FindUniquePrefabWithComponent<T>(
        List<string> report) where T : Component
    {
        List<FruitBush> matches = new List<FruitBush>();

        foreach (string path in FindPrefabPaths())
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            T component = prefab.GetComponent<T>();
            if (component == null)
                continue;

            FruitBush fruitBush = component as FruitBush;
            if (fruitBush != null)
                matches.Add(fruitBush);
        }

        if (matches.Count == 1)
            return matches[0];

        if (matches.Count == 0)
            report.Add("ERROR: No prefab with component " +
                       typeof(T).Name + " found.");

        else
            report.Add(
                "ERROR: Multiple prefabs with component " +
                typeof(T).Name + " found: " +
                string.Join(", ", matches.Select(
                    match => AssetDatabase.GetAssetPath(match.gameObject))));

        return null;
    }

    private static GameObject FindUniquePrefabByName(
        string prefabName,
        List<string> report)
    {
        List<string> matches = FindPrefabPaths()
            .Where(path => string.Equals(
                System.IO.Path.GetFileNameWithoutExtension(path),
                prefabName,
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 1)
            return AssetDatabase.LoadAssetAtPath<GameObject>(matches[0]);

        if (matches.Count == 0)
            report.Add("ERROR: Prefab not found: " + prefabName);
        else
            report.Add(
                "ERROR: Multiple prefabs named " + prefabName + ": " +
                string.Join(", ", matches));

        return null;
    }

    private static GameObject FindPreferredGrassPrefab(
        string prefabName,
        List<string> report)
    {
        List<string> matches = FindPrefabPaths()
            .Where(path => string.Equals(
                System.IO.Path.GetFileNameWithoutExtension(path),
                prefabName,
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
        {
            report.Add("ERROR: Grass prefab not found: " + prefabName);
            return null;
        }

        // Prefer the dedicated Grass subfolder over a same-named legacy copy.
        List<string> preferred = matches
            .Where(path => path.Replace('\\', '/').Contains("/Grass/"))
            .ToList();

        if (preferred.Count == 1)
        {
            if (matches.Count > 1)
                report.Add(
                    "WARNING: " + prefabName +
                    " has duplicate copies. Selected " + preferred[0] +
                    "; ignored: " +
                    string.Join(", ", matches.Except(preferred)));

            return AssetDatabase.LoadAssetAtPath<GameObject>(preferred[0]);
        }

        if (preferred.Count > 1 || matches.Count > 1)
        {
            report.Add(
                "ERROR: Ambiguous grass prefab '" + prefabName +
                "'. Resolve duplicates before assigning. Matches: " +
                string.Join(", ", matches));
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<GameObject>(matches[0]);
    }

    private static List<string> FindPrefabPaths()
    {
        return AssetDatabase.FindAssets(
                "t:Prefab", new[] { ContentRoot })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => !string.IsNullOrEmpty(path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void SetObject(
        SerializedObject target,
        string propertyName,
        UnityEngine.Object value)
    {
        SerializedProperty property =
            target.FindProperty(propertyName);

        if (property != null)
            property.objectReferenceValue = value;
        else
            Debug.LogError(
                "Catalog field not found: " + propertyName);
    }

    private static void SetObjectArray(
        SerializedObject target,
        string propertyName,
        UnityEngine.Object[] values)
    {
        SerializedProperty property =
            target.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogError(
                "Catalog array field not found: " + propertyName);
            return;
        }

        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue =
                values[i];
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = System.IO.Path.GetDirectoryName(folderPath)
            .Replace('\\', '/');
        string folderName = System.IO.Path.GetFileName(folderPath);

        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolderExists(parent);

        AssetDatabase.CreateFolder(parent, folderName);
    }
}
#endif
