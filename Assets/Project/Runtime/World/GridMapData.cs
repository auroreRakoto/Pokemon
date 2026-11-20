
using System.Collections.Generic;
using UnityEngine;

namespace Pokemon.World
{
    [CreateAssetMenu(
        fileName = "GridMapData",
        menuName = "Pokemon/World/Grid Map Data"
    )]
    public class GridMapData : ScriptableObject, IMapData
    {
        [SerializeField]
        private List<string> rows = new List<string>();

        public int Width =>
            rows != null && rows.Count > 0
                ? rows[0].Length
                : 0;

        public int Height =>
            rows != null ? rows.Count : 0;

        public TileType GetTileAt(Vector2Int position)
        {
            if (position.x < 0 ||
                position.x >= Width ||
                position.y < 0 ||
                position.y >= Height)
            {
                return TileType.Wall;
            }

            char value = rows[position.y][position.x];

            switch (value)
            {
                case '.':
                    return TileType.Grass;

                case 'T':
                    return TileType.Tree;

                case '#':
                    return TileType.Wall;

                case 'W':
                    return TileType.Water;

                default:
                    return TileType.Wall;
            }
        }

        public bool IsWalkable(Vector2Int position)
        {
            return GetTileAt(position) == TileType.Grass;
        }

        public bool Validate(out string error)
        {
            error = "";

            if (rows == null || rows.Count == 0)
            {
                error = "Map is empty.";
                return false;
            }

            int expectedWidth = rows[0].Length;

            if (expectedWidth == 0)
            {
                error = "Map contains an empty row.";
                return false;
            }

            for (int y = 0; y < rows.Count; y++)
            {
                if (rows[y] == null ||
                    rows[y].Length != expectedWidth)
                {
                    error = $"Invalid row width at row {y}.";
                    return false;
                }

                foreach (char tile in rows[y])
                {
                    if (tile != '.' &&
                        tile != 'T' &&
                        tile != '#' &&
                        tile != 'W')
                    {
                        error = $"Unknown tile '{tile}' at row {y}.";
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
