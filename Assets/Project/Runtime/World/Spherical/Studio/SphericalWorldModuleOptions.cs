using System;

namespace Pokemon.Project.World.Spherical.Studio
{
    /// <summary>
    /// Stores optional world capabilities selected when a world profile is created.
    /// </summary>
    [Serializable]
    public sealed class SphericalWorldModuleOptions
    {
        public bool terrainPresentationEnabled = true;
        public bool biomeClassificationEnabled = true;
        public bool biomeGroundPresentationEnabled = true;
        public bool environmentAssetsEnabled;
        public bool playerEnabled = true;
        public bool pokemonEnabled;

        /// <summary>
        /// Creates an independent copy for profile and settings ownership boundaries.
        /// </summary>
        public SphericalWorldModuleOptions Copy()
        {
            return new SphericalWorldModuleOptions
            {
                terrainPresentationEnabled = terrainPresentationEnabled,
                biomeClassificationEnabled = biomeClassificationEnabled,
                biomeGroundPresentationEnabled = biomeGroundPresentationEnabled,
                environmentAssetsEnabled = environmentAssetsEnabled,
                playerEnabled = playerEnabled,
                pokemonEnabled = pokemonEnabled
            };
        }
    }
}
