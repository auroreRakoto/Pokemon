
using System;

namespace Pokemon.World
{
    [Serializable]
    public class MapJsonData
    {
        public int width;
        public int height;

        public MapJsonSpawnPoints spawnPoints;
    }

    [Serializable]
    public class MapJsonSpawnPoints
    {
        public MapJsonPoint start;
        public MapJsonPoint end;
    }

    [Serializable]
    public class MapJsonPoint
    {
        public int x;
        public int y;
    }
}
