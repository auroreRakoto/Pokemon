
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

    public void Render(
        WorldEcologyData data,
        WorldDefinition definition
    )
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
        generatedTexture.filterMode = FilterMode.Bilinear;

        generatedTexture.SetPixels(pixels);
        generatedTexture.Apply();

        ApplyTexture(generatedTexture);
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
        return Color.Lerp(
            emptyColor,
            fullColor,
            Mathf.Clamp01(value)
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
