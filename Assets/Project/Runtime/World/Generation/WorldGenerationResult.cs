using System.Collections.Generic;
using UnityEngine;

namespace Pokemon.Project.World.Generation
{
    public sealed class WorldGenerationResult
    {
        public readonly int Width;
        public readonly int Height;
        public readonly float WorldWidthMeters;
        public readonly float WorldDepthMeters;
        public readonly float[] HeightMeters;
        public readonly float[] Temperature01;
        public readonly float[] WaterMask;
        public readonly byte[] WaterType;
        public readonly float[] DistanceToWaterMeters;
        public readonly float[] Moisture01;
        public readonly float[] Vegetation01;
        public readonly byte[] Biome;
        public readonly float[] SettlementSuitability;
        public readonly float[] TraversalCost;
        public readonly float[] RoadMask;
        public readonly List<SettlementSite> Settlements;
        public readonly List<RoadRoute> Roads;

        public WorldGenerationResult(int width, int height, float worldWidth, float worldDepth,
            float[] heightMeters, float[] temperature01, float[] waterMask, byte[] waterType, float[] distanceToWater,
            float[] moisture01, float[] vegetation01, byte[] biome, float[] settlementSuitability,
            float[] traversalCost, float[] roadMask, List<SettlementSite> settlements, List<RoadRoute> roads)
        {
            Width = width; Height = height; WorldWidthMeters = worldWidth; WorldDepthMeters = worldDepth;
            HeightMeters = heightMeters; Temperature01 = temperature01; WaterMask = waterMask; WaterType = waterType;
            DistanceToWaterMeters = distanceToWater; Moisture01 = moisture01; Vegetation01 = vegetation01;
            Biome = biome; SettlementSuitability = settlementSuitability; TraversalCost = traversalCost;
            RoadMask = roadMask; Settlements = settlements; Roads = roads;
        }

        public int Index(int x, int z) => z * Width + x;
        public Vector3 CellToWorld(int x, int z)
        {
            float px = -WorldWidthMeters * 0.5f + x * (WorldWidthMeters / (Width - 1));
            float pz = -WorldDepthMeters * 0.5f + z * (WorldDepthMeters / (Height - 1));
            return new Vector3(px, HeightMeters[Index(x, z)], pz);
        }
    }
}
