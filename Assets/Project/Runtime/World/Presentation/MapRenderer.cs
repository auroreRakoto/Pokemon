
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MapRenderer : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private BiomeLayer displayLayer =
        BiomeLayer.Combined;

    private Texture2D generatedTexture;

    public void Render(MeadowData data, BiomeDefinition definition)
    {
        int resolution = data.Resolution;

        Color[] pixels = new Color[resolution * resolution];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = GetPixelColor(data, definition, i);
        }

        if (generatedTexture != null)
        {
            if (Application.isPlaying)
                Destroy(generatedTexture);
            else
                DestroyImmediate(generatedTexture);
        }

        generatedTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false
        );

        generatedTexture.wrapMode = TextureWrapMode.Clamp;
        generatedTexture.filterMode = FilterMode.Point;

        generatedTexture.SetPixels(pixels);
        generatedTexture.Apply();

        ApplyTexture(generatedTexture);
    }

    private Color GetPixelColor(
        MeadowData data,
        BiomeDefinition definition,
        int index
    )
    {
        float vegetation = data.Vegetation[index];
        float flora = data.Flora[index];
        float rocks = data.Rocks[index];
        float water = data.Water[index];
        float humidity = data.Humidity[index];

        switch (displayLayer)
        {
            case BiomeLayer.Vegetation:
                return GetGrayscaleColor(
                    vegetation, definition
                );

            case BiomeLayer.Flora:
                return GetGrayscaleColor(
                    flora, definition
                );

            case BiomeLayer.Rocks:
                return GetGrayscaleColor(
                    rocks, definition
                );

            case BiomeLayer.Water:
                return GetGrayscaleColor(
                    water, definition
                );

            case BiomeLayer.Humidity:
                return GetGrayscaleColor(
                    humidity, definition
                );

            case BiomeLayer.Combined:
            default:
                return GetCombinedColor(
                    vegetation,
                    flora,
                    rocks,
                    water,
                    definition
                );
        }
    }

    private Color GetGrayscaleColor(
        float value,
        BiomeDefinition definition
    )
    {
        return Color.Lerp(
            definition.emptyLayerColor,
            definition.fullLayerColor,
            value
        );
    }

    private Color GetCombinedColor(
        float vegetation,
        float flora,
        float rocks,
        float water,
        BiomeDefinition definition
    )
    {
        Color color = definition.meadowColor;

        if (vegetation >= 0.5f)
            color = definition.pastureColor;

        if (vegetation >= 1f)
            color = definition.denseVegetationColor;

        if (flora > 0f)
            color = definition.flowerColor;

        if (rocks > 0f)
            color = definition.rockColor;

        if (water > 0f)
            color = definition.waterColor;

        return color;
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

    private void OnDestroy()
    {
        if (generatedTexture != null)
        {
            if (Application.isPlaying)
                Destroy(generatedTexture);
            else
                DestroyImmediate(generatedTexture);
        }
    }
}
