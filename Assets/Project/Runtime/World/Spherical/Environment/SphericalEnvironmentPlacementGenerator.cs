using System;
using System.Collections.Generic;
using UnityEngine;
using Pokemon.Project.World.Spherical.Geography;

namespace Pokemon.Project.World.Spherical.Environment
{
    /// <summary>
    /// Deterministic first-pass placement on the existing WORLD-002 cube-sphere fields.
    /// Does not regenerate geography or modify terrain meshes.
    /// </summary>
    public static class SphericalEnvironmentPlacementGenerator
    {
        public sealed class Result
        {
            public int CandidateCount { get; internal set; }
            public int PlacedCount { get; internal set; }
            public int RejectedByBiomeOrWater { get; internal set; }
            public int RejectedByFilters { get; internal set; }
            public int RejectedBySpacing { get; internal set; }
            public int RejectedByGlobalLimit { get; internal set; }
        }

        private sealed class PlacedRecord
        {
            public Vector3 Position;
            public string Group;
            public float Radius;
        }

        public static Result Generate(
            SphericalGeographicFieldSet fields,
            SphericalEnvironmentPlacementJson.ResolvedDocument document,
            Transform parent,
            Vector3 planetCenter,
            Vector3 spawnDirection)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (document == null || document.Settings == null || document.Rules == null)
                throw new ArgumentException("A resolved placement document is required.", nameof(document));
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            var result = new Result();
            var placedRecords = new List<PlacedRecord>();
            int resolution = fields.Resolution;
            float radius = fields.PlanetRadiusMeters;
            int globalLimit = document.Settings.maxInstances;
            int globalSeed = fields.Seed ^ document.Settings.globalSeedOffset;

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                CubeSphereFace face = (CubeSphereFace)faceIndex;
                for (int y = 0; y < resolution; y++)
                for (int x = 0; x < resolution; x++)
                {
                    // One candidate cell per non-overlapping face patch. Sampling is jittered
                    // inside each patch; shared face borders are never independently sampled.
                    float u0 = -1f + 2f * x / resolution;
                    float u1 = -1f + 2f * (x + 1) / resolution;
                    float v0 = -1f + 2f * y / resolution;
                    float v1 = -1f + 2f * (y + 1) / resolution;
                    Vector3 a = CubeSphereTopology.GetDirection(face, u0, v0);
                    Vector3 b = CubeSphereTopology.GetDirection(face, u1, v0);
                    Vector3 c = CubeSphereTopology.GetDirection(face, u1, v1);
                    Vector3 d = CubeSphereTopology.GetDirection(face, u0, v1);
                    double cellAreaKm2 = SphericalQuadArea(a, b, c, d) * radius * radius / 1000000.0;

                    for (int ruleIndex = 0; ruleIndex < document.Rules.Count; ruleIndex++)
                    {
                        SphericalEnvironmentPlacementJson.ResolvedRule resolved = document.Rules[ruleIndex];
                        SphericalEnvironmentPlacementJson.Rule rule = resolved.Settings;
                        if (rule == null || !rule.enabled || resolved.Prefab == null)
                            continue;

                        Vector3 cellCenterDirection = CubeSphereTopology.GetDirection(
                            face, (u0 + u1) * 0.5f, (v0 + v1) * 0.5f);
                        float spawnDistanceMeters = Vector3.Angle(
                            cellCenterDirection, spawnDirection.normalized) * Mathf.Deg2Rad * radius;
                        float focusMultiplier = spawnDistanceMeters <= document.Settings.spawnFocusRadiusMeters
                            ? document.Settings.spawnFocusDensityMultiplier
                            : 1f;

                        double expected = cellAreaKm2 *
                            rule.targetDensityPerSquareKilometre *
                            Mathf.Max(0f, rule.selectionWeight) *
                            document.Settings.globalDensityMultiplier *
                            focusMultiplier;
                        if (expected <= 0.0) continue;

                        int baseCount = (int)Math.Floor(expected);
                        double fraction = expected - baseCount;
                        uint random = Hash(globalSeed, faceIndex, x, y, ruleIndex);
                        int count = baseCount + (UnitFloat(ref random) < fraction ? 1 : 0);

                        for (int candidate = 0; candidate < count; candidate++)
                        {
                            if (result.PlacedCount >= globalLimit)
                            {
                                result.RejectedByGlobalLimit++;
                                continue;
                            }

                            result.CandidateCount++;
                            float u = Mathf.Lerp(u0, u1, UnitFloat(ref random));
                            float v = Mathf.Lerp(v0, v1, UnitFloat(ref random));
                            Vector3 direction = CubeSphereTopology.GetDirection(face, u, v);

                            if (fields.SampleWaterClass(direction) != SphericalWaterClass.Land ||
                                fields.SampleBiome(direction) != resolved.Biome)
                            {
                                result.RejectedByBiomeOrWater++;
                                continue;
                            }

                            float slope = fields.Sample(SphericalGeographicFieldType.SlopeDegrees, direction);
                            if (slope > rule.maximumSlopeDegrees ||
                                (rule.useAltitudeRange && !InRange(
                                    fields.Sample(SphericalGeographicFieldType.AltitudeMeters, direction),
                                    rule.minimumAltitudeMeters, rule.maximumAltitudeMeters)) ||
                                (rule.useMoistureRange && !InRange(
                                    fields.Sample(SphericalGeographicFieldType.MoistureNormalized, direction),
                                    rule.minimumMoisture, rule.maximumMoisture)) ||
                                (rule.useTemperatureRange && !InRange(
                                    fields.Sample(SphericalGeographicFieldType.TemperatureNormalized, direction),
                                    rule.minimumTemperature, rule.maximumTemperature)))
                            {
                                result.RejectedByFilters++;
                                continue;
                            }

                            float altitude = fields.Sample(SphericalGeographicFieldType.AltitudeMeters, direction);
                            float surfaceOffset = document.Settings.defaultSurfaceOffsetMeters + rule.surfaceOffsetMeters;
                            Vector3 worldPosition = planetCenter + direction * (radius + altitude + surfaceOffset);

                            if (ViolatesSpacing(placedRecords, worldPosition, rule))
                            {
                                result.RejectedBySpacing++;
                                continue;
                            }

                            SphericalSurfaceQuery.GetTangentFrame(direction, out Vector3 north, out _);
                            float yaw = Mathf.Lerp(rule.minimumYawDegrees, rule.maximumYawDegrees, UnitFloat(ref random));
                            Quaternion rotation = Quaternion.AngleAxis(yaw, direction) *
                                                   Quaternion.LookRotation(north, direction);
                            float scale = Mathf.Lerp(rule.minimumScale, rule.maximumScale, UnitFloat(ref random));

                            GameObject instance = UnityEngine.Object.Instantiate(
                                resolved.Prefab, worldPosition, rotation, parent);
                            instance.name = resolved.Prefab.name;
                            instance.transform.localScale = instance.transform.localScale * scale;

                            float exclusion = Mathf.Max(rule.minimumSpacingMeters, rule.exclusionRadiusMeters);
                            if (exclusion > 0f)
                                placedRecords.Add(new PlacedRecord
                                {
                                    Position = worldPosition,
                                    Group = rule.spacingGroup ?? string.Empty,
                                    Radius = exclusion
                                });

                            result.PlacedCount++;
                        }
                    }
                }
            }

            return result;
        }

        private static bool ViolatesSpacing(List<PlacedRecord> records, Vector3 position,
            SphericalEnvironmentPlacementJson.Rule rule)
        {
            float requested = Mathf.Max(rule.minimumSpacingMeters, rule.exclusionRadiusMeters);
            if (requested <= 0f) return false;
            string group = rule.spacingGroup ?? string.Empty;
            for (int i = 0; i < records.Count; i++)
            {
                PlacedRecord existing = records[i];
                if (!string.Equals(existing.Group, group, StringComparison.Ordinal))
                    continue;
                float required = Mathf.Max(requested, existing.Radius);
                if ((existing.Position - position).sqrMagnitude < required * required)
                    return true;
            }
            return false;
        }

        private static bool InRange(float value, float minimum, float maximum)
            => value >= minimum && value <= maximum;

        private static double SphericalQuadArea(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            => SphericalTriangleArea(a, b, c) + SphericalTriangleArea(a, c, d);

        private static double SphericalTriangleArea(Vector3 a, Vector3 b, Vector3 c)
        {
            double numerator = Math.Abs(Vector3.Dot(a, Vector3.Cross(b, c)));
            double denominator = 1.0 + Vector3.Dot(a, b) + Vector3.Dot(b, c) + Vector3.Dot(c, a);
            return 2.0 * Math.Atan2(numerator, Math.Max(0.0, denominator));
        }

        private static uint Hash(int seed, int face, int x, int y, int rule)
        {
            unchecked
            {
                uint value = (uint)seed;
                value ^= (uint)(face + 1) * 0x9E3779B9u;
                value ^= (uint)(x + 1) * 0x85EBCA6Bu;
                value ^= (uint)(y + 1) * 0xC2B2AE35u;
                value ^= (uint)(rule + 1) * 0x27D4EB2Fu;
                value ^= value >> 16;
                value *= 0x7FEB352Du;
                value ^= value >> 15;
                value *= 0x846CA68Bu;
                value ^= value >> 16;
                return value == 0u ? 0xA341316Cu : value;
            }
        }

        private static float UnitFloat(ref uint state)
        {
            unchecked
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
            }
            return (state & 0x00FFFFFFu) / 16777216f;
        }
    }
}
