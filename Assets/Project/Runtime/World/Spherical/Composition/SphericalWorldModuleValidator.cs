using System;
using System.Collections.Generic;
using Pokemon.Project.World.Spherical.Studio;

namespace Pokemon.Project.World.Spherical.Composition
{
    /// <summary>
    /// Validates requested optional modules without creating Unity objects or mutating world data.
    /// </summary>
    public static class SphericalWorldModuleValidator
    {
        /// <summary>
        /// Produces a deterministic activation report for one world-creation request.
        /// </summary>
        public static SphericalWorldModuleActivationReport Validate(
            SphericalWorldModuleOptions options, SphericalWorldModuleValidationContext context)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (context == null) throw new ArgumentNullException(nameof(context));

            List<SphericalWorldModuleActivation> results = new List<SphericalWorldModuleActivation>(7);
            bool coreActive = context.worldCoreValid;
            results.Add(new SphericalWorldModuleActivation(
                SphericalWorldModuleId.WorldCore,
                coreActive ? SphericalWorldModuleStatus.Required : SphericalWorldModuleStatus.Blocked,
                coreActive ? "World Core is valid and required." : "World Core configuration is invalid."));
            AddTerrain(options, context, coreActive, results);
            AddBiomeClassification(options, coreActive, results);
            AddBiomeGround(options, context, coreActive, results);
            AddEnvironment(options, context, coreActive, results);
            AddPlayer(options, context, coreActive, results);
            AddPokemon(options, context, coreActive, results);
            return new SphericalWorldModuleActivationReport(results);
        }

        /// <summary>
        /// Evaluates optional 3D terrain presentation against World Core and renderer availability.
        /// </summary>
        private static void AddTerrain(
            SphericalWorldModuleOptions options, SphericalWorldModuleValidationContext context,
            bool coreActive, List<SphericalWorldModuleActivation> results)
        {
            if (!options.terrainPresentationEnabled)
                results.Add(Disabled(SphericalWorldModuleId.TerrainPresentation));
            else if (!coreActive)
                results.Add(Blocked(SphericalWorldModuleId.TerrainPresentation, "World Core is invalid."));
            else if (!context.terrainPresentationAvailable)
                results.Add(Blocked(SphericalWorldModuleId.TerrainPresentation, "Terrain presentation provider is unavailable."));
            else
                results.Add(Enabled(SphericalWorldModuleId.TerrainPresentation));
        }

        /// <summary>
        /// Evaluates biome classification independently from biome ground rendering.
        /// </summary>
        private static void AddBiomeClassification(
            SphericalWorldModuleOptions options, bool coreActive,
            List<SphericalWorldModuleActivation> results)
        {
            if (!options.biomeClassificationEnabled)
                results.Add(Disabled(SphericalWorldModuleId.BiomeClassification));
            else if (!coreActive)
                results.Add(Blocked(SphericalWorldModuleId.BiomeClassification, "World Core is invalid."));
            else
                results.Add(Enabled(SphericalWorldModuleId.BiomeClassification));
        }

        /// <summary>
        /// Requires active terrain presentation, biome classification and biome rendering resources.
        /// </summary>
        private static void AddBiomeGround(
            SphericalWorldModuleOptions options, SphericalWorldModuleValidationContext context,
            bool coreActive, List<SphericalWorldModuleActivation> results)
        {
            if (!options.biomeGroundPresentationEnabled)
                results.Add(Disabled(SphericalWorldModuleId.BiomeGroundPresentation));
            else if (!coreActive)
                results.Add(Blocked(SphericalWorldModuleId.BiomeGroundPresentation, "World Core is invalid."));
            else if (!options.terrainPresentationEnabled || !context.terrainPresentationAvailable)
                results.Add(Blocked(SphericalWorldModuleId.BiomeGroundPresentation, "Terrain presentation is not active."));
            else if (!options.biomeClassificationEnabled)
                results.Add(Blocked(SphericalWorldModuleId.BiomeGroundPresentation, "Biome classification is disabled."));
            else if (!context.biomePresentationAvailable)
                results.Add(Blocked(SphericalWorldModuleId.BiomeGroundPresentation, "Biome presentation resources are unavailable."));
            else
                results.Add(Enabled(SphericalWorldModuleId.BiomeGroundPresentation));
        }

        /// <summary>
        /// Requires a valid prefab catalog and biome classification only for biome-dependent placement.
        /// </summary>
        private static void AddEnvironment(
            SphericalWorldModuleOptions options, SphericalWorldModuleValidationContext context,
            bool coreActive, List<SphericalWorldModuleActivation> results)
        {
            if (!options.environmentAssetsEnabled)
                results.Add(Disabled(SphericalWorldModuleId.EnvironmentAssets));
            else if (!coreActive)
                results.Add(Blocked(SphericalWorldModuleId.EnvironmentAssets, "World Core is invalid."));
            else if (!context.environmentCatalogAvailable)
                results.Add(Blocked(SphericalWorldModuleId.EnvironmentAssets, "Environment prefab catalog is unavailable."));
            else if (context.environmentPlacementRequiresBiomes && !options.biomeClassificationEnabled)
                results.Add(Blocked(SphericalWorldModuleId.EnvironmentAssets, "The selected placement policy requires biome classification."));
            else
                results.Add(Enabled(SphericalWorldModuleId.EnvironmentAssets));
        }

        /// <summary>
        /// Requires both a valid spawn query and movement configuration without requiring terrain rendering.
        /// </summary>
        private static void AddPlayer(
            SphericalWorldModuleOptions options, SphericalWorldModuleValidationContext context,
            bool coreActive, List<SphericalWorldModuleActivation> results)
        {
            if (!options.playerEnabled)
                results.Add(Disabled(SphericalWorldModuleId.Player));
            else if (!coreActive)
                results.Add(Blocked(SphericalWorldModuleId.Player, "World Core is invalid."));
            else if (!context.playerSpawnAvailable)
                results.Add(Blocked(SphericalWorldModuleId.Player, "Player spawn query is unavailable."));
            else if (!context.playerMovementAvailable)
                results.Add(Blocked(SphericalWorldModuleId.Player, "Player movement configuration is unavailable."));
            else
                results.Add(Enabled(SphericalWorldModuleId.Player));
        }

        /// <summary>
        /// Keeps Pokémon optional and reports unavailable runtime implementation without placeholders.
        /// </summary>
        private static void AddPokemon(
            SphericalWorldModuleOptions options, SphericalWorldModuleValidationContext context,
            bool coreActive, List<SphericalWorldModuleActivation> results)
        {
            if (!options.pokemonEnabled)
                results.Add(Disabled(SphericalWorldModuleId.Pokemon));
            else if (!coreActive)
                results.Add(Blocked(SphericalWorldModuleId.Pokemon, "World Core is invalid."));
            else if (!context.pokemonRuntimeAvailable)
                results.Add(Blocked(SphericalWorldModuleId.Pokemon, "Pokémon runtime is not implemented or available."));
            else
                results.Add(Enabled(SphericalWorldModuleId.Pokemon));
        }

        /// <summary>
        /// Creates the standard enabled decision for an available optional capability.
        /// </summary>
        private static SphericalWorldModuleActivation Enabled(SphericalWorldModuleId module)
        {
            return new SphericalWorldModuleActivation(module, SphericalWorldModuleStatus.Enabled, "Dependencies are satisfied.");
        }

        /// <summary>
        /// Creates the standard disabled decision for an unrequested optional capability.
        /// </summary>
        private static SphericalWorldModuleActivation Disabled(SphericalWorldModuleId module)
        {
            return new SphericalWorldModuleActivation(module, SphericalWorldModuleStatus.Disabled, "Not requested by the world profile.");
        }

        /// <summary>
        /// Creates a blocked decision with the exact dependency failure.
        /// </summary>
        private static SphericalWorldModuleActivation Blocked(SphericalWorldModuleId module, string diagnostic)
        {
            return new SphericalWorldModuleActivation(module, SphericalWorldModuleStatus.Blocked, diagnostic);
        }
    }
}
