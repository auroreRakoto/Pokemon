
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Synchronizes Blender FBX exports into the Unity project.
/// Editor-only: Blender owns generation; this service owns copy/import/reporting.
/// </summary>
[InitializeOnLoad]
internal static class BlenderFbxSync
{
    private const double ScanIntervalSeconds = 3.0;
    private const string EditorPrefsPrefix = "Pokemon.BlenderFbxSync.Processed.";

    private static readonly Dictionary<string, FileStamp> ObservedFiles =
        new Dictionary<string, FileStamp>(StringComparer.OrdinalIgnoreCase);

    private static double _nextScanTime;
    private static bool _paused = true;
    private static bool _updateHooked;

    static BlenderFbxSync()
    {
        // Automatic polling is deliberately disabled by default. Use the
        // explicit Resume menu command to opt into continuous synchronization.
        Debug.Log("[Blender Sync] Initialisé en pause. Utiliser Tools/Blender Sync/Resume pour activer la surveillance.");
    }

    [MenuItem("Tools/Blender Sync/Rescan Now")]
    private static void RescanNow()
    {
        _nextScanTime = 0;
        Scan();
    }

    [MenuItem("Tools/Blender Sync/Pause")]
    private static void Pause()
    {
        _paused = true;
        if (_updateHooked)
        {
            EditorApplication.update -= Update;
            _updateHooked = false;
        }
        Debug.Log("[Blender Sync] Synchronisation mise en pause.");
    }

    [MenuItem("Tools/Blender Sync/Resume")]
    private static void Resume()
    {
        _paused = false;
        _nextScanTime = EditorApplication.timeSinceStartup + ScanIntervalSeconds;
        if (!_updateHooked)
        {
            EditorApplication.update += Update;
            _updateHooked = true;
        }
        Debug.Log("[Blender Sync] Synchronisation reprise (scan automatique toutes les 3 secondes).");
    }

    private static void Update()
    {
        if (_paused || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        if (EditorApplication.timeSinceStartup < _nextScanTime)
            return;

        _nextScanTime = EditorApplication.timeSinceStartup + ScanIntervalSeconds;
        Scan();
    }

    private static void Scan()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;

        if (string.IsNullOrEmpty(projectRoot))
            return;

        BlenderProjectConfig config;
        string configPath = Path.Combine(projectRoot, "Blender", "config", "project.json");

        try
        {
            if (!File.Exists(configPath))
                return;

            config = JsonUtility.FromJson<BlenderProjectConfig>(
                File.ReadAllText(configPath));
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[Blender Sync] Configuration illisible ({configPath}) : {exception.Message}");
            return;
        }

        string exportsRelative = config?.paths?.exports_root;
        string modelsRelative = config?.paths?.unity_models_root;

        if (string.IsNullOrWhiteSpace(exportsRelative) ||
            string.IsNullOrWhiteSpace(modelsRelative))
        {
            Debug.LogError(
                "[Blender Sync] project.json doit définir paths.exports_root et paths.unity_models_root.");
            return;
        }

        string exportsRoot = ResolveProjectPath(projectRoot, exportsRelative);
        string modelsRoot = ResolveProjectPath(projectRoot, modelsRelative);

        if (!Directory.Exists(exportsRoot))
            return;

        string[] manifests;

        try
        {
            manifests = Directory.GetFiles(
                exportsRoot,
                "*.manifest.json",
                SearchOption.AllDirectories);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[Blender Sync] Impossible de parcourir {exportsRoot} : {exception.Message}");
            return;
        }

        var candidates = new List<ManifestCandidate>();

        int invalidManifests = 0;

        foreach (string manifestPath in manifests)
        {
            try
            {
                BlenderManifest manifest =
                    JsonUtility.FromJson<BlenderManifest>(
                        File.ReadAllText(manifestPath));

                if (manifest == null ||
                    string.IsNullOrWhiteSpace(manifest.asset_id) ||
                    manifest.export == null ||
                    manifest.unity == null)
                {
                    throw new InvalidDataException(
                        "Champs requis absents (asset_id, export ou unity).");
                }

                if (!string.Equals(
                    manifest.export.format,
                    "fbx",
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException(
                        "Seuls les exports FBX sont pris en charge.");
                }

                string fbxRelative = manifest.export.path;

                if (string.IsNullOrWhiteSpace(fbxRelative))
                {
                    fbxRelative = Path.GetFileName(manifestPath)
                        .Replace(".manifest.json", ".fbx");
                }

                string fbxPath = Path.GetFullPath(
                    Path.IsPathRooted(fbxRelative)
                        ? fbxRelative
                        : Path.Combine(
                            Path.GetDirectoryName(manifestPath) ?? exportsRoot,
                            fbxRelative));

                EnsureInside(fbxPath, exportsRoot, "FBX source");

                if (!File.Exists(fbxPath))
                {
                    throw new FileNotFoundException(
                        "FBX associé au manifeste introuvable.",
                        fbxPath);
                }

                string destinationRelative = NormalizeDestination(
                    manifest.unity.destination,
                    modelsRelative);

                string destinationFull = ResolveProjectPath(
                    projectRoot,
                    Path.Combine(modelsRelative, destinationRelative));

                EnsureInside(destinationFull, modelsRoot, "Destination Unity");

                bool testOnly =
                    manifest.extra != null && manifest.extra.test_only;

                ValidateDestination(destinationRelative, testOnly);

                if (string.IsNullOrWhiteSpace(manifest.export.profile))
                {
                    throw new InvalidDataException(
                        "Le profil d'export (export.profile) est absent.");
                }

                candidates.Add(new ManifestCandidate(
                    manifestPath,
                    fbxPath,
                    destinationRelative,
                    destinationFull,
                    manifest));
            }
            catch (Exception exception)
            {
                invalidManifests++;

                Debug.LogError(
                    $"[Blender Sync] Manifeste invalide : {manifestPath}\n{exception.Message}"
                );
            }
        }

        var duplicateDestinations = candidates
            .GroupBy(
                candidate => candidate.DestinationFull,
                StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.Manifest.asset_id).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        int imported = 0;
        int unchanged = 0;
        int failed = 0;
        int waiting = 0;

        foreach (ManifestCandidate candidate in candidates)
        {
            if (duplicateDestinations.TryGetValue(
                candidate.DestinationFull,
                out string[] conflictingIds))
            {
                Debug.LogError(
                    $"[Blender Sync] Destination dupliquée : {candidate.DestinationRelative}. " +
                    $"Assets concernés : {string.Join(", ", conflictingIds)}");

                failed++;
                continue;
            }

            try
            {
                FileStamp stamp = FileStamp.Read(
                    candidate.FbxPath,
                    candidate.ManifestPath);

                if (!ObservedFiles.TryGetValue(
                    candidate.FbxPath,
                    out FileStamp previous) ||
                    !previous.Equals(stamp))
                {
                    ObservedFiles[candidate.FbxPath] = stamp;
                    waiting++;
                    continue;
                }

                bool testOnly =
                    candidate.Manifest.extra != null &&
                    candidate.Manifest.extra.test_only;

                if (!testOnly &&
                    !IsProfileValidated(config, candidate.Manifest.export.profile))
                {
                    Debug.LogWarning(
                        $"[Blender Sync] En attente de validation round-trip : " +
                        $"{candidate.Manifest.asset_id} ({candidate.Manifest.export.profile}). " +
                        "Les exports de production ne sont pas transférés.");

                    waiting++;
                    continue;
                }

                string signature = stamp.Signature;
                string preferenceKey =
                    EditorPrefsPrefix + candidate.Manifest.asset_id;

                if (File.Exists(candidate.DestinationFull) &&
                    EditorPrefs.GetString(preferenceKey, string.Empty) == signature)
                {
                    unchanged++;
                    continue;
                }

                Directory.CreateDirectory(
                    Path.GetDirectoryName(candidate.DestinationFull) ?? modelsRoot);

                string assetPath = ToAssetPath(
                    candidate.DestinationFull,
                    projectRoot);

                using (FileReplacement replacement = FileReplacement.Begin(
                    candidate.FbxPath,
                    candidate.DestinationFull))
                {
                    try
                    {
                        AssetDatabase.ImportAsset(
                            assetPath,
                            ImportAssetOptions.ForceUpdate |
                            ImportAssetOptions.ForceSynchronousImport);

                        ApplyImporterProfile(
                            assetPath,
                            candidate.Manifest.export.profile);

                        if (AssetDatabase.LoadMainAssetAtPath(assetPath) == null)
                        {
                            throw new InvalidOperationException(
                                "Unity n'a pas créé d'asset importable à la destination attendue.");
                        }

                        replacement.Commit();
                    }
                    catch
                    {
                        replacement.Rollback();

                        if (File.Exists(candidate.DestinationFull))
                        {
                            AssetDatabase.ImportAsset(
                                assetPath,
                                ImportAssetOptions.ForceUpdate |
                                ImportAssetOptions.ForceSynchronousImport);
                        }
                        else
                        {
                            AssetDatabase.DeleteAsset(assetPath);
                        }

                        throw;
                    }
                }

                EditorPrefs.SetString(preferenceKey, signature);
                imported++;

                Debug.Log(
                    $"[Blender Sync] IMPORTÉ : {candidate.Manifest.asset_id}\n" +
                    $"Source : {candidate.FbxPath}\n" +
                    $"Destination : {assetPath}\n" +
                    $"Profil : {candidate.Manifest.export.profile}");
            }
            catch (Exception exception)
            {
                failed++;

                Debug.LogError(
                    $"[Blender Sync] ÉCHEC : {candidate.Manifest.asset_id}\n" +
                    $"Source : {candidate.FbxPath}\n" +
                    $"Destination : {candidate.DestinationRelative}\n{exception}");
            }
        }

        int scanned = candidates.Count;

        Debug.Log(
            $"[Blender Sync] Scan report: " +
            $"{manifests.Length} manifest(s) found, " +
            $"{scanned} asset(s) scanned, " +
            $"{invalidManifests} invalid manifest(s), " +
            $"{imported} imported/updated, " +
            $"{unchanged} unchanged, " +
            $"{waiting} waiting, " +
            $"{failed} failed."
        );
    }

    private static void ApplyImporterProfile(string assetPath, string profile)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;

        if (importer == null)
        {
            throw new InvalidOperationException(
                "Le fichier a été copié mais Unity ne l'a pas reconnu comme ModelImporter.");
        }

        // Import FBX materials and preserve material slots.
        // Unity shader authoring remains a separate responsibility.
        // importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        // importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
        // importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        // importer.materialSearch = ModelImporterMaterialSearch.Local;
        
        // Preserve material slots without extracting or creating Unity materials.
        importer.materialImportMode = ModelImporterMaterialImportMode.None;

        importer.importCameras = false;
        importer.importLights = false;
        importer.importBlendShapes = true;

        importer.meshCompression = ModelImporterMeshCompression.Off;
        importer.isReadable = false;

        // Unity 6 replacements for the deprecated optimizeMesh property.
        importer.optimizeMeshPolygons = true;
        importer.optimizeMeshVertices = true;

        importer.importNormals = ModelImporterNormals.Import;
        importer.importTangents = ModelImporterTangents.CalculateMikk;

        bool isStatic = string.Equals(
            profile, "static_mesh", StringComparison.OrdinalIgnoreCase);

        bool isSkinned = string.Equals(
            profile, "skinned_mesh", StringComparison.OrdinalIgnoreCase);

        bool isAnimated = string.Equals(
            profile, "animated_mesh", StringComparison.OrdinalIgnoreCase);

        if (!isStatic && !isSkinned && !isAnimated)
        {
            throw new InvalidDataException(
                $"Profil FBX non reconnu : {profile}");
        }

        importer.importAnimation = isAnimated;

        importer.animationType = isStatic
            ? ModelImporterAnimationType.None
            : ModelImporterAnimationType.Generic;

        if (!isStatic)
        {
            importer.avatarSetup =
                ModelImporterAvatarSetup.CreateFromThisModel;
        }

        importer.SaveAndReimport();
    }

    private static bool IsProfileValidated(
        BlenderProjectConfig config,
        string profile)
    {
        if (config?.round_trip == null)
            return false;

        if (string.Equals(
            config.round_trip.status,
            "VALIDATED",
            StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(
                    config.round_trip.validated_profile) ||
                string.Equals(
                    config.round_trip.validated_profile,
                    profile,
                    StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static void ValidateDestination(string relative, bool testOnly)
    {
        string[] segments = relative.Split('/');

        if (segments.Length < 2)
        {
            throw new InvalidDataException(
                "La destination doit contenir un domaine et un nom de fichier.");
        }

        if (!string.Equals(
            Path.GetExtension(relative),
            ".fbx",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "La destination doit se terminer par .fbx.");
        }

        bool hasTestSegment = segments.Any(segment =>
            string.Equals(
                segment,
                "_ART_TEST",
                StringComparison.OrdinalIgnoreCase));

        if (hasTestSegment && !testOnly)
        {
            throw new InvalidDataException(
                "Une destination _ART_TEST exige extra.test_only=true.");
        }

        if (testOnly && !hasTestSegment)
        {
            throw new InvalidDataException(
                "Un manifeste test_only doit viser un dossier _ART_TEST.");
        }

        string domain = segments[0];

        string[] allowedDomains =
        {
            "Pokemon",
            "Environment",
            "Collectibles",
            "Props",
            "Shared"
        };

        if (!allowedDomains.Contains(
            domain,
            StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"Domaine Unity non autorisé : {domain}");
        }

        if (!testOnly && segments.Length >= 3)
        {
            string category = segments[1];

            var allowed = new Dictionary<string, string[]>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["Environment"] = new[]
                {
                    "Trees", "Vegetation", "Rocks", "Ground",
                    "Water", "Structures", "NaturalFeatures"
                },

                ["Collectibles"] = new[]
                {
                    "Berries", "Food", "Materials", "QuestItems"
                },

                ["Props"] = new[]
                {
                    "Furniture", "Containers", "Tools",
                    "Decorations", "Interactive"
                }
            };

            if (allowed.TryGetValue(domain, out string[] categories) &&
                !categories.Contains(category, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Catégorie '{category}' non documentée pour {domain}. " +
                    "Ajouter la catégorie à l'architecture avant transfert.");
            }
        }
    }

    private static string NormalizeDestination(
        string destination,
        string modelsRootRelative)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new InvalidDataException(
                "unity.destination est absent.");
        }

        string normalized = destination.Replace('\\', '/').Trim();

        string fullPrefix =
            modelsRootRelative.Replace('\\', '/').TrimEnd('/') + "/";

        if (normalized.StartsWith(
            fullPrefix,
            StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized.Substring(fullPrefix.Length);
        }

        if (Path.IsPathRooted(normalized) ||
            normalized.Contains(":") ||
            normalized.StartsWith("/", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                "unity.destination doit être relatif à Models/.");
        }

        string[] segments = normalized.Split('/');

        if (segments.Any(segment =>
            string.IsNullOrWhiteSpace(segment) ||
            segment == "." ||
            segment == ".."))
        {
            throw new InvalidDataException(
                "unity.destination contient un segment vide ou interdit.");
        }

        return string.Join("/", segments);
    }

    private sealed class FileReplacement : IDisposable
    {
        private readonly string _destination;
        private readonly string _backup;

        private bool _committed;
        private bool _rolledBack;

        private FileReplacement(string destination, string backup)
        {
            _destination = destination;
            _backup = backup;
        }

        public static FileReplacement Begin(
            string source,
            string destination)
        {
            string directory =
                Path.GetDirectoryName(destination) ??
                throw new InvalidOperationException("Destination sans dossier.");

            string staged = Path.Combine(
                directory,
                "." + Path.GetFileName(destination) + ".sync-tmp");

            string backup = Path.Combine(
                directory,
                "." + Path.GetFileName(destination) + ".sync-backup");

            if (File.Exists(staged))
                File.Delete(staged);

            if (File.Exists(backup))
                File.Delete(backup);

            File.Copy(source, staged, true);

            if (new FileInfo(staged).Length != new FileInfo(source).Length)
            {
                File.Delete(staged);
                throw new IOException(
                    "La copie temporaire est incomplète (taille différente).");
            }

            bool hadOriginal = File.Exists(destination);

            try
            {
                if (hadOriginal)
                    File.Move(destination, backup);

                File.Move(staged, destination);

                return new FileReplacement(
                    destination,
                    hadOriginal ? backup : null);
            }
            catch
            {
                if (File.Exists(staged))
                    File.Delete(staged);

                if (hadOriginal &&
                    File.Exists(backup) &&
                    !File.Exists(destination))
                {
                    File.Move(backup, destination);
                }

                throw;
            }
        }

        public void Commit()
        {
            _committed = true;

            if (!string.IsNullOrEmpty(_backup) &&
                File.Exists(_backup))
            {
                File.Delete(_backup);
            }
        }

        public void Rollback()
        {
            if (_committed || _rolledBack)
                return;

            if (File.Exists(_destination))
                File.Delete(_destination);

            if (!string.IsNullOrEmpty(_backup) &&
                File.Exists(_backup))
            {
                File.Move(_backup, _destination);
            }

            _rolledBack = true;
        }

        public void Dispose()
        {
            if (!_committed && !_rolledBack)
                Rollback();
        }
    }

    private static string ResolveProjectPath(
        string projectRoot,
        string relative)
    {
        if (Path.IsPathRooted(relative))
        {
            throw new InvalidDataException(
                "Les chemins de configuration doivent être relatifs à la racine du projet.");
        }

        return Path.GetFullPath(
            Path.Combine(
                projectRoot,
                relative.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static void EnsureInside(
        string path,
        string root,
        string label)
    {
        string rootFull =
            Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) +
            Path.DirectorySeparatorChar;

        string pathFull = Path.GetFullPath(path);

        if (!pathFull.StartsWith(
            rootFull,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"{label} hors du dossier autorisé : {pathFull}");
        }
    }

    private static string ToAssetPath(
        string fullPath,
        string projectRoot)
    {
        string relative = fullPath.Substring(
            projectRoot.TrimEnd(Path.DirectorySeparatorChar).Length + 1);

        return relative.Replace('\\', '/');
    }

    [Serializable]
    private sealed class BlenderProjectConfig
    {
        public BlenderPaths paths;
        public BlenderRoundTrip round_trip;
    }

    [Serializable]
    private sealed class BlenderPaths
    {
        public string exports_root;
        public string unity_models_root;
    }

    [Serializable]
    private sealed class BlenderRoundTrip
    {
        public string status;
        public string validated_profile;
    }

    [Serializable]
    private sealed class BlenderManifest
    {
        public string asset_id;
        public BlenderExport export;
        public BlenderUnity unity;
        public BlenderManifestExtra extra;
    }

    [Serializable]
    private sealed class BlenderExport
    {
        public string format;
        public string path;
        public string profile;
    }

    [Serializable]
    private sealed class BlenderUnity
    {
        public string destination;
    }

    [Serializable]
    private sealed class BlenderManifestExtra
    {
        public bool test_only;
    }

    private sealed class ManifestCandidate
    {
        public readonly string ManifestPath;
        public readonly string FbxPath;
        public readonly string DestinationRelative;
        public readonly string DestinationFull;
        public readonly BlenderManifest Manifest;

        public ManifestCandidate(
            string manifestPath,
            string fbxPath,
            string destinationRelative,
            string destinationFull,
            BlenderManifest manifest)
        {
            ManifestPath = manifestPath;
            FbxPath = fbxPath;
            DestinationRelative = destinationRelative;
            DestinationFull = destinationFull;
            Manifest = manifest;
        }
    }

    private struct FileStamp : IEquatable<FileStamp>
    {
        private readonly long _fbxLength;
        private readonly long _fbxTicks;
        private readonly long _manifestLength;
        private readonly long _manifestTicks;

        public string Signature =>
            $"{_fbxLength}:{_fbxTicks}:{_manifestLength}:{_manifestTicks}";

        private FileStamp(FileInfo fbx, FileInfo manifest)
        {
            _fbxLength = fbx.Length;
            _fbxTicks = fbx.LastWriteTimeUtc.Ticks;
            _manifestLength = manifest.Length;
            _manifestTicks = manifest.LastWriteTimeUtc.Ticks;
        }

        public static FileStamp Read(string fbx, string manifest)
        {
            return new FileStamp(
                new FileInfo(fbx),
                new FileInfo(manifest));
        }

        public bool Equals(FileStamp other)
        {
            return _fbxLength == other._fbxLength &&
                   _fbxTicks == other._fbxTicks &&
                   _manifestLength == other._manifestLength &&
                   _manifestTicks == other._manifestTicks;
        }

        public override bool Equals(object obj)
        {
            return obj is FileStamp other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Signature.GetHashCode();
        }
    }
}
#endif
