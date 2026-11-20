
using System;
using System.Collections.Generic;

namespace Pokemon.World
{
    // A single numerical layer of the map.
    public class MapLayerData
    {
        public string Id { get; }
        public float[] Values { get; }

        public MapLayerData(string id, float[] values)
        {
            Id = id;
            Values = values;
        }

        public float GetValue(int x, int y, int width)
        {
            if (x < 0 || y < 0 || x >= width)
                throw new ArgumentOutOfRangeException();

            int index = y * width + x;

            if (index >= Values.Length)
                throw new ArgumentOutOfRangeException();

            return Values[index];
        }
    }

    // A point on the map grid.
    public class MapPoint
    {
        public int X { get; }
        public int Y { get; }

        public MapPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    // Map starting and ending positions.
    public class MapSpawnPoints
    {
        public MapPoint Start { get; }
        public MapPoint End { get; }

        public MapSpawnPoints(MapPoint start, MapPoint end)
        {
            Start = start;
            End = end;
        }
    }

    // The constant output structure of the Map module.
    public class MapData
    {
        public int Width { get; }
        public int Height { get; }

        public IReadOnlyList<MapLayerData> Layers { get; }
        public MapSpawnPoints SpawnPoints { get; }

        public MapData(
            int width,
            int height,
            List<MapLayerData> layers,
            MapSpawnPoints spawnPoints)
        {
            Width = width;
            Height = height;
            Layers = layers == null ? null : layers.AsReadOnly();
            SpawnPoints = spawnPoints;
        }

        public MapLayerData GetLayer(string id)
        {
            if (Layers == null)
                return null;

            foreach (MapLayerData layer in Layers)
            {
                if (layer != null && layer.Id == id)
                    return layer;
            }

            return null;
        }

        public bool Validate()
        {
            if (Width <= 0 || Height <= 0)
                return false;

            if (Layers == null || Layers.Count == 0)
                return false;

            if (SpawnPoints == null ||
                SpawnPoints.Start == null ||
                SpawnPoints.End == null)
                return false;

            if (!IsInsideMap(SpawnPoints.Start) ||
                !IsInsideMap(SpawnPoints.End))
                return false;

            foreach (MapLayerData layer in Layers)
            {
                if (layer == null ||
                    string.IsNullOrWhiteSpace(layer.Id) ||
                    layer.Values == null ||
                    layer.Values.Length != Width * Height)
                    return false;
            }

            return true;
        }

        private bool IsInsideMap(MapPoint point)
        {
            return point.X >= 0 &&
                   point.X < Width &&
                   point.Y >= 0 &&
                   point.Y < Height;
        }
    }
}
