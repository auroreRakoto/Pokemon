#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.Text;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Geography;
using Pokemon.Project.World.Spherical.Studio;
using UnityEditor;
using UnityEngine;

namespace Pokemon.Project.Editor.World.Spherical
{
    /// <summary>
    /// Independent pure-data preview for spherical World Core fields with or without biome classification.
    /// </summary>
    public sealed class SphericalGeographicFieldsPreviewWindow : EditorWindow
    {
        private const int FixtureSeed = 481516;
        private const int FixtureResolution = 16;
        private SphericalPlanetStudioSettings _profile;
        private SphericalPlanetStudioSettings _workingSettings;
        private SphericalGeographicFieldSet _fields;
        private int _seed = FixtureSeed;
        private int _resolution = FixtureResolution;
        private bool _includeBiomes = true;
        private long _elapsedMilliseconds;
        private string _diagnostics = "No preview generated.";

        /// <summary>
        /// Opens the isolated geographic data preview without requiring a scene or playable world.
        /// </summary>
        [MenuItem("Tools/World/Spherical Geographic Fields Preview")]
        public static void Open()
        {
            GetWindow<SphericalGeographicFieldsPreviewWindow>("Geographic Fields").Show();
        }

        /// <summary>
        /// Draws profile inputs, deterministic fixture controls, generation actions and data diagnostics.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Spherical World Core — Geographic Fields", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Pure-data preview. No planet mesh, renderer, player, camera or environment assets are created.", MessageType.Info);
            _profile = (SphericalPlanetStudioSettings)EditorGUILayout.ObjectField(
                "Source Profile", _profile, typeof(SphericalPlanetStudioSettings), false);
            _seed = EditorGUILayout.IntField("Fixture Seed", _seed);
            _resolution = EditorGUILayout.IntSlider("Face Resolution", _resolution, 2, 64);
            _includeBiomes = EditorGUILayout.Toggle("Biome Classification", _includeBiomes);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reset to Fixture")) ResetToFixture();
                if (GUILayout.Button("Generate")) Generate();
                if (GUILayout.Button("Validate")) ValidateFields();
                if (GUILayout.Button("Show Diagnostics")) ShowDiagnostics();
                if (GUILayout.Button("Clear")) Clear();
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(_diagnostics, GUILayout.MinHeight(160f));
        }

        /// <summary>
        /// Clones the selected profile, applies fixture inputs and generates only the requested data fields.
        /// </summary>
        private void Generate()
        {
            Clear();
            if (_profile == null)
            {
                _diagnostics = "BLOCKED: assign a SphericalPlanetStudioSettings profile.";
                return;
            }
            Stopwatch timer = Stopwatch.StartNew();
            try
            {
                _workingSettings = Instantiate(_profile);
                _workingSettings.seed = _seed;
                _workingSettings.faceResolution = _resolution;
                _workingSettings.Validate();
                _fields = SphericalGeographicFieldGenerator.Generate(_workingSettings, _includeBiomes);
                timer.Stop();
                _elapsedMilliseconds = timer.ElapsedMilliseconds;
                _diagnostics = BuildDiagnostics(_elapsedMilliseconds);
            }
            catch (Exception exception)
            {
                Clear();
                _diagnostics = $"Generation failed: {exception.GetType().Name}: {exception.Message}";
            }
        }

        /// <summary>
        /// Releases the in-memory field snapshot and cloned settings without modifying project assets.
        /// </summary>
        private void Clear()
        {
            _fields = null;
            _elapsedMilliseconds = 0;
            if (_workingSettings != null) DestroyImmediate(_workingSettings);
            _workingSettings = null;
        }

        /// <summary>
        /// Restores the named deterministic fixture values without changing the selected source profile.
        /// </summary>
        private void ResetToFixture()
        {
            _seed = FixtureSeed;
            _resolution = FixtureResolution;
            _includeBiomes = true;
            Clear();
            _diagnostics = "Fixture reset. Select Generate to refresh the preview.";
        }

        /// <summary>
        /// Validates World Core ranges, water classes and the requested biome availability contract.
        /// </summary>
        private void ValidateFields()
        {
            if (_fields == null)
            {
                _diagnostics = "NOT RUN: generate a preview before validation.";
                return;
            }
            if (_fields.HasBiomeClassification != _includeBiomes)
            {
                _diagnostics = "FAIL: biome availability does not match the requested fixture option.";
                return;
            }
            for (int face = 0; face < 6; face++)
            for (int y = 0; y <= _fields.Resolution; y++)
            for (int x = 0; x <= _fields.Resolution; x++)
            {
                CubeSphereFace cubeFace = (CubeSphereFace)face;
                float altitude = _fields.GetValue(SphericalGeographicFieldType.AltitudeMeters, cubeFace, x, y);
                float temperature = _fields.GetValue(SphericalGeographicFieldType.TemperatureNormalized, cubeFace, x, y);
                float slope = _fields.GetValue(SphericalGeographicFieldType.SlopeDegrees, cubeFace, x, y);
                float moisture = _fields.GetValue(SphericalGeographicFieldType.MoistureNormalized, cubeFace, x, y);
                float waterDistance = _fields.GetValue(SphericalGeographicFieldType.DistanceToWaterMeters, cubeFace, x, y);
                SphericalWaterClass water = _fields.GetWaterClass(cubeFace, x, y);
                if (float.IsNaN(altitude) || float.IsInfinity(altitude) ||
                    float.IsNaN(waterDistance) || float.IsInfinity(waterDistance) || waterDistance < 0f ||
                    temperature < 0f || temperature > 1f || slope < 0f || slope > 90f ||
                    moisture < 0f || moisture > 1f || !Enum.IsDefined(typeof(SphericalWaterClass), water))
                {
                    _diagnostics = $"FAIL: invalid World Core value at {cubeFace} ({x}, {y}).";
                    return;
                }
            }
            _diagnostics = "PASS: all World Core samples are finite/in range and biome availability matches the fixture.";
        }

        /// <summary>
        /// Refreshes the visible diagnostic report for the current in-memory preview.
        /// </summary>
        private void ShowDiagnostics()
        {
            _diagnostics = _fields == null
                ? "NOT RUN: generate a preview before requesting diagnostics."
                : BuildDiagnostics(_elapsedMilliseconds);
        }

        /// <summary>
        /// Builds face, water, biome and range summaries from the generated in-memory field snapshot.
        /// </summary>
        private string BuildDiagnostics(long elapsedMilliseconds)
        {
            int ocean = 0;
            int land = 0;
            int[] biomeCounts = new int[5];
            float minAltitude = float.PositiveInfinity;
            float maxAltitude = float.NegativeInfinity;
            for (int face = 0; face < 6; face++)
            for (int y = 0; y <= _fields.Resolution; y++)
            for (int x = 0; x <= _fields.Resolution; x++)
            {
                CubeSphereFace cubeFace = (CubeSphereFace)face;
                float altitude = _fields.GetValue(SphericalGeographicFieldType.AltitudeMeters, cubeFace, x, y);
                minAltitude = Mathf.Min(minAltitude, altitude);
                maxAltitude = Mathf.Max(maxAltitude, altitude);
                if (_fields.GetWaterClass(cubeFace, x, y) == SphericalWaterClass.Ocean) ocean++;
                else land++;
                if (_fields.TryGetBiome(cubeFace, x, y, out SphericalBiome biome))
                    biomeCounts[(int)biome]++;
            }
            StringBuilder report = new StringBuilder();
            report.AppendLine("PASS: generation completed from a cloned profile.");
            report.AppendLine($"Seed: {_fields.Seed} | Resolution: {_fields.Resolution} | Biomes: {_fields.HasBiomeClassification}");
            report.AppendLine($"Ocean target/actual: {_fields.OceanCoverageTargetPercent:F2}% / {_fields.ActualOceanCoveragePercent:F2}%");
            report.AppendLine($"Sea level: {_fields.DerivedSeaLevelMeters:F2} m | Altitude range: {minAltitude:F2} to {maxAltitude:F2} m");
            report.AppendLine($"Face samples: {_fields.SamplesPerFace * 6} | Ocean samples: {ocean} | Land samples: {land}");
            report.AppendLine($"Biome counts (Plains/Forest/Desert/Mountains): {biomeCounts[1]}/{biomeCounts[2]}/{biomeCounts[3]}/{biomeCounts[4]}");
            report.AppendLine($"Elapsed: {elapsedMilliseconds} ms | Source asset modified: no");
            return report.ToString();
        }

        /// <summary>
        /// Releases temporary preview data when the Editor window closes.
        /// </summary>
        private void OnDisable()
        {
            Clear();
        }
    }
}
#endif
