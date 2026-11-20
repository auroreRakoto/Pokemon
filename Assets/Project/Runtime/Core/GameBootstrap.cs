
using UnityEngine;
using Pokemon.World;
using Pokemon.Visual;

namespace Pokemon.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Received Data")]
        private MapData mapData;

        [SerializeField]
        private string characterData = "ready player one";

        [SerializeField]
        private string pokemonData = "pokemon data";

        [SerializeField]
        private string visualData = "something working";

        [SerializeField]
        private string storyData = "once upon a time there goes pokemon";

        [Header("Visualization")]
        [SerializeField]
        private MapVisualizationMode mode = MapVisualizationMode.Simple;

        [Header("Created Objects")]
        private Map map;

        private CoordTranslator coordTranslator;

        private void Start()
        {
            // 1. Create the shared coordinate translator.
            coordTranslator = new CoordTranslator();

            // 2. Receive data from providers.
            mapData = MapExample.Generate();

            // 3. Validate received data.
            if (!ValidateData(
                mapData,
                characterData,
                pokemonData,
                visualData,
                storyData))
            {
                Debug.LogError("Data validation failed.");
                enabled = false;
                return;
            }

            // 4. Create runtime objects.
            CreateWorld();
        }

        private bool ValidateData(
            MapData map,
            string character,
            string pokemon,
            string visual,
            string story)
        {
            if (map == null || !map.Validate())
                return false;

            if (string.IsNullOrWhiteSpace(character))
                return false;

            if (string.IsNullOrWhiteSpace(pokemon))
                return false;

            if (string.IsNullOrWhiteSpace(visual))
                return false;

            if (string.IsNullOrWhiteSpace(story))
                return false;

            return true;
        }

        private void CreateWorld()
        {
            Debug.Log("Creating world...");

            map = new Map(mapData);

            Debug.Log("Map object created.");

            MapVisualizer.RunVisualization(
                map,
                mode,
                coordTranslator
            );
        }
    }
}
