using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Presentation
{
    /// <summary>
    /// Owns the six renderable/collidable face objects for one prototype planet.
    /// Meshes and the fallback material are created at runtime and released on rebuild/destruction.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SphericalPlanetPresenter : MonoBehaviour
    {
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private bool createMeshColliders = true;
        [SerializeField] private Color fallbackColor = new Color(0.24f, 0.48f, 0.29f, 1f);

        private readonly Mesh[] _ownedMeshes = new Mesh[6];
        private Material _ownedMaterial;
        private Transform _facesRoot;

        public void SetTerrainMaterial(Material material)
        {
            terrainMaterial = material;
        }

        public void Build(SphericalWorldSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            ClearGeneratedContent();
            _facesRoot = new GameObject("PlanetFaces").transform;
            _facesRoot.SetParent(transform, false);

            Mesh[] meshes = SphericalPlanetMeshGenerator.GenerateAllFaces(settings);
            Material material = ResolveMaterial();

            try
            {
                for (int i = 0; i < meshes.Length; i++)
                    _ownedMeshes[i] = meshes[i];

                for (int i = 0; i < meshes.Length; i++)
                    CreateFaceObject((CubeSphereFace)i, meshes[i], material);
            }
            catch
            {
                ClearGeneratedContent();
                throw;
            }
        }

        public void ClearGeneratedContent()
        {
            for (int i = 0; i < _ownedMeshes.Length; i++)
            {
                if (_ownedMeshes[i] != null)
                {
                    DestroyOwnedObject(_ownedMeshes[i]);
                    _ownedMeshes[i] = null;
                }
            }

            if (_facesRoot != null)
            {
                DestroyOwnedObject(_facesRoot.gameObject);
                _facesRoot = null;
            }

            if (_ownedMaterial != null)
            {
                DestroyOwnedObject(_ownedMaterial);
                _ownedMaterial = null;
            }
        }

        private void CreateFaceObject(CubeSphereFace face, Mesh mesh, Material material)
        {
            GameObject faceObject = new GameObject(face.ToString());
            faceObject.transform.SetParent(_facesRoot, false);

            MeshFilter filter = faceObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = faceObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;

            if (createMeshColliders)
            {
                MeshCollider meshCollider = faceObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
            }
        }

        private Material ResolveMaterial()
        {
            if (terrainMaterial != null)
                return terrainMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException("No compatible shader was found for the spherical demo terrain.");

            _ownedMaterial = new Material(shader)
            {
                name = "SphericalWorld_DemoMaterial",
                color = fallbackColor
            };
            return _ownedMaterial;
        }

        private void OnDestroy()
        {
            ClearGeneratedContent();
        }

        private static void DestroyOwnedObject(UnityEngine.Object ownedObject)
        {
            if (ownedObject == null)
                return;

            if (Application.isPlaying)
                Destroy(ownedObject);
            else
                DestroyImmediate(ownedObject);
        }
    }
}
