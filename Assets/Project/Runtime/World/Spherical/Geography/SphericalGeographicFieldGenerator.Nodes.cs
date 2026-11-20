using System;
using UnityEngine;
using Pokemon.Project.World.Spherical.Studio;

namespace Pokemon.Project.World.Spherical.Geography
{
    public static partial class SphericalGeographicFieldGenerator
    {
        /// <summary>
        /// Builds canonical graph-node arrays from face samples and accumulates spherical area weights.
        /// </summary>
        private static NodeFields CreateNodeFields(SphericalPlanetStudioSettings settings, BaseFields fields)
        {
            SphericalNeighbourGraph graph = SphericalNeighbourGraph.Build(settings.faceResolution, settings.radiusMeters);
            int count = graph.NodeCount;
            float[] altitude = new float[count];
            float[] temperature = new float[count];
            float[] slope = new float[count];
            double[] areaWeights = new double[count];
            PopulateCanonicalContinuousFields(graph, fields, altitude, temperature, slope);
            AccumulateSphericalNodeAreaWeights(graph, areaWeights);
            return new NodeFields(graph, altitude, temperature, slope, areaWeights);
        }

        /// <summary>
        /// Copies one deterministic sample per canonical graph node from the six face arrays.
        /// </summary>
        private static void PopulateCanonicalContinuousFields(
            SphericalNeighbourGraph graph, BaseFields fields, float[] altitude,
            float[] temperature, float[] slope)
        {
            bool[] assigned = new bool[graph.NodeCount];
            int resolution = graph.Resolution;
            int side = resolution + 1;
            for (int face = 0; face < 6; face++)
            for (int y = 0; y <= resolution; y++)
            for (int x = 0; x <= resolution; x++)
            {
                int node = graph.GetNodeId((CubeSphereFace)face, x, y);
                int index = y * side + x;
                if (assigned[node]) continue;
                assigned[node] = true;
                altitude[node] = fields.altitude[face][index];
                temperature[node] = fields.temperature[face][index];
                slope[node] = fields.slope[face][index];
            }
        }

    }
}
