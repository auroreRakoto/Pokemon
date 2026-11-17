using NUnit.Framework;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Presentation;
using UnityEngine;

namespace Pokemon.Project.Tests.World.Spherical
{
    public sealed class SphericalPresentationTests
    {
        [Test]
        public void SurfaceQueryReturnsConfiguredBaseRadiusPlusElevation()
        {
            SphericalWorldSettings settings = new SphericalWorldSettings
            {
                planetRadiusMeters = 1200f,
                maximumElevationMeters = 300f,
                seed = 42
            };

            Vector3 position = SphericalSurfaceQuery.GetSurfacePosition(new Vector3(0.2f, 0.8f, -0.3f), settings);
            float expectedRadius = SphericalSurfaceQuery.GetSurfaceRadius(position.normalized, settings);

            Assert.That(position.magnitude, Is.EqualTo(expectedRadius).Within(0.01f));
        }

        [Test]
        public void TangentFrameIsOrthonormalForRepresentativeDirections()
        {
            Vector3[] directions = { Vector3.up, Vector3.down, Vector3.right, Vector3.forward, new Vector3(1f, 2f, 3f).normalized };
            foreach (Vector3 direction in directions)
            {
                SphericalSurfaceQuery.GetTangentFrame(direction, out Vector3 north, out Vector3 east);
                Assert.That(Vector3.Dot(direction.normalized, north), Is.EqualTo(0f).Within(1e-5f));
                Assert.That(Vector3.Dot(direction.normalized, east), Is.EqualTo(0f).Within(1e-5f));
                Assert.That(Vector3.Dot(north, east), Is.EqualTo(0f).Within(1e-5f));
                Assert.That(north.magnitude, Is.EqualTo(1f).Within(1e-5f));
                Assert.That(east.magnitude, Is.EqualTo(1f).Within(1e-5f));
            }
        }

        [Test]
        public void MeshGeneratorBuildsSixFiniteFaceMeshes()
        {
            SphericalWorldSettings settings = new SphericalWorldSettings
            {
                planetRadiusMeters = 1000f,
                faceResolution = 8,
                seed = 123
            };

            Mesh[] meshes = SphericalPlanetMeshGenerator.GenerateAllFaces(settings);
            try
            {
                Assert.That(meshes.Length, Is.EqualTo(6));
                foreach (Mesh mesh in meshes)
                {
                    Assert.IsNotNull(mesh);
                    Assert.That(mesh.vertexCount, Is.EqualTo(81));
                    Assert.That(mesh.triangles.Length, Is.EqualTo(8 * 8 * 6));
                    Vector3[] vertices = mesh.vertices;
                    Vector3[] normals = mesh.normals;
                    for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
                    {
                        Vector3 vertex = vertices[vertexIndex];
                        Assert.IsFalse(float.IsNaN(vertex.x) || float.IsInfinity(vertex.x));
                        Assert.IsFalse(float.IsNaN(vertex.y) || float.IsInfinity(vertex.y));
                        Assert.IsFalse(float.IsNaN(vertex.z) || float.IsInfinity(vertex.z));
                        Assert.That(Vector3.Dot(normals[vertexIndex], vertex.normalized), Is.GreaterThan(0f));
                    }
                }
            }
            finally
            {
                foreach (Mesh mesh in meshes)
                {
                    if (mesh != null)
                        Object.DestroyImmediate(mesh);
                }
            }
        }
    }
}
