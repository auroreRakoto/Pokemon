#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates editable Unity prefab assets from environment FBX model assets.
/// Categories and subcategories are inferred from the FBX folder hierarchy.
/// </summary>
public sealed class EnvironmentPrefabGeneratorWindow : EditorWindow
{
    private const string DefaultSourceRoot =
        "Assets/Project/Content/Models/Environment";

    private const string DefaultDestinationRoot =
        "Assets/Project/Content/Prefabs/Environment";

    private string _sourceRoot = DefaultSourceRoot;
    private string _destinationRoot = DefaultDestinationRoot;
    private bool _overwriteExisting;
    private bool _addBoxCollider;
    private Vector2 _scroll;
    private List<PrefabCandidate> _candidates = new List<PrefabCandidate>();

    [MenuItem("Tools/Assets/Generate Environment Prefabs")]
    private static void Open()
    {
        EnvironmentPrefabGeneratorWindow window =
            GetWindow<EnvironmentPrefabGeneratorWindow>();

        window.titleContent = new GUIContent("Environment Prefabs");
        window.minSize = new Vector2(560f, 420f);
        window.Scan();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("FBX → Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Les catégories sont déduites automatiquement des dossiers situés sous le dossier source. " +
            "Les dossiers _ART_TEST sont ignorés.",
            MessageType.Info);

        EditorGUILayout.Space(6f);
        DrawFolderField("Dossier des FBX", ref _sourceRoot);
        DrawFolderField("Dossier de destination", ref _destinationRoot);

        EditorGUILayout.Space(6f);
        _overwriteExisting = EditorGUILayout.ToggleLeft(
            "Remplacer les prefabs déjà existants", _overwriteExisting);
        _addBoxCollider = EditorGUILayout.ToggleLeft(
            "Ajouter un BoxCollider à chaque prefab", _addBoxCollider);

        EditorGUILayout.Space(8f);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Analyser les FBX", GUILayout.Height(28f)))
                Scan();

            GUI.enabled = _candidates.Count > 0;
            if (GUILayout.Button(
                $"Générer {_candidates.Count} prefab(s)", GUILayout.Height(28f)))
                Generate();
            GUI.enabled = true;
        }

        EditorGUILayout.Space(8f);
        DrawCandidateList();
    }

    private static void DrawFolderField(string label, ref string path)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PrefixLabel(label);
            path = EditorGUILayout.TextField(path);

            if (GUILayout.Button("…", GUILayout.Width(28f)))
            {
                string absolute = Path.GetFullPath(path);
                string selected = EditorUtility.OpenFolderPanel(
                    $"Choisir : {label}", absolute, string.Empty);

                if (!string.IsNullOrEmpty(selected))
                {
                    string projectRoot =
                        Directory.GetParent(Application.dataPath).FullName;

                    if (selected.StartsWith(
                        projectRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        path = selected.Substring(projectRoot.Length)
                            .TrimStart(Path.DirectorySeparatorChar)
                            .Replace('\\', '/');
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(
                            "Dossier invalide",
                            "Le dossier doit se trouver dans le projet Unity.",
                            "OK");
                    }
                }
            }
        }
    }

    private void Scan()
    {
        _candidates.Clear();

        string source = NormalizeAssetPath(_sourceRoot);
        if (!IsExistingAssetFolder(source))
        {
            Repaint();
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { source });

        foreach (string guid in guids)
        {
            string assetPath = NormalizeAssetPath(
                AssetDatabase.GUIDToAssetPath(guid));

            if (!assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                continue;

            string relativePath = assetPath.Substring(source.Length).TrimStart('/');
            string[] segments = relativePath.Split('/');

            if (segments.Any(segment => string.Equals(
                segment, "_ART_TEST", StringComparison.OrdinalIgnoreCase)))
                continue;

            if (segments.Length < 2)
                continue;

            GameObject model =
                AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (model == null)
                continue;

            string relativeDirectory = string.Join(
                "/", segments.Take(segments.Length - 1));

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string prefabName = MakePrefabName(fileName);

            string destinationPath = NormalizeAssetPath(
                $"{_destinationRoot}/{relativeDirectory}/{prefabName}.prefab");

            bool exists =
                AssetDatabase.LoadMainAssetAtPath(destinationPath) != null;

            _candidates.Add(new PrefabCandidate(
                assetPath, destinationPath, model, exists));
        }

        _candidates = _candidates
            .OrderBy(candidate => candidate.DestinationPath)
            .ToList();

        Repaint();
    }

    private void DrawCandidateList()
    {
        EditorGUILayout.LabelField(
            $"Résultats : {_candidates.Count} FBX", EditorStyles.boldLabel);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        if (_candidates.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Aucun FBX de production trouvé. Vérifie le dossier source puis clique sur Analyser les FBX.",
                MessageType.Warning);
        }

        foreach (PrefabCandidate candidate in _candidates)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField(candidate.Model.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Source", candidate.SourcePath);
                EditorGUILayout.LabelField("Prefab", candidate.DestinationPath);

                if (candidate.Exists)
                {
                    EditorGUILayout.LabelField(
                        _overwriteExisting
                            ? "Le prefab existant sera remplacé."
                            : "Déjà existant — ignoré.",
                        EditorStyles.miniLabel);
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void Generate()
    {
        string source = NormalizeAssetPath(_sourceRoot);
        string destination = NormalizeAssetPath(_destinationRoot);

        if (!IsExistingAssetFolder(source) || !IsValidAssetPath(destination))
        {
            EditorUtility.DisplayDialog(
                "Chemin invalide",
                "Le dossier source doit exister dans le projet Unity. " +
                "Le dossier de destination doit être situé sous Assets, mais il sera créé automatiquement s'il n'existe pas.",
                "OK");
            return;
        }

        // Create the destination hierarchy before StartAssetEditing so Unity
        // can resolve every folder while prefab assets are being written.
        try
        {
            EnsureAssetFolder(destination);
        }
        catch (Exception exception)
        {
            EditorUtility.DisplayDialog(
                "Destination invalide", exception.Message, "OK");
            return;
        }

        int created = 0;
        int skipped = 0;
        int failed = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (PrefabCandidate candidate in _candidates)
            {
                bool destinationExists =
                    AssetDatabase.LoadMainAssetAtPath(candidate.DestinationPath) != null;

                if (destinationExists && !_overwriteExisting)
                {
                    skipped++;
                    continue;
                }

                GameObject wrapper = null;

                try
                {
                    string destinationDirectory =
                        Path.GetDirectoryName(candidate.DestinationPath)
                            ?.Replace('\\', '/');

                    EnsureAssetFolder(destinationDirectory);

                    wrapper = new GameObject(
                        Path.GetFileNameWithoutExtension(candidate.DestinationPath));

                    GameObject modelInstance =
                        PrefabUtility.InstantiatePrefab(candidate.Model) as GameObject;

                    if (modelInstance == null)
                        throw new InvalidOperationException(
                            $"Impossible d'instancier le modèle : {candidate.SourcePath}");

                    modelInstance.name = candidate.Model.name;
                    modelInstance.transform.SetParent(wrapper.transform, false);

                    if (_addBoxCollider)
                        AddBoxCollider(wrapper, modelInstance);

                    bool success;
                    PrefabUtility.SaveAsPrefabAsset(
                        wrapper, candidate.DestinationPath, out success);

                    if (!success)
                        throw new InvalidOperationException(
                            $"Unity n'a pas confirmé la création de {candidate.DestinationPath}");

                    created++;
                }
                catch (Exception exception)
                {
                    failed++;
                    Debug.LogError(
                        $"[Environment Prefabs] Échec pour {candidate.SourcePath}\n{exception}");
                }
                finally
                {
                    if (wrapper != null)
                        DestroyImmediate(wrapper);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        Scan();

        string report =
            $"Créés/remplacés : {created}\n" +
            $"Ignorés : {skipped}\n" +
            $"Échecs : {failed}";

        Debug.Log($"[Environment Prefabs] Terminé.\n{report}");
        EditorUtility.DisplayDialog("Génération terminée", report, "OK");
    }

    private static void AddBoxCollider(GameObject root, GameObject modelInstance)
    {
        Renderer[] renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.center = root.transform.InverseTransformPoint(bounds.center);

        Vector3 scale = root.transform.lossyScale;
        collider.size = new Vector3(
            Mathf.Abs(scale.x) > 0.0001f ? bounds.size.x / Mathf.Abs(scale.x) : bounds.size.x,
            Mathf.Abs(scale.y) > 0.0001f ? bounds.size.y / Mathf.Abs(scale.y) : bounds.size.y,
            Mathf.Abs(scale.z) > 0.0001f ? bounds.size.z / Mathf.Abs(scale.z) : bounds.size.z);
    }

    private static string MakePrefabName(string modelName)
    {
        const string prefix = "SM_";
        if (modelName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            modelName = modelName.Substring(prefix.Length);

        return "PF_" + modelName;
    }

    private static bool IsExistingAssetFolder(string path)
    {
        return IsValidAssetPath(path) && AssetDatabase.IsValidFolder(path);
    }

    private static bool IsValidAssetPath(string path)
    {
        string normalized = NormalizeAssetPath(path);
        return normalized == "Assets" ||
               normalized.StartsWith("Assets/", StringComparison.Ordinal);
    }

    private static string NormalizeAssetPath(string path)
    {
        return (path ?? string.Empty).Replace('\\', '/').Trim().TrimEnd('/');
    }

    private static void EnsureAssetFolder(string folderPath)
    {
        string normalized = NormalizeAssetPath(folderPath);
        string[] segments = normalized.Split('/');

        if (segments.Length == 0 || segments[0] != "Assets")
            throw new InvalidOperationException(
                $"Le dossier doit être situé sous Assets : {folderPath}");

        string current = "Assets";
        for (int i = 1; i < segments.Length; i++)
        {
            string next = current + "/" + segments[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, segments[i]);
            current = next;
        }
    }

    private sealed class PrefabCandidate
    {
        public readonly string SourcePath;
        public readonly string DestinationPath;
        public readonly GameObject Model;
        public readonly bool Exists;

        public PrefabCandidate(
            string sourcePath, string destinationPath,
            GameObject model, bool exists)
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            Model = model;
            Exists = exists;
        }
    }
}
#endif
