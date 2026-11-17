using System;
using NUnit.Framework;
using Pokemon.Project.World.Spherical;
using UnityEngine;

namespace Pokemon.Project.Tests.World.Spherical
{
    public sealed class CubeSphereTopologyRegressionTests
    {
        private static readonly CubeSphereFace[] Faces =
        {
            CubeSphereFace.PositiveX, CubeSphereFace.NegativeX,
            CubeSphereFace.PositiveY, CubeSphereFace.NegativeY,
            CubeSphereFace.PositiveZ, CubeSphereFace.NegativeZ
        };

        [Test]
        public void EveryFaceCentreMapsToItsOwnFace()
        {
            foreach (CubeSphereFace face in Faces)
            {
                Vector3 direction = CubeSphereTopology.GetDirection(face, 0f, 0f);
                CubeSphereTopology.DirectionToFaceCoordinates(direction, out CubeSphereFace mappedFace, out Vector2 uv);
                Assert.AreEqual(face, mappedFace);
                Assert.That(uv.magnitude, Is.LessThan(1e-6f));
            }
        }

        [Test]
        public void FaceGridDirectionsAreFiniteAndNormalized()
        {
            const int resolution = 8;
            foreach (CubeSphereFace face in Faces)
            for (int y = 0; y <= resolution; y++)
            for (int x = 0; x <= resolution; x++)
            {
                Vector3 direction = CubeSphereTopology.GetDirection(face,
                    -1f + 2f * x / resolution, -1f + 2f * y / resolution);
                Assert.IsFalse(float.IsNaN(direction.x) || float.IsNaN(direction.y) || float.IsNaN(direction.z));
                Assert.That(direction.magnitude, Is.EqualTo(1f).Within(1e-5f));
            }
        }

        [Test]
        public void ZeroDirectionIsRejected()
        {
            Assert.Throws<ArgumentException>(() =>
                CubeSphereTopology.DirectionToFaceCoordinates(Vector3.zero, out _, out _));
        }
    }
}
