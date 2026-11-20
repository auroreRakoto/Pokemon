using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Geography
{
    public static partial class SphericalGeographicFieldGenerator
    {
        /// <summary>
        /// Distributes spherical cell area across canonical graph nodes for area-weighted ocean coverage.
        /// </summary>
        private static void AccumulateSphericalNodeAreaWeights(SphericalNeighbourGraph graph, double[] weights)
        {
            int resolution = graph.Resolution;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            for (int y = 0; y < resolution; y++)
            for (int x = 0; x < resolution; x++)
            {
                CubeSphereFace face = (CubeSphereFace)faceIndex;
                Vector3 a = DirectionAt(face, x, y, resolution);
                Vector3 b = DirectionAt(face, x + 1, y, resolution);
                Vector3 c = DirectionAt(face, x, y + 1, resolution);
                Vector3 d = DirectionAt(face, x + 1, y + 1, resolution);
                double quarter = (SphericalTriangleArea(a, b, c) + SphericalTriangleArea(b, d, c)) * 0.25;
                weights[graph.GetNodeId(face, x, y)] += quarter;
                weights[graph.GetNodeId(face, x + 1, y)] += quarter;
                weights[graph.GetNodeId(face, x, y + 1)] += quarter;
                weights[graph.GetNodeId(face, x + 1, y + 1)] += quarter;
            }
        }

        /// <summary>
        /// Computes the unit-sphere area of a spherical triangle using its solid angle.
        /// </summary>
        private static double SphericalTriangleArea(Vector3 a, Vector3 b, Vector3 c)
        {
            double determinant = Math.Abs(Vector3.Dot(a, Vector3.Cross(b, c)));
            double denominator = 1.0 + Vector3.Dot(a, b) + Vector3.Dot(b, c) + Vector3.Dot(c, a);
            return 2.0 * Math.Atan2(determinant, denominator);
        }
    }
}
