using UnityEngine;

/// <summary>
/// Central registry of project-authored content references.
/// This asset contains static content references only; it must never hold mutable
/// per-individual or per-session runtime state.
/// </summary>
[CreateAssetMenu(
    fileName = "ProjectContentCatalog",
    menuName = "Pokemon/Content/Project Content Catalog"
)]
public sealed class ProjectContentCatalog : ScriptableObject
{
    [Header("Definitions")]
    [SerializeField] private PokemonDefinition[] pokemonDefinitions =
        new PokemonDefinition[0];

    [SerializeField] private BiomeDefinition[] biomeDefinitions =
        new BiomeDefinition[0];

    [SerializeField] private WorldDefinition[] worldDefinitions =
        new WorldDefinition[0];

    [Header("Pokémon Prefabs")]
    [SerializeField] private GameObject[] pokemonPrefabs =
        new GameObject[0];

    [Header("Ecology Prefabs")]
    [SerializeField] private FruitBush fruitBushPrefab;
    [SerializeField] private GameObject blueberryPrefab;
    [SerializeField] private GameObject redberryPrefab;

    [Header("Vegetation Prefabs")]
    [SerializeField] private GameObject[] grassPrefabs =
        new GameObject[0];

    public PokemonDefinition[] PokemonDefinitions => pokemonDefinitions;
    public BiomeDefinition[] BiomeDefinitions => biomeDefinitions;
    public WorldDefinition[] WorldDefinitions => worldDefinitions;
    public GameObject[] PokemonPrefabs => pokemonPrefabs;

    public FruitBush FruitBushPrefab => fruitBushPrefab;
    public GameObject BlueberryPrefab => blueberryPrefab;
    public GameObject RedberryPrefab => redberryPrefab;
    public GameObject[] GrassPrefabs => grassPrefabs;

    public PokemonDefinition FindPokemonDefinition(string pokemonName)
    {
        if (string.IsNullOrWhiteSpace(pokemonName) ||
            pokemonDefinitions == null)
            return null;

        foreach (PokemonDefinition definition in pokemonDefinitions)
        {
            if (definition != null &&
                string.Equals(
                    definition.pokemonName,
                    pokemonName,
                    System.StringComparison.OrdinalIgnoreCase))
                return definition;
        }

        return null;
    }

    public BiomeDefinition FindBiomeDefinition(string biomeName)
    {
        if (string.IsNullOrWhiteSpace(biomeName) ||
            biomeDefinitions == null)
            return null;

        foreach (BiomeDefinition definition in biomeDefinitions)
        {
            if (definition != null &&
                string.Equals(
                    definition.biomeName,
                    biomeName,
                    System.StringComparison.OrdinalIgnoreCase))
                return definition;
        }

        return null;
    }

    public WorldDefinition FindWorldDefinition(string worldName)
    {
        if (string.IsNullOrWhiteSpace(worldName) ||
            worldDefinitions == null)
            return null;

        foreach (WorldDefinition definition in worldDefinitions)
        {
            if (definition != null &&
                string.Equals(
                    definition.worldName,
                    worldName,
                    System.StringComparison.OrdinalIgnoreCase))
                return definition;
        }

        return null;
    }
}
