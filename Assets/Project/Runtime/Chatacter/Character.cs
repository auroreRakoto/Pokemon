
using System;
using UnityEngine;
using Pokemon.World;

namespace Pokemon.CharacterSystem
{
    public class Character
    {
        public Vector2Int Position { get; private set; }

        public Character(Vector2Int startPosition, IMapData map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (!map.IsWalkable(startPosition))
            {
                throw new ArgumentException(
                    "Starting position is not walkable."
                );
            }

            Position = startPosition;
        }

        public bool TryMove(Vector2Int direction, IMapData map)
        {
            if (map == null)
                return false;

            int distance =
                Math.Abs(direction.x) +
                Math.Abs(direction.y);

            // One tile at a time, no diagonal movement.
            if (distance != 1)
                return false;

            Vector2Int targetPosition = Position + direction;

            if (!map.IsWalkable(targetPosition))
                return false;

            Position = targetPosition;

            return true;
        }
    }
}
