using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Geography
{
    public sealed partial class SphericalGeographicFieldSet
    {
        /// <summary>
        /// Resolves a non-zero direction to its nearest canonical lattice cell.
        /// </summary>
        private void GetNearestCell(Vector3 direction, out CubeSphereFace face, out int x, out int y)
        {
            CubeSphereTopology.DirectionToFaceCoordinates(ValidateDirection(direction), out face, out Vector2 uv);
            x = Mathf.Clamp(Mathf.RoundToInt((uv.x + 1f) * 0.5f * Resolution), 0, Resolution);
            y = Mathf.Clamp(Mathf.RoundToInt((uv.y + 1f) * 0.5f * Resolution), 0, Resolution);
        }

        /// <summary>
        /// Validates a lattice coordinate and returns its row-major array index.
        /// </summary>
        private int GetIndex(CubeSphereFace face, int x, int y)
        {
            ValidateCell(face, x, y);
            return y * (Resolution + 1) + x;
        }

        /// <summary>
        /// Rejects face or coordinate values outside the six-face lattice.
        /// </summary>
        private void ValidateCell(CubeSphereFace face, int x, int y)
        {
            int faceIndex = (int)face;
            if (faceIndex < 0 || faceIndex >= 6) throw new ArgumentOutOfRangeException(nameof(face));
            if (x < 0 || x > Resolution) throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y > Resolution) throw new ArgumentOutOfRangeException(nameof(y));
        }

        /// <summary>
        /// Rejects a zero direction and returns the normalized direction used by field sampling.
        /// </summary>
        private static Vector3 ValidateDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 1e-12f)
                throw new ArgumentException("Direction must be non-zero.", nameof(direction));
            return direction.normalized;
        }
    }
}
