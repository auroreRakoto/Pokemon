using System;
using UnityEngine;
using Pokemon.Project.World.Spherical.Studio;

namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>
    /// Deterministic spherical geographic pipeline. Ocean coverage is selected from the
    /// actual Cube-Sphere surface-area distribution; moisture uses graph-geodesic distance
    /// to the ocean; terrestrial biomes are classified from shared climate/terrain fields.
    /// </summary>
    public static class SphericalGeographicFieldGenerator
    {
        public static SphericalGeographicFieldSet Generate(SphericalPlanetStudioSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Validate();

            int resolution = settings.faceResolution;
            int side = resolution + 1;
            int faceSampleCount = side * side;
            var altitude = CreateFloatFaces(faceSampleCount);
            var temperature = CreateFloatFaces(faceSampleCount);
            var slope = CreateFloatFaces(faceSampleCount);
            var distanceToWater = CreateFloatFaces(faceSampleCount);
            var moisture = CreateFloatFaces(faceSampleCount);
            var water = CreateWaterFaces(faceSampleCount);
            var biomes = CreateBiomeFaces(faceSampleCount);

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                CubeSphereFace face = (CubeSphereFace)faceIndex;
                for (int y = 0; y <= resolution; y++)
                for (int x = 0; x <= resolution; x++)
                {
                    int index = y * side + x;
                    Vector3 direction = DirectionAt(face, x, y, resolution);
                    float height = SphericalPlanetStudioGenerator.SamplePreviewHeight(
                        direction, settings, SphericalPlanetStudioPreviewStage.Final);
                    altitude[faceIndex][index] = height;
                    temperature[faceIndex][index] = SampleTemperature(direction, height, settings);
                    slope[faceIndex][index] = SampleSlope(direction, settings, resolution);
                }
            }

            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(resolution, settings.radiusMeters);
            int nodeCount = graph.NodeCount;
            var nodeAltitude = new float[nodeCount];
            var nodeTemperature = new float[nodeCount];
            var nodeSlope = new float[nodeCount];
            var nodeAreaWeights = new double[nodeCount];
            var hasNodeValue = new bool[nodeCount];
            PopulateCanonicalContinuousFields(graph, altitude, temperature, slope,
                nodeAltitude, nodeTemperature, nodeSlope, hasNodeValue);
            AccumulateSphericalNodeAreaWeights(graph, nodeAreaWeights);

            float seaLevel = DeriveSeaLevel(nodeAltitude, nodeAreaWeights,
                settings.oceanCoverageTargetPercent, out bool[] oceanMask, out double actualOceanArea,
                out double totalArea, out float coverageTolerancePercent);
            float actualOceanPercent = (float)(actualOceanArea / totalArea * 100.0);
            float[] distanceToOcean = CalculateDistanceToSources(graph, oceanMask);
            var nodeMoisture = new float[nodeCount];
            var nodeBiome = new SphericalBiome[nodeCount];

            for (int node = 0; node < nodeCount; node++)
            {
                float temperatureValue = nodeTemperature[node];
                float slopeValue = nodeSlope[node];
                float distance = distanceToOcean[node];
                float moistureValue = CalculateMoisture(distance, temperatureValue, slopeValue, settings);
                nodeMoisture[node] = moistureValue;

                if (oceanMask[node])
                {
                    nodeBiome[node] = SphericalBiome.None;
                    continue;
                }

                nodeBiome[node] = ClassifyBiome(nodeAltitude[node], temperatureValue,
                    moistureValue, slopeValue, settings);
            }

            // Copy canonical node values back to each face lattice. Shared seam/corner
            // samples therefore receive identical categorical and continuous values.
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                CubeSphereFace face = (CubeSphereFace)faceIndex;
                for (int y = 0; y <= resolution; y++)
                for (int x = 0; x <= resolution; x++)
                {
                    int index = y * side + x;
                    int node = graph.GetNodeId(face, x, y);
                    distanceToWater[faceIndex][index] = distanceToOcean[node];
                    moisture[faceIndex][index] = nodeMoisture[node];
                    water[faceIndex][index] = oceanMask[node] ? SphericalWaterClass.Ocean : SphericalWaterClass.Land;
                    biomes[faceIndex][index] = nodeBiome[node];
                }
            }

            return new SphericalGeographicFieldSet(resolution, settings.seed, settings.radiusMeters,
                altitude, temperature, slope, distanceToWater, moisture, water, biomes,
                settings.oceanCoverageTargetPercent, actualOceanPercent, coverageTolerancePercent, seaLevel);
        }

        private static float[][] CreateFloatFaces(int count)
        {
            var result = new float[6][];
            for (int i = 0; i < result.Length; i++) result[i] = new float[count];
            return result;
        }

        private static SphericalWaterClass[][] CreateWaterFaces(int count)
        {
            var result = new SphericalWaterClass[6][];
            for (int i = 0; i < result.Length; i++) result[i] = new SphericalWaterClass[count];
            return result;
        }

        private static SphericalBiome[][] CreateBiomeFaces(int count)
        {
            var result = new SphericalBiome[6][];
            for (int i = 0; i < result.Length; i++) result[i] = new SphericalBiome[count];
            return result;
        }

        private static Vector3 DirectionAt(CubeSphereFace face, int x, int y, int resolution)
        {
            return CubeSphereTopology.GetDirection(face,
                -1f + 2f * x / resolution, -1f + 2f * y / resolution);
        }

        private static void PopulateCanonicalContinuousFields(SphericalNeighbourGraph graph,
            float[][] altitude, float[][] temperature, float[][] slope,
            float[] nodeAltitude, float[] nodeTemperature, float[] nodeSlope, bool[] assigned)
        {
            int resolution = graph.Resolution;
            int side = resolution + 1;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            for (int y = 0; y <= resolution; y++)
            for (int x = 0; x <= resolution; x++)
            {
                int node = graph.GetNodeId((CubeSphereFace)faceIndex, x, y);
                int index = y * side + x;
                if (assigned[node]) continue;
                assigned[node] = true;
                nodeAltitude[node] = altitude[faceIndex][index];
                nodeTemperature[node] = temperature[faceIndex][index];
                nodeSlope[node] = slope[faceIndex][index];
            }
        }

        private static void AccumulateSphericalNodeAreaWeights(SphericalNeighbourGraph graph, double[] weights)
        {
            int resolution = graph.Resolution;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                CubeSphereFace face = (CubeSphereFace)faceIndex;
                for (int y = 0; y < resolution; y++)
                for (int x = 0; x < resolution; x++)
                {
                    Vector3 a = DirectionAt(face, x, y, resolution);
                    Vector3 b = DirectionAt(face, x + 1, y, resolution);
                    Vector3 c = DirectionAt(face, x, y + 1, resolution);
                    Vector3 d = DirectionAt(face, x + 1, y + 1, resolution);
                    double area = SphericalTriangleArea(a, b, c) + SphericalTriangleArea(b, d, c);
                    double quarter = area * 0.25;
                    weights[graph.GetNodeId(face, x, y)] += quarter;
                    weights[graph.GetNodeId(face, x + 1, y)] += quarter;
                    weights[graph.GetNodeId(face, x, y + 1)] += quarter;
                    weights[graph.GetNodeId(face, x + 1, y + 1)] += quarter;
                }
            }
        }

        private static double SphericalTriangleArea(Vector3 a, Vector3 b, Vector3 c)
        {
            double determinant = Math.Abs(Vector3.Dot(a, Vector3.Cross(b, c)));
            double denominator = 1.0 + Vector3.Dot(a, b) + Vector3.Dot(b, c) + Vector3.Dot(c, a);
            return 2.0 * Math.Atan2(determinant, denominator);
        }

        private static float DeriveSeaLevel(float[] elevations, double[] areaWeights,
            float targetPercent, out bool[] oceanMask, out double oceanArea, out double totalArea,
            out float quantizationTolerancePercent)
        {
            var samples = new WeightedElevation[elevations.Length];
            totalArea = 0.0;
            float minimumElevation = float.PositiveInfinity;
            float maximumElevation = float.NegativeInfinity;
            for (int i = 0; i < elevations.Length; i++)
            {
                samples[i] = new WeightedElevation(elevations[i], i, areaWeights[i]);
                totalArea += areaWeights[i];
                minimumElevation = Math.Min(minimumElevation, elevations[i]);
                maximumElevation = Math.Max(maximumElevation, elevations[i]);
            }
            if (maximumElevation - minimumElevation < 0.001f)
                throw new InvalidOperationException("Ocean coverage requires a non-flat generated terrain heightfield. Enable at least the macro-relief layer before generating Ocean or Biomes maps.");
            if (totalArea <= 0.0) throw new InvalidOperationException("Spherical cell-area weights are empty.");
            Array.Sort(samples, WeightedElevationComparer.Instance);

            double targetArea = totalArea * (targetPercent / 100.0);
            double accumulated = 0.0;
            float seaLevel = samples[0].Elevation;
            double smallestError = double.PositiveInfinity;
            double maximumElevationClassWeight = 0.0;
            int cursor = 0;
            while (cursor < samples.Length)
            {
                float groupElevation = samples[cursor].Elevation;
                double groupWeight = 0.0;
                while (cursor < samples.Length && samples[cursor].Elevation.Equals(groupElevation))
                {
                    groupWeight += samples[cursor].AreaWeight;
                    cursor++;
                }
                accumulated += groupWeight;
                maximumElevationClassWeight = Math.Max(maximumElevationClassWeight, groupWeight);
                double error = Math.Abs(accumulated - targetArea);
                if (error < smallestError)
                {
                    smallestError = error;
                    seaLevel = groupElevation;
                }
            }

            oceanMask = new bool[elevations.Length];
            oceanArea = 0.0;
            for (int i = 0; i < elevations.Length; i++)
            {
                oceanMask[i] = elevations[i] <= seaLevel;
                if (oceanMask[i]) oceanArea += areaWeights[i];
            }
            // The closest representable elevation class is selected. Its full area is
            // the conservative quantization bound for this sampled spherical surface.
            quantizationTolerancePercent = (float)(maximumElevationClassWeight / (2.0 * totalArea) * 100.0);
            return seaLevel;
        }

        private static float[] CalculateDistanceToSources(SphericalNeighbourGraph graph, bool[] sourceMask)
        {
            int count = graph.NodeCount;
            var distances = new float[count];
            var heap = new IndexedMinHeap(count, distances);
            for (int node = 0; node < count; node++)
            {
                distances[node] = sourceMask[node] ? 0f : float.PositiveInfinity;
                if (sourceMask[node]) heap.Insert(node);
            }

            while (heap.Count > 0)
            {
                int current = heap.PopMinimum();
                float currentDistance = distances[current];
                int neighbourCount = graph.GetNeighbourCount(current);
                for (int edge = 0; edge < neighbourCount; edge++)
                {
                    int neighbour = graph.GetNeighbour(current, edge);
                    if (heap.IsSettled(neighbour)) continue;
                    float candidate = currentDistance + graph.GetEdgeDistanceMeters(current, edge);
                    if (candidate + 1e-4f >= distances[neighbour]) continue;
                    distances[neighbour] = candidate;
                    if (heap.Contains(neighbour)) heap.DecreaseKey(neighbour);
                    else heap.Insert(neighbour);
                }
            }
            return distances;
        }

        private static float CalculateMoisture(float distanceMeters, float temperature, float slope,
            SphericalPlanetStudioSettings settings)
        {
            float distanceScale = Mathf.Max(1f, settings.radiusMeters * settings.moistureDistanceScaleRadiusMultiplier);
            float proximity = Mathf.Exp(-distanceMeters / distanceScale);
            float evaporation = 1f - temperature * settings.moistureTemperatureEvaporation;
            float runoff = 1f - Mathf.Clamp01(slope / 90f) * settings.moistureSlopeLoss;
            return Mathf.Clamp01(proximity * evaporation * runoff);
        }

        private static SphericalBiome ClassifyBiome(float altitude, float temperature,
            float moisture, float slope, SphericalPlanetStudioSettings settings)
        {
            if (altitude >= settings.mountainElevationThresholdMeters || slope >= settings.mountainSlopeThresholdDegrees)
                return SphericalBiome.Mountains;
            if (moisture <= settings.desertMoistureThreshold && temperature >= settings.desertMinimumTemperature)
                return SphericalBiome.Desert;
            if (moisture >= settings.forestMoistureThreshold &&
                temperature >= settings.forestMinimumTemperature && temperature <= settings.forestMaximumTemperature)
                return SphericalBiome.Forest;
            return SphericalBiome.Plains;
        }

        private static float SampleTemperature(Vector3 direction, float altitudeMeters, SphericalPlanetStudioSettings settings)
        {
            Vector3 d = direction.normalized;
            float latitudeFactor = 1f - Mathf.Abs(d.y);
            float noise = SphericalPlanetStudioGenerator.SampleClimateNoise(d, settings.seed + 104729);
            float normalized = latitudeFactor * settings.temperatureLatitudeInfluence +
                               noise * settings.temperatureNoiseInfluence;
            float altitudeCooling = Mathf.Max(0f, altitudeMeters) / 8000f;
            return Mathf.Clamp01(normalized - altitudeCooling);
        }

        private static float SampleSlope(Vector3 direction, SphericalPlanetStudioSettings settings, int resolution)
        {
            Vector3 normal = direction.normalized;
            Vector3 reference = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
            Vector3 tangentA = Vector3.Cross(reference, normal).normalized;
            Vector3 tangentB = Vector3.Cross(normal, tangentA).normalized;
            float angularStep = Mathf.Clamp(2f / resolution, 0.0005f, 0.1f);
            float distanceMeters = Mathf.Max(0.01f, settings.radiusMeters * angularStep);
            float aPlus = SampleHeightAtOffset(normal, tangentA, angularStep, settings);
            float aMinus = SampleHeightAtOffset(normal, -tangentA, angularStep, settings);
            float bPlus = SampleHeightAtOffset(normal, tangentB, angularStep, settings);
            float bMinus = SampleHeightAtOffset(normal, -tangentB, angularStep, settings);
            float gradientA = (aPlus - aMinus) / (2f * distanceMeters);
            float gradientB = (bPlus - bMinus) / (2f * distanceMeters);
            return Mathf.Atan(Mathf.Sqrt(gradientA * gradientA + gradientB * gradientB)) * Mathf.Rad2Deg;
        }

        private static float SampleHeightAtOffset(Vector3 normal, Vector3 tangent, float angularStep,
            SphericalPlanetStudioSettings settings)
        {
            Vector3 direction = (normal * Mathf.Cos(angularStep) + tangent * Mathf.Sin(angularStep)).normalized;
            return SphericalPlanetStudioGenerator.SamplePreviewHeight(
                direction, settings, SphericalPlanetStudioPreviewStage.Final);
        }

        private struct WeightedElevation
        {
            public readonly float Elevation;
            public readonly int NodeId;
            public readonly double AreaWeight;
            public WeightedElevation(float elevation, int nodeId, double areaWeight)
            { Elevation = elevation; NodeId = nodeId; AreaWeight = areaWeight; }
        }

        private sealed class WeightedElevationComparer : System.Collections.Generic.IComparer<WeightedElevation>
        {
            public static readonly WeightedElevationComparer Instance = new WeightedElevationComparer();
            public int Compare(WeightedElevation a, WeightedElevation b)
            {
                int compare = a.Elevation.CompareTo(b.Elevation);
                return compare != 0 ? compare : a.NodeId.CompareTo(b.NodeId);
            }
        }

        /// <summary>Binary heap with one live entry per graph node; bounded O(node count) memory.</summary>
        private sealed class IndexedMinHeap
        {
            private readonly int[] _heap;
            private readonly int[] _positions;
            private readonly float[] _keys;
            public int Count { get; private set; }
            public IndexedMinHeap(int capacity, float[] keys)
            {
                _heap = new int[capacity];
                _positions = new int[capacity];
                _keys = keys;
                for (int i = 0; i < _positions.Length; i++) _positions[i] = -1;
            }
            public bool Contains(int node) => _positions[node] >= 0;
            public bool IsSettled(int node) => _positions[node] == -2;
            public void Insert(int node)
            {
                if (_positions[node] >= 0) return;
                int index = Count++;
                _heap[index] = node;
                _positions[node] = index;
                SiftUp(index);
            }
            public void DecreaseKey(int node) => SiftUp(_positions[node]);
            public int PopMinimum()
            {
                int result = _heap[0];
                _positions[result] = -2;
                Count--;
                if (Count > 0)
                {
                    int last = _heap[Count];
                    _heap[0] = last;
                    _positions[last] = 0;
                    SiftDown(0);
                }
                return result;
            }
            private void SiftUp(int index)
            {
                int node = _heap[index];
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (Compare(_heap[parent], node) <= 0) break;
                    _heap[index] = _heap[parent];
                    _positions[_heap[index]] = index;
                    index = parent;
                }
                _heap[index] = node;
                _positions[node] = index;
            }
            private void SiftDown(int index)
            {
                int node = _heap[index];
                while (true)
                {
                    int left = index * 2 + 1;
                    if (left >= Count) break;
                    int right = left + 1;
                    int child = right < Count && Compare(_heap[right], _heap[left]) < 0 ? right : left;
                    if (Compare(node, _heap[child]) <= 0) break;
                    _heap[index] = _heap[child];
                    _positions[_heap[index]] = index;
                    index = child;
                }
                _heap[index] = node;
                _positions[node] = index;
            }
            private int Compare(int a, int b)
            {
                int c = _keys[a].CompareTo(_keys[b]);
                return c != 0 ? c : a.CompareTo(b);
            }
        }
    }
}
