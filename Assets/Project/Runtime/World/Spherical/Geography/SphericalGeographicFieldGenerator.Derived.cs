using System;
using UnityEngine;
using Pokemon.Project.World.Spherical.Studio;

namespace Pokemon.Project.World.Spherical.Geography
{
    public static partial class SphericalGeographicFieldGenerator
    {
        /// <summary>
        /// Derives water, distance and moisture fields, then classifies biomes only when requested.
        /// </summary>
        private static DerivedFields GenerateDerivedFields(
            SphericalPlanetStudioSettings settings, BaseFields baseFields, bool includeBiomeClassification)
        {
            NodeFields nodes = CreateNodeFields(settings, baseFields);
            OceanFields ocean = CreateOceanFields(settings, nodes);
            float[] nodeMoisture = CalculateNodeMoisture(settings, nodes, ocean);
            SphericalBiome[] nodeBiomes = includeBiomeClassification
                ? ClassifyNodes(settings, nodes, ocean, nodeMoisture) : null;
            return PopulateDerivedFaces(settings, nodes, ocean, nodeMoisture, nodeBiomes);
        }

        /// <summary>
        /// Derives the area-weighted sea level, ocean mask and graph distance to ocean sources.
        /// </summary>
        private static OceanFields CreateOceanFields(SphericalPlanetStudioSettings settings, NodeFields nodes)
        {
            float seaLevel = DeriveSeaLevel(nodes.altitude, nodes.areaWeights,
                settings.oceanCoverageTargetPercent, out bool[] mask, out double oceanArea,
                out double totalArea, out float tolerance);
            float coverage = (float)(oceanArea / totalArea * 100.0);
            float[] distance = CalculateDistanceToSources(nodes.graph, mask);
            return new OceanFields(mask, distance, seaLevel, coverage, tolerance);
        }

        /// <summary>
        /// Selects the closest representable area-weighted sea level and reports its quantization bound.
        /// </summary>
        private static float DeriveSeaLevel(float[] elevations, double[] areaWeights,
            float targetPercent, out bool[] oceanMask, out double oceanArea, out double totalArea,
            out float quantizationTolerancePercent)
        {
            SphericalWeightedElevation[] samples = new SphericalWeightedElevation[elevations.Length];
            totalArea = 0.0;
            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int i = 0; i < elevations.Length; i++)
            {
                samples[i] = new SphericalWeightedElevation(elevations[i], i, areaWeights[i]);
                totalArea += areaWeights[i];
                minimum = Math.Min(minimum, elevations[i]);
                maximum = Math.Max(maximum, elevations[i]);
            }
            if (maximum - minimum < 0.001f)
                throw new InvalidOperationException("Ocean coverage requires a non-flat generated terrain heightfield. Enable at least the macro-relief layer before generating Ocean or Biomes maps.");
            if (totalArea <= 0.0) throw new InvalidOperationException("Spherical cell-area weights are empty.");
            Array.Sort(samples);
            float seaLevel = FindClosestSeaLevel(samples, totalArea * targetPercent / 100.0,
                out double maximumElevationClassWeight);
            oceanMask = new bool[elevations.Length];
            oceanArea = 0.0;
            for (int i = 0; i < elevations.Length; i++)
            {
                oceanMask[i] = elevations[i] <= seaLevel;
                if (oceanMask[i]) oceanArea += areaWeights[i];
            }
            quantizationTolerancePercent = (float)(maximumElevationClassWeight / (2.0 * totalArea) * 100.0);
            return seaLevel;
        }

        /// <summary>
        /// Finds the elevation class whose cumulative spherical area is closest to the target ocean area.
        /// </summary>
        private static float FindClosestSeaLevel(SphericalWeightedElevation[] samples,
            double targetArea, out double maximumElevationClassWeight)
        {
            double accumulated = 0.0;
            double smallestError = double.PositiveInfinity;
            maximumElevationClassWeight = 0.0;
            float seaLevel = samples[0].elevation;
            int cursor = 0;
            while (cursor < samples.Length)
            {
                float elevation = samples[cursor].elevation;
                double groupWeight = 0.0;
                while (cursor < samples.Length && samples[cursor].elevation.Equals(elevation))
                    groupWeight += samples[cursor++].areaWeight;
                accumulated += groupWeight;
                maximumElevationClassWeight = Math.Max(maximumElevationClassWeight, groupWeight);
                double error = Math.Abs(accumulated - targetArea);
                if (error < smallestError) { smallestError = error; seaLevel = elevation; }
            }
            return seaLevel;
        }

        /// <summary>
        /// Computes deterministic graph-geodesic distance from every node to the ocean mask.
        /// </summary>
        private static float[] CalculateDistanceToSources(SphericalNeighbourGraph graph, bool[] sourceMask)
        {
            int count = graph.NodeCount;
            float[] distances = new float[count];
            SphericalIndexedMinHeap heap = new SphericalIndexedMinHeap(count, distances);
            for (int node = 0; node < count; node++)
            {
                distances[node] = sourceMask[node] ? 0f : float.PositiveInfinity;
                if (sourceMask[node]) heap.Insert(node);
            }
            while (heap.Count > 0)
            {
                int current = heap.PopMinimum();
                int edgeCount = graph.GetNeighbourCount(current);
                for (int edge = 0; edge < edgeCount; edge++)
                    RelaxDistance(graph, heap, distances, current, edge);
            }
            return distances;
        }

        /// <summary>
        /// Relaxes one graph edge during the multi-source shortest-path calculation.
        /// </summary>
        private static void RelaxDistance(SphericalNeighbourGraph graph, SphericalIndexedMinHeap heap,
            float[] distances, int current, int edge)
        {
            int neighbour = graph.GetNeighbour(current, edge);
            if (heap.IsSettled(neighbour)) return;
            float candidate = distances[current] + graph.GetEdgeDistanceMeters(current, edge);
            if (candidate + 1e-4f >= distances[neighbour]) return;
            distances[neighbour] = candidate;
            if (heap.Contains(neighbour)) heap.DecreaseKey(neighbour);
            else heap.Insert(neighbour);
        }

        /// <summary>
        /// Derives moisture for every canonical node from ocean distance, temperature and slope.
        /// </summary>
        private static float[] CalculateNodeMoisture(
            SphericalPlanetStudioSettings settings, NodeFields nodes, OceanFields ocean)
        {
            float[] moisture = new float[nodes.graph.NodeCount];
            for (int node = 0; node < moisture.Length; node++)
                moisture[node] = CalculateMoisture(ocean.distanceToOcean[node],
                    nodes.temperature[node], nodes.slope[node], settings);
            return moisture;
        }

        /// <summary>
        /// Applies the existing moisture, evaporation and runoff formula without biome dependencies.
        /// </summary>
        private static float CalculateMoisture(float distanceMeters, float temperature, float slope,
            SphericalPlanetStudioSettings settings)
        {
            float distanceScale = Mathf.Max(1f, settings.radiusMeters * settings.moistureDistanceScaleRadiusMultiplier);
            float proximity = Mathf.Exp(-distanceMeters / distanceScale);
            float evaporation = 1f - temperature * settings.moistureTemperatureEvaporation;
            float runoff = 1f - Mathf.Clamp01(slope / 90f) * settings.moistureSlopeLoss;
            return Mathf.Clamp01(proximity * evaporation * runoff);
        }


        /// <summary>
        /// Applies canonical water, distance, moisture and optional biome values to face lattices.
        /// </summary>
        private static DerivedFields PopulateDerivedFaces(
            SphericalPlanetStudioSettings settings, NodeFields nodes, OceanFields ocean,
            float[] nodeMoisture, SphericalBiome[] nodeBiomes)
        {
            int side = settings.faceResolution + 1;
            int count = side * side;
            float[][] distance = CreateFloatFaces(count);
            float[][] moisture = CreateFloatFaces(count);
            SphericalWaterClass[][] water = CreateWaterFaces(count);
            SphericalBiome[][] biomes = nodeBiomes == null ? null : CreateBiomeFaces(count);
            for (int face = 0; face < 6; face++)
            for (int y = 0; y <= settings.faceResolution; y++)
            for (int x = 0; x <= settings.faceResolution; x++)
            {
                int index = y * side + x;
                int node = nodes.graph.GetNodeId((CubeSphereFace)face, x, y);
                distance[face][index] = ocean.distanceToOcean[node];
                moisture[face][index] = nodeMoisture[node];
                water[face][index] = ocean.mask[node] ? SphericalWaterClass.Ocean : SphericalWaterClass.Land;
                if (biomes != null) biomes[face][index] = nodeBiomes[node];
            }
            return new DerivedFields(distance, moisture, water, biomes,
                ocean.seaLevel, ocean.actualCoveragePercent, ocean.quantizationTolerancePercent);
        }

    }
}
