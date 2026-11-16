
using UnityEngine;
using UnityEngine.UI;

public class ResourceMinimap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private RawImage minimapImage;

    [Header("Display")]
    [SerializeField] private int textureResolution = 256;

    [Min(0.05f)]
    [SerializeField] private float refreshInterval = 0.2f;

    [Header("Colors")]
    [SerializeField] private Color emptyResourceColor =
        new Color(0.12f, 0.025f, 0.025f);

    [SerializeField] private Color noVegetationColor =
        new Color(0.025f, 0.06f, 0.12f);

    [SerializeField] private Color lowResourceColor =
        new Color(0.85f, 0.12f, 0.08f);

    [SerializeField] private Color mediumResourceColor =
        new Color(1f, 0.85f, 0.1f);

    [SerializeField] private Color highResourceColor =
        new Color(0.12f, 0.75f, 0.2f);

    private Texture2D minimapTexture;
    private Color[] pixels;

    private float refreshTimer;

    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    private void Start()
    {
        if (resourceManager == null || minimapImage == null)
        {
            Debug.LogError(
                "ResourceMinimap: missing references."
            );

            enabled = false;
            return;
        }

        textureResolution = Mathf.Max(16, textureResolution);

        minimapTexture = new Texture2D(
            textureResolution,
            textureResolution,
            TextureFormat.RGBA32,
            false
        );

        minimapTexture.filterMode = FilterMode.Point;
        minimapTexture.wrapMode = TextureWrapMode.Clamp;

        pixels = new Color[
            textureResolution * textureResolution
        ];

        minimapImage.texture = minimapTexture;
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (!resourceManager.IsInitialized)
            return;

        // Use real time instead of simulation time.
        refreshTimer += Time.unscaledDeltaTime;

        if (refreshTimer < refreshInterval)
            return;

        refreshTimer = 0f;

        UpdateMinimap();
    }

    // --------------------------------------------------
    // MINIMAP GENERATION
    // --------------------------------------------------

    private void UpdateMinimap()
    {
        int sourceResolution = resourceManager.Resolution;

        if (sourceResolution <= 0)
            return;

        for (int y = 0; y < textureResolution; y++)
        {
            for (int x = 0; x < textureResolution; x++)
            {
                int sourceX = Mathf.Clamp(
                    Mathf.FloorToInt(
                        (float)x / textureResolution * sourceResolution
                    ),
                    0,
                    sourceResolution - 1
                );

                int sourceY = Mathf.Clamp(
                    Mathf.FloorToInt(
                        (float)y / textureResolution * sourceResolution
                    ),
                    0,
                    sourceResolution - 1
                );

                int sourceIndex =
                    sourceY * sourceResolution + sourceX;

                int pixelIndex =
                    y * textureResolution + x;

                float initial =
                    resourceManager.GetInitialFoodAtIndex(sourceIndex);

                float available =
                    resourceManager.GetAvailableFoodAtIndex(sourceIndex);

                pixels[pixelIndex] =
                    GetResourceColor(initial, available);
            }
        }

        minimapTexture.SetPixels(pixels);
        minimapTexture.Apply(false);
    }

    // --------------------------------------------------
    // RESOURCE COLORS
    // --------------------------------------------------

    private Color GetResourceColor(
        float initial,
        float available
    )
    {
        // No initial vegetation in this cell.
        if (initial <= 0.001f)
            return noVegetationColor;

        float ratio = Mathf.Clamp01(available / initial);

        // Vegetation completely depleted.
        if (ratio <= 0.001f)
            return emptyResourceColor;

        // Less than 50% remaining.
        if (ratio < 0.5f)
        {
            return Color.Lerp(
                lowResourceColor,
                mediumResourceColor,
                ratio / 0.5f
            );
        }

        // More than 50% remaining.
        return Color.Lerp(
            mediumResourceColor,
            highResourceColor,
            (ratio - 0.5f) / 0.5f
        );
    }

    // --------------------------------------------------
    // CLEANUP
    // --------------------------------------------------

    private void OnDestroy()
    {
        if (minimapTexture != null)
            Destroy(minimapTexture);
    }
}
