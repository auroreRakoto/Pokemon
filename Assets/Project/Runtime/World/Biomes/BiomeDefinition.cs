
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewBiome",
    menuName = "Pokemon/Biomes/Biome Definition"
)]
public class BiomeDefinition : ScriptableObject
{
    [Header("General Settings")]
    public string biomeName = "Meadow";
    public int seed = 42;
    [Min(32)] public int resolution = 512;

    [Header("Vegetation")]
    [Min(0.01f)] public float vegetationScale = 4f;
    [Min(1)] public int octaves = 4;
    [Range(0f, 1f)] public float persistence = 0.5f;
    [Min(1f)] public float lacunarity = 2f;

    [Range(0f, 1f)] public float pastureThreshold = 0.48f;
    [Range(0f, 1f)] public float denseVegetationThreshold = 0.65f;

    [Header("Flora")]
    [Min(0)] public int flowerCount = 250;
    [Min(1)] public int flowerRadius = 2;

    [Header("Rocks")]
    [Min(0)] public int rockCount = 70;
    [Min(1)] public int rockRadius = 4;

    [Header("Fruit Bushes")]
    [Min(0.01f)] public float bushNoiseScale = 5f;
    [Range(0f, 1f)] public float bushNoiseThreshold = 0.72f;
    [Min(0)] public int bushCount = 30;
    [Min(1)] public int bushRadius = 2;

    [Header("Water")]
    [Min(0.01f)] public float waterScale = 3f;
    [Range(0f, 1f)] public float waterThreshold = 0.73f;

    [Header("Ecological Rules")]
    [Header("Humidity")]
    [Min(1)] public int humidityRadius = 60;
    [Min(1f)] public float humidityDecayDistance = 20f;
    [Range(0f, 1f)] public float humidityInfluence = 0.6f;

    [Range(0f, 1f)] public float floraVegetationThreshold = 0.5f;
    [Range(0f, 1f)] public float floraWaterThreshold = 0.25f;
    [Range(0f, 1f)] public float rockVegetationExclusion = 1f;

    [Header("Colors")]
    public Color mapColor = Color.green;
    public Color meadowColor = new Color(0.61f, 0.80f, 0.41f);
    public Color pastureColor = new Color(0.33f, 0.54f, 0.26f);
    public Color denseVegetationColor = new Color(0.14f, 0.32f, 0.17f);
    public Color flowerColor = new Color(0.08f, 0.19f, 0.11f);
    public Color rockColor = new Color(0.43f, 0.42f, 0.38f);
    public Color waterColor = new Color(0.32f, 0.52f, 0.66f);
    public Color bushColor = new Color(0.35f, 0.65f, 0.20f);

    public Color emptyLayerColor = Color.black;
    public Color fullLayerColor = Color.white;
}
