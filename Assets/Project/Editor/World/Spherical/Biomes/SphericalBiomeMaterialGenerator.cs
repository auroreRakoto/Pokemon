#if UNITY_EDITOR
using System.IO;
using Pokemon.Project.World.Spherical.Presentation;
using Pokemon.Project.World.Spherical.Geography;
using UnityEditor;
using UnityEngine;

namespace Pokemon.Project.Editor.World.Spherical.Biomes
{
    /// <summary>Creates and refreshes the approved WORLD-002 URP biome and water material assets.</summary>
    public static class SphericalBiomeMaterialGenerator
    {
        private const string MaterialFolder = "Assets/Project/Content/Materials/Environment";
        private const string MappingFolder = "Assets/Project/Content/World/PlanetStudio";
        private const string TextureFolder = "Assets/Project/Content/Textures/Environment/Biomes/Generated";
        private const float TextureWorldSizeMeters = 18f;
        private const string MappingPath = MappingFolder + "/CFG_BiomeMaterialMapping.asset";
        private const string UrpLitShaderName = "Universal Render Pipeline/Lit";

        private struct MaterialDefinition
        {
            public readonly string AssetName;
            public readonly Color BaseColor;
            public readonly float Smoothness;

            public MaterialDefinition(string assetName, Color baseColor, float smoothness)
            {
                AssetName = assetName;
                BaseColor = baseColor;
                Smoothness = smoothness;
            }
        }

        private static readonly MaterialDefinition[] Definitions =
        {
            new MaterialDefinition("MAT_Ground_Plains", new Color(0.72f, 0.68f, 0.36f, 1f), 0.12f),
            new MaterialDefinition("MAT_Ground_Forest", new Color(0.12f, 0.42f, 0.20f, 1f), 0.10f),
            new MaterialDefinition("MAT_Ground_Desert", new Color(0.86f, 0.56f, 0.25f, 1f), 0.08f),
            new MaterialDefinition("MAT_Ground_Mountains", new Color(0.62f, 0.63f, 0.66f, 1f), 0.16f),
            new MaterialDefinition("MAT_Water_Surface", new Color(0.04f, 0.28f, 0.62f, 1f), 0.82f)
        };

        [MenuItem("Tools/World/Generate or Refresh Biome Materials (URP)")]
        public static void GenerateOrRefresh()
        {
            Shader shader = Shader.Find(UrpLitShaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog("Biome Materials",
                    "The URP/Lit shader could not be found. Check that the project is using URP and that the URP package is installed.", "OK");
                Debug.LogError("WORLD-002 material generation aborted: missing shader " + UrpLitShaderName);
                return;
            }

            EnsureFolder(MaterialFolder);
            EnsureFolder(MappingFolder);
            EnsureFolder(TextureFolder);

            Material[] materials = new Material[Definitions.Length];
            for (int i = 0; i < Definitions.Length; i++)
                materials[i] = CreateOrRefreshMaterial(Definitions[i], shader);

            GenerateAndAssignGroundTextures(materials);

            SphericalBiomeMaterialMapping mapping = AssetDatabase.LoadAssetAtPath<SphericalBiomeMaterialMapping>(MappingPath);
            if (mapping == null)
            {
                mapping = ScriptableObject.CreateInstance<SphericalBiomeMaterialMapping>();
                AssetDatabase.CreateAsset(mapping, MappingPath);
            }

            mapping.SetMaterials(materials[0], materials[1], materials[2], materials[3], materials[4]);
            EditorUtility.SetDirty(mapping);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = mapping;
            EditorGUIUtility.PingObject(mapping);
            Debug.Log("WORLD-002 URP biome materials generated/refreshed: 4 terrestrial biome materials, 1 water material and CFG_BiomeMaterialMapping.", mapping);
        }


        private static void GenerateAndAssignGroundTextures(Material[] materials)
        {
            SphericalBiome[] biomes =
            {
                SphericalBiome.Plains, SphericalBiome.Forest,
                SphericalBiome.Desert, SphericalBiome.Mountains
            };

            for (int i = 0; i < biomes.Length; i++)
            {
                string textureName = "TEX_Ground_" + biomes[i];
                string texturePath = TextureFolder + "/" + textureName + ".png";
                Color32[] pixels = SphericalBiomeTextureGenerator.Generate(biomes[i]);
                Texture2D generated = new Texture2D(SphericalBiomeTextureGenerator.DefaultResolution,
                    SphericalBiomeTextureGenerator.DefaultResolution, TextureFormat.RGBA32, true, false)
                {
                    name = textureName,
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Bilinear,
                    anisoLevel = 8
                };
                generated.SetPixels32(pixels);
                generated.Apply(true, false);
                File.WriteAllBytes(texturePath, generated.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(generated);
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.sRGBTexture = true;
                    importer.alphaSource = TextureImporterAlphaSource.None;
                    importer.mipmapEnabled = true;
                    importer.wrapMode = TextureWrapMode.Repeat;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.anisoLevel = 8;
                    importer.maxTextureSize = SphericalBiomeTextureGenerator.DefaultResolution;
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.SaveAndReimport();
                }

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                    throw new System.InvalidOperationException("Generated biome texture could not be imported: " + texturePath);
                materials[i].SetTexture("_BaseMap", texture);
                materials[i].SetTextureScale("_BaseMap", Vector2.one / TextureWorldSizeMeters);
                EditorUtility.SetDirty(materials[i]);
            }
        }

        private static Material CreateOrRefreshMaterial(MaterialDefinition definition, Shader shader)
        {
            string path = MaterialFolder + "/" + definition.AssetName + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = definition.AssetName };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                Texture existingBaseMap = material.GetTexture("_BaseMap");
                material.shader = shader;
                if (existingBaseMap != null)
                    material.SetTexture("_BaseMap", existingBaseMap);
            }

            // These are generator-owned properties. Existing texture assignments are preserved.
            material.SetColor("_BaseColor", definition.BaseColor);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", definition.Smoothness);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif
