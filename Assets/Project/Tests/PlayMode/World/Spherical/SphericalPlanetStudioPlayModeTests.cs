using System.Collections;
using NUnit.Framework;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Studio;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pokemon.Project.Tests.World.Spherical.PlayMode
{
    /// <summary>
    /// Initial runtime smoke/integration tests for the spherical mesh-generation API.
    /// These tests exercise generated meshes inside PlayMode and verify scene attachment
    /// and cleanup. Pure field mathematics remains covered by EditMode tests.
    /// </summary>
    public sealed class SphericalPlanetStudioPlayModeTests
    {
        private SphericalPlanetStudioSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SphericalPlanetStudioSettings>();
            _settings.radiusMeters = 50000f;
            _settings.faceResolution = 8;
            _settings.planisphereWidth = 32;
            _settings.planisphereHeight = 16;
            _settings.layer1Enabled = true;
            _settings.layer2Enabled = true;
            _settings.layer3Enabled = true;
            _settings.Validate();
        }

        [TearDown]
        public void TearDown()
        {
            if (_settings != null)
                Object.DestroyImmediate(_settings);
        }

        [Test]
        public void Runtime_GeneratesSixValidGlobeFaceMeshes()
        {
            Mesh[] meshes = SphericalPlanetStudioGenerator.GenerateGlobeFaces(_settings);
            try
            {
                Assert.AreEqual(6, meshes.Length);
                for (int i = 0; i < meshes.Length; i++)
                {
                    Assert.IsNotNull(meshes[i], $"Globe face {i} was not generated.");
                    Assert.AreEqual(81, meshes[i].vertexCount);
                    Assert.AreEqual(8 * 8 * 6, meshes[i].triangles.Length);
                    Assert.Greater(meshes[i].bounds.extents.magnitude, 0f);
                    Assert.AreEqual(81, meshes[i].normals.Length);
                }
            }
            finally
            {
                DestroyMeshes(meshes);
            }
        }

        [Test]
        public void Runtime_GeneratesValidPlanisphereMesh()
        {
            Mesh mesh = SphericalPlanetStudioGenerator.GeneratePlanisphere(_settings);
            try
            {
                Assert.IsNotNull(mesh);
                Assert.AreEqual((32 + 1) * (16 + 1), mesh.vertexCount);
                Assert.AreEqual(32 * 16 * 6, mesh.triangles.Length);
                Assert.AreEqual(mesh.vertexCount, mesh.normals.Length);
                Assert.AreEqual(mesh.vertexCount, mesh.uv.Length);
                Assert.Greater(mesh.bounds.size.x, 0f);
                Assert.Greater(mesh.bounds.size.y, 0f);
            }
            finally
            {
                if (mesh != null) Object.Destroy(mesh);
            }
        }

        [UnityTest]
        public IEnumerator Runtime_GeneratedGlobeMeshCanBeAttachedToSceneAndCleanedUp()
        {
            Mesh mesh = SphericalPlanetStudioGenerator.GenerateCubeFace(
                CubeSphereFace.PositiveX, _settings);
            var root = new GameObject("PlayMode_GeneratedPlanet_Test");
            try
            {
                var filter = root.AddComponent<MeshFilter>();
                root.AddComponent<MeshRenderer>();
                filter.sharedMesh = mesh;

                yield return null;

                Assert.IsNotNull(root.GetComponent<MeshRenderer>());
                Assert.AreSame(mesh, filter.sharedMesh);
                Assert.Greater(filter.sharedMesh.vertexCount, 0);
            }
            finally
            {
                if (root != null) Object.Destroy(root);
                if (mesh != null) Object.Destroy(mesh);
            }

            yield return null;
            Assert.IsTrue(root == null, "Generated scene root should be destroyed during cleanup.");
        }

        [UnityTest]
        public IEnumerator Runtime_GenerationCanBeRepeatedWithoutReusingDestroyedMeshes()
        {
            Mesh first = SphericalPlanetStudioGenerator.GeneratePlanisphere(_settings);
            int firstVertexCount = first.vertexCount;
            Object.Destroy(first);
            yield return null;

            Mesh second = SphericalPlanetStudioGenerator.GeneratePlanisphere(_settings);
            try
            {
                Assert.IsNotNull(second);
                Assert.AreNotSame(first, second);
                Assert.AreEqual(firstVertexCount, second.vertexCount);
            }
            finally
            {
                if (second != null) Object.Destroy(second);
            }
        }

        private static void DestroyMeshes(Mesh[] meshes)
        {
            if (meshes == null) return;
            foreach (Mesh mesh in meshes)
                if (mesh != null) Object.Destroy(mesh);
        }
    }
}
