using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pokemon.Project.World.Generation
{
    /// <summary>Deterministic planar 2D world generator. No UnityEditor dependency.</summary>
    public static class WorldGenerator
    {
        private const float Infinity = 1e20f;

        public static WorldGenerationResult Generate(WorldGenerationSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Validate();
            int w = settings.resolution, h = settings.resolution, count = w * h;
            var height = new float[count];
            var temperature = new float[count];
            var water = new float[count];
            var waterType = new byte[count];
            var distance = new float[count];
            var moisture = new float[count];
            var vegetation = new float[count];
            var biome = new byte[count];
            var settlement = new float[count];
            var traversal = new float[count];
            var roadsMask = new float[count];

            int seed = settings.seed;
            float ox = Hash01(seed, 11) * 10000f, oz = Hash01(seed, 23) * 10000f;
            float tx = Hash01(seed, 37) * 10000f, tz = Hash01(seed, 41) * 10000f;
            float wx = Hash01(seed, 53) * 10000f, wz = Hash01(seed, 67) * 10000f;
            float vx = Hash01(seed, 71) * 10000f, vz = Hash01(seed, 89) * 10000f;
            float cellX = settings.CellSizeX, cellZ = settings.CellSizeZ;

            // 1. Base fields: multi-octave fBm built from Unity's deterministic 2D Perlin sampler.
            for (int z = 0; z < h; z++)
            for (int x = 0; x < w; x++)
            {
                int i = z * w + x;
                float worldX = x * cellX, worldZ = z * cellZ;
                float n = Fbm(worldX * settings.baseNoiseScale + ox, worldZ * settings.baseNoiseScale + oz, settings.heightOctaves, 2f, 0.5f);
                float broad = Fbm(worldX * settings.baseNoiseScale * 0.28f + ox * 0.17f, worldZ * settings.baseNoiseScale * 0.28f + oz * 0.17f, 3, 2f, 0.52f);
                height[i] = Mathf.Clamp01(0.12f + n * 0.58f + broad * 0.30f);
                float latitudinal = 1f - Mathf.Abs((z / (float)(h - 1)) * 2f - 1f) * 0.18f;
                temperature[i] = Mathf.Clamp01((0.82f - height[i] * 0.38f) * latitudinal + (Fbm(worldX * 0.0007f + tx, worldZ * 0.0007f + tz, settings.climateOctaves, 2f, 0.5f) - 0.5f) * 0.22f);
                float waterNoise = Fbm(worldX * 0.00125f + wx, worldZ * 0.00125f + wz, 4, 2f, 0.53f);
                // Oceans are low-elevation connected basins; smaller inland basins become lakes.
                float waterPotential = 0.72f - height[i] + (waterNoise - 0.5f) * 0.22f;
                bool ocean = height[i] < settings.oceanLevel;
                bool inlandBasin = height[i] < settings.oceanLevel + settings.lakeThreshold && waterPotential > 0.52f;
                waterType[i] = ocean ? (byte)WaterBodyType.Ocean : inlandBasin ? (byte)WaterBodyType.Lake : (byte)WaterBodyType.Land;
                water[i] = waterType[i] == (byte)WaterBodyType.Land ? 0f : 1f;
            }

            // 2. Derive rivers by tracing seeded highland sources downhill to an existing water cell.
            GenerateRivers(height, water, waterType, w, h, seed);

            // 3. Exact separable Euclidean distance transform to nearest water cell.
            EuclideanDistanceTransform(water, distance, w, h, cellX, cellZ);

            // 4. Moisture is derived, not an independent noise field.
            for (int i = 0; i < count; i++)
            {
                float retention = Mathf.Clamp01((0.62f - height[i]) * 1.5f);
                float evaporation = Mathf.Lerp(1.08f, 0.58f, temperature[i]);
                moisture[i] = Mathf.Clamp01((0.78f * Mathf.Exp(-distance[i] / settings.moistureDistanceScaleMeters) + 0.24f * retention + 0.04f) * evaporation);
            }

            // 5. Vegetation potential combines seeded variation with environmental suitability.
            for (int z = 0; z < h; z++)
            for (int x = 0; x < w; x++)
            {
                int i = z * w + x;
                float variation = Fbm(x * 0.035f + vx, z * 0.035f + vz, 4, 2f, 0.52f);
                float thermal = 1f - Mathf.Abs(temperature[i] - 0.58f) * 1.25f;
                float elevation = 1f - Mathf.Clamp01((height[i] - 0.68f) * 2.8f);
                vegetation[i] = Mathf.Clamp01(variation * 0.32f + moisture[i] * 0.48f + Mathf.Clamp01(thermal) * 0.14f + elevation * 0.06f);
                biome[i] = ClassifyBiome(height[i], temperature[i], moisture[i], distance[i], settings);
            }

            // 6. Settlement suitability and movement cost are independent fields.
            for (int z = 0; z < h; z++)
            for (int x = 0; x < w; x++)
            {
                int i = z * w + x;
                float slope = EstimateSlopeDegrees(height, x, z, w, h, cellX, cellZ, settings.heightScaleMeters);
                float buildable = 1f - Mathf.InverseLerp(8f, 42f, slope);
                float elevationPreference = 1f - Mathf.Abs(height[i] - 0.43f) * 1.8f;
                float waterAccess = Mathf.Exp(-distance[i] / 320f);
                float floodPenalty = distance[i] < 25f ? 0.7f : 0f;
                float localVariation = Fbm(x * 0.018f + ox * 0.3f, z * 0.018f + oz * 0.3f, 3, 2f, 0.5f);
                settlement[i] = water[i] > 0.5f || slope > settings.maxWalkableSlopeDegrees
                    ? 0f : Mathf.Clamp01(0.32f * buildable + 0.23f * Mathf.Clamp01(elevationPreference) + 0.22f * waterAccess + 0.13f * localVariation + 0.10f * (1f - floodPenalty));
                traversal[i] = water[i] > 0.5f || slope >= settings.cliffSlopeDegrees
                    ? Infinity : 1f + slope * 0.065f + Mathf.Max(0f, height[i] - 0.72f) * 5f + moisture[i] * 0.35f;
            }

            // 7. Select sparse hierarchical settlements, then route only meaningful links.
            List<SettlementSite> sites = SelectSettlements(settlement, height, w, h, settings);
            List<RoadRoute> routes = BuildRoadNetwork(sites, traversal, roadsMask, w, h, settings);

            // Convert normalized height to meters in CellToWorld consumers via heightScale.
            for (int i = 0; i < count; i++) height[i] *= settings.heightScaleMeters;
            return new WorldGenerationResult(w, h, settings.worldWidthMeters, settings.worldDepthMeters,
                height, temperature, water, waterType, distance, moisture, vegetation, biome, settlement, traversal, roadsMask, sites, routes);
        }

        private static float Fbm(float x, float y, int octaves, float lacunarity, float persistence)
        {
            float sum = 0f, amplitude = 1f, total = 0f;
            for (int o = 0; o < octaves; o++)
            {
                sum += Mathf.PerlinNoise(x, y) * amplitude;
                total += amplitude;
                x *= lacunarity; y *= lacunarity; amplitude *= persistence;
            }
            return total > 0f ? sum / total : 0f;
        }

        private static float Hash01(int seed, int salt)
        {
            unchecked
            {
                uint x = unchecked((uint)seed ^ ((uint)salt * 0x9E3779B9u));
                x ^= x >> 16; x *= 0x7FEB352Du; x ^= x >> 15; x *= 0x846CA68Bu; x ^= x >> 16;
                return (x & 0x00FFFFFFu) / 16777215f;
            }
        }

        private static byte ClassifyBiome(float elevation, float temp, float moisture, float waterDistance, WorldGenerationSettings s)
        {
            if (elevation < s.oceanLevel) return (byte)WorldBiome.Ocean;
            if (elevation < s.oceanLevel + 0.035f) return (byte)WorldBiome.Beach;
            if (waterDistance < 35f && moisture > 0.62f) return (byte)WorldBiome.Wetland;
            if (elevation > 0.78f) return (byte)WorldBiome.Mountain;
            if (elevation > 0.64f) return (byte)WorldBiome.Hills;
            if (moisture > 0.58f && temp > 0.30f) return (byte)WorldBiome.Forest;
            return (byte)WorldBiome.Meadow;
        }

        private static void GenerateRivers(float[] height, float[] water, byte[] waterType, int w, int h, int seed)
        {
            int sourceCount = Mathf.Max(6, (w * h) / 12000);
            for (int s = 0; s < sourceCount; s++)
            {
                int x = 2 + (int)(Hash01(seed, 101 + s * 7) * (w - 4));
                int z = 2 + (int)(Hash01(seed, 103 + s * 11) * (h - 4));
                int start = z * w + x;
                if (height[start] < 0.58f) continue;
                int previous = -1;
                for (int step = 0; step < w + h; step++)
                {
                    int i = z * w + x;
                    if (waterType[i] == (byte)WaterBodyType.Land) waterType[i] = (byte)WaterBodyType.River;
                    water[i] = 1f;
                    if (height[i] < 0.34f || waterType[i] == (byte)WaterBodyType.Ocean || waterType[i] == (byte)WaterBodyType.Lake) break;
                    float best = height[i]; int bx = x, bz = z;
                    for (int dz = -1; dz <= 1; dz++) for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dz == 0) continue;
                        int nx = x + dx, nz = z + dz;
                        if (nx < 1 || nz < 1 || nx >= w - 1 || nz >= h - 1) continue;
                        int ni = nz * w + nx;
                        float candidate = height[ni] + (ni == previous ? 1f : 0f);
                        if (candidate < best) { best = candidate; bx = nx; bz = nz; }
                    }
                    if (bx == x && bz == z) break;
                    previous = i; x = bx; z = bz;
                }
            }
        }

        // Felzenszwalb-Huttenlocher exact squared EDT, separable row then column.
        private static void EuclideanDistanceTransform(float[] water, float[] outputMeters, int w, int h, float cellX, float cellZ)
        {
            float[] first = new float[w * h];
            float[] input = new float[Mathf.Max(w, h)];
            float[] result = new float[input.Length];
            for (int z = 0; z < h; z++)
            {
                for (int x = 0; x < w; x++) input[x] = water[z * w + x] > 0.5f ? 0f : Infinity;
                DistanceTransform1D(input, result, w, cellX);
                for (int x = 0; x < w; x++) first[z * w + x] = result[x];
            }
            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < h; z++) input[z] = first[z * w + x];
                DistanceTransform1D(input, result, h, cellZ);
                for (int z = 0; z < h; z++) outputMeters[z * w + x] = Mathf.Sqrt(Mathf.Max(0f, result[z]));
            }
        }

        private static void DistanceTransform1D(float[] f, float[] d, int n, float spacing)
        {
            int firstFinite = -1;
            for (int i = 0; i < n; i++) if (f[i] < Infinity * 0.5f) { firstFinite = i; break; }
            if (firstFinite < 0) { for (int i = 0; i < n; i++) d[i] = Infinity; return; }
            int[] v = new int[n]; float[] z = new float[n + 1]; int k = 0;
            float spacing2 = spacing * spacing;
            v[0] = firstFinite; z[0] = -Infinity; z[1] = Infinity;
            for (int q = firstFinite + 1; q < n; q++)
            {
                if (f[q] >= Infinity * 0.5f) continue;
                float intersection;
                do
                {
                    int vk = v[k];
                    intersection = ((f[q] + q * q * spacing2) - (f[vk] + vk * vk * spacing2)) / (2f * spacing * (q - vk));
                    if (intersection <= z[k]) k--; else break;
                } while (k >= 0);
                if (k < 0) { k = 0; v[0] = q; z[0] = -Infinity; z[1] = Infinity; }
                else { k++; v[k] = q; z[k] = intersection; z[k + 1] = Infinity; }
            }
            k = 0;
            for (int q = 0; q < n; q++)
            {
                while (z[k + 1] < q * spacing) k++;
                float delta = (q - v[k]) * spacing; d[q] = delta * delta + f[v[k]];
            }
        }

        private static float EstimateSlopeDegrees(float[] height, int x, int z, int w, int h, float dx, float dz, float heightScale)
        {
            int xl = Mathf.Max(0, x - 1), xr = Mathf.Min(w - 1, x + 1);
            int zd = Mathf.Max(0, z - 1), zu = Mathf.Min(h - 1, z + 1);
            float gx = (height[z * w + xr] - height[z * w + xl]) * heightScale / Mathf.Max(dx * (xr - xl), 0.001f);
            float gz = (height[zu * w + x] - height[zd * w + x]) * heightScale / Mathf.Max(dz * (zu - zd), 0.001f);
            return Mathf.Atan(Mathf.Sqrt(gx * gx + gz * gz)) * Mathf.Rad2Deg;
        }

        private static List<SettlementSite> SelectSettlements(float[] suitability, float[] height, int w, int h, WorldGenerationSettings s)
        {
            var candidates = new List<int>(suitability.Length);
            for (int i = 0; i < suitability.Length; i++) if (suitability[i] >= s.villageThreshold) candidates.Add(i);
            candidates.Sort((a, b) => suitability[b].CompareTo(suitability[a]));
            var sites = new List<SettlementSite>();
            int cityCount = 0, townCount = 0;
            foreach (int index in candidates)
            {
                if (sites.Count >= s.maxSettlements) break;
                int x = index % w, z = index / w;
                bool separated = true;
                foreach (var site in sites)
                {
                    float dx = (site.cell.x - x) * s.CellSizeX, dz = (site.cell.y - z) * s.CellSizeZ;
                    if (dx * dx + dz * dz < s.settlementSeparationMeters * s.settlementSeparationMeters) { separated = false; break; }
                }
                if (!separated) continue;
                float score = suitability[index];
                SettlementKind kind = score >= s.cityThreshold && cityCount == 0 ? SettlementKind.City :
                    score >= s.townThreshold && townCount < 3 ? SettlementKind.Town : SettlementKind.Village;
                if (kind == SettlementKind.City) cityCount++;
                if (kind == SettlementKind.Town) townCount++;
                sites.Add(new SettlementSite { id = sites.Count, kind = kind, cell = new Vector2Int(x, z), suitability = score,
                    worldPosition = new Vector3(-s.worldWidthMeters * .5f + x * s.CellSizeX, height[index] * s.heightScaleMeters, -s.worldDepthMeters * .5f + z * s.CellSizeZ) });
            }
            return sites;
        }

        private static List<RoadRoute> BuildRoadNetwork(List<SettlementSite> sites, float[] cost, float[] mask, int w, int h, WorldGenerationSettings s)
        {
            var routes = new List<RoadRoute>();
            if (sites.Count < 2) return routes;
            // Hierarchy: villages connect to one nearest town/city; towns connect selectively to a city.
            for (int i = 0; i < sites.Count; i++)
            {
                SettlementSite source = sites[i];
                if (source.kind == SettlementKind.City) continue;
                SettlementSite target = null; float best = float.MaxValue;
                for (int j = 0; j < sites.Count; j++)
                {
                    if (i == j) continue;
                    SettlementSite candidate = sites[j];
                    bool eligible = source.kind == SettlementKind.Village
                        ? candidate.kind == SettlementKind.Town || candidate.kind == SettlementKind.City
                        : candidate.kind == SettlementKind.City;
                    if (!eligible) continue;
                    float dx = (candidate.cell.x - source.cell.x) * s.CellSizeX;
                    float dz = (candidate.cell.y - source.cell.y) * s.CellSizeZ;
                    float d = dx * dx + dz * dz;
                    if (d < best) { best = d; target = candidate; }
                }
                if (target == null) continue;
                Vector2Int[] path = FindPath(source.cell, target.cell, cost, w, h, s);
                if (path == null || path.Length < 2) continue;
                foreach (var p in path) mask[p.y * w + p.x] = 1f;
                routes.Add(new RoadRoute { fromSettlementId = source.id, toSettlementId = target.id, cells = path });
            }
            return routes;
        }

        private static Vector2Int[] FindPath(Vector2Int start, Vector2Int goal, float[] costs, int w, int h, WorldGenerationSettings s)
        {
            int count = w * h, startI = start.y * w + start.x, goalI = goal.y * w + goal.x;
            var g = new float[count]; var parent = new int[count]; var closed = new bool[count];
            for (int i = 0; i < count; i++) { g[i] = float.PositiveInfinity; parent[i] = -1; }
            var open = new List<int> { startI }; g[startI] = 0f;
            int[] dxs = { -1, 0, 1, -1, 1, -1, 0, 1 }, dzs = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int iterations = 0;
            while (open.Count > 0 && iterations++ < count * 2)
            {
                int bestIndex = 0; float bestF = float.MaxValue;
                for (int oi = 0; oi < open.Count; oi++)
                {
                    int node = open[oi]; int x = node % w, z = node / w;
                    float hx = Mathf.Abs(goal.x - x) * s.CellSizeX, hz = Mathf.Abs(goal.y - z) * s.CellSizeZ;
                    float heuristic = Mathf.Sqrt(hx * hx + hz * hz);
                    float f = g[node] + heuristic;
                    if (f < bestF) { bestF = f; bestIndex = oi; }
                }
                int current = open[bestIndex]; open.RemoveAt(bestIndex);
                if (current == goalI) return ReconstructPath(parent, current, w);
                if (closed[current]) continue; closed[current] = true;
                int cx = current % w, cz = current / w;
                for (int n = 0; n < 8; n++)
                {
                    int nx = cx + dxs[n], nz = cz + dzs[n];
                    if (nx < 0 || nz < 0 || nx >= w || nz >= h) continue;
                    int ni = nz * w + nx; if (closed[ni] || float.IsInfinity(costs[ni]) || costs[ni] >= Infinity * .5f) continue;
                    if (dxs[n] != 0 && dzs[n] != 0)
                    {
                        int sideA = cz * w + nx, sideB = nz * w + cx;
                        if (costs[sideA] >= Infinity * .5f || costs[sideB] >= Infinity * .5f) continue;
                    }
                    float physicalStep = Mathf.Sqrt(dxs[n] * dxs[n] * s.CellSizeX * s.CellSizeX + dzs[n] * dzs[n] * s.CellSizeZ * s.CellSizeZ);
                    float step = physicalStep * (costs[current] + costs[ni]) * 0.5f;
                    float tentative = g[current] + step;
                    if (tentative >= g[ni]) continue;
                    parent[ni] = current; g[ni] = tentative;
                    if (!open.Contains(ni)) open.Add(ni);
                }
            }
            return null;
        }

        private static Vector2Int[] ReconstructPath(int[] parent, int current, int w)
        {
            var path = new List<Vector2Int>();
            while (current >= 0) { path.Add(new Vector2Int(current % w, current / w)); current = parent[current]; }
            path.Reverse(); return path.ToArray();
        }
    }
}
