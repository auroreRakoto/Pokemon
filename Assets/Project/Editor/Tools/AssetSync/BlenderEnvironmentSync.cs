#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Imports Blender environment FBX exports from Blender/exports/environment.
/// Safe defaults:
/// - Dry-run is the first menu action.
/// - Test-only exports require an explicit import menu action.
/// - Destinations are restricted to explicit Environment/_ART_TEST folders.
/// - Existing files are updated only when the ownership ledger proves that
///   this tool previously imported that exact asset at that exact path.
/// - Unity .meta files are never copied, deleted, or edited by this tool.
/// </summary>
public static class BlenderEnvironmentSync
{
    private const string ExportRootRelative = "Blender/exports/environment";
    private static readonly string[] AllowedUnityRoots =
    {
        "Assets/Project/Content/Models/Environment/_ART_TEST",
        "Assets/Project/Content/Models/Environment/Vegetation/_ART_TEST"
    };
    private const string LedgerPath =
        "ProjectSettings/BlenderAssetSync/ownership.json";

    [Serializable]
    private sealed class Manifest
    {
        public int schema_version;
        public string asset_id;
        public string domain;
        public string asset_type;
        public ExportData export;
        public UnityData unity;
        public ValidationData validation;
        public ExtraData extra;
    }

    [Serializable]
    private sealed class ExportData
    {
        public string format;
        public string path;
        public string profile;
    }

    [Serializable]
    private sealed class UnityData
    {
        public string destination;
    }

    [Serializable]
    private sealed class ValidationData
    {
        public string status;
        public string[] errors;
    }

    [Serializable]
    private sealed class ExtraData
    {
        public bool test_only;
        public bool test_asset;
    }

    [Serializable]
    private sealed class OwnershipLedger
    {
        public List<OwnershipEntry> entries = new List<OwnershipEntry>();
    }

    [Serializable]
    private sealed class OwnershipEntry
    {
        public string asset_id;
        public string asset_path;
        public string source_fbx;
        public string imported_sha256;
        public string unity_guid;
    }

    private sealed class Candidate
    {
        public string ManifestPath;
        public string SourceFbx;
        public string Destination;
        public Manifest Data;
    }

    [MenuItem("Pokemon/Tools/Blender Environment/1 - Dry Run")]
    public static void DryRun()
    {
        Execute(sync: false);
    }

    [MenuItem("Pokemon/Tools/Blender Environment/2 - Import TEST Assets (explicit opt-in)")]
    public static void ImportTestAssets()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Import Blender TEST assets?",
            "This imports validated FBX files whose manifests mark them as test_only/test_asset.\n\n" +
            "Destinations are restricted to these TEST folders:\n" +
            string.Join("\n", AllowedUnityRoots) +
            "\n\nExisting files are updated only when ownership can be verified. " +
            "Existing .meta files are preserved.",
            "Import TEST assets",
            "Cancel");

        if (!confirmed)
            return;

        Execute(sync: true);
    }

    private static void Execute(bool sync)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string exportRoot = Path.GetFullPath(
            Path.Combine(projectRoot, ExportRootRelative));

        if (!Directory.Exists(exportRoot))
        {
            EditorUtility.DisplayDialog(
                "Blender Environment Sync",
                "Export folder not found:\n" + exportRoot,
                "OK");
            return;
        }

        OwnershipLedger ledger = LoadLedger(projectRoot);
        List<string> report = new List<string>();
        int errors = 0;
        List<Candidate> candidates = DiscoverCandidates(
            projectRoot, exportRoot, report, ref errors);

        HashSet<string> seenAssetIds = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);
        HashSet<string> seenDestinations = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        int imported = 0;
        int updated = 0;
        int skipped = 0;

        foreach (Candidate candidate in candidates)
        {
            Manifest manifest = candidate.Data;

            if (!seenAssetIds.Add(manifest.asset_id))
            {
                report.Add("ERROR duplicate asset_id: " + manifest.asset_id);
                errors++;
                continue;
            }

            if (!seenDestinations.Add(candidate.Destination))
            {
                report.Add("ERROR duplicate destination: " + candidate.Destination);
                errors++;
                continue;
            }

            bool isTestOnly =
                manifest.extra != null &&
                (manifest.extra.test_only || manifest.extra.test_asset);

            if (isTestOnly && !sync)
            {
                report.Add("DRY-RUN / TEST-ONLY (not imported): " +
                           manifest.asset_id + " -> " + candidate.Destination);
                skipped++;
                continue;
            }

            if (!sync)
            {
                report.Add("DRY-RUN / READY: " +
                           manifest.asset_id + " -> " + candidate.Destination);
                continue;
            }

            try
            {
                bool existed = File.Exists(ToAbsolute(projectRoot, candidate.Destination));
                ImportCandidate(projectRoot, candidate, ledger);

                if (existed)
                    updated++;
                else
                    imported++;

                report.Add((existed ? "UPDATED: " : "IMPORTED: ") +
                           manifest.asset_id + " -> " + candidate.Destination);
            }
            catch (Exception exception)
            {
                errors++;
                report.Add("ERROR " + manifest.asset_id + ": " + exception.Message);
                Debug.LogException(exception);
            }
        }

        if (sync)
            SaveLedger(projectRoot, ledger);

        AssetDatabase.Refresh();

        string summary =
            "Manifests found: " + candidates.Count +
            "\nImported: " + imported +
            "\nUpdated: " + updated +
            "\nSkipped: " + skipped +
            "\nErrors: " + errors +
            "\n\nSee the Unity Console for the full report.";

        string fullReport = string.Join("\n", report);
        if (errors > 0)
            Debug.LogError("Blender Environment Sync:\n" + fullReport);
        else
            Debug.Log("Blender Environment Sync:\n" + fullReport);

        EditorUtility.DisplayDialog(
            sync ? "Blender import finished" : "Blender dry run finished",
            summary,
            "OK");
    }

    private static List<Candidate> DiscoverCandidates(
        string projectRoot,
        string exportRoot,
        List<string> report,
        ref int errors)
    {
        List<Candidate> result = new List<Candidate>();

        string[] manifestFiles = Directory.GetFiles(
            exportRoot, "*.manifest.json", SearchOption.AllDirectories);

        foreach (string manifestPath in manifestFiles.OrderBy(
                     path => path, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                string json = File.ReadAllText(manifestPath, Encoding.UTF8);
                Manifest manifest = JsonUtility.FromJson<Manifest>(json);

                ValidateManifest(manifest, manifestPath);

                string exportFileName = manifest.export.path;
                if (Path.GetFileName(exportFileName) != exportFileName ||
                    !exportFileName.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException(
                        "export.path must be a simple FBX filename, without directories.");

                string sourceFbx = Path.GetFullPath(
                    Path.Combine(Path.GetDirectoryName(manifestPath), exportFileName));

                EnsureInsideRoot(sourceFbx, exportRoot, "Source FBX");

                if (!File.Exists(sourceFbx))
                    throw new FileNotFoundException("FBX referenced by manifest is missing.", sourceFbx);

                string destination = NormalizeAssetPath(manifest.unity.destination);
                ValidateDestination(destination);

                result.Add(new Candidate
                {
                    ManifestPath = manifestPath,
                    SourceFbx = sourceFbx,
                    Destination = destination,
                    Data = manifest
                });
            }
            catch (Exception exception)
            {
                errors++;
                report.Add("INVALID MANIFEST " + manifestPath + ": " + exception.Message);
                Debug.LogError("Blender manifest rejected: " + manifestPath, null);
            }
        }

        return result;
    }

    private static void ValidateManifest(Manifest manifest, string path)
    {
        if (manifest == null)
            throw new InvalidDataException("JSON could not be parsed.");

        if (manifest.schema_version != 1)
            throw new InvalidDataException(
                "Unsupported schema_version: " + manifest.schema_version);

        if (string.IsNullOrWhiteSpace(manifest.asset_id) ||
            !manifest.asset_id.StartsWith("environment.", StringComparison.Ordinal))
            throw new InvalidDataException("asset_id must start with 'environment.'.");

        if (manifest.domain != "environment")
            throw new InvalidDataException("domain must be 'environment'.");

        if (manifest.asset_type != "static_mesh")
            throw new InvalidDataException("Only static_mesh assets are supported.");

        if (manifest.export == null ||
            manifest.export.format != "fbx" ||
            manifest.export.profile != "static_mesh" ||
            string.IsNullOrWhiteSpace(manifest.export.path))
            throw new InvalidDataException("Expected export format/profile: fbx/static_mesh.");

        if (manifest.unity == null ||
            string.IsNullOrWhiteSpace(manifest.unity.destination))
            throw new InvalidDataException("unity.destination is missing.");

        if (manifest.validation == null ||
            manifest.validation.status != "passed")
            throw new InvalidDataException("Manifest validation.status is not 'passed'.");

        if (manifest.validation.errors != null &&
            manifest.validation.errors.Length > 0)
            throw new InvalidDataException("Manifest contains validation errors.");
    }

    private static void ValidateDestination(string assetPath)
    {
        bool inAllowedRoot = AllowedUnityRoots.Any(root =>
            assetPath.StartsWith(root + "/", StringComparison.Ordinal));

        if (!inAllowedRoot)
            throw new InvalidDataException(
                "Destination outside the allowed TEST folders: " + assetPath);

        if (!assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Unity destination must end in .fbx.");

        if (assetPath.Contains("/../") || assetPath.Contains("\\"))
            throw new InvalidDataException("Destination contains an unsafe path segment.");
    }

    private static void ImportCandidate(
        string projectRoot,
        Candidate candidate,
        OwnershipLedger ledger)
    {
        string absoluteDestination = ToAbsolute(projectRoot, candidate.Destination);
        string destinationDirectory = Path.GetDirectoryName(absoluteDestination);

        Directory.CreateDirectory(destinationDirectory);

        string sourceHash = ComputeSha256(candidate.SourceFbx);
        OwnershipEntry owned = ledger.entries.FirstOrDefault(
            entry => entry.asset_id == candidate.Data.asset_id);

        if (File.Exists(absoluteDestination))
        {
            if (owned == null)
                throw new IOException(
                    "Destination already exists but is not owned by this importer. " +
                    "Refusing to overwrite: " + candidate.Destination);

            if (!string.Equals(owned.asset_path, candidate.Destination,
                    StringComparison.Ordinal))
                throw new IOException("Ownership ledger path mismatch.");

            string currentGuid = AssetDatabase.AssetPathToGUID(candidate.Destination);
            if (string.IsNullOrEmpty(currentGuid) ||
                !string.Equals(currentGuid, owned.unity_guid, StringComparison.Ordinal))
                throw new IOException(
                    "Unity GUID mismatch. The existing asset may have been replaced or moved.");

            string currentHash = ComputeSha256(absoluteDestination);
            if (!string.Equals(currentHash, owned.imported_sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new IOException(
                    "Existing FBX was modified outside this importer. Refusing to overwrite.");
        }
        else if (owned != null)
        {
            throw new IOException(
                "Ledger claims this asset is managed, but destination is missing. " +
                "Resolve manually before importing.");
        }

        // Copy only the FBX. Never copy, remove, or rewrite its .meta file.
        File.Copy(candidate.SourceFbx, absoluteDestination, overwrite: File.Exists(absoluteDestination));

        AssetDatabase.ImportAsset(
            candidate.Destination,
            ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

        string guidAfterImport = AssetDatabase.AssetPathToGUID(candidate.Destination);
        if (string.IsNullOrEmpty(guidAfterImport))
            throw new IOException("Unity did not assign a GUID after importing the FBX.");

        OwnershipEntry entry = owned ?? new OwnershipEntry();
        entry.asset_id = candidate.Data.asset_id;
        entry.asset_path = candidate.Destination;
        entry.source_fbx = MakeProjectRelative(projectRoot, candidate.SourceFbx);
        entry.imported_sha256 = sourceHash;
        entry.unity_guid = guidAfterImport;

        if (owned == null)
            ledger.entries.Add(entry);
    }

    private static OwnershipLedger LoadLedger(string projectRoot)
    {
        string path = Path.Combine(projectRoot, LedgerPath);
        if (!File.Exists(path))
            return new OwnershipLedger();

        OwnershipLedger ledger = JsonUtility.FromJson<OwnershipLedger>(
            File.ReadAllText(path, Encoding.UTF8));

        if (ledger == null)
            throw new InvalidDataException("Ownership ledger JSON is invalid.");

        if (ledger.entries == null)
            ledger.entries = new List<OwnershipEntry>();

        return ledger;
    }

    private static void SaveLedger(string projectRoot, OwnershipLedger ledger)
    {
        string path = Path.Combine(projectRoot, LedgerPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(ledger, true), new UTF8Encoding(false));
    }

    private static string NormalizeAssetPath(string path)
    {
        return (path ?? string.Empty).Replace('\\', '/').Trim();
    }

    private static string ToAbsolute(string projectRoot, string assetPath)
    {
        string combined = Path.GetFullPath(Path.Combine(
            projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar)));

        string assetsRoot = Path.GetFullPath(Path.Combine(projectRoot, "Assets"));
        EnsureInsideRoot(combined, assetsRoot, "Unity destination");
        return combined;
    }

    private static void EnsureInsideRoot(string candidate, string root, string label)
    {
        string fullCandidate = Path.GetFullPath(candidate)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string fullRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (!fullCandidate.StartsWith(
                fullRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(label + " escapes its allowed root.");
    }

    private static string MakeProjectRelative(string projectRoot, string absolutePath)
    {
        string root = Path.GetFullPath(projectRoot)
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string full = Path.GetFullPath(absolutePath);
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            return full;
        return full.Substring(root.Length).Replace('\\', '/');
    }

    private static string ComputeSha256(string path)
    {
        using (SHA256 sha = SHA256.Create())
        using (FileStream stream = File.OpenRead(path))
        {
            byte[] hash = sha.ComputeHash(stream);
            StringBuilder builder = new StringBuilder(hash.Length * 2);
            foreach (byte value in hash)
                builder.Append(value.ToString("x2"));
            return builder.ToString();
        }
    }
}
#endif
