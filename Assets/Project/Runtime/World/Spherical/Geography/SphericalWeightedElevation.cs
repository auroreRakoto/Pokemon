using System;

namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>
    /// Area-weighted elevation sample with stable node ordering for deterministic sorting.
    /// </summary>
    internal struct SphericalWeightedElevation : IComparable<SphericalWeightedElevation>
    {
        public readonly float elevation;
        public readonly int nodeId;
        public readonly double areaWeight;

        /// <summary>
        /// Creates one weighted elevation sample for sea-level selection.
        /// </summary>
        public SphericalWeightedElevation(float elevation, int nodeId, double areaWeight)
        {
            this.elevation = elevation;
            this.nodeId = nodeId;
            this.areaWeight = areaWeight;
        }

        /// <summary>
        /// Orders equal elevations by stable graph node identity.
        /// </summary>
        public int CompareTo(SphericalWeightedElevation other)
        {
            int result = elevation.CompareTo(other.elevation);
            return result != 0 ? result : nodeId.CompareTo(other.nodeId);
        }
    }
}
