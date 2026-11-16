
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWorld",
    menuName = "Pokemon/World/World Definition"
)]
public class WorldDefinition : ScriptableObject
{
    [Header("General Settings")]
    public string worldName = "First World";

    public int seed = 42;

    [Min(32)]
    public int resolution = 512;

    [Header("Biome Configuration")]
    public BiomeDefinition meadowBiome;
    public BiomeDefinition forestBiome;

    [Header("Biome Distribution")]
    [Min(0.01f)]
    public float biomeNoiseScale = 2.5f;

    [Range(0f, 1f)]
    public float forestThreshold = 0.5f;

    [Range(0.001f, 0.5f)]
    public float transitionWidth = 0.12f;
}
