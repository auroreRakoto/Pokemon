namespace Pokemon.Project.World.Spherical.Composition
{
    /// <summary>
    /// Describes runtime providers available to validate one world-creation request.
    /// </summary>
    public sealed class SphericalWorldModuleValidationContext
    {
        public readonly bool worldCoreValid;
        public readonly bool terrainPresentationAvailable;
        public readonly bool biomePresentationAvailable;
        public readonly bool environmentCatalogAvailable;
        public readonly bool environmentPlacementRequiresBiomes;
        public readonly bool playerSpawnAvailable;
        public readonly bool playerMovementAvailable;
        public readonly bool pokemonRuntimeAvailable;

        /// <summary>
        /// Creates an immutable snapshot of provider availability for one world creation.
        /// </summary>
        public SphericalWorldModuleValidationContext(
            bool worldCoreValid = true,
            bool terrainPresentationAvailable = true,
            bool biomePresentationAvailable = true,
            bool environmentCatalogAvailable = false,
            bool environmentPlacementRequiresBiomes = true,
            bool playerSpawnAvailable = true,
            bool playerMovementAvailable = true,
            bool pokemonRuntimeAvailable = false)
        {
            this.worldCoreValid = worldCoreValid;
            this.terrainPresentationAvailable = terrainPresentationAvailable;
            this.biomePresentationAvailable = biomePresentationAvailable;
            this.environmentCatalogAvailable = environmentCatalogAvailable;
            this.environmentPlacementRequiresBiomes = environmentPlacementRequiresBiomes;
            this.playerSpawnAvailable = playerSpawnAvailable;
            this.playerMovementAvailable = playerMovementAvailable;
            this.pokemonRuntimeAvailable = pokemonRuntimeAvailable;
        }
    }
}
