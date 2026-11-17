using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical
{
    public enum CubeSphereFace : byte
    {
        PositiveX = 0,
        NegativeX = 1,
        PositiveY = 2,
        NegativeY = 3,
        PositiveZ = 4,
        NegativeZ = 5
    }

    /// <summary>
    /// Canonical cube-face mapping for a six-face cube projected onto a unit sphere.
    /// Face coordinates use the inclusive range [-1, 1].
    /// </summary>
    public static class CubeSphereTopology
    {
        public static Vector3 GetCubePoint(CubeSphereFace face, float u, float v)
        {
            GetFaceBasis(face, out Vector3 normal, out Vector3 axisU, out Vector3 axisV);
            return normal + axisU * u + axisV * v;
        }

        public static Vector3 GetDirection(CubeSphereFace face, float u, float v)
        {
            Vector3 cubePoint = GetCubePoint(face, u, v);
            return cubePoint.normalized;
        }

        /// <summary>
        /// Finds the dominant cube face and its local coordinates for a direction.
        /// Ties are resolved X, then Y, then Z to keep the result deterministic.
        /// </summary>
        public static void DirectionToFaceCoordinates(Vector3 direction, out CubeSphereFace face, out Vector2 coordinates)
        {
            if (direction.sqrMagnitude < 1e-12f)
                throw new ArgumentException("Direction must be non-zero.", nameof(direction));

            direction.Normalize();
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);
            float absZ = Mathf.Abs(direction.z);

            if (absX >= absY && absX >= absZ)
            {
                face = direction.x >= 0f ? CubeSphereFace.PositiveX : CubeSphereFace.NegativeX;
            }
            else if (absY >= absZ)
            {
                face = direction.y >= 0f ? CubeSphereFace.PositiveY : CubeSphereFace.NegativeY;
            }
            else
            {
                face = direction.z >= 0f ? CubeSphereFace.PositiveZ : CubeSphereFace.NegativeZ;
            }

            GetFaceBasis(face, out Vector3 normal, out Vector3 axisU, out Vector3 axisV);
            float denominator = Vector3.Dot(direction, normal);
            if (denominator <= 0f)
                throw new InvalidOperationException("Direction could not be projected onto its selected cube face.");

            coordinates = new Vector2(
                Vector3.Dot(direction, axisU) / denominator,
                Vector3.Dot(direction, axisV) / denominator);
        }

        public static Vector3 GetFaceNormal(CubeSphereFace face)
        {
            GetFaceBasis(face, out Vector3 normal, out _, out _);
            return normal;
        }

        private static void GetFaceBasis(CubeSphereFace face, out Vector3 normal, out Vector3 axisU, out Vector3 axisV)
        {
            switch (face)
            {
                case CubeSphereFace.PositiveX:
                    normal = Vector3.right;
                    axisU = Vector3.back;
                    axisV = Vector3.up;
                    break;
                case CubeSphereFace.NegativeX:
                    normal = Vector3.left;
                    axisU = Vector3.forward;
                    axisV = Vector3.up;
                    break;
                case CubeSphereFace.PositiveY:
                    normal = Vector3.up;
                    axisU = Vector3.right;
                    axisV = Vector3.back;
                    break;
                case CubeSphereFace.NegativeY:
                    normal = Vector3.down;
                    axisU = Vector3.right;
                    axisV = Vector3.forward;
                    break;
                case CubeSphereFace.PositiveZ:
                    normal = Vector3.forward;
                    axisU = Vector3.right;
                    axisV = Vector3.up;
                    break;
                case CubeSphereFace.NegativeZ:
                    normal = Vector3.back;
                    axisU = Vector3.left;
                    axisV = Vector3.up;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(face), face, "Unknown cube-sphere face.");
            }
        }
    }
}
