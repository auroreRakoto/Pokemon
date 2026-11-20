
using System.Text;
using UnityEngine;
using Pokemon.World;
using Pokemon.CharacterSystem;

namespace Pokemon.Visualization
{
    public class TerminalVisualization : IWorldVisualization
    {
        public void Render(
            IMapData map,
            Character character,
            string message
        )
        {
            StringBuilder output = new StringBuilder();

            output.AppendLine("========== POKEMON WORLD ==========");

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);

                    if (character.Position == position)
                    {
                        output.Append('P');
                        continue;
                    }

                    TileType tile = map.GetTileAt(position);

                    switch (tile)
                    {
                        case TileType.Grass:
                            output.Append('.');
                            break;

                        case TileType.Tree:
                            output.Append('T');
                            break;

                        case TileType.Wall:
                            output.Append('#');
                            break;

                        case TileType.Water:
                            output.Append('W');
                            break;
                    }
                }

                output.AppendLine();
            }

            output.AppendLine("-----------------------------------");
            output.AppendLine(
                $"Position X: {character.Position.x} | Y: {character.Position.y}"
            );

            output.AppendLine(message);
            output.AppendLine("===================================");

            Debug.Log(output.ToString());
        }
    }
}
