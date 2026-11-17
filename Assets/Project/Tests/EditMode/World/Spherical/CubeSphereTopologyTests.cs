using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Pokemon.Project.World.Spherical;

namespace Pokemon.Project.Tests.World.Spherical
{
    public sealed class CubeSphereTopologyTests
    {
        private const float DirectionTolerance = 1e-5f;

        [TestCase(CubeSphereFace.PositiveX)]
        [TestCase(CubeSphereFace.NegativeX)]
        [TestCase(CubeSphereFace.PositiveY)]
        [TestCase(CubeSphereFace.NegativeY)]
        [TestCase(CubeSphereFace.PositiveZ)]
        [TestCase(CubeSphereFace.NegativeZ)]
        public void FaceGridProducesFiniteNormalizedDirections(CubeSphereFace face)
        {
            for (int y = 0; y <= 8; y++)
            {
                for (int x = 0; x <= 8; x++)
                {
                    float u = -1f + x * 0.25f;
                    float v = -1f + y * 0.25f;
                    Vector3 direction = CubeSphereTopology.GetDirection(face, u, v);

                    Assert.IsFalse(float.IsNaN(direction.x) || float.IsInfinity(direction.x));
                    Assert.IsFalse(float.IsNaN(direction.y) || float.IsInfinity(direction.y));
                    Assert.IsFalse(float.IsNaN(direction.z) || float.IsInfinity(direction.z));
                    Assert.That(direction.magnitude, Is.EqualTo(1f).Within(DirectionTolerance));
                }
            }
        }

        [TestCase(CubeSphereFace.PositiveX)]
        [TestCase(CubeSphereFace.NegativeX)]
        [TestCase(CubeSphereFace.PositiveY)]
        [TestCase(CubeSphereFace.NegativeY)]
        [TestCase(CubeSphereFace.PositiveZ)]
        [TestCase(CubeSphereFace.NegativeZ)]
        public void DirectionAndFaceCoordinateConversionRoundTrips(CubeSphereFace face)
        {
            for (int y = 0; y <= 8; y++)
            {
                for (int x = 0; x <= 8; x++)
                {
                    float u = -1f + x * 0.25f;
                    float v = -1f + y * 0.25f;
                    Vector3 original = CubeSphereTopology.GetDirection(face, u, v);

                    CubeSphereTopology.DirectionToFaceCoordinates(original, out CubeSphereFace roundTripFace, out Vector2 coordinates);
                    Vector3 reconstructed = CubeSphereTopology.GetDirection(roundTripFace, coordinates.x, coordinates.y);

                    Assert.That(Vector3.Distance(original, reconstructed), Is.LessThan(DirectionTolerance));
                }
            }
        }

        [Test]
        public void FaceEdgeSamplesAreSharedAcrossAdjacentFaces()
        {
            const int samples = 16;
            Dictionary<string, int> edgeDirectionCounts = new Dictionary<string, int>();

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                CubeSphereFace face = (CubeSphereFace)faceIndex;
                for (int i = 0; i <= samples; i++)
                {
                    float t = -1f + 2f * i / samples;
                    CountEdgeDirection(edgeDirectionCounts, CubeSphereTopology.GetDirection(face, -1f, t));
                    CountEdgeDirection(edgeDirectionCounts, CubeSphereTopology.GetDirection(face, 1f, t));
                    CountEdgeDirection(edgeDirectionCounts, CubeSphereTopology.GetDirection(face, t, -1f));
                    CountEdgeDirection(edgeDirectionCounts, CubeSphereTopology.GetDirection(face, t, 1f));
                }
            }

            foreach (KeyValuePair<string, int> entry in edgeDirectionCounts)
                Assert.That(entry.Value, Is.GreaterThanOrEqualTo(2), $"Edge direction {entry.Key} was not shared by adjacent faces.");
        }

        private static void CountEdgeDirection(Dictionary<string, int> counts, Vector3 direction)
        {
            string key = $"{Mathf.RoundToInt(direction.x * 100000f)}:{Mathf.RoundToInt(direction.y * 100000f)}:{Mathf.RoundToInt(direction.z * 100000f)}";
            counts.TryGetValue(key, out int count);
            counts[key] = count + 1;
        }

    }
}
