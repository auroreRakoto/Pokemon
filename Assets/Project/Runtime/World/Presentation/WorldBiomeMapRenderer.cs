
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WorldBiomeMapRenderer : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private WorldEcologyLayer displayLayer =
        WorldEcologyLayer.BiomeMap;

    [Header("Ecology Visualization")]
    [SerializeField] private Color emptyColor = Color.black;
    [SerializeField] private Color fullColor = Color.white;

    private Texture2D generatedTexture;
    private Texture2D vegetationTexture;

    public Texture2D GeneratedTexture => generatedTexture;
    public Texture2D VegetationTexture => vegetationTexture;

    public void Render(
        WorldEcologyData data,
        WorldDefinition definition
    )
    {
        int resolution = data.Resolution;

        Color[] displayPixels = new Color[resolution * resolution];
        Color[] vegetationPixels = new Color[resolution * resolution];

        for (int i = 0; i < displayPixels.Length; i++)
        {
            displayPixels[i] = GetPixelColor(data, definition, i);

            float vegetation = Mathf.Clamp01(data.Vegetation[i]);

            vegetationPixels[i] = new Color(
                vegetation,
                vegetation,
                vegetation,
                1f
            );
        }

        DestroyGeneratedTextures();

        // Texture used for world visualization.
        generatedTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false
        );

        generatedTexture.wrapMode = TextureWrapMode.Clamp;
        generatedTexture.filterMode = FilterMode.Bilinear;
        generatedTexture.SetPixels(displayPixels);
        generatedTexture.Apply();

        // Dedicated linear texture used by vegetation placement.
        vegetationTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false,
            true
        );

        vegetationTexture.wrapMode = TextureWrapMode.Clamp;
        vegetationTexture.filterMode = FilterMode.Bilinear;
        vegetationTexture.SetPixels(vegetationPixels);
        vegetationTexture.Apply();

        ApplyTexture(generatedTexture);

        Debug.Log(
            $"World textures generated: {resolution}x{resolution} | " +
            $"Display layer: {displayLayer} | Vegetation texture ready."
        );
    }

    private Color GetPixelColor(
        WorldEcologyData data,
        WorldDefinition definition,
        int index
    )
    {
        float forestWeight = data.ForestWeight[index];

        switch (displayLayer)
        {
            case WorldEcologyLayer.Vegetation:
                return GetGrayscale(data.Vegetation[index]);

            case WorldEcologyLayer.Flora:
                return GetGrayscale(data.Flora[index]);

            case WorldEcologyLayer.Rocks:
                return GetGrayscale(data.Rocks[index]);

            case WorldEcologyLayer.Water:
                return GetGrayscale(data.Water[index]);

            case WorldEcologyLayer.Humidity:
                return GetGrayscale(data.Humidity[index]);

            case WorldEcologyLayer.BiomeMap:
            default:
                return Color.Lerp(
                    definition.meadowBiome.mapColor,
                    definition.forestBiome.mapColor,
                    forestWeight
                );
        }
    }

    private Color GetGrayscale(float value)
    {
        float intensity = Mathf.Clamp01(value);

        return new Color(
            intensity,
            intensity,
            intensity,
            1f
        );
    }

    private void ApplyTexture(Texture2D texture)
    {
        Renderer targetRenderer = GetComponent<Renderer>();

        MaterialPropertyBlock properties =
            new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(properties);

        properties.SetTexture("_BaseMap", texture);
        properties.SetTexture("_MainTex", texture);

        targetRenderer.SetPropertyBlock(properties);
    }

    private void DestroyGeneratedTextures()
    {
        if (generatedTexture != null)
        {
            if (Application.isPlaying)
                Destroy(generatedTexture);
            else
                DestroyImmediate(generatedTexture);

            generatedTexture = null;
        }

        if (vegetationTexture != null)
        {
            if (Application.isPlaying)
                Destroy(vegetationTexture);
            else
                DestroyImmediate(vegetationTexture);

            vegetationTexture = null;
        }
    }

    private void OnDestroy()
    {
        DestroyGeneratedTextures();
    }
}
