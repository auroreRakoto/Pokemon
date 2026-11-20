using System;
using UnityEngine;
using Pokemon.Project.World.Spherical.Studio;

namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>
    /// Deterministic spherical geographic pipeline with optional biome classification.
    /// World Core fields are generated identically regardless of the biome option.
    /// </summary>
    public static partial class SphericalGeographicFieldGenerator
    {
        /// <summary>
        /// Generates the historical full geographic field set, including biome classification.
        /// </summary>
        public static SphericalGeographicFieldSet Generate(SphericalPlanetStudioSettings settings)
        {
            return Generate(settings, true);
        }

        /// <summary>
        /// Generates authoritative World Core fields and optionally derives biome classification.
        /// </summary>
        public static SphericalGeographicFieldSet Generate(
            SphericalPlanetStudioSettings settings, bool includeBiomeClassification)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.Validate();
            BaseFields baseFields = GenerateBaseFields(settings);
            DerivedFields derivedFields = GenerateDerivedFields(settings, baseFields, includeBiomeClassification);
            return CreateFieldSet(settings, baseFields, derivedFields);
        }

        /// <summary>
        /// Publishes the generated arrays and metadata as one immutable geographic field set.
        /// </summary>
        private static SphericalGeographicFieldSet CreateFieldSet(
            SphericalPlanetStudioSettings settings, BaseFields baseFields, DerivedFields derivedFields)
        {
            return new SphericalGeographicFieldSet(settings.faceResolution, settings.seed, settings.radiusMeters,
                baseFields.altitude, baseFields.temperature, baseFields.slope,
                derivedFields.distanceToWater, derivedFields.moisture, derivedFields.water, derivedFields.biomes,
                settings.oceanCoverageTargetPercent, derivedFields.actualOceanPercent,
                derivedFields.coverageTolerancePercent, derivedFields.seaLevel);
        }

        /// <summary>
        /// Allocates six-face arrays for one continuous geographic field.
        /// </summary>
        private static float[][] CreateFloatFaces(int count)
        {
            var result = new float[6][];
            for (int i = 0; i < result.Length; i++) result[i] = new float[count];
            return result;
        }

        /// <summary>
        /// Allocates six-face arrays for categorical water classes.
        /// </summary>
        private static SphericalWaterClass[][] CreateWaterFaces(int count)
        {
            var result = new SphericalWaterClass[6][];
            for (int i = 0; i < result.Length; i++) result[i] = new SphericalWaterClass[count];
            return result;
        }

        /// <summary>
        /// Allocates six-face arrays for optional biome classifications.
        /// </summary>
        private static SphericalBiome[][] CreateBiomeFaces(int count)
        {
            var result = new SphericalBiome[6][];
            for (int i = 0; i < result.Length; i++) result[i] = new SphericalBiome[count];
            return result;
        }

        private sealed class BaseFields
        {
            public readonly float[][] altitude;
            public readonly float[][] temperature;
            public readonly float[][] slope;

            /// <summary>
            /// Stores the base continuous fields sampled before hydrology and biome derivation.
            /// </summary>
            public BaseFields(float[][] altitude, float[][] temperature, float[][] slope)
            {
                this.altitude = altitude;
                this.temperature = temperature;
                this.slope = slope;
            }
        }

        private sealed class DerivedFields
        {
            public readonly float[][] distanceToWater;
            public readonly float[][] moisture;
            public readonly SphericalWaterClass[][] water;
            public readonly SphericalBiome[][] biomes;
            public readonly float seaLevel;
            public readonly float actualOceanPercent;
            public readonly float coverageTolerancePercent;

            /// <summary>
            /// Stores hydrology outputs and an optional biome field without duplicating World Core data.
            /// </summary>
            public DerivedFields(float[][] distanceToWater, float[][] moisture,
                SphericalWaterClass[][] water, SphericalBiome[][] biomes,
                float seaLevel, float actualOceanPercent, float coverageTolerancePercent)
            {
                this.distanceToWater = distanceToWater;
                this.moisture = moisture;
                this.water = water;
                this.biomes = biomes;
                this.seaLevel = seaLevel;
                this.actualOceanPercent = actualOceanPercent;
                this.coverageTolerancePercent = coverageTolerancePercent;
            }
        }

        private sealed class NodeFields
        {
            public readonly SphericalNeighbourGraph graph;
            public readonly float[] altitude;
            public readonly float[] temperature;
            public readonly float[] slope;
            public readonly double[] areaWeights;

            /// <summary>
            /// Stores canonical graph-node values shared by all cube-sphere face seams.
            /// </summary>
            public NodeFields(SphericalNeighbourGraph graph, float[] altitude, float[] temperature,
                float[] slope, double[] areaWeights)
            {
                this.graph = graph;
                this.altitude = altitude;
                this.temperature = temperature;
                this.slope = slope;
                this.areaWeights = areaWeights;
            }
        }

        private sealed class OceanFields
        {
            public readonly bool[] mask;
            public readonly float[] distanceToOcean;
            public readonly float seaLevel;
            public readonly float actualCoveragePercent;
            public readonly float quantizationTolerancePercent;

            /// <summary>
            /// Stores the authoritative ocean mask and derived coverage metadata.
            /// </summary>
            public OceanFields(bool[] mask, float[] distanceToOcean, float seaLevel,
                float actualCoveragePercent, float quantizationTolerancePercent)
            {
                this.mask = mask;
                this.distanceToOcean = distanceToOcean;
                this.seaLevel = seaLevel;
                this.actualCoveragePercent = actualCoveragePercent;
                this.quantizationTolerancePercent = quantizationTolerancePercent;
            }
        }
    }
}
