using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>
    /// Immutable-by-contract snapshot of authoritative continuous and categorical world fields.
    /// Biome classification is optional; topology, altitude, climate, moisture and water remain available.
    /// </summary>
    public sealed partial class SphericalGeographicFieldSet
    {
        private readonly float[][] _altitude;
        private readonly float[][] _temperature;
        private readonly float[][] _slope;
        private readonly float[][] _distanceToWater;
        private readonly float[][] _moisture;
        private readonly SphericalWaterClass[][] _water;
        private readonly SphericalBiome[][] _biomes;

        public int Resolution { get; }
        public int SamplesPerFace => (Resolution + 1) * (Resolution + 1);
        public int Seed { get; }
        public float PlanetRadiusMeters { get; }
        public float OceanCoverageTargetPercent { get; }
        public float ActualOceanCoveragePercent { get; }
        public float OceanCoverageQuantizationTolerancePercent { get; }
        public float DerivedSeaLevelMeters { get; }
        public bool HasBiomeClassification => _biomes != null;

        /// <summary>
        /// Creates an immutable snapshot; a null biome array explicitly means classification was not generated.
        /// </summary>
        internal SphericalGeographicFieldSet(
            int resolution, int seed, float radius,
            float[][] altitude, float[][] temperature, float[][] slope,
            float[][] distanceToWater, float[][] moisture, SphericalWaterClass[][] water, SphericalBiome[][] biomes,
            float oceanTargetPercent, float actualOceanPercent, float coverageTolerancePercent, float seaLevelMeters)
        {
            Resolution = resolution;
            Seed = seed;
            PlanetRadiusMeters = radius;
            _altitude = altitude;
            _temperature = temperature;
            _slope = slope;
            _distanceToWater = distanceToWater;
            _moisture = moisture;
            _water = water;
            _biomes = biomes;
            OceanCoverageTargetPercent = oceanTargetPercent;
            ActualOceanCoveragePercent = actualOceanPercent;
            OceanCoverageQuantizationTolerancePercent = coverageTolerancePercent;
            DerivedSeaLevelMeters = seaLevelMeters;
        }

        /// <summary>
        /// Returns one continuous geographic field value at a validated face-lattice coordinate.
        /// </summary>
        public float GetValue(SphericalGeographicFieldType field, CubeSphereFace face, int x, int y)
        {
            int index = GetIndex(face, x, y);
            switch (field)
            {
                case SphericalGeographicFieldType.AltitudeMeters: return _altitude[(int)face][index];
                case SphericalGeographicFieldType.TemperatureNormalized: return _temperature[(int)face][index];
                case SphericalGeographicFieldType.SlopeDegrees: return _slope[(int)face][index];
                case SphericalGeographicFieldType.DistanceToWaterMeters: return _distanceToWater[(int)face][index];
                case SphericalGeographicFieldType.MoistureNormalized: return _moisture[(int)face][index];
                default: throw new ArgumentOutOfRangeException(nameof(field));
            }
        }

        /// <summary>
        /// Returns the categorical water class at a validated face-lattice coordinate.
        /// </summary>
        public SphericalWaterClass GetWaterClass(CubeSphereFace face, int x, int y)
        {
            int index = GetIndex(face, x, y);
            return _water[(int)face][index];
        }

        /// <summary>
        /// Bilinearly samples continuous fields using canonical spherical direction coordinates.
        /// </summary>
        public float Sample(SphericalGeographicFieldType field, Vector3 direction)
        {
            CubeSphereTopology.DirectionToFaceCoordinates(ValidateDirection(direction), out CubeSphereFace face, out Vector2 uv);
            float gx = Mathf.Clamp01((uv.x + 1f) * 0.5f) * Resolution;
            float gy = Mathf.Clamp01((uv.y + 1f) * 0.5f) * Resolution;
            int x0 = Mathf.Clamp(Mathf.FloorToInt(gx), 0, Resolution);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(gy), 0, Resolution);
            int x1 = Mathf.Min(x0 + 1, Resolution);
            int y1 = Mathf.Min(y0 + 1, Resolution);
            float tx = gx - x0, ty = gy - y0;
            float a = Mathf.Lerp(GetValue(field, face, x0, y0), GetValue(field, face, x1, y0), tx);
            float b = Mathf.Lerp(GetValue(field, face, x0, y1), GetValue(field, face, x1, y1), tx);
            return Mathf.Lerp(a, b, ty);
        }

        /// <summary>
        /// Nearest-lattice categorical lookup for water; enum identifiers are never interpolated.
        /// </summary>
        public SphericalWaterClass SampleWaterClass(Vector3 direction)
        {
            GetNearestCell(direction, out CubeSphereFace face, out int x, out int y);
            return GetWaterClass(face, x, y);
        }

        /// <summary>
        /// Returns the canonical unit direction for one validated face-lattice coordinate.
        /// </summary>
        public Vector3 GetDirection(CubeSphereFace face, int x, int y)
        {
            ValidateCell(face, x, y);
            float u = -1f + 2f * x / Resolution;
            float v = -1f + 2f * y / Resolution;
            return CubeSphereTopology.GetDirection(face, u, v);
        }
    }
}
