
using UnityEngine;

namespace Pokemon.World
{
    public interface IMapData
    {
        int Width { get; }
        int Height { get; }

        TileType GetTileAt(Vector2Int position);

        bool IsWalkable(Vector2Int position);
    }
}
