#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Pokemon.Project.World.Spherical;
using Pokemon.Project.World.Spherical.Geography;
using Pokemon.Project.World.Spherical.Studio;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pokemon.Project.Editor.World.Spherical
{
    /// <summary>Builds the experimental drainage data and a reusable visual overlay mesh for Planet Studio.</summary>
    public static class SphericalDrainageStudioPreview
    {
        public static SphericalDrainageResult Generate(SphericalPlanetStudioSettings settings, out SphericalNeighbourGraph graph)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Validate();
            graph = SphericalNeighbourGraph.Build(settings.drainageResolution, settings.radiusMeters);
            var elevations = new float[graph.NodeCount];
            var water = new bool[graph.NodeCount];
            if (settings.drainageInputMode == SphericalDrainageInputMode.FlatBaseSphereDiagnostic)
            {
                // Diagnostic only: a perfectly flat sphere has no natural downhill
                // direction and a sea-level cutoff would classify every node alike.
                // Use a single deterministic north-pole outlet to exercise the
                // complete drainage + overlay pipeline without pretending this is
                // a physically meaningful river network.
                int outletNode = 0;
                float highestY = float.NegativeInfinity;
                for (int node = 0; node < graph.NodeCount; node++)
                {
                    elevations[node] = 0f;
                    Vector3 direction = graph.GetDirection(node);
                    if (direction.y > highestY)
                    {
                        highestY = direction.y;
                        outletNode = node;
                    }
                }
                water[outletNode] = true;

                int diagnosticOutletCount = CountTrue(water);
                if (diagnosticOutletCount != 1)
                    throw new InvalidOperationException(
                        $"Flat Base Sphere Diagnostic must create exactly one outlet, but created {diagnosticOutletCount}.");
            }
            else
            {
                for (int node = 0; node < graph.NodeCount; node++)
                {
                    Vector3 direction = graph.GetDirection(node);
                    float height = SphericalPlanetStudioGenerator.SamplePreviewHeight(direction, settings, settings.previewStage);
                    elevations[node] = height;
                    water[node] = height <= settings.drainageSeaLevelMeters;
                }
                if (CountTrue(water) == 0)
                    throw new InvalidOperationException("The selected sea level produced no water outlets. Increase Sea Level (m) or adjust terrain settings.");
            }
            return SphericalDrainage.Build(graph, elevations, water, settings.riverAccumulationThreshold);
        }


        /// <summary>
        /// Builds obvious test geometry without running or reading the drainage algorithm.
        /// This isolates Scene rendering, mesh placement, material and camera visibility.
        /// </summary>
        public static Mesh BuildRendererDiagnosticMesh(float radiusMeters)
        {
            if (radiusMeters <= 0f || float.IsNaN(radiusMeters) || float.IsInfinity(radiusMeters))
                throw new ArgumentOutOfRangeException(nameof(radiusMeters));

            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            float width = Mathf.Max(250f, radiusMeters * 0.012f);
            float lift = Mathf.Max(500f, radiusMeters * 0.02f);
            Color[] palette =
            {
                new Color(1f, 0.02f, 0.75f, 1f),
                new Color(1f, 0.85f, 0.02f, 1f),
                new Color(0.02f, 1f, 0.9f, 1f)
            };

            AddGlobeSegment(Vector3.right, Vector3.up, radiusMeters, 0f, 0f,
                width, lift, palette[0], vertices, colors, triangles);
            AddGlobeSegment(Vector3.up, Vector3.forward, radiusMeters, 0f, 0f,
                width, lift, palette[1], vertices, colors, triangles);
            AddGlobeSegment(Vector3.forward, Vector3.left, radiusMeters, 0f, 0f,
                width, lift, palette[2], vertices, colors, triangles);

            var mesh = new Mesh
            {
                name = "WORLD-002_RendererDiagnostic_Independent",
                indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            // Add reverse faces so the diagnostic is independent of triangle winding/culling.
            int triangleCount = triangles.Count;
            var doubleSided = new List<int>(triangleCount * 2);
            doubleSided.AddRange(triangles);
            for (int i = 0; i < triangleCount; i += 3)
            {
                doubleSided.Add(triangles[i]);
                doubleSided.Add(triangles[i + 2]);
                doubleSided.Add(triangles[i + 1]);
            }
            mesh.SetTriangles(doubleSided, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh BuildOverlayMesh(SphericalNeighbourGraph graph, SphericalDrainageResult result,
            SphericalPlanetStudioSettings settings, bool planisphere,
            out int eligibleSegments, out int renderedSegments, out int samplingStride)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            const int maximumRenderedSegments = 150000;
            eligibleSegments = 0;
            renderedSegments = 0;
            samplingStride = 1;
            int directionStride = Mathf.Max(1, settings.flowDirectionStride);

            // First pass counts exactly the segments selected by the current overlay mode.
            for (int node = 0; node < graph.NodeCount; node++)
            {
                if (result.DownstreamNodeIds[node] < 0) continue;
                bool include;
                switch (settings.drainagePreviewMode)
                {
                    case SphericalDrainagePreviewMode.FlowAccumulation:
                    case SphericalDrainagePreviewMode.RiverCandidates:
                        include = result.FlowAccumulation[node] >= settings.riverAccumulationThreshold &&
                                  !result.OutletMask[node];
                        break;
                    case SphericalDrainagePreviewMode.FlowDirection:
                        include = result.FlowAccumulation[node] >= settings.riverAccumulationThreshold &&
                                  !result.OutletMask[node] && node % directionStride == 0;
                        break;
                    default:
                        include = result.RiverMask[node];
                        break;
                }
                if (include) eligibleSegments++;
            }

            if (eligibleSegments == 0) return null;

            // High-resolution graphs can contain millions of candidate edges.
            // Cap preview geometry to keep the Editor responsive while preserving
            // deterministic, evenly spaced sampling through canonical node order.
            samplingStride = Mathf.Max(1, Mathf.CeilToInt(eligibleSegments / (float)maximumRenderedSegments));

            var vertices = new List<Vector3>(Mathf.Min(eligibleSegments, maximumRenderedSegments) * 4);
            var colors = new List<Color>(Mathf.Min(eligibleSegments, maximumRenderedSegments) * 4);
            var triangles = new List<int>(Mathf.Min(eligibleSegments, maximumRenderedSegments) * 6);

            float width = Mathf.Max(10f, settings.radiusMeters * 0.008f);
            float visualLift = Mathf.Max(50f, settings.radiusMeters * 0.003f);
            int eligibleOrdinal = 0;

            for (int node = 0; node < graph.NodeCount; node++)
            {
                int downstream = result.DownstreamNodeIds[node];
                if (downstream < 0) continue;

                bool include;
                Color color;
                switch (settings.drainagePreviewMode)
                {
                    case SphericalDrainagePreviewMode.FlowAccumulation:
                        include = result.FlowAccumulation[node] >= settings.riverAccumulationThreshold &&
                                  !result.OutletMask[node];
                        float normalized = Mathf.Clamp01(Mathf.Log(1f + result.FlowAccumulation[node]) /
                            Mathf.Log(1f + Mathf.Max(settings.riverAccumulationThreshold * 20f, 2f)));
                        color = Color.Lerp(new Color(0.05f, 0.45f, 1f, 1f),
                            new Color(1f, 0.12f, 0.02f, 1f), normalized);
                        break;
                    case SphericalDrainagePreviewMode.FlowDirection:
                        include = result.FlowAccumulation[node] >= settings.riverAccumulationThreshold &&
                                  !result.OutletMask[node] && node % directionStride == 0;
                        color = new Color(1f, 0.82f, 0.08f, 1f);
                        break;
                    default:
                        include = result.RiverMask[node];
                        color = new Color(0.02f, 0.48f, 1f, 1f);
                        break;
                }

                if (!include) continue;
                bool sampled = eligibleOrdinal % samplingStride == 0;
                eligibleOrdinal++;
                if (!sampled) continue;

                Vector3 aDirection = graph.GetDirection(node);
                Vector3 bDirection = graph.GetDirection(downstream);
                float aHeight = result.FilledElevationMeters[node];
                float bHeight = result.FilledElevationMeters[downstream];
                if (planisphere)
                    AddPlanisphereSegment(aDirection, bDirection, settings.radiusMeters, width,
                        color, vertices, colors, triangles);
                else
                    AddGlobeSegment(aDirection, bDirection, settings.radiusMeters, aHeight, bHeight,
                        width, visualLift, color, vertices, colors, triangles);
                renderedSegments++;
            }

            if (vertices.Count == 0) return null;
            var mesh = new Mesh
            {
                name = planisphere ? "WORLD-002_DrainageOverlay_Planisphere" : "WORLD-002_DrainageOverlay_Globe",
                indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddGlobeSegment(Vector3 aDirection, Vector3 bDirection, float radius,
            float aHeight, float bHeight, float width, float visualLift, Color color,
            List<Vector3> vertices, List<Color> colors, List<int> triangles)
        {
            // Never represent a long spherical segment with one straight chord:
            // the chord cuts through the globe and is hidden by the terrain.
            // Subdivide along the great-circle arc and build a short ribbon quad
            // for each angular step. This also keeps high-resolution drainage
            // edges slightly lifted above the lower-resolution display mesh.
            Vector3 from = aDirection.normalized;
            Vector3 to = bDirection.normalized;
            float angleDegrees = Vector3.Angle(from, to);
            int subdivisions = Mathf.Max(1, Mathf.CeilToInt(angleDegrees / 2f));

            Vector3 previousDirection = from;
            float previousHeight = aHeight;
            for (int step = 1; step <= subdivisions; step++)
            {
                float t = step / (float)subdivisions;
                Vector3 currentDirection = Vector3.Slerp(from, to, t).normalized;
                float currentHeight = Mathf.Lerp(aHeight, bHeight, t);

                Vector3 a = previousDirection * (radius + previousHeight + visualLift);
                Vector3 b = currentDirection * (radius + currentHeight + visualLift);
                Vector3 tangent = (b - a).normalized;
                Vector3 radial = (a + b).normalized;
                Vector3 side = Vector3.Cross(radial, tangent).normalized * (width * 0.5f);
                if (side.sqrMagnitude >= 1e-8f)
                    AddQuad(a - side, a + side, b - side, b + side,
                        color, vertices, colors, triangles);

                previousDirection = currentDirection;
                previousHeight = currentHeight;
            }
        }

        private static void AddPlanisphereSegment(Vector3 aDirection, Vector3 bDirection, float radius,
            float width, Color color, List<Vector3> vertices, List<Color> colors, List<int> triangles)
        {
            Vector2 a = SphericalPlanetStudioGenerator.DirectionToPlanisphereCoordinates(aDirection);
            Vector2 b = SphericalPlanetStudioGenerator.DirectionToPlanisphereCoordinates(bDirection);
            if (b.x - a.x > 0.5f) b.x -= 1f;
            else if (a.x - b.x > 0.5f) b.x += 1f;
            Vector2 delta = b - a;
            Vector2 side = new Vector2(-delta.y, delta.x).normalized * (width / (2f * Mathf.PI * radius));
            if (side.sqrMagnitude < 1e-12f) return;
            Vector3 p0 = PlanispherePoint(a - side, radius);
            Vector3 p1 = PlanispherePoint(a + side, radius);
            Vector3 p2 = PlanispherePoint(b - side, radius);
            Vector3 p3 = PlanispherePoint(b + side, radius);
            AddQuad(p0, p1, p2, p3, color, vertices, colors, triangles);
            // Duplicate at the opposite horizontal edge when the shortest segment crosses the wrap.
            if (Mathf.Min(a.x, b.x) < 0f || Mathf.Max(a.x, b.x) > 1f)
            {
                Vector2 shift = new Vector2(a.x < 0f || b.x < 0f ? 1f : -1f, 0f);
                AddQuad(PlanispherePoint(a - side + shift, radius), PlanispherePoint(a + side + shift, radius),
                    PlanispherePoint(b - side + shift, radius), PlanispherePoint(b + side + shift, radius),
                    color, vertices, colors, triangles);
            }
        }

        private static Vector3 PlanispherePoint(Vector2 uv, float radius)
        {
            return new Vector3((uv.x - 0.5f) * Mathf.PI * 2f * radius,
                (uv.y - 0.5f) * Mathf.PI * radius, 0.1f);
        }

        private static void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color,
            List<Vector3> vertices, List<Color> colors, List<int> triangles)
        {
            int start = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
            colors.Add(color); colors.Add(color); colors.Add(color); colors.Add(color);
            // The original winding faced inward on both the globe and planisphere.
            // Reverse it so the visible side faces the Scene camera under back-face culling.
            triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 1);
            triangles.Add(start + 1); triangles.Add(start + 2); triangles.Add(start + 3);
        }

        private static int CountTrue(bool[] values)
        {
            int count = 0;
            for (int i = 0; i < values.Length; i++) if (values[i]) count++;
            return count;
        }
    }
}
#endif
