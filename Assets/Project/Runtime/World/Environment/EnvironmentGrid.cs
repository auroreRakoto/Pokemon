using System;
using Pokemon.Project.Data;

namespace Pokemon.Project.World.Environment
{
    /// <summary>Immutable view over one continuous environment field.</summary>
    public sealed class EnvironmentGrid
    {
        public MapLayerManifestData Manifest { get; }
        public int Width => Manifest.width;
        public int Height => Manifest.height;
        private readonly float[] values;

        public EnvironmentGrid(MapLayerManifestData manifest, float[] values)
        {
            Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            if (values == null || values.Length != manifest.width * manifest.height)
                throw new ArgumentException("Grid data length does not match manifest.", nameof(values));
            this.values = values;
        }

        public float GetValue(int x, int y)
        {
            if (x < 0 || x >= Width) throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y));
            return values[y * Width + x];
        }

        /// <summary>Sample a continuous field in world X/Z coordinates using bilinear interpolation.</summary>
        public bool TrySampleBilinear(float worldX, float worldZ, out float value)
        {
            value = 0f;
            float u = (worldX - Manifest.origin.x) / Manifest.worldSize.x;
            float v = (worldZ - Manifest.origin.z) / Manifest.worldSize.z;
            if (u < 0f || u > 1f || v < 0f || v > 1f) return false;

            float px = u * (Width - 1);
            float py = v * (Height - 1); // rowOrder=south_to_north
            int x0 = (int)Math.Floor(px);
            int y0 = (int)Math.Floor(py);
            int x1 = Math.Min(x0 + 1, Width - 1);
            int y1 = Math.Min(y0 + 1, Height - 1);
            float tx = px - x0;
            float ty = py - y0;

            float a = Lerp(GetValue(x0, y0), GetValue(x1, y0), tx);
            float b = Lerp(GetValue(x0, y1), GetValue(x1, y1), tx);
            value = Lerp(a, b, ty);
            return true;
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
