using System;

namespace Pokemon.Project.Data
{
    [Serializable]
    public sealed class WorldDefinitionData
    {
        public int schemaVersion;
        public string id;
        public string displayName;
        public int seed;
        public string generatorVersion;
        public WorldSizeData size;
        public WorldOriginData origin;
        public MapResolutionData mapResolution;
        public WorldLayerReferenceData[] layers;
        public string[] biomeIds;
        public string notes;
    }

    [Serializable]
    public sealed class WorldSizeData
    {
        public float x;
        public float z;
    }

    [Serializable]
    public sealed class WorldOriginData
    {
        public float x;
        public float z;
    }

    [Serializable]
    public sealed class MapResolutionData
    {
        public int width;
        public int height;
    }

    [Serializable]
    public sealed class WorldLayerReferenceData
    {
        public string id;
        public string manifestPath;
    }

    [Serializable]
    public sealed class BiomeDefinitionData
    {
        public int schemaVersion;
        public string id;
        public string displayName;
        public int priority;
        public BiomeConditionsData conditions;
        public BiomePresentationData presentation;
    }

    [Serializable]
    public sealed class BiomeConditionsData
    {
        public WeightedRangeData humidity;
        public WeightedRangeData temperature;
        public WeightedRangeData vegetationDensity;
        public WeightedRangeData height;
    }

    [Serializable]
    public sealed class WeightedRangeData
    {
        public float min;
        public float max;
        public float weight;
    }

    [Serializable]
    public sealed class BiomePresentationData
    {
        public string paletteId;
        public string placementProfileId;
    }

    [Serializable]
    public sealed class PokemonSpeciesData
    {
        public int schemaVersion;
        public string id;
        public string displayName;
        public string[] types;
        public PokemonHabitatPreferencesData habitatPreferences;
        public PokemonPresentationData presentation;
        public string notes;
    }

    [Serializable]
    public sealed class PokemonHabitatPreferencesData
    {
        public string[] preferredBiomeIds;
        public NumericRangeData humidity;
        public NumericRangeData vegetationDensity;
    }

    [Serializable]
    public sealed class NumericRangeData
    {
        public float min;
        public float max;
    }

    [Serializable]
    public sealed class PokemonPresentationData
    {
        public string assetId;
    }

    [Serializable]
    public sealed class MapLayerManifestData
    {
        public int schemaVersion;
        public string id;
        public string worldId;
        public string layerType;
        public string semantic;
        public string dataFile;
        public string encoding;
        public string byteOrder;
        public int channels;
        public int width;
        public int height;
        public string units;
        public NumericRangeData valueRange;
        public string coordinateSystem;
        public WorldOriginData origin;
        public WorldSizeData worldSize;
        public string rowOrder;
        public string sampling;
        public GeneratorMetadataData generator;
    }

    [Serializable]
    public sealed class GeneratorMetadataData
    {
        public string id;
        public string version;
        public int seed;
    }
}
