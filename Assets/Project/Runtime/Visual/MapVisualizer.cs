
using UnityEngine;
using UnityEngine.Rendering;
using Pokemon.World;

namespace Pokemon.Visual
{
    public enum MapVisualizationMode
    {
        Simple,
        Medium,
        Advanced
    }

    public static class MapVisualizer
    {
        private static GameObject visualizationObject;
        private static Material visualizationMaterial;
        private static Texture2D visualizationTexture;

        private static CoordTranslator coordinateTranslator;

        public static void RunVisualization(
            Map map,
            MapVisualizationMode mode,
            CoordTranslator translator)
        {
            ClearVisualization();

            if (map == null || map.Data == null)
            {
                Debug.LogError("Map is null.");
                return;
            }

            if (translator == null)
            {
                Debug.LogError("Coordinate translator is null.");
                return;
            }

            if (!map.Data.Validate())
            {
                Debug.LogError("Invalid map data.");
                return;
            }

            coordinateTranslator = translator;

            switch (mode)
            {
                case MapVisualizationMode.Simple:
                    RunSimpleVisualization(map);
                    break;

                case MapVisualizationMode.Medium:
                    Debug.Log("Medium visualization: IN PROGRESS");
                    break;

                case MapVisualizationMode.Advanced:
                    Debug.Log("Advanced visualization (Sphere): IN PROGRESS");
                    break;

                default:
                    Debug.LogError("Unknown visualization mode.");
                    break;
            }
        }

        // SIMPLE MODE
        private static void RunSimpleVisualization(Map map)
        {
            Texture2D texture = GenerateHeightTexture(map);

            if (texture == null)
                return;

            DisplayTexture(texture);

            Debug.Log("Simple visualization completed.");
        }

        // HEIGHTMAP GENERATION
        private static Texture2D GenerateHeightTexture(Map map)
        {
            MapData data = map.Data;

            MapLayerData heightLayer = data.GetLayer("height");

            if (heightLayer == null)
            {
                Debug.LogError("Height layer not found.");
                return null;
            }

            Texture2D texture = new Texture2D(
                data.Width,
                data.Height,
                TextureFormat.RGBA32,
                false
            );

            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    // Grid coordinates pass through the translator.
                    // Grid2D -> Grid2D is an identity operation.
                    Vector3 translatedCoordinates =
                        coordinateTranslator.Translate(
                            new Vector3(x, y, 0f),
                            CoordTranslator.CoordinateSpace.Grid2D,
                            CoordTranslator.CoordinateSpace.Grid2D
                        );

                    int pixelX = Mathf.RoundToInt(
                        translatedCoordinates.x);

                    int pixelY = Mathf.RoundToInt(
                        translatedCoordinates.y);

                    float height = heightLayer.GetValue(
                        x,
                        y,
                        data.Width
                    );

                    float grayscale = Mathf.Clamp01(height);

                    Color color = new Color(
                        grayscale,
                        grayscale,
                        grayscale
                    );

                    texture.SetPixel(pixelX, pixelY, color);
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();

            return texture;
        }

        // SIMPLE MODE DISPLAY
        private static void DisplayTexture(Texture2D texture)
        {
            Shader shader = GetDisplayShader();

            if (shader == null)
            {
                Debug.LogError(
                    "No compatible visualization shader found.");

                Object.Destroy(texture);
                return;
            }

            Camera camera = Camera.main;

            if (camera == null)
            {
                Debug.LogError("No Main Camera found.");

                Object.Destroy(texture);
                return;
            }

            visualizationTexture = texture;

            visualizationObject = GameObject.CreatePrimitive(
                PrimitiveType.Quad
            );

            visualizationObject.name = "Map Visualization";

            visualizationObject.transform.position =
                camera.transform.position +
                camera.transform.forward * 5f;

            visualizationObject.transform.rotation =
                camera.transform.rotation;

            float displayHeight = 4f;

            float aspectRatio =
                (float)texture.width / texture.height;

            visualizationObject.transform.localScale =
                new Vector3(
                    displayHeight * aspectRatio,
                    displayHeight,
                    1f
                );

            visualizationMaterial = new Material(shader);
            visualizationMaterial.mainTexture = texture;

            Renderer objectRenderer =
                visualizationObject.GetComponent<Renderer>();

            objectRenderer.material = visualizationMaterial;

            Collider quadCollider =
                visualizationObject.GetComponent<Collider>();

            if (quadCollider != null)
                Object.Destroy(quadCollider);
        }

        // SHADER SELECTION
        private static Shader GetDisplayShader()
        {
            bool usesScriptableRenderPipeline =
                GraphicsSettings.currentRenderPipeline != null;

            if (usesScriptableRenderPipeline)
            {
                return Shader.Find(
                    "Universal Render Pipeline/Unlit");
            }

            return Shader.Find("Unlit/Texture");
        }

        // CLEANUP
        public static void ClearVisualization()
        {
            if (visualizationObject != null)
                Object.Destroy(visualizationObject);

            if (visualizationMaterial != null)
                Object.Destroy(visualizationMaterial);

            if (visualizationTexture != null)
                Object.Destroy(visualizationTexture);

            visualizationObject = null;
            visualizationMaterial = null;
            visualizationTexture = null;
            coordinateTranslator = null;
        }
    }
}
