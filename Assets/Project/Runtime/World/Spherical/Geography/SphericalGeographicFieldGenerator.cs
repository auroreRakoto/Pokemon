using System;
using UnityEngine;
using Pokemon.Project.World.Spherical.Studio;

namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>
    /// First deterministic geographic-field stage. Generates altitude, temperature and
    /// slope on one shared six-face lattice. Hydrology/moisture are deliberately separate
    /// until the approved spherical distance-to-water design gate is implemented.
    /// </summary>
    public static class SphericalGeographicFieldGenerator
    {
        public static SphericalGeographicFieldSet Generate(SphericalPlanetStudioSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Validate();

            int resolution = settings.faceResolution;
            int side = resolution + 1;
            int count = side * side;
            var altitude = CreateFaces(count);
            var temperature = CreateFaces(count);
            var slope = CreateFaces(count);

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                CubeSphereFace face = (CubeSphereFace)faceIndex;
                for (int y = 0; y <= resolution; y++)
                for (int x = 0; x <= resolution; x++)
                {
                    int index = y * side + x;
                    Vector3 direction = DirectionAt(face, x, y, resolution);
                    float height = SphericalPlanetStudioGenerator.SamplePreviewHeight(
                        direction, settings, SphericalPlanetStudioPreviewStage.Final);
                    altitude[faceIndex][index] = height;
                    temperature[faceIndex][index] = SampleTemperature(direction, height, settings);
                    slope[faceIndex][index] = SampleSlope(direction, settings, resolution);
                }
            }

            return new SphericalGeographicFieldSet(
                resolution, settings.seed, settings.radiusMeters,
                altitude, temperature, slope);
        }

        private static float[][] CreateFaces(int count)
        {
            var result = new float[6][];
            for (int i = 0; i < result.Length; i++) result[i] = new float[count];
            return result;
        }

        private static Vector3 DirectionAt(CubeSphereFace face, int x, int y, int resolution)
        {
            float u = -1f + 2f * x / resolution;
            float v = -1f + 2f * y / resolution;
            return CubeSphereTopology.GetDirection(face, u, v);
        }

        private static float SampleTemperature(Vector3 direction, float altitudeMeters, SphericalPlanetStudioSettings settings)
        {
            // Prototype climate baseline: latitude + deterministic spherical noise + lapse cooling.
            // It is not a weather, rainfall, seasonal or physically complete climate simulation.
            Vector3 d = direction.normalized;
            float latitudeFactor = 1f - Mathf.Abs(d.y);
            float noise = SphericalPlanetStudioGenerator.SampleClimateNoise(
                d, settings.seed + 104729);
            float normalized = latitudeFactor * settings.temperatureLatitudeInfluence
                             + noise * settings.temperatureNoiseInfluence;
            float altitudeCooling = Mathf.Max(0f, altitudeMeters) / 8000f;
            return Mathf.Clamp01(normalized - altitudeCooling);
        }

        private static float SampleSlope(Vector3 direction, SphericalPlanetStudioSettings settings, int resolution)
        {
            Vector3 normal = direction.normalized;
            Vector3 reference = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.95f
                ? Vector3.right : Vector3.up;
            Vector3 tangentA = Vector3.Cross(reference, normal).normalized;
            Vector3 tangentB = Vector3.Cross(normal, tangentA).normalized;

            // Angular sample spacing approximates one cell on the sphere. All samples are
            // direction-based, so the derivative does not depend on a face-local UV seam.
            float angularStep = Mathf.Clamp(2f / resolution, 0.0005f, 0.1f);
            float distanceMeters = Mathf.Max(0.01f, settings.radiusMeters * angularStep);
            float center = SphericalPlanetStudioGenerator.SamplePreviewHeight(
                normal, settings, SphericalPlanetStudioPreviewStage.Final);
            float aPlus = SampleHeightAtOffset(normal, tangentA, angularStep, settings);
            float aMinus = SampleHeightAtOffset(normal, -tangentA, angularStep, settings);
            float bPlus = SampleHeightAtOffset(normal, tangentB, angularStep, settings);
            float bMinus = SampleHeightAtOffset(normal, -tangentB, angularStep, settings);

            float gradientA = (aPlus - aMinus) / (2f * distanceMeters);
            float gradientB = (bPlus - bMinus) / (2f * distanceMeters);
            float gradientMagnitude = Mathf.Sqrt(gradientA * gradientA + gradientB * gradientB);
            // atan(|grad h|) converts rise/run into an angle in degrees.
            return Mathf.Atan(gradientMagnitude) * Mathf.Rad2Deg;
        }

        private static float SampleHeightAtOffset(Vector3 normal, Vector3 tangent, float angularStep,
            SphericalPlanetStudioSettings settings)
        {
            Vector3 direction = (normal * Mathf.Cos(angularStep) + tangent * Mathf.Sin(angularStep)).normalized;
            return SphericalPlanetStudioGenerator.SamplePreviewHeight(
                direction, settings, SphericalPlanetStudioPreviewStage.Final);
        }
    }
}
