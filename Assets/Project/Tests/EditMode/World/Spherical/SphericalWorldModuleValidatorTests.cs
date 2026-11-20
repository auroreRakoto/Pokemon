using NUnit.Framework;
using Pokemon.Project.World.Spherical.Composition;
using Pokemon.Project.World.Spherical.Studio;

namespace Pokemon.Project.Tests.EditMode.World.Spherical
{
    /// <summary>
    /// Verifies deterministic dependency decisions for optional world capabilities.
    /// </summary>
    public sealed class SphericalWorldModuleValidatorTests
    {
        /// <summary>
        /// Keeps World Core required and active for a valid world-creation request.
        /// </summary>
        [Test]
        public void Validate_RequiresWorldCoreAndActivatesIt()
        {
            SphericalWorldModuleActivationReport report = SphericalWorldModuleValidator.Validate(
                new SphericalWorldModuleOptions(), new SphericalWorldModuleValidationContext());

            Assert.That(report.GetActivation(SphericalWorldModuleId.WorldCore).status, Is.EqualTo(SphericalWorldModuleStatus.Required));
            Assert.That(report.GetActivation(SphericalWorldModuleId.WorldCore).isActive, Is.True);
        }

        /// <summary>
        /// Blocks requested capabilities when World Core is invalid but leaves unrequested ones disabled.
        /// </summary>
        [Test]
        public void Validate_InvalidWorldCoreBlocksRequestedModulesOnly()
        {
            SphericalWorldModuleOptions options = new SphericalWorldModuleOptions { playerEnabled = false };
            SphericalWorldModuleActivationReport report = SphericalWorldModuleValidator.Validate(
                options, new SphericalWorldModuleValidationContext(worldCoreValid: false));

            Assert.That(report.GetActivation(SphericalWorldModuleId.WorldCore).status, Is.EqualTo(SphericalWorldModuleStatus.Blocked));
            Assert.That(report.GetActivation(SphericalWorldModuleId.TerrainPresentation).status, Is.EqualTo(SphericalWorldModuleStatus.Blocked));
            Assert.That(report.GetActivation(SphericalWorldModuleId.Player).status, Is.EqualTo(SphericalWorldModuleStatus.Disabled));
        }

        /// <summary>
        /// Blocks biome ground presentation when terrain or biome classification is disabled.
        /// </summary>
        [Test]
        public void Validate_BiomeGroundRequiresTerrainAndClassification()
        {
            SphericalWorldModuleOptions options = new SphericalWorldModuleOptions
            {
                terrainPresentationEnabled = false,
                biomeClassificationEnabled = false
            };
            SphericalWorldModuleActivationReport report = SphericalWorldModuleValidator.Validate(
                options, new SphericalWorldModuleValidationContext());

            Assert.That(report.GetActivation(SphericalWorldModuleId.BiomeGroundPresentation).status, Is.EqualTo(SphericalWorldModuleStatus.Blocked));
            Assert.That(report.GetActivation(SphericalWorldModuleId.BiomeGroundPresentation).diagnostic, Does.Contain("Terrain"));

            options.terrainPresentationEnabled = true;
            report = SphericalWorldModuleValidator.Validate(options, new SphericalWorldModuleValidationContext());
            Assert.That(report.GetActivation(SphericalWorldModuleId.BiomeGroundPresentation).diagnostic, Does.Contain("classification"));
        }

        /// <summary>
        /// Blocks biome ground presentation when its shader or material mapping provider is unavailable.
        /// </summary>
        [Test]
        public void Validate_BiomeGroundRequiresPresentationResources()
        {
            SphericalWorldModuleActivationReport report = SphericalWorldModuleValidator.Validate(
                new SphericalWorldModuleOptions(),
                new SphericalWorldModuleValidationContext(biomePresentationAvailable: false));

            Assert.That(report.GetActivation(SphericalWorldModuleId.BiomeGroundPresentation).status, Is.EqualTo(SphericalWorldModuleStatus.Blocked));
            Assert.That(report.GetActivation(SphericalWorldModuleId.BiomeGroundPresentation).diagnostic, Does.Contain("resources"));
        }

        /// <summary>
        /// Blocks environment placement when the requested prefab catalog is unavailable.
        /// </summary>
        [Test]
        public void Validate_EnvironmentAssetsRequireCatalog()
        {
            SphericalWorldModuleOptions options = new SphericalWorldModuleOptions { environmentAssetsEnabled = true };
            SphericalWorldModuleActivationReport report = SphericalWorldModuleValidator.Validate(
                options, new SphericalWorldModuleValidationContext(environmentCatalogAvailable: false));

            Assert.That(report.GetActivation(SphericalWorldModuleId.EnvironmentAssets).status, Is.EqualTo(SphericalWorldModuleStatus.Blocked));
            Assert.That(report.GetActivation(SphericalWorldModuleId.Player).status, Is.EqualTo(SphericalWorldModuleStatus.Enabled));
        }

        /// <summary>
        /// Blocks only biome-dependent environment placement when biome classification is disabled.
        /// </summary>
        [Test]
        public void Validate_BiomeDependentEnvironmentRequiresClassification()
        {
            SphericalWorldModuleOptions options = new SphericalWorldModuleOptions
            {
                biomeClassificationEnabled = false,
                biomeGroundPresentationEnabled = false,
                environmentAssetsEnabled = true
            };
            SphericalWorldModuleActivationReport report = SphericalWorldModuleValidator.Validate(
                options, new SphericalWorldModuleValidationContext(environmentCatalogAvailable: true));

            Assert.That(report.GetActivation(SphericalWorldModuleId.EnvironmentAssets).status, Is.EqualTo(SphericalWorldModuleStatus.Blocked));
            Assert.That(report.GetActivation(SphericalWorldModuleId.EnvironmentAssets).diagnostic, Does.Contain("biome"));
        }

        /// <summary>
        /// Allows generic environment placement without biome classification when its policy does not require biomes.
        /// </summary>
        [Test]
        public void Validate_GenericEnvironmentDoesNotRequireClassification()
        {
            SphericalWorldModuleOptions options = new SphericalWorldModuleOptions
            {
                biomeClassificationEnabled = false,
                biomeGroundPresentationEnabled = false,
                environmentAssetsEnabled = true
            };
            SphericalWorldModuleActivationReport report = SphericalWorldModuleValidator.Validate(
                options, new SphericalWorldModuleValidationContext(
                    environmentCatalogAvailable: true, environmentPlacementRequiresBiomes: false));

            Assert.That(report.GetActivation(SphericalWorldModuleId.EnvironmentAssets).status, Is.EqualTo(SphericalWorldModuleStatus.Enabled));
        }

        /// <summary>
        /// Blocks player composition when either spawn lookup or movement configuration is missing.
        /// </summary>
        [Test]
        public void Validate_PlayerRequiresSpawnAndMovementProviders()
        {
            SphericalWorldModuleActivationReport report = SphericalWorldModuleValidator.Validate(
                new SphericalWorldModuleOptions(),
                new SphericalWorldModuleValidationContext(playerMovementAvailable: false));
            Assert.That(report.GetActivation(SphericalWorldModuleId.Player).diagnostic, Does.Contain("movement"));

            report = SphericalWorldModuleValidator.Validate(
                new SphericalWorldModuleOptions(),
                new SphericalWorldModuleValidationContext(playerSpawnAvailable: false));
            Assert.That(report.GetActivation(SphericalWorldModuleId.Player).diagnostic, Does.Contain("spawn"));
            Assert.That(report.GetActivation(SphericalWorldModuleId.TerrainPresentation).status, Is.EqualTo(SphericalWorldModuleStatus.Enabled));
        }

        /// <summary>
        /// Reports the absent Pokémon runtime as a local module block without creating a placeholder.
        /// </summary>
        [Test]
        public void Validate_PokemonUnavailableDoesNotBlockOtherModules()
        {
            SphericalWorldModuleOptions options = new SphericalWorldModuleOptions { pokemonEnabled = true };
            SphericalWorldModuleActivationReport report = SphericalWorldModuleValidator.Validate(
                options, new SphericalWorldModuleValidationContext());

            Assert.That(report.GetActivation(SphericalWorldModuleId.Pokemon).status, Is.EqualTo(SphericalWorldModuleStatus.Blocked));
            Assert.That(report.GetActivation(SphericalWorldModuleId.Player).status, Is.EqualTo(SphericalWorldModuleStatus.Enabled));
        }

        /// <summary>
        /// Enables all requested current modules when every declared provider is available.
        /// </summary>
        [Test]
        public void Validate_AllAvailableModulesAreEnabled()
        {
            SphericalWorldModuleOptions options = new SphericalWorldModuleOptions
            {
                environmentAssetsEnabled = true,
                pokemonEnabled = true
            };
            SphericalWorldModuleActivationReport report = SphericalWorldModuleValidator.Validate(
                options, new SphericalWorldModuleValidationContext(
                    environmentCatalogAvailable: true, pokemonRuntimeAvailable: true));

            Assert.That(report.hasBlockedModules, Is.False);
            Assert.That(report.GetActivation(SphericalWorldModuleId.BiomeGroundPresentation).isActive, Is.True);
            Assert.That(report.GetActivation(SphericalWorldModuleId.EnvironmentAssets).isActive, Is.True);
            Assert.That(report.GetActivation(SphericalWorldModuleId.Player).isActive, Is.True);
            Assert.That(report.GetActivation(SphericalWorldModuleId.Pokemon).isActive, Is.True);
        }
    }
}
