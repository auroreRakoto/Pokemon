using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>
    /// Memory-conscious canonical graph over the six-face Cube-Sphere lattice.
    /// Adjacency is stored in compressed sparse row (CSR) arrays rather than one
    /// managed list/array per node.
    /// </summary>
    public sealed class SphericalNeighbourGraph
    {
        private readonly Vector3[] _directions;
        private readonly int[] _faceNodeIds;
        private readonly int[] _neighbourOffsets;
        private readonly int[] _neighbourIds;
        private readonly float[] _edgeDistances;

        public int Resolution { get; }
        public float PlanetRadiusMeters { get; }
        public int NodeCount => _directions.Length;
        public int EdgeCount => _neighbourIds.Length / 2;
        public long EstimatedStorageBytes =>
            (long)_directions.Length * 12L +
            (long)_faceNodeIds.Length * 4L +
            (long)_neighbourOffsets.Length * 4L +
            (long)_neighbourIds.Length * 4L +
            (long)_edgeDistances.Length * 4L;

        private SphericalNeighbourGraph(int resolution, float radius, Vector3[] directions,
            int[] faceNodeIds, int[] offsets, int[] neighbours, float[] edgeDistances)
        {
            Resolution = resolution;
            PlanetRadiusMeters = radius;
            _directions = directions;
            _faceNodeIds = faceNodeIds;
            _neighbourOffsets = offsets;
            _neighbourIds = neighbours;
            _edgeDistances = edgeDistances;
        }

        public Vector3 GetDirection(int nodeId)
        {
            ValidateNode(nodeId);
            return _directions[nodeId];
        }

        public int GetNeighbourCount(int nodeId)
        {
            ValidateNode(nodeId);
            return _neighbourOffsets[nodeId + 1] - _neighbourOffsets[nodeId];
        }

        public int GetNeighbour(int nodeId, int neighbourIndex)
        {
            ValidateNeighbourIndex(nodeId, neighbourIndex);
            return _neighbourIds[_neighbourOffsets[nodeId] + neighbourIndex];
        }

        public float GetEdgeDistanceMeters(int nodeId, int neighbourIndex)
        {
            ValidateNeighbourIndex(nodeId, neighbourIndex);
            return _edgeDistances[_neighbourOffsets[nodeId] + neighbourIndex];
        }

        // Compatibility accessor for tests/tools. Hot runtime loops should use indexed accessors above.
        public IReadOnlyList<int> GetNeighbours(int nodeId)
        {
            ValidateNode(nodeId);
            int start = _neighbourOffsets[nodeId];
            return new ArraySegment<int>(_neighbourIds, start, _neighbourOffsets[nodeId + 1] - start);
        }

        public int GetNodeId(CubeSphereFace face, int x, int y)
        {
            int faceIndex = (int)face;
            if (faceIndex < 0 || faceIndex >= 6) throw new ArgumentOutOfRangeException(nameof(face));
            if (x < 0 || x > Resolution) throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y > Resolution) throw new ArgumentOutOfRangeException(nameof(y));
            int side = Resolution + 1;
            return _faceNodeIds[faceIndex * side * side + y * side + x];
        }

        public static SphericalNeighbourGraph Build(int resolution, float planetRadiusMeters)
        {
            if (resolution < 2) throw new ArgumentOutOfRangeException(nameof(resolution));
            if (planetRadiusMeters <= 0f || float.IsNaN(planetRadiusMeters) || float.IsInfinity(planetRadiusMeters))
                throw new ArgumentOutOfRangeException(nameof(planetRadiusMeters));

            int side = resolution + 1;
            int samplesPerFace = checked(side * side);
            int totalFaceSamples = checked(6 * samplesPerFace);
            var faceNodeIds = new int[totalFaceSamples];
            var directions = new List<Vector3>(6 * resolution * resolution + 2);
            var nodeByKey = new Dictionary<DirectionKey, int>(6 * resolution * resolution + 2);

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                CubeSphereFace face = (CubeSphereFace)faceIndex;
                int faceOffset = faceIndex * samplesPerFace;
                for (int y = 0; y <= resolution; y++)
                for (int x = 0; x <= resolution; x++)
                {
                    float u = -1f + 2f * x / resolution;
                    float v = -1f + 2f * y / resolution;
                    Vector3 direction = CubeSphereTopology.GetDirection(face, u, v);
                    var key = new DirectionKey(direction);
                    if (!nodeByKey.TryGetValue(key, out int nodeId))
                    {
                        nodeId = directions.Count;
                        nodeByKey.Add(key, nodeId);
                        directions.Add(direction);
                    }
                    faceNodeIds[faceOffset + y * side + x] = nodeId;
                }
            }

            // Each face contributes 2 * N * (N + 1) grid edges. Sort packed
            // undirected edge keys and compact duplicates instead of allocating a Dictionary.
            int maximumEdgeCount = checked(12 * resolution * (resolution + 1));
            var edgeKeys = new ulong[maximumEdgeCount];
            int edgeKeyCount = 0;
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                int faceOffset = faceIndex * samplesPerFace;
                for (int y = 0; y <= resolution; y++)
                for (int x = 0; x <= resolution; x++)
                {
                    int current = faceNodeIds[faceOffset + y * side + x];
                    if (x < resolution)
                        edgeKeys[edgeKeyCount++] = PackEdge(current, faceNodeIds[faceOffset + y * side + x + 1]);
                    if (y < resolution)
                        edgeKeys[edgeKeyCount++] = PackEdge(current, faceNodeIds[faceOffset + (y + 1) * side + x]);
                }
            }
            Array.Sort(edgeKeys, 0, edgeKeyCount);

            int uniqueEdgeCount = 0;
            ulong previous = ulong.MaxValue;
            for (int i = 0; i < edgeKeyCount; i++)
            {
                if (edgeKeys[i] == previous) continue;
                previous = edgeKeys[i];
                edgeKeys[uniqueEdgeCount++] = previous;
            }

            int nodeCount = directions.Count;
            var degrees = new int[nodeCount];
            for (int i = 0; i < uniqueEdgeCount; i++)
            {
                int a = (int)(edgeKeys[i] >> 32);
                int b = (int)(edgeKeys[i] & 0xffffffffUL);
                degrees[a]++;
                degrees[b]++;
            }

            var offsets = new int[nodeCount + 1];
            for (int i = 0; i < nodeCount; i++) offsets[i + 1] = offsets[i] + degrees[i];
            var neighbours = new int[uniqueEdgeCount * 2];
            var distances = new float[uniqueEdgeCount * 2];
            var cursors = new int[nodeCount];
            Array.Copy(offsets, cursors, nodeCount);

            for (int i = 0; i < uniqueEdgeCount; i++)
            {
                int a = (int)(edgeKeys[i] >> 32);
                int b = (int)(edgeKeys[i] & 0xffffffffUL);
                Vector3 da = directions[a];
                Vector3 db = directions[b];
                float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(da, db), -1f, 1f));
                float distance = angle * planetRadiusMeters;
                int ai = cursors[a]++;
                int bi = cursors[b]++;
                neighbours[ai] = b;
                distances[ai] = distance;
                neighbours[bi] = a;
                distances[bi] = distance;
            }

            // Stable neighbour order for deterministic diagnostics and traversal.
            for (int node = 0; node < nodeCount; node++)
            {
                int start = offsets[node];
                int count = offsets[node + 1] - start;
                Array.Sort(neighbours, distances, start, count);
            }

            return new SphericalNeighbourGraph(resolution, planetRadiusMeters,
                directions.ToArray(), faceNodeIds, offsets, neighbours, distances);
        }

        private static ulong PackEdge(int a, int b)
        {
            if (a == b) return ulong.MaxValue;
            int min = Math.Min(a, b);
            int max = Math.Max(a, b);
            return ((ulong)(uint)min << 32) | (uint)max;
        }

        private void ValidateNode(int nodeId)
        {
            if (nodeId < 0 || nodeId >= NodeCount) throw new ArgumentOutOfRangeException(nameof(nodeId));
        }

        private void ValidateNeighbourIndex(int nodeId, int neighbourIndex)
        {
            ValidateNode(nodeId);
            int count = _neighbourOffsets[nodeId + 1] - _neighbourOffsets[nodeId];
            if (neighbourIndex < 0 || neighbourIndex >= count)
                throw new ArgumentOutOfRangeException(nameof(neighbourIndex));
        }

        private struct DirectionKey : IEquatable<DirectionKey>
        {
            private const float Quantization = 1000000f;
            private readonly int _x, _y, _z;
            public DirectionKey(Vector3 d)
            {
                _x = Mathf.RoundToInt(d.x * Quantization);
                _y = Mathf.RoundToInt(d.y * Quantization);
                _z = Mathf.RoundToInt(d.z * Quantization);
            }
            public bool Equals(DirectionKey other) => _x == other._x && _y == other._y && _z == other._z;
            public override bool Equals(object obj) => obj is DirectionKey other && Equals(other);
            public override int GetHashCode()
            {
                unchecked { return ((_x * 397) ^ _y) * 397 ^ _z; }
            }
        }
    }
}
