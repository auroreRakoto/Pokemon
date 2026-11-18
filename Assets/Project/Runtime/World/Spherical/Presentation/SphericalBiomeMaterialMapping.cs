using UnityEngine;
using Pokemon.Project.World.Spherical.Geography;

namespace Pokemon.Project.World.Spherical.Presentation
{
    /// <summary>Authoritative material references for the four terrestrial biomes and water.</summary>
    [CreateAssetMenu(menuName = "Pokemon/World/Spherical Biome Material Mapping", fileName = "CFG_BiomeMaterialMapping")]
    public sealed class SphericalBiomeMaterialMapping : ScriptableObject
    {
        [Header("Terrestrial biomes")]
        [SerializeField] private Material plains;
        [SerializeField] private Material forest;
        [SerializeField] private Material desert;
        [SerializeField] private Material mountains;

        [Header("Water")]
        [SerializeField] private Material waterSurface;

        public Material Plains => plains;
        public Material Forest => forest;
        public Material Desert => desert;
        public Material Mountains => mountains;
        public Material WaterSurface => waterSurface;

        /// <summary>Copies the approved material base colours into the shared biome-blend shader.</summary>
        public void ConfigureBlendMaterial(Material blendMaterial)
        {
            if (blendMaterial == null) throw new System.ArgumentNullException(nameof(blendMaterial));
            if (plains == null || forest == null || desert == null || mountains == null || waterSurface == null)
                throw new System.InvalidOperationException("CFG_BiomeMaterialMapping must reference all four ground materials and the water material.");
            SetBlendColor(blendMaterial, "_PlainsColor", plains);
            SetBlendColor(blendMaterial, "_ForestColor", forest);
            SetBlendColor(blendMaterial, "_DesertColor", desert);
            SetBlendColor(blendMaterial, "_MountainsColor", mountains);
            SetBlendColor(blendMaterial, "_WaterColor", waterSurface);
            SetBlendTexture(blendMaterial, "_PlainsTex", plains);
            SetBlendTexture(blendMaterial, "_ForestTex", forest);
            SetBlendTexture(blendMaterial, "_DesertTex", desert);
            SetBlendTexture(blendMaterial, "_MountainsTex", mountains);
            if (blendMaterial.HasProperty("_TextureWorldSize")) blendMaterial.SetFloat("_TextureWorldSize", 8f);
        }

        private static void SetBlendTexture(Material target, string propertyName, Material source)
        {
            if (!target.HasProperty(propertyName) || source == null || !source.HasProperty("_BaseMap")) return;
            target.SetTexture(propertyName, source.GetTexture("_BaseMap"));
        }

        private static void SetBlendColor(Material target, string propertyName, Material source)
        {
            if (!target.HasProperty(propertyName) || source == null) return;
            Color color = source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : source.color;
            target.SetColor(propertyName, color);
        }

        public Material GetGroundMaterial(SphericalBiome biome)
        {
            switch (biome)
            {
                case SphericalBiome.Plains: return plains;
                case SphericalBiome.Forest: return forest;
                case SphericalBiome.Desert: return desert;
                case SphericalBiome.Mountains: return mountains;
                default: return null;
            }
        }

#if UNITY_EDITOR
        public void SetMaterials(Material plainsMaterial, Material forestMaterial,
            Material desertMaterial, Material mountainMaterial, Material waterMaterial)
        {
            plains = plainsMaterial;
            forest = forestMaterial;
            desert = desertMaterial;
            mountains = mountainMaterial;
            waterSurface = waterMaterial;
        }
#endif
    }
}
