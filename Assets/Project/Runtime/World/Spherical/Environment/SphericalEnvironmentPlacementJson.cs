using System;
using System.Collections.Generic;
using UnityEngine;
using Pokemon.Project.World.Spherical.Geography;

namespace Pokemon.Project.World.Spherical.Environment
{
    /// <summary>JSON DTOs and validated runtime representation for WORLD-011 placement rules.</summary>
    public static class SphericalEnvironmentPlacementJson
    {
        public const int SupportedSchemaVersion = 1;

        [Serializable]
        public sealed class Document
        {
            public int schemaVersion = SupportedSchemaVersion;
            public int globalSeedOffset;
            public float globalDensityMultiplier = 1f;
            public int maxInstances = 10000;
            public float defaultSurfaceOffsetMeters = 0.05f;
            public float spawnFocusRadiusMeters = 80f;
            public float spawnFocusDensityMultiplier = 20f;
            public Biome[] biomes = Array.Empty<Biome>();
        }

        [Serializable]
        public sealed class Biome
        {
            public string name;
            public bool enabled = true;
            public Rule[] rules = Array.Empty<Rule>();
        }

        [Serializable]
        public sealed class Rule
        {
            public string id;
            public bool enabled;
            public float selectionWeight = 1f;
            public float targetDensityPerSquareKilometre;
            public float minimumSpacingMeters;
            public string spacingGroup;
            public float minimumScale = 1f;
            public float maximumScale = 1f;
            public float minimumYawDegrees;
            public float maximumYawDegrees = 360f;
            public float surfaceOffsetMeters;
            public float exclusionRadiusMeters;
            public float maximumSlopeDegrees = 30f;
            public bool useAltitudeRange;
            public float minimumAltitudeMeters;
            public float maximumAltitudeMeters;
            public bool useMoistureRange;
            public float minimumMoisture;
            public float maximumMoisture = 1f;
            public bool useTemperatureRange;
            public float minimumTemperature;
            public float maximumTemperature = 1f;
        }

        public sealed class ResolvedRule
        {
            public SphericalBiome Biome { get; internal set; }
            public Rule Settings { get; internal set; }
            public GameObject Prefab { get; internal set; }
        }

        public sealed class ResolvedDocument
        {
            public Document Settings { get; internal set; }
            public IReadOnlyList<ResolvedRule> Rules { get; internal set; }
        }

        public static bool TryLoad(TextAsset json, SphericalEnvironmentPrefabCatalog catalog,
            out ResolvedDocument resolved, out List<string> issues)
        {
            resolved = null;
            issues = new List<string>();
            if (json == null) { issues.Add("Placement JSON TextAsset is not assigned."); return false; }
            if (catalog == null) { issues.Add("Environment prefab catalog is not assigned."); return false; }

            Document document;
            try { document = JsonUtility.FromJson<Document>(json.text); }
            catch (Exception exception) { issues.Add("Could not parse placement JSON: " + exception.Message); return false; }
            if (document == null) { issues.Add("Placement JSON is empty or invalid."); return false; }

            if (document.schemaVersion != SupportedSchemaVersion)
                issues.Add($"Unsupported schemaVersion {document.schemaVersion}; supported version is {SupportedSchemaVersion}.");
            NonNegative(document.globalDensityMultiplier, "globalDensityMultiplier", issues);
            if (document.maxInstances < 1) issues.Add("maxInstances must be at least 1.");
            NonNegative(document.defaultSurfaceOffsetMeters, "defaultSurfaceOffsetMeters", issues);
            NonNegative(document.spawnFocusRadiusMeters, "spawnFocusRadiusMeters", issues);
            NonNegative(document.spawnFocusDensityMultiplier, "spawnFocusDensityMultiplier", issues);

            issues.AddRange(catalog.GetValidationIssues());
            var resolvedRules = new List<ResolvedRule>();
            var biomeIds = new HashSet<SphericalBiome>();
            var ruleIds = new HashSet<string>(StringComparer.Ordinal);
            if (document.biomes == null) issues.Add("biomes cannot be null.");
            else
            {
                for (int b = 0; b < document.biomes.Length; b++)
                {
                    Biome biome = document.biomes[b];
                    if (biome == null) { issues.Add($"biomes[{b}] is null."); continue; }
                    if (!Enum.TryParse(biome.name, true, out SphericalBiome biomeId) ||
                        (biomeId != SphericalBiome.Plains && biomeId != SphericalBiome.Forest))
                    { issues.Add($"biomes[{b}].name '{biome.name}' is unsupported; only Plains and Forest are supported."); continue; }
                    if (!biomeIds.Add(biomeId)) issues.Add($"Duplicate biome '{biome.name}'.");
                    if (biome.rules == null) { issues.Add($"biomes[{b}].rules cannot be null."); continue; }
                    for (int r = 0; r < biome.rules.Length; r++)
                    {
                        Rule rule = biome.rules[r];
                        string path = $"biomes[{b}].rules[{r}]";
                        if (rule == null) { issues.Add(path + " is null."); continue; }
                        if (string.IsNullOrWhiteSpace(rule.id)) issues.Add(path + ".id must not be empty.");
                        else if (!ruleIds.Add(biomeId + ":" + rule.id.Trim())) issues.Add("Duplicate placement rule id '" + rule.id + "' in biome " + biome.name + ".");
                        if (rule.enabled && !catalog.TryGetPrefab(rule.id, out _))
                            issues.Add(path + $" is enabled but catalog prefab '{rule.id}' is missing.");
                        NonNegative(rule.selectionWeight, path + ".selectionWeight", issues);
                        NonNegative(rule.targetDensityPerSquareKilometre, path + ".targetDensityPerSquareKilometre", issues);
                        NonNegative(rule.minimumSpacingMeters, path + ".minimumSpacingMeters", issues);
                        NonNegative(rule.surfaceOffsetMeters, path + ".surfaceOffsetMeters", issues);
                        NonNegative(rule.exclusionRadiusMeters, path + ".exclusionRadiusMeters", issues);
                        Positive(rule.minimumScale, path + ".minimumScale", issues);
                        Positive(rule.maximumScale, path + ".maximumScale", issues);
                        if (Finite(rule.minimumScale) && Finite(rule.maximumScale) && rule.maximumScale < rule.minimumScale)
                            issues.Add(path + ".maximumScale must be >= minimumScale.");
                        FiniteValue(rule.minimumYawDegrees, path + ".minimumYawDegrees", issues);
                        FiniteValue(rule.maximumYawDegrees, path + ".maximumYawDegrees", issues);
                        if (!Finite(rule.maximumSlopeDegrees) || rule.maximumSlopeDegrees < 0f || rule.maximumSlopeDegrees > 90f)
                            issues.Add(path + ".maximumSlopeDegrees must be within 0..90.");
                        if (rule.useAltitudeRange) Range(rule.minimumAltitudeMeters, rule.maximumAltitudeMeters, path + " altitude", false, issues);
                        if (rule.useMoistureRange) Range(rule.minimumMoisture, rule.maximumMoisture, path + " moisture", true, issues);
                        if (rule.useTemperatureRange) Range(rule.minimumTemperature, rule.maximumTemperature, path + " temperature", true, issues);
                        if (biome.enabled && rule.enabled && catalog.TryGetPrefab(rule.id, out GameObject prefab))
                            resolvedRules.Add(new ResolvedRule { Biome = biomeId, Settings = rule, Prefab = prefab });
                    }
                }
            }
            if (issues.Count > 0) return false;
            resolved = new ResolvedDocument { Settings = document, Rules = resolvedRules };
            return true;
        }

        private static void Range(float min, float max, string label, bool normalized, List<string> issues)
        {
            if (!Finite(min) || !Finite(max)) { issues.Add(label + " values must be finite."); return; }
            if (normalized && (min < 0f || max > 1f)) issues.Add(label + " must stay within 0..1.");
            if (max < min) issues.Add(label + " maximum must be >= minimum.");
        }
        private static void NonNegative(float value, string label, List<string> issues)
        { if (!Finite(value) || value < 0f) issues.Add(label + " must be finite and non-negative."); }
        private static void Positive(float value, string label, List<string> issues)
        { if (!Finite(value) || value <= 0f) issues.Add(label + " must be finite and greater than zero."); }
        private static void FiniteValue(float value, string label, List<string> issues)
        { if (!Finite(value)) issues.Add(label + " must be finite."); }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
