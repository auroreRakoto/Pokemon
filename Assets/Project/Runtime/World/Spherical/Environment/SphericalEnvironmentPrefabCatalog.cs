using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Environment
{
    /// <summary>Unity asset references addressed by stable IDs from the placement JSON.</summary>
    [CreateAssetMenu(menuName = "Pokemon/World/Environment Prefab Catalog", fileName = "CAT_EnvironmentPrefabs")]
    public sealed class SphericalEnvironmentPrefabCatalog : ScriptableObject
    {
        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public bool TryGetPrefab(string id, out GameObject prefab)
        {
            prefab = null;
            if (string.IsNullOrWhiteSpace(id) || entries == null) return false;
            for (int i = 0; i < entries.Length; i++)
            {
                Entry entry = entries[i];
                if (entry != null && string.Equals(entry.id?.Trim(), id.Trim(), StringComparison.Ordinal))
                {
                    prefab = entry.prefab;
                    return prefab != null;
                }
            }
            return false;
        }

        public List<string> GetValidationIssues()
        {
            var issues = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (entries == null) { issues.Add("entries cannot be null."); return issues; }
            for (int i = 0; i < entries.Length; i++)
            {
                Entry entry = entries[i];
                if (entry == null) { issues.Add($"entries[{i}] is null."); continue; }
                if (string.IsNullOrWhiteSpace(entry.id)) issues.Add($"entries[{i}].id must not be empty.");
                else if (!ids.Add(entry.id.Trim())) issues.Add($"Duplicate prefab catalog id '{entry.id}'.");
            }
            return issues;
        }

        [Serializable]
        private sealed class Entry
        {
            public string id;
            public GameObject prefab;
        }
    }
}
