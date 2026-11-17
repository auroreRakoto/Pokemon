using System;
using UnityEngine;

namespace Pokemon.Project.World.Generation
{
    [Serializable]
    public sealed class WorldGenerationSettings
    {
        public int seed = 481516;
        [Min(2)] public int resolution = 256;
        [Min(1f)] public float worldWidthMeters = 2000f;
        [Min(1f)] public float worldDepthMeters = 2000f;
        [Min(0f)] public float heightScaleMeters = 180f;
        [Min(0.0001f)] public float baseNoiseScale = 0.0018f;
        [Range(1, 8)] public int heightOctaves = 5;
        [Range(1, 8)] public int climateOctaves = 4;
        [Range(0.1f, 0.9f)] public float oceanLevel = 0.30f;
        [Range(0.01f, 0.5f)] public float lakeThreshold = 0.12f;
        [Min(1f)] public float moistureDistanceScaleMeters = 450f;
        [Min(0f)] public float minimumRoadSeparationMeters = 120f;
        [Min(0f)] public float settlementSeparationMeters = 220f;
        [Range(0f, 1f)] public float villageThreshold = 0.48f;
        [Range(0f, 1f)] public float townThreshold = 0.63f;
        [Range(0f, 1f)] public float cityThreshold = 0.78f;
        [Range(1, 8)] public int maxSettlements = 18;
        [Min(0f)] public float maxWalkableSlopeDegrees = 38f;
        [Min(0f)] public float cliffSlopeDegrees = 58f;

        public float CellSizeX => worldWidthMeters / (resolution - 1);
        public float CellSizeZ => worldDepthMeters / (resolution - 1);

        public void Validate()
        {
            if (resolution < 16) throw new ArgumentOutOfRangeException(nameof(resolution), "Resolution must be at least 16.");
            if (worldWidthMeters <= 0f || worldDepthMeters <= 0f) throw new ArgumentOutOfRangeException("World dimensions must be positive.");
            if (heightScaleMeters < 0f) throw new ArgumentOutOfRangeException(nameof(heightScaleMeters));
            if (villageThreshold > townThreshold || townThreshold > cityThreshold)
                throw new ArgumentException("Settlement thresholds must satisfy village <= town <= city.");
        }
    }

    public enum WorldBiome : byte { Ocean = 0, Beach = 1, Meadow = 2, Forest = 3, Wetland = 4, Hills = 5, Mountain = 6 }
    public enum WaterBodyType : byte { Land = 0, Ocean = 1, Lake = 2, River = 3 }
    public enum SettlementKind : byte { Village = 0, Town = 1, City = 2 }

    [Serializable]
    public sealed class SettlementSite
    {
        public int id;
        public SettlementKind kind;
        public Vector2Int cell;
        public Vector3 worldPosition;
        public float suitability;
    }

    [Serializable]
    public sealed class RoadRoute
    {
        public int fromSettlementId;
        public int toSettlementId;
        public Vector2Int[] cells;
    }
}
