using UnityEngine;
using Pokemon.Project.World.Spherical.Studio;

namespace Pokemon.Project.World.Spherical.Geography
{
    public static partial class SphericalGeographicFieldGenerator
    {
        /// <summary>
        /// Samples deterministic altitude, temperature and slope on all six face lattices.
        /// </summary>
        private static BaseFields GenerateBaseFields(SphericalPlanetStudioSettings settings)
        {
            int resolution = settings.faceResolution;
            int side = resolution + 1;
            int count = side * side;
            float[][] altitude = CreateFloatFaces(count);
            float[][] temperature = CreateFloatFaces(count);
            float[][] slope = CreateFloatFaces(count);
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            for (int y = 0; y <= resolution; y++)
            for (int x = 0; x <= resolution; x++)
            {
                int index = y * side + x;
                Vector3 direction = DirectionAt((CubeSphereFace)faceIndex, x, y, resolution);
                float height = SphericalPlanetStudioGenerator.SamplePreviewHeight(
                    direction, settings, SphericalPlanetStudioPreviewStage.Final);
                altitude[faceIndex][index] = height;
                temperature[faceIndex][index] = SampleTemperature(direction, height, settings);
                slope[faceIndex][index] = SampleSlope(direction, settings, resolution);
            }
            return new BaseFields(altitude, temperature, slope);
        }

        /// <summary>
        /// Converts a face-lattice coordinate to its canonical unit direction.
        /// </summary>
        private static Vector3 DirectionAt(CubeSphereFace face, int x, int y, int resolution)
        {
            return CubeSphereTopology.GetDirection(face, -1f + 2f * x / resolution, -1f + 2f * y / resolution);
        }

        /// <summary>
        /// Derives normalized temperature from latitude, deterministic climate noise and altitude cooling.
        /// </summary>
        private static float SampleTemperature(Vector3 direction, float altitudeMeters, SphericalPlanetStudioSettings settings)
        {
            Vector3 d = direction.normalized;
            float latitudeFactor = 1f - Mathf.Abs(d.y);
            float noise = SphericalPlanetStudioGenerator.SampleClimateNoise(d, settings.seed + 104729);
            float normalized = latitudeFactor * settings.temperatureLatitudeInfluence +
                               noise * settings.temperatureNoiseInfluence;
            float altitudeCooling = Mathf.Max(0f, altitudeMeters) / 8000f;
            return Mathf.Clamp01(normalized - altitudeCooling);
        }

        /// <summary>
        /// Estimates slope from four tangent-offset height samples on the spherical surface.
        /// </summary>
        private static float SampleSlope(Vector3 direction, SphericalPlanetStudioSettings settings, int resolution)
        {
            Vector3 normal = direction.normalized;
            Vector3 reference = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
            Vector3 tangentA = Vector3.Cross(reference, normal).normalized;
            Vector3 tangentB = Vector3.Cross(normal, tangentA).normalized;
            float angularStep = Mathf.Clamp(2f / resolution, 0.0005f, 0.1f);
            float distanceMeters = Mathf.Max(0.01f, settings.radiusMeters * angularStep);
            float aPlus = SampleHeightAtOffset(normal, tangentA, angularStep, settings);
            float aMinus = SampleHeightAtOffset(normal, -tangentA, angularStep, settings);
            float bPlus = SampleHeightAtOffset(normal, tangentB, angularStep, settings);
            float bMinus = SampleHeightAtOffset(normal, -tangentB, angularStep, settings);
            float gradientA = (aPlus - aMinus) / (2f * distanceMeters);
            float gradientB = (bPlus - bMinus) / (2f * distanceMeters);
            return Mathf.Atan(Mathf.Sqrt(gradientA * gradientA + gradientB * gradientB)) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Samples terrain height at a tangent offset without crossing cube-face UV boundaries.
        /// </summary>
        private static float SampleHeightAtOffset(Vector3 normal, Vector3 tangent, float angularStep,
            SphericalPlanetStudioSettings settings)
        {
            Vector3 direction = (normal * Mathf.Cos(angularStep) + tangent * Mathf.Sin(angularStep)).normalized;
            return SphericalPlanetStudioGenerator.SamplePreviewHeight(
                direction, settings, SphericalPlanetStudioPreviewStage.Final);
        }
    }
}
