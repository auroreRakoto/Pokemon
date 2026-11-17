using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pokemon.Project.World.Spherical.Presentation
{
    /// <summary>
    /// Builds one Unity mesh per cube face. The six meshes share the exact same
    /// canonical edge positions, while remaining independently usable by renderers/colliders.
    /// The caller owns and must destroy the returned meshes.
    /// </summary>
    public static class SphericalPlanetMeshGenerator
    {
        public static Mesh[] GenerateAllFaces(SphericalWorldSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            settings.Validate();
            Mesh[] meshes = new Mesh[6];

            try
            {
                for (int faceIndex = 0; faceIndex < meshes.Length; faceIndex++)
                {
                    meshes[faceIndex] = GenerateFace((CubeSphereFace)faceIndex, settings);
                }

                return meshes;
            }
            catch
            {
                for (int i = 0; i < meshes.Length; i++)
                {
                    if (meshes[i] != null)
                        DestroyMesh(meshes[i]);
                }

                throw;
            }
        }

        public static Mesh GenerateFace(CubeSphereFace face, SphericalWorldSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            settings.Validate();
            int resolution = settings.faceResolution;
            int sideLength = resolution + 1;
            int vertexCount = sideLength * sideLength;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[resolution * resolution * 6];

            int vertexIndex = 0;
            for (int y = 0; y <= resolution; y++)
            {
                float v = -1f + 2f * y / resolution;
                for (int x = 0; x <= resolution; x++)
                {
                    float u = -1f + 2f * x / resolution;
                    Vector3 direction = CubeSphereTopology.GetDirection(face, u, v);
                    float elevation = SphericalHeightField.SampleHeightValidated(direction, settings);
                    float radius = settings.planetRadiusMeters + elevation;

                    vertices[vertexIndex] = direction * radius;
                    normals[vertexIndex] = CalculateSurfaceNormal(direction, settings, 1f / resolution);
                    uvs[vertexIndex] = new Vector2((float)x / resolution, (float)y / resolution);
                    vertexIndex++;
                }
            }

            int triangleIndex = 0;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int a = y * sideLength + x;
                    int b = a + 1;
                    int c = a + sideLength;
                    int d = c + 1;

                    // The face bases are oriented so this winding points outward.
                    triangles[triangleIndex++] = a;
                    triangles[triangleIndex++] = b;
                    triangles[triangleIndex++] = c;
                    triangles[triangleIndex++] = b;
                    triangles[triangleIndex++] = d;
                    triangles[triangleIndex++] = c;
                }
            }

            Mesh mesh = new Mesh
            {
                name = $"SphericalPlanet_{face}",
                indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
            };

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }


        private static Vector3 CalculateSurfaceNormal(Vector3 direction, SphericalWorldSettings settings, float angularStep)
        {
            SphericalSurfaceQuery.GetTangentFrame(direction, out Vector3 north, out Vector3 east);
            Vector3 northPositive = GetSurfacePosition(direction + north * angularStep, settings);
            Vector3 northNegative = GetSurfacePosition(direction - north * angularStep, settings);
            Vector3 eastPositive = GetSurfacePosition(direction + east * angularStep, settings);
            Vector3 eastNegative = GetSurfacePosition(direction - east * angularStep, settings);

            Vector3 northDerivative = northPositive - northNegative;
            Vector3 eastDerivative = eastPositive - eastNegative;
            Vector3 normal = Vector3.Cross(northDerivative, eastDerivative).normalized;
            return Vector3.Dot(normal, direction) >= 0f ? normal : -normal;
        }

        private static Vector3 GetSurfacePosition(Vector3 direction, SphericalWorldSettings settings)
        {
            Vector3 unitDirection = direction.normalized;
            float elevation = SphericalHeightField.SampleHeightValidated(unitDirection, settings);
            return unitDirection * (settings.planetRadiusMeters + elevation);
        }

        private static void DestroyMesh(Mesh mesh)
        {
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(mesh);
            else
                UnityEngine.Object.DestroyImmediate(mesh);
        }
    }
}
