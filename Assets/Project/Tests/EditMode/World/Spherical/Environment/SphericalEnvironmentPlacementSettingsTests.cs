using System.Collections.Generic;
using NUnit.Framework;
using Pokemon.Project.World.Spherical.Environment;
using UnityEngine;

namespace Pokemon.Project.Tests.World.Spherical
{
    public sealed class SphericalEnvironmentPlacementJsonTests
    {
        private SphericalEnvironmentPrefabCatalog _catalog;
        private TextAsset _json;

        [SetUp]
        public void SetUp()
        {
            _catalog = ScriptableObject.CreateInstance<SphericalEnvironmentPrefabCatalog>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_json != null) Object.DestroyImmediate(_json);
            if (_catalog != null) Object.DestroyImmediate(_catalog);
        }

        [Test]
        public void LoadsValidJsonWithDisabledRulesWithoutRequiringPrefabReferences()
        {
            _json = new TextAsset("{\"schemaVersion\":1,\"biomes\":[{\"name\":\"Plains\",\"enabled\":true,\"rules\":[{\"id\":\"grass_short\",\"enabled\":false}]}]}");
            bool success = SphericalEnvironmentPlacementJson.TryLoad(_json, _catalog, out var resolved, out List<string> issues);
            Assert.IsTrue(success, string.Join("\n", issues));
            Assert.AreEqual(0, resolved.Rules.Count);
        }

        [Test]
        public void EnabledRuleRequiresCatalogPrefabReference()
        {
            _json = new TextAsset("{\"schemaVersion\":1,\"biomes\":[{\"name\":\"Plains\",\"rules\":[{\"id\":\"grass_short\",\"enabled\":true}]}]}");
            bool success = SphericalEnvironmentPlacementJson.TryLoad(_json, _catalog, out _, out List<string> issues);
            Assert.IsFalse(success);
            Assert.That(issues.Exists(issue => issue.Contains("catalog prefab 'grass_short' is missing")), Is.True);
        }


        [Test]
        public void ResolvesEnabledRuleToCatalogPrefab()
        {
            var prefab = new GameObject("PF_Grass_Short_Test");
            try
            {
                var catalogType = typeof(SphericalEnvironmentPrefabCatalog);
                var entryType = catalogType.GetNestedType("Entry", System.Reflection.BindingFlags.NonPublic);
                object entry = System.Activator.CreateInstance(entryType, true);
                entryType.GetField("id").SetValue(entry, "grass_short");
                entryType.GetField("prefab").SetValue(entry, prefab);
                System.Array entries = System.Array.CreateInstance(entryType, 1);
                entries.SetValue(entry, 0);
                catalogType.GetField("entries", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(_catalog, entries);

                _json = new TextAsset("{\"schemaVersion\":1,\"biomes\":[{\"name\":\"Plains\",\"rules\":[{\"id\":\"grass_short\",\"enabled\":true}]}]}");
                bool success = SphericalEnvironmentPlacementJson.TryLoad(_json, _catalog, out var resolved, out List<string> issues);
                Assert.IsTrue(success, string.Join("\n", issues));
                Assert.AreEqual(1, resolved.Rules.Count);
                Assert.AreSame(prefab, resolved.Rules[0].Prefab);
            }
            finally { Object.DestroyImmediate(prefab); }
        }

        [Test]
        public void RejectsUnsupportedBiomeAndSchemaVersion()
        {
            _json = new TextAsset("{\"schemaVersion\":2,\"biomes\":[{\"name\":\"Desert\",\"rules\":[]}]}");
            bool success = SphericalEnvironmentPlacementJson.TryLoad(_json, _catalog, out _, out List<string> issues);
            Assert.IsFalse(success);
            Assert.That(issues.Exists(issue => issue.Contains("Unsupported schemaVersion")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("only Plains and Forest")), Is.True);
        }

        [Test]
        public void ReportsDuplicateBiomeDefinitions()
        {
            _json = new TextAsset("{\"schemaVersion\":1,\"biomes\":[{\"name\":\"Plains\",\"rules\":[]},{\"name\":\"plains\",\"rules\":[]}]}");
            bool success = SphericalEnvironmentPlacementJson.TryLoad(_json, _catalog, out _, out List<string> issues);
            Assert.IsFalse(success);
            Assert.That(issues.Exists(issue => issue.Contains("Duplicate biome")), Is.True);
        }

        [Test]
        public void RejectsInvalidNumericRanges()
        {
            _json = new TextAsset("{\"schemaVersion\":1,\"biomes\":[{\"name\":\"Forest\",\"rules\":[{\"id\":\"mushroom_red\",\"enabled\":false,\"minimumScale\":2,\"maximumScale\":1,\"maximumSlopeDegrees\":91,\"useMoistureRange\":true,\"minimumMoisture\":0.8,\"maximumMoisture\":0.2}]}]}");
            bool success = SphericalEnvironmentPlacementJson.TryLoad(_json, _catalog, out _, out List<string> issues);
            Assert.IsFalse(success);
            Assert.That(issues.Exists(issue => issue.Contains("maximumScale")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("maximumSlopeDegrees")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("moisture maximum")), Is.True);
        }
    }
}
