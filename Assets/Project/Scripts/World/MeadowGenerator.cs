
using UnityEngine;

[RequireComponent(typeof(MapRenderer))]
public class MeadowGenerator : MonoBehaviour
{
    [Header("Biome Configuration")]
    [SerializeField] private BiomeDefinition biomeDefinition;

    private MapRenderer mapRenderer;

    [ContextMenu("Generate Biome")]
    public void Generate()
    {
        if (biomeDefinition == null)
        {
            Debug.LogError("No BiomeDefinition assigned!");
            return;
        }

        mapRenderer = GetComponent<MapRenderer>();

        if (mapRenderer == null)
        {
            Debug.LogError("MapRenderer component is missing!");
            return;
        }

        int resolution = Mathf.Max(32, biomeDefinition.resolution);

        MeadowData data = BiomeEcologyGenerator.Generate(
            biomeDefinition,
            resolution
        );

        mapRenderer.Render(data, biomeDefinition);

        Debug.Log($"Biome generated: {biomeDefinition.biomeName}");
    }
}
