using Pokemon.Project.World.Spherical.Geography;
using Pokemon.Project.World.Spherical;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Movement
{
    /// <summary>Queries the authoritative WORLD-002 geographic fields; no planet-scale collider is required.</summary>
    public static class SphericalGroundProbe
    {
        public struct Sample
        {
            public Vector3 Direction;
            public Vector3 SurfacePosition;
            public Vector3 Normal;
            public float AltitudeMeters;
            public float SlopeDegrees;
            public SphericalWaterClass WaterClass;
            public SphericalBiome Biome;
            public bool IsLand => WaterClass == SphericalWaterClass.Land;
        }

        public static bool TrySample(SphericalGeographicFieldSet fields, Vector3 planetCentre,
            float planetRadiusMeters, Vector3 direction, out Sample sample)
        {
            sample = default;
            if (fields == null || direction.sqrMagnitude < 1e-10f || planetRadiusMeters <= 0f)
                return false;

            direction.Normalize();
            float altitude = fields.Sample(SphericalGeographicFieldType.AltitudeMeters, direction);
            Vector3 surface = planetCentre + direction * (planetRadiusMeters + altitude);
            Vector3 normal = EstimateNormal(fields, planetCentre, planetRadiusMeters, direction);
            sample = new Sample
            {
                Direction = direction,
                SurfacePosition = surface,
                Normal = normal,
                AltitudeMeters = altitude,
                SlopeDegrees = fields.Sample(SphericalGeographicFieldType.SlopeDegrees, direction),
                WaterClass = fields.SampleWaterClass(direction),
                Biome = fields.SampleBiome(direction)
            };
            return true;
        }

        private static Vector3 EstimateNormal(SphericalGeographicFieldSet fields, Vector3 centre,
            float radius, Vector3 direction)
        {
            SphericalSurfaceQuery.GetTangentFrame(direction, out Vector3 tangentA, out Vector3 tangentB);
            // Two lattice intervals provide a stable finite difference without crossing a face in UV space.
            float angularStep = Mathf.Clamp(2f / fields.Resolution, 0.0005f, 0.02f);
            Vector3 a0 = Surface(fields, centre, radius, (direction - tangentA * angularStep).normalized);
            Vector3 a1 = Surface(fields, centre, radius, (direction + tangentA * angularStep).normalized);
            Vector3 b0 = Surface(fields, centre, radius, (direction - tangentB * angularStep).normalized);
            Vector3 b1 = Surface(fields, centre, radius, (direction + tangentB * angularStep).normalized);
            Vector3 normal = Vector3.Cross(a1 - a0, b1 - b0).normalized;
            if (normal.sqrMagnitude < 1e-8f) return direction;
            return Vector3.Dot(normal, direction) < 0f ? -normal : normal;
        }

        private static Vector3 Surface(SphericalGeographicFieldSet fields, Vector3 centre, float radius, Vector3 direction)
        {
            float altitude = fields.Sample(SphericalGeographicFieldType.AltitudeMeters, direction);
            return centre + direction * (radius + altitude);
        }
    }
}
