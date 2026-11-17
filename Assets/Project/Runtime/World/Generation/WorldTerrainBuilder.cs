using System;
using UnityEngine;

namespace Pokemon.Project.World.Generation
{
    /// <summary>
    /// Runtime-safe presentation layer for generated world data.
    /// This class contains no UnityEditor dependency and can be called in Play Mode.
    /// </summary>
    public static class WorldTerrainBuilder
    {
        public const string DefaultObjectName = "Generated World";
        public const string DefaultShaderName = "Pokemon/WorldVertexColor";

        /// <summary>Builds a mesh from the immutable generation result.</summary>
        public static Mesh BuildMesh(WorldGenerationResult result, float verticalExaggeration = 1f)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (result.Width < 2 || result.Height < 2)
                throw new ArgumentException("A terrain mesh requires at least 2 samples on each axis.", nameof(result));
            if (verticalExaggeration < 0f)
                throw new ArgumentOutOfRangeException(nameof(verticalExaggeration));

            int width = result.Width;
            int depth = result.Height;
            int vertexCount = checked(width * depth);
            var vertices = new Vector3[vertexCount];
            var colors = new Color[vertexCount];
            var uv = new Vector2[vertexCount];
            var triangles = new int[checked((width - 1) * (depth - 1) * 6)];

            float stepX = result.WorldWidthMeters / (width - 1);
            float stepZ = result.WorldDepthMeters / (depth - 1);
            float originX = -result.WorldWidthMeters * 0.5f;
            float originZ = -result.WorldDepthMeters * 0.5f;

            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    vertices[index] = new Vector3(
                        originX + x * stepX,
                        result.HeightMeters[index] * verticalExaggeration,
                        originZ + z * stepZ);
                    uv[index] = new Vector2(x / (float)(width - 1), z / (float)(depth - 1));
                    colors[index] = GetCellColor(result, index);
                }
            }

            int triangleIndex = 0;
            for (int z = 0; z < depth - 1; z++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int southWest = z * width + x;
                    int southEast = southWest + 1;
                    int northWest = southWest + width;
                    int northEast = northWest + 1;

                    // Consistent upward-facing winding for an X/Z ground plane.
                    triangles[triangleIndex++] = southWest;
                    triangles[triangleIndex++] = northWest;
                    triangles[triangleIndex++] = southEast;
                    triangles[triangleIndex++] = southEast;
                    triangles[triangleIndex++] = northWest;
                    triangles[triangleIndex++] = northEast;
                }
            }

            var mesh = new Mesh
            {
                name = $"GeneratedWorld_{width}x{depth}",
                indexFormat = vertexCount > 65535
                    ? UnityEngine.Rendering.IndexFormat.UInt32
                    : UnityEngine.Rendering.IndexFormat.UInt16
            };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Creates a new terrain GameObject. The caller owns the returned object and
        /// any transient material/mesh created for it.
        /// </summary>
        public static GameObject CreateSceneObject(
            WorldGenerationResult result,
            Transform parent = null,
            Material materialOverride = null,
            float verticalExaggeration = 1f,
            bool addMeshCollider = true,
            string objectName = DefaultObjectName)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            Mesh mesh = BuildMesh(result, verticalExaggeration);
            Material material = materialOverride != null ? materialOverride : CreateRuntimeMaterial();
            var root = new GameObject(string.IsNullOrWhiteSpace(objectName) ? DefaultObjectName : objectName);
            if (parent != null) root.transform.SetParent(parent, false);

            var filter = root.AddComponent<MeshFilter>();
            var renderer = root.AddComponent<MeshRenderer>();
            filter.sharedMesh = mesh;
            renderer.sharedMaterial = material;
            if (addMeshCollider)
            {
                var collider = root.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
            }

            var generated = root.AddComponent<GeneratedWorldSurface>();
            generated.Initialize(mesh, materialOverride == null ? material : null);
            return root;
        }

        /// <summary>
        /// Replaces the mesh on an existing scene object, or creates the object if
        /// target is null. Suitable for runtime regeneration without duplicate roots.
        /// </summary>
        public static GameObject ApplyToScene(
            WorldGenerationResult result,
            GameObject target,
            Transform parent = null,
            Material materialOverride = null,
            float verticalExaggeration = 1f,
            bool addMeshCollider = true,
            string objectName = DefaultObjectName)
        {
            if (target == null)
                return CreateSceneObject(result, parent, materialOverride, verticalExaggeration, addMeshCollider, objectName);

            Mesh mesh = BuildMesh(result, verticalExaggeration);
            var filter = target.GetComponent<MeshFilter>() ?? target.AddComponent<MeshFilter>();
            var renderer = target.GetComponent<MeshRenderer>() ?? target.AddComponent<MeshRenderer>();
            var oldSurface = target.GetComponent<GeneratedWorldSurface>();
            Material material = materialOverride;
            bool ownsMaterial = false;
            if (material == null && renderer.sharedMaterial != null)
                material = renderer.sharedMaterial;
            if (material == null)
            {
                material = CreateRuntimeMaterial();
                ownsMaterial = true;
            }

            filter.sharedMesh = mesh;
            renderer.sharedMaterial = material;

            var collider = target.GetComponent<MeshCollider>();
            if (addMeshCollider)
            {
                collider = collider != null ? collider : target.AddComponent<MeshCollider>();
                collider.sharedMesh = null;
                collider.sharedMesh = mesh;
            }
            else if (collider != null)
            {
                UnityEngine.Object.Destroy(collider);
            }

            if (oldSurface == null) oldSurface = target.AddComponent<GeneratedWorldSurface>();
            oldSurface.Initialize(mesh, ownsMaterial ? material : null);
            target.name = string.IsNullOrWhiteSpace(objectName) ? DefaultObjectName : objectName;
            if (parent != null && target.transform.parent != parent)
                target.transform.SetParent(parent, false);
            return target;
        }

        public static Material CreateRuntimeMaterial()
        {
            Shader shader = Shader.Find(DefaultShaderName);
            if (shader == null)
                throw new InvalidOperationException(
                    $"Shader '{DefaultShaderName}' was not found. Ensure WorldVertexColor.shader is imported.");
            return new Material(shader) { name = "Generated World Runtime Material" };
        }

        private static Color GetCellColor(WorldGenerationResult result, int index)
        {
            if (result.WaterType[index] != (byte)WaterBodyType.Land)
            {
                switch ((WaterBodyType)result.WaterType[index])
                {
                    case WaterBodyType.Ocean: return new Color(0.035f, 0.19f, 0.52f);
                    case WaterBodyType.Lake: return new Color(0.08f, 0.42f, 0.76f);
                    case WaterBodyType.River: return new Color(0.12f, 0.62f, 0.86f);
                }
            }

            Color color;
            switch ((WorldBiome)result.Biome[index])
            {
                case WorldBiome.Beach: color = new Color(0.88f, 0.77f, 0.48f); break;
                case WorldBiome.Forest: color = new Color(0.08f, 0.31f, 0.14f); break;
                case WorldBiome.Wetland: color = new Color(0.20f, 0.46f, 0.37f); break;
                case WorldBiome.Hills: color = new Color(0.53f, 0.47f, 0.28f); break;
                case WorldBiome.Mountain: color = new Color(0.63f, 0.64f, 0.66f); break;
                default: color = new Color(0.39f, 0.66f, 0.30f); break;
            }

            if (result.RoadMask[index] > 0.5f)
                color = Color.Lerp(color, new Color(0.38f, 0.30f, 0.20f), 0.85f);
            return color;
        }
    }

    /// <summary>Tracks transient runtime-generated resources so they are released with the scene object.</summary>
    [DisallowMultipleComponent]
    public sealed class GeneratedWorldSurface : MonoBehaviour
    {
        private Mesh ownedMesh;
        private Material ownedMaterial;

        public void Initialize(Mesh mesh, Material material)
        {
            if (ownedMesh != null && ownedMesh != mesh)
                DestroyResource(ownedMesh);
            if (ownedMaterial != null && ownedMaterial != material)
                DestroyResource(ownedMaterial);
            ownedMesh = mesh;
            ownedMaterial = material;
        }

        private void OnDestroy()
        {
            if (ownedMesh != null) DestroyResource(ownedMesh);
            if (ownedMaterial != null) DestroyResource(ownedMaterial);
        }

        private static void DestroyResource(UnityEngine.Object resource)
        {
            if (resource == null) return;
            if (Application.isPlaying) Destroy(resource);
            else DestroyImmediate(resource);
        }
    }
}
