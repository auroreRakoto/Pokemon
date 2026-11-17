using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pokemon.Project.World.Spherical.Studio
{
    /// <summary>
    /// Shared deterministic field sampler for both studio views. The planisphere and globe
    /// call the same direction-based functions; projection never changes generated values.
    /// </summary>
    public static class SphericalPlanetStudioGenerator
    {
        public static float SampleLayerHeight(Vector3 direction, SphericalPlanetStudioSettings settings, int layer)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (direction.sqrMagnitude < 1e-12f) throw new ArgumentException("Direction must be non-zero.", nameof(direction));
            Vector3 d = direction.normalized;
            float frequency;
            float amplitude;
            switch (layer)
            {
                case 1: frequency = settings.layer1Frequency; amplitude = settings.layer1AmplitudeMeters; break;
                case 2: frequency = Mathf.Min(settings.layer2Frequency, settings.faceResolution * 0.12f); amplitude = settings.layer2AmplitudeMeters; break;
                case 3: frequency = Mathf.Min(settings.layer3Frequency, settings.faceResolution * 0.15f); amplitude = settings.layer3AmplitudeMeters; break;
                default: throw new ArgumentOutOfRangeException(nameof(layer));
            }

            float noise = SampleSeamContinuousPerlin(d * frequency, settings.seed + layer * 7919);
            if (layer == 1)
            {
                // Shape broad noise into large landform masses instead of radial spikes.
                float threshold = settings.continentThreshold;
                float land = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(threshold - 0.12f, threshold + 0.18f, noise));
                float basin = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 0.48f, noise));
                return (land * 0.82f + basin * 0.18f - 0.38f) * amplitude;
            }

            return (noise - 0.5f) * 2f * amplitude;
        }

        public static float SampleTotalHeight(Vector3 direction, SphericalPlanetStudioSettings settings)
        {
            return SamplePreviewHeight(direction, settings, SphericalPlanetStudioPreviewStage.Final);
        }

        public static float SamplePreviewHeight(Vector3 direction, SphericalPlanetStudioSettings settings, SphericalPlanetStudioPreviewStage stage)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            int maximumLayer = stage == SphericalPlanetStudioPreviewStage.BaseSphere ? 0 :
                stage == SphericalPlanetStudioPreviewStage.Layer1 ? 1 :
                stage == SphericalPlanetStudioPreviewStage.Layer2 ? 2 : 3;
            float height = 0f;
            if (maximumLayer >= 1 && settings.layer1Enabled) height += SampleLayerHeight(direction, settings, 1);
            if (maximumLayer >= 2 && settings.layer2Enabled) height += SampleLayerHeight(direction, settings, 2);
            if (maximumLayer >= 3 && settings.layer3Enabled) height += SampleLayerHeight(direction, settings, 3);
            return height;
        }

        public static float SampleTemperature(Vector3 direction, SphericalPlanetStudioSettings settings)
        {
            Vector3 d = direction.normalized;
            float latitudeFactor = 1f - Mathf.Abs(d.y);
            float noise = SampleSeamContinuousPerlin(d * 3.1f, settings.seed + 104729);
            float normalized = latitudeFactor * settings.temperatureLatitudeInfluence + noise * settings.temperatureNoiseInfluence;
            float altitudeCooling = Mathf.Max(0f, SamplePreviewHeight(d, settings, settings.previewStage)) / 8000f;
            return Mathf.Clamp01(normalized - altitudeCooling);
        }

        public static Mesh[] GenerateGlobeFaces(SphericalPlanetStudioSettings settings)
        {
            settings.Validate();
            Mesh[] meshes = new Mesh[6];
            try
            {
                for (int f = 0; f < 6; f++) meshes[f] = GenerateCubeFace((CubeSphereFace)f, settings);
                return meshes;
            }
            catch
            {
                foreach (Mesh mesh in meshes) if (mesh != null) DestroyMesh(mesh);
                throw;
            }
        }

        public static Mesh GenerateCubeFace(CubeSphereFace face, SphericalPlanetStudioSettings settings)
        {
            int resolution = settings.faceResolution;
            int side = resolution + 1;
            int count = side * side;
            Vector3[] vertices = new Vector3[count];
            Vector3[] normals = new Vector3[count];
            Vector2[] uv = new Vector2[count];
            Color[] colors = new Color[count];
            int[] triangles = new int[resolution * resolution * 6];
            int index = 0;
            for (int y = 0; y <= resolution; y++)
            for (int x = 0; x <= resolution; x++)
            {
                float u = -1f + 2f * x / resolution;
                float v = -1f + 2f * y / resolution;
                Vector3 direction = CubeSphereTopology.GetDirection(face, u, v);
                float height = SamplePreviewHeight(direction, settings, settings.previewStage);
                vertices[index] = direction * (settings.radiusMeters + height);
                normals[index] = direction;
                uv[index] = new Vector2((float)x / resolution, (float)y / resolution);
                colors[index] = GetMapColor(direction, height, settings);
                index++;
            }

            int t = 0;
            for (int y = 0; y < resolution; y++)
            for (int x = 0; x < resolution; x++)
            {
                int a = y * side + x, b = a + 1, c = a + side, d = c + 1;
                triangles[t++] = a; triangles[t++] = b; triangles[t++] = c;
                triangles[t++] = b; triangles[t++] = d; triangles[t++] = c;
            }

            Mesh mesh = new Mesh { name = $"Planet_{face}", indexFormat = count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.vertices = vertices; mesh.normals = normals; mesh.uv = uv; mesh.colors = colors; mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Vector3 PlanisphereCoordinatesToDirection(float u, float v)
        {
            float longitude = (Mathf.Clamp01(u) - 0.5f) * Mathf.PI * 2f;
            float latitude = (Mathf.Clamp01(v) - 0.5f) * Mathf.PI;
            return new Vector3(Mathf.Cos(latitude) * Mathf.Sin(longitude), Mathf.Sin(latitude), Mathf.Cos(latitude) * Mathf.Cos(longitude)).normalized;
        }

        public static Vector2 DirectionToPlanisphereCoordinates(Vector3 direction)
        {
            if (direction.sqrMagnitude < 1e-12f) throw new ArgumentException("Direction must be non-zero.", nameof(direction));
            Vector3 d = direction.normalized;
            float longitude = Mathf.Atan2(d.x, d.z);
            float latitude = Mathf.Asin(Mathf.Clamp(d.y, -1f, 1f));
            return new Vector2(longitude / (Mathf.PI * 2f) + 0.5f, latitude / Mathf.PI + 0.5f);
        }

        public static Mesh GeneratePlanisphere(SphericalPlanetStudioSettings settings)
        {
            settings.Validate();
            int width = settings.planisphereWidth;
            int height = settings.planisphereHeight;
            int count = (width + 1) * (height + 1);
            Vector3[] vertices = new Vector3[count];
            Vector3[] normals = new Vector3[count];
            Vector2[] uv = new Vector2[count];
            Color[] colors = new Color[count];
            int[] triangles = new int[width * height * 6];
            int index = 0;
            for (int y = 0; y <= height; y++)
            for (int x = 0; x <= width; x++)
            {
                float u = (float)x / width;
                float v = (float)y / height;
                Vector3 direction = PlanisphereCoordinatesToDirection(u, v);
                float heightValue = SamplePreviewHeight(direction, settings, settings.previewStage);
                vertices[index] = new Vector3((u - 0.5f) * Mathf.PI * 2f * settings.radiusMeters, (v - 0.5f) * Mathf.PI * settings.radiusMeters, 0f);
                normals[index] = Vector3.forward;
                uv[index] = new Vector2(u, v);
                colors[index] = GetMapColor(direction, heightValue, settings);
                index++;
            }
            int t = 0;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int a = y * (width + 1) + x, b = a + 1, c = a + width + 1, d = c + 1;
                triangles[t++] = a; triangles[t++] = b; triangles[t++] = c;
                triangles[t++] = b; triangles[t++] = d; triangles[t++] = c;
            }
            Mesh mesh = new Mesh { name = "Planet_Planisphere_Equirectangular", indexFormat = count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.vertices = vertices; mesh.normals = normals; mesh.uv = uv; mesh.colors = colors; mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Color GetMapColor(Vector3 direction, float height, SphericalPlanetStudioSettings settings)
        {
            if (settings.map == SphericalPlanetStudioMap.Temperature)
            {
                float t = SampleTemperature(direction, settings);
                return Color.Lerp(new Color(0.12f, 0.28f, 0.82f), new Color(0.92f, 0.28f, 0.08f), t);
            }
            if (settings.map == SphericalPlanetStudioMap.Altitude)
            {
                float h = Mathf.InverseLerp(-settings.layer1AmplitudeMeters, settings.layer1AmplitudeMeters + settings.layer2AmplitudeMeters + settings.layer3AmplitudeMeters, height);
                return Color.Lerp(new Color(0.05f, 0.12f, 0.35f), new Color(0.9f, 0.88f, 0.72f), h);
            }
            if (settings.map != SphericalPlanetStudioMap.Terrain)
                return new Color(0.35f, 0.35f, 0.35f, 1f); // Unavailable maps are blocked by the Editor UI.
            if (!settings.layer1Enabled && !settings.layer2Enabled && !settings.layer3Enabled)
                return new Color(0.34f, 0.48f, 0.58f, 1f);
            float normalizedHeight = Mathf.InverseLerp(-settings.layer1AmplitudeMeters, settings.layer1AmplitudeMeters + settings.layer2AmplitudeMeters + settings.layer3AmplitudeMeters, height);
            return Color.Lerp(new Color(0.12f, 0.3f, 0.2f), new Color(0.68f, 0.62f, 0.46f), normalizedHeight);
        }

        private static float SampleSeamContinuousPerlin(Vector3 p, int seed)
        {
            float ox = Offset(seed, 0), oy = Offset(seed, 1), oz = Offset(seed, 2);
            Vector3 w = new Vector3(Mathf.Pow(Mathf.Abs(p.x) / Mathf.Max(1e-5f, p.magnitude), 4f), Mathf.Pow(Mathf.Abs(p.y) / Mathf.Max(1e-5f, p.magnitude), 4f), Mathf.Pow(Mathf.Abs(p.z) / Mathf.Max(1e-5f, p.magnitude), 4f));
            float sum = w.x + w.y + w.z;
            w /= Mathf.Max(1e-5f, sum);
            float nx = Mathf.PerlinNoise(p.y + oy, p.z + oz);
            float ny = Mathf.PerlinNoise(p.x + ox, p.z + oz);
            float nz = Mathf.PerlinNoise(p.x + ox, p.y + oy);
            return nx * w.x + ny * w.y + nz * w.z;
        }

        private static float Offset(int seed, int axis)
        {
            unchecked { uint h = (uint)seed + (uint)(axis + 1) * 0x9E3779B9u; h ^= h >> 16; h *= 0x7FEB352Du; h ^= h >> 15; return (h & 0xFFFFu) / 257f; }
        }

        private static void DestroyMesh(Mesh mesh)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(mesh); else UnityEngine.Object.DestroyImmediate(mesh);
        }
    }
}
