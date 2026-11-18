namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>Semantic identity of a generated spherical continuous geographic field.</summary>
    public enum SphericalGeographicFieldType
    {
        AltitudeMeters,
        TemperatureNormalized,
        SlopeDegrees,
        DistanceToWaterMeters,
        MoistureNormalized
    }

    /// <summary>Water surface classification. Inland water is reserved for later hydrology integration.</summary>
    public enum SphericalWaterClass : byte
    {
        Land = 0,
        Ocean = 1,
        LakeOrPond = 2,
        River = 3
    }

    /// <summary>Stable categorical IDs for the four approved initial terrestrial biomes.</summary>
    public enum SphericalBiome : byte
    {
        None = 0,
        Plains = 1,
        Forest = 2,
        Desert = 3,
        Mountains = 4
    }
}
