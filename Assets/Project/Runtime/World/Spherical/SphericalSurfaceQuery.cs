using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical
{
    /// <summary>Direction-based queries shared by the demo controller and camera.</summary>
    public static class SphericalSurfaceQuery
    {
        public static Vector3 GetSurfacePosition(Vector3 direction, SphericalWorldSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            settings.Validate();
            if (direction.sqrMagnitude < 1e-12f)
                throw new ArgumentException("Direction must be non-zero.", nameof(direction));

            Vector3 unitDirection = direction.normalized;
            float elevation = SphericalHeightField.SampleHeightValidated(unitDirection, settings);
            return unitDirection * (settings.planetRadiusMeters + elevation);
        }

        public static float GetSurfaceRadius(Vector3 direction, SphericalWorldSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            return settings.planetRadiusMeters + SphericalHeightField.SampleHeight(direction, settings);
        }

        public static void GetTangentFrame(Vector3 up, out Vector3 north, out Vector3 east)
        {
            if (up.sqrMagnitude < 1e-12f)
                throw new ArgumentException("Up direction must be non-zero.", nameof(up));

            up.Normalize();
            Vector3 reference = Mathf.Abs(Vector3.Dot(up, Vector3.forward)) > 0.98f
                ? Vector3.right
                : Vector3.forward;

            north = Vector3.ProjectOnPlane(reference, up).normalized;
            east = Vector3.Cross(up, north).normalized;
        }
    }
}
