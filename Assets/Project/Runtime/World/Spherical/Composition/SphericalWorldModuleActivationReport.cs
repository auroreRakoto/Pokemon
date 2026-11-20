using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pokemon.Project.World.Spherical.Composition
{
    /// <summary>
    /// Stable identifiers for capabilities evaluated during world creation.
    /// </summary>
    public enum SphericalWorldModuleId
    {
        WorldCore,
        TerrainPresentation,
        BiomeClassification,
        BiomeGroundPresentation,
        EnvironmentAssets,
        Player,
        Pokemon
    }

    /// <summary>
    /// Describes whether a requested world capability can be composed.
    /// </summary>
    public enum SphericalWorldModuleStatus
    {
        Required,
        Enabled,
        Disabled,
        Blocked
    }

    /// <summary>
    /// One module decision and its human-readable diagnostic.
    /// </summary>
    public sealed class SphericalWorldModuleActivation
    {
        public SphericalWorldModuleId module { get; }
        public SphericalWorldModuleStatus status { get; }
        public string diagnostic { get; }
        public bool isActive => status == SphericalWorldModuleStatus.Required ||
                                status == SphericalWorldModuleStatus.Enabled;

        /// <summary>
        /// Creates a module activation result.
        /// </summary>
        public SphericalWorldModuleActivation(
            SphericalWorldModuleId module, SphericalWorldModuleStatus status, string diagnostic)
        {
            this.module = module;
            this.status = status;
            this.diagnostic = diagnostic ?? string.Empty;
        }
    }

    /// <summary>
    /// Immutable ordered result of validating a world module configuration.
    /// </summary>
    public sealed class SphericalWorldModuleActivationReport
    {
        private readonly ReadOnlyCollection<SphericalWorldModuleActivation> _activations;

        public IReadOnlyList<SphericalWorldModuleActivation> activations => _activations;
        public bool hasBlockedModules
        {
            get
            {
                for (int i = 0; i < _activations.Count; i++)
                    if (_activations[i].status == SphericalWorldModuleStatus.Blocked) return true;
                return false;
            }
        }

        /// <summary>
        /// Creates a report with a defensive copy of the activation decisions.
        /// </summary>
        public SphericalWorldModuleActivationReport(List<SphericalWorldModuleActivation> activations)
        {
            if (activations == null) throw new ArgumentNullException(nameof(activations));
            _activations = new ReadOnlyCollection<SphericalWorldModuleActivation>(
                new List<SphericalWorldModuleActivation>(activations));
        }

        /// <summary>
        /// Finds the decision for a module or throws when the report is incomplete.
        /// </summary>
        public SphericalWorldModuleActivation GetActivation(SphericalWorldModuleId module)
        {
            for (int i = 0; i < _activations.Count; i++)
                if (_activations[i].module == module) return _activations[i];
            throw new ArgumentOutOfRangeException(nameof(module), module, "The module is absent from this report.");
        }
    }
}
