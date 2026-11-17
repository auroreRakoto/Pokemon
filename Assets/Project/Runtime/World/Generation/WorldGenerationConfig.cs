using System;
using UnityEngine;

namespace Pokemon.Project.World.Generation
{
    /// <summary>
    /// Loads the authoritative world-generation settings from the Resources JSON.
    /// The same loader is available in Editor and Play Mode.
    /// </summary>
    public static class WorldGenerationConfig
    {
        private const string ResourcePath = "WorldGeneration/world_generation";

        public static WorldGenerationSettings Load()
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>(ResourcePath);
            if (jsonAsset == null)
                throw new InvalidOperationException(
                    $"Missing Resources/{ResourcePath}.json. Restore the world-generation configuration file.");

            WorldGenerationSettings settings = JsonUtility.FromJson<WorldGenerationSettings>(jsonAsset.text);
            if (settings == null)
                throw new InvalidOperationException("The world-generation JSON could not be deserialized.");

            settings.Validate();
            return settings;
        }
    }
}
