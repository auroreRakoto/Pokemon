using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>
    /// Immutable-by-contract snapshot of scalar geographic fields sampled on the same
    /// six-face Cube-Sphere lattice. Arrays are owned by this instance and are not exposed.
    /// </summary>
    public sealed class SphericalGeographicFieldSet
    {
        private readonly float[][] _altitude;
        private readonly float[][] _temperature;
        private readonly float[][] _slope;

        public int Resolution { get; }
        public int SamplesPerFace => (Resolution + 1) * (Resolution + 1);
        public int Seed { get; }
        public float PlanetRadiusMeters { get; }

        internal SphericalGeographicFieldSet(
            int resolution, int seed, float radius,
            float[][] altitude, float[][] temperature, float[][] slope)
        {
            Resolution = resolution;
            Seed = seed;
            PlanetRadiusMeters = radius;
            _altitude = altitude;
            _temperature = temperature;
            _slope = slope;
        }

        public float GetValue(SphericalGeographicFieldType field, CubeSphereFace face, int x, int y)
        {
            if (x < 0 || x > Resolution) throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y > Resolution) throw new ArgumentOutOfRangeException(nameof(y));
            int faceIndex = (int)face;
            if (faceIndex < 0 || faceIndex >= 6) throw new ArgumentOutOfRangeException(nameof(face));
            int index = y * (Resolution + 1) + x;

            switch (field)
            {
                case SphericalGeographicFieldType.AltitudeMeters: return _altitude[faceIndex][index];
                case SphericalGeographicFieldType.TemperatureNormalized: return _temperature[faceIndex][index];
                case SphericalGeographicFieldType.SlopeDegrees: return _slope[faceIndex][index];
                default: throw new ArgumentOutOfRangeException(nameof(field));
            }
        }

        /// <summary>
        /// Bilinearly samples a field using canonical spherical direction coordinates.
        /// Face selection is deterministic at seams through CubeSphereTopology.
        /// </summary>
        public float Sample(SphericalGeographicFieldType field, Vector3 direction)
        {
            if (direction.sqrMagnitude < 1e-12f)
                throw new ArgumentException("Direction must be non-zero.", nameof(direction));

            CubeSphereTopology.DirectionToFaceCoordinates(direction, out CubeSphereFace face, out Vector2 uv);
            float gx = Mathf.Clamp01((uv.x + 1f) * 0.5f) * Resolution;
            float gy = Mathf.Clamp01((uv.y + 1f) * 0.5f) * Resolution;
            int x0 = Mathf.Clamp(Mathf.FloorToInt(gx), 0, Resolution);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(gy), 0, Resolution);
            int x1 = Mathf.Min(x0 + 1, Resolution);
            int y1 = Mathf.Min(y0 + 1, Resolution);
            float tx = gx - x0;
            float ty = gy - y0;

            float a = Mathf.Lerp(GetValue(field, face, x0, y0), GetValue(field, face, x1, y0), tx);
            float b = Mathf.Lerp(GetValue(field, face, x0, y1), GetValue(field, face, x1, y1), tx);
            return Mathf.Lerp(a, b, ty);
        }

        public Vector3 GetDirection(CubeSphereFace face, int x, int y)
        {
            if (x < 0 || x > Resolution) throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y > Resolution) throw new ArgumentOutOfRangeException(nameof(y));
            float u = -1f + 2f * x / Resolution;
            float v = -1f + 2f * y / Resolution;
            return CubeSphereTopology.GetDirection(face, u, v);
        }
    }
}
