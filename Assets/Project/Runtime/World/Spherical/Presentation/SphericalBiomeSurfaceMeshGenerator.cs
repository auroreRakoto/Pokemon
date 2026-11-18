using System;
using UnityEngine;
using UnityEngine.Rendering;
using Pokemon.Project.World.Spherical.Geography;
using Pokemon.Project.World.Spherical.Studio;

namespace Pokemon.Project.World.Spherical.Presentation
{
    /// <summary>
    /// Builds the playable globe surface from the shared WORLD-002 fields. Vertex colour RGBA
    /// stores normalized Plains/Forest/Desert/Mountains blend weights; UV channel 1 stores
    /// the ocean blend weight. All transition samples are direction-based, so cube-face seams
    /// use the same canonical geographic lookup.
    /// </summary>
    public static class SphericalBiomeSurfaceMeshGenerator
    {
        private const float CentreSampleWeight = 2f;
        private const int RingSampleCount = 8;

        public static Mesh[] GenerateAllFaces(SphericalPlanetStudioSettings settings,
            SphericalGeographicFieldSet fields, float transitionWidthCells = 2f)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (fields.Resolution != settings.faceResolution)
                throw new ArgumentException("Geographic field resolution must match the mesh face resolution.", nameof(fields));
            if (float.IsNaN(transitionWidthCells) || float.IsInfinity(transitionWidthCells) || transitionWidthCells < 0f)
                throw new ArgumentOutOfRangeException(nameof(transitionWidthCells));

            settings.Validate();
            Mesh[] meshes = new Mesh[6];
            try
            {
                for (int face = 0; face < meshes.Length; face++)
                    meshes[face] = GenerateFace((CubeSphereFace)face, settings, fields, transitionWidthCells);
                return meshes;
            }
            catch
            {
                for (int i = 0; i < meshes.Length; i++)
                    if (meshes[i] != null) DestroyMesh(meshes[i]);
                throw;
            }
        }

        public static Color CalculateBiomeWeights(SphericalGeographicFieldSet fields, Vector3 direction,
            float transitionWidthCells, out float oceanWeight)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (direction.sqrMagnitude < 1e-12f) throw new ArgumentException("Direction must be non-zero.", nameof(direction));
            if (transitionWidthCells < 0f || float.IsNaN(transitionWidthCells) || float.IsInfinity(transitionWidthCells))
                throw new ArgumentOutOfRangeException(nameof(transitionWidthCells));

            Vector3 centre = direction.normalized;
            float plainsVotes = 0f;
            float forestVotes = 0f;
            float desertVotes = 0f;
            float mountainVotes = 0f;
            float waterVotes = 0f;
            float totalVotes = 0f;
            Accumulate(fields, centre, CentreSampleWeight, ref plainsVotes, ref forestVotes,
                ref desertVotes, ref mountainVotes, ref waterVotes, ref totalVotes);

            if (transitionWidthCells > 0f)
            {
                SphericalSurfaceQuery.GetTangentFrame(centre, out Vector3 tangentA, out Vector3 tangentB);
                float angularRadius = transitionWidthCells * 2f / fields.Resolution;
                for (int i = 0; i < RingSampleCount; i++)
                {
                    float angle = i * (Mathf.PI * 2f / RingSampleCount);
                    Vector3 tangent = tangentA * Mathf.Cos(angle) + tangentB * Mathf.Sin(angle);
                    Vector3 sampleDirection = (centre + tangent * angularRadius).normalized;
                    Accumulate(fields, sampleDirection, 1f, ref plainsVotes, ref forestVotes,
                        ref desertVotes, ref mountainVotes, ref waterVotes, ref totalVotes);
                }
            }

            oceanWeight = totalVotes > 0f ? waterVotes / totalVotes : 0f;
            float landVotes = plainsVotes + forestVotes + desertVotes + mountainVotes;
            if (landVotes <= 0f)
                return Color.clear;

            float inverseLandVotes = 1f / landVotes;
            return new Color(plainsVotes * inverseLandVotes, forestVotes * inverseLandVotes,
                desertVotes * inverseLandVotes, mountainVotes * inverseLandVotes);
        }

        private static void Accumulate(SphericalGeographicFieldSet fields, Vector3 direction, float weight,
            ref float plainsVotes, ref float forestVotes, ref float desertVotes, ref float mountainVotes,
            ref float waterVotes, ref float totalVotes)
        {
            totalVotes += weight;
            if (fields.SampleWaterClass(direction) == SphericalWaterClass.Ocean)
            {
                waterVotes += weight;
                return;
            }

            switch (fields.SampleBiome(direction))
            {
                case SphericalBiome.Plains: plainsVotes += weight; break;
                case SphericalBiome.Forest: forestVotes += weight; break;
                case SphericalBiome.Desert: desertVotes += weight; break;
                case SphericalBiome.Mountains: mountainVotes += weight; break;
            }
        }

        private static Mesh GenerateFace(CubeSphereFace face, SphericalPlanetStudioSettings settings,
            SphericalGeographicFieldSet fields, float transitionWidthCells)
        {
            int resolution = settings.faceResolution;
            int side = resolution + 1;
            int vertexCount = side * side;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector2[] waterWeights = new Vector2[vertexCount];
            Color[] biomeWeights = new Color[vertexCount];
            int[] triangles = new int[resolution * resolution * 6];

            int index = 0;
            for (int y = 0; y <= resolution; y++)
            for (int x = 0; x <= resolution; x++)
            {
                Vector3 direction = CubeSphereTopology.GetDirection(face,
                    -1f + 2f * x / resolution, -1f + 2f * y / resolution);
                Color weights = CalculateBiomeWeights(fields, direction, transitionWidthCells, out float oceanWeight);
                float terrainAltitude = fields.Sample(SphericalGeographicFieldType.AltitudeMeters, direction);
                float waterAltitude = fields.DerivedSeaLevelMeters + 0.5f;
                float altitude = Mathf.Lerp(terrainAltitude, waterAltitude, oceanWeight);

                vertices[index] = direction * (settings.radiusMeters + altitude);
                normals[index] = direction;
                uv[index] = new Vector2((float)x / resolution, (float)y / resolution);
                biomeWeights[index] = weights;
                waterWeights[index] = new Vector2(oceanWeight, 0f);
                index++;
            }

            int triangleIndex = 0;
            for (int y = 0; y < resolution; y++)
            for (int x = 0; x < resolution; x++)
            {
                int a = y * side + x;
                int b = a + 1;
                int c = a + side;
                int d = c + 1;
                triangles[triangleIndex++] = a; triangles[triangleIndex++] = b; triangles[triangleIndex++] = c;
                triangles[triangleIndex++] = b; triangles[triangleIndex++] = d; triangles[triangleIndex++] = c;
            }

            Mesh mesh = new Mesh
            {
                name = $"WORLD-002_BiomeSurface_{face}",
                indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.colors = biomeWeights;
            mesh.uv2 = waterWeights;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void DestroyMesh(Mesh mesh)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(mesh);
            else UnityEngine.Object.DestroyImmediate(mesh);
        }
    }
}
