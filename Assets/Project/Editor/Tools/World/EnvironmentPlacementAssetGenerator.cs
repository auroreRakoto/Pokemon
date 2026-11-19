#if UNITY_EDITOR
using System.Collections.Generic;
using Pokemon.Project.World.Spherical.Environment;
using UnityEditor;
using UnityEditor.SceneManagement;
using Pokemon.Project.World.Spherical.Inspection;
using UnityEngine;

/// <summary>
/// Generates or refreshes the WORLD-011 environment placement catalog and config
/// by resolving stable logical IDs to the generated environment prefabs.
/// No manual Inspector assignment is required.
/// </summary>
public static class EnvironmentPlacementAssetGenerator
{
    private const string OutputFolder =
        "Assets/Project/Content/Settings/World/EnvironmentPlacement";
    private const string JsonPath =
        OutputFolder + "/CFG_SphericalEnvironmentPlacement.json";
    private const string CatalogPath =
        OutputFolder + "/CAT_EnvironmentPrefabs.asset";
    private const string ConfigPath =
        OutputFolder + "/CFG_EnvironmentPlacement.asset";
    private const string PrefabRoot =
        "Assets/Project/Content/Prefabs/Environment";

    private static readonly (string id, string relativePath)[] PrefabDefinitions =
    {
        ("grass_short", "Vegetation/Grass_Short/PF_Grass_Short.prefab"),
        ("grass_wild", "Vegetation/Wild_Grass/PF_Grass_Wild.prefab"),
        ("grass_tall", "Vegetation/Grass_Tall/PF_Grass_Tall.prefab"),
        ("reed_clump", "Vegetation/Reed_Clump/PF_Reed_Clump.prefab"),
        ("leaf_litter", "Ground/Leaf_Litter/PF_Leaf_Litter.prefab"),
        ("mushroom_red", "NaturalFeatures/Mushroom_Red/PF_Mushroom_Red.prefab"),
        ("fallen_log", "NaturalFeatures/Fallen_Log/PF_Fallen_Log.prefab"),
        ("tree_stump", "NaturalFeatures/Tree_Stump/PF_Tree_Stump.prefab"),
        ("stone_cluster_small", "Rocks/Stone_Cluster_Small/PF_Stone_Cluster_Small.prefab"),
        ("stone_cluster_large", "Rocks/Stone_Cluster_Large/PF_Stone_Cluster_Large.prefab")
    };

    [MenuItem("Tools/World/Generate Environment Placement Assets")]
    public static void Generate()
    {
        EnsureFolder(OutputFolder);

        TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(JsonPath);
        if (json == null)
        {
            EditorUtility.DisplayDialog("WORLD-011",
                "Placement JSON introuvable :\n" + JsonPath, "OK");
            return;
        }

        SphericalEnvironmentPrefabCatalog catalog =
            AssetDatabase.LoadAssetAtPath<SphericalEnvironmentPrefabCatalog>(CatalogPath);

        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<SphericalEnvironmentPrefabCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        var missing = new List<string>();
        var serializedCatalog = new SerializedObject(catalog);
        SerializedProperty entries = serializedCatalog.FindProperty("entries");
        entries.arraySize = PrefabDefinitions.Length;

        for (int i = 0; i < PrefabDefinitions.Length; i++)
        {
            var definition = PrefabDefinitions[i];
            string prefabPath = PrefabRoot + "/" + definition.relativePath;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("id").stringValue = definition.id;
            entry.FindPropertyRelative("prefab").objectReferenceValue = prefab;

            if (prefab == null)
                missing.Add(definition.id + " → " + prefabPath);
        }

        serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);

        SphericalEnvironmentPlacementConfig config =
            AssetDatabase.LoadAssetAtPath<SphericalEnvironmentPlacementConfig>(ConfigPath);

        if (config == null)
        {
            config = ScriptableObject.CreateInstance<SphericalEnvironmentPlacementConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }

        var serializedConfig = new SerializedObject(config);
        serializedConfig.FindProperty("placementJson").objectReferenceValue = json;
        serializedConfig.FindProperty("prefabCatalog").objectReferenceValue = catalog;
        serializedConfig.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(config);

        // Wire the generated config into the inspection bootstrap in the open scene.
        SphericalBiomeInspectionBootstrap[] bootstraps =
            Resources.FindObjectsOfTypeAll<SphericalBiomeInspectionBootstrap>();
        for (int i = 0; i < bootstraps.Length; i++)
        {
            if (bootstraps[i] == null || !bootstraps[i].gameObject.scene.IsValid())
                continue;
            var bootstrapObject = new SerializedObject(bootstraps[i]);
            SerializedProperty configProperty =
                bootstrapObject.FindProperty("environmentPlacementConfig");
            if (configProperty != null)
            {
                configProperty.objectReferenceValue = config;
                bootstrapObject.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(bootstraps[i].gameObject.scene);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);

        string summary =
            "Configuration et catalogue générés automatiquement.\n\n" +
            "Catalogue : " + CatalogPath + "\n" +
            "Configuration : " + ConfigPath + "\n\n" +
            "Prefabs trouvés : " + (PrefabDefinitions.Length - missing.Count) +
            "/" + PrefabDefinitions.Length;

        if (missing.Count > 0)
            summary += "\n\nPrefabs manquants (entrées laissées vides) :\n- " +
                       string.Join("\n- ", missing);
        else
            summary += "\n\nTous les prefabs ont été associés.";

        EditorUtility.DisplayDialog("WORLD-011 — génération terminée", summary, "OK");
    }

    private static void EnsureFolder(string assetFolder)
    {
        string[] parts = assetFolder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
