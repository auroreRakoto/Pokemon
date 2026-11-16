using UnityEngine;

namespace Pokemon.Project.Data
{
    /// <summary>
    /// Generated projection of canonical Data/worlds/*.json.
    /// Do not edit this asset manually; regenerate it from JSON.
    /// </summary>
    public sealed class WorldDefinitionAsset : ScriptableObject
    {
        public WorldDefinitionData data;
    }

    /// <summary>Generated projection of canonical Data/biomes/*.json.</summary>
    public sealed class BiomeDefinitionAsset : ScriptableObject
    {
        public BiomeDefinitionData data;
    }

    /// <summary>Generated projection of canonical Data/pokemon/*.json.</summary>
    public sealed class PokemonSpeciesDefinitionAsset : ScriptableObject
    {
        public PokemonSpeciesData data;
    }
}
