
using UnityEngine;
using Pokemon.World;
using Pokemon.CharacterSystem;
using Pokemon.Visualization;

namespace Pokemon.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("World")]
        [SerializeField]
        private GridMapData mapData;

        [SerializeField]
        private Vector2Int startPosition = new Vector2Int(2, 2);

        private Character character;

        private IWorldVisualization visualization;

        private void Start()
        {
            if (mapData == null)
            {
                Debug.LogError("No MapData assigned.");
                enabled = false;
                return;
            }

            if (!mapData.Validate(out string error))
            {
                Debug.LogError("Invalid map: " + error);
                enabled = false;
                return;
            }

            try
            {
                character = new Character(startPosition, mapData);
            }
            catch (System.Exception exception)
            {
                Debug.LogError(exception.Message);
                enabled = false;
                return;
            }

            visualization = new TerminalVisualization();

            visualization.Render(
                mapData,
                character,
                "Game started."
            );
        }

        private void Update()
        {
            Vector2Int direction = ReadMovementInput();

            if (direction == Vector2Int.zero)
                return;

            bool moved = character.TryMove(direction, mapData);

            string message = moved
                ? "Player moved."
                : "Movement blocked.";

            visualization.Render(
                mapData,
                character,
                message
            );
        }

        private Vector2Int ReadMovementInput()
        {
            if (Input.GetKeyDown(KeyCode.Z) ||
                Input.GetKeyDown(KeyCode.UpArrow))
                return Vector2Int.down;

            if (Input.GetKeyDown(KeyCode.S) ||
                Input.GetKeyDown(KeyCode.DownArrow))
                return Vector2Int.up;

            if (Input.GetKeyDown(KeyCode.Q) ||
                Input.GetKeyDown(KeyCode.LeftArrow))
                return Vector2Int.left;

            if (Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.RightArrow))
                return Vector2Int.right;

            return Vector2Int.zero;
        }
    }
}
