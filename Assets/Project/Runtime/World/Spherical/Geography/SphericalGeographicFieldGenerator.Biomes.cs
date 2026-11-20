using System;
using UnityEngine;
using Pokemon.Project.World.Spherical.Studio;

namespace Pokemon.Project.World.Spherical.Geography
{
    public static partial class SphericalGeographicFieldGenerator
    {
        /// <summary>
        /// Classifies canonical nodes only when the optional biome module is requested.
        /// </summary>
        private static SphericalBiome[] ClassifyNodes(
            SphericalPlanetStudioSettings settings, NodeFields nodes, OceanFields ocean, float[] moisture)
        {
            SphericalBiome[] biomes = new SphericalBiome[nodes.graph.NodeCount];
            for (int node = 0; node < biomes.Length; node++)
            {
                if (ocean.mask[node]) { biomes[node] = SphericalBiome.None; continue; }
                biomes[node] = ClassifyBiome(nodes.altitude[node], nodes.temperature[node],
                    moisture[node], nodes.slope[node], settings);
            }
            return biomes;
        }

        /// <summary>
        /// Applies the approved four-biome thresholds to one terrestrial sample.
        /// </summary>
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
    }
}
