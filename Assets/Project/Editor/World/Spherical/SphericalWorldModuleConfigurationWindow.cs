#if UNITY_EDITOR
using System;
using System.IO;
using Pokemon.Project.World.Spherical.Studio;
using UnityEditor;
using UnityEngine;

namespace Pokemon.Project.Editor.World.Spherical
{
    /// <summary>
    /// Edits optional capabilities stored in a world-creation profile.
    /// </summary>
    public sealed class SphericalWorldModuleConfigurationWindow : EditorWindow
    {
        private const string ProfilesFolder = "Assets/Project/Data/WorldProfiles";
        private string profilePath = string.Empty;
        private SphericalWorldProfile profile;
        private string statusMessage = "Open a world profile to configure its modules.";

        /// <summary>
        /// Opens the world-module profile configuration window.
        /// </summary>
        [MenuItem("Tools/World/Configure World Modules")]
        public static void Open()
        {
            SphericalWorldModuleConfigurationWindow window =
                GetWindow<SphericalWorldModuleConfigurationWindow>("World Modules");
            window.minSize = new Vector2(360f, 320f);
            window.Show();
        }

        /// <summary>
        /// Draws profile selection, module options and save controls.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.LabelField("World creation modules", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These options are saved in the selected world profile and are fixed when that world is created.",
                MessageType.Info);
            DrawProfilePicker();
            if (profile == null) return;
            DrawModuleOptions();
            if (GUILayout.Button("Save module configuration", GUILayout.Height(28f)))
                SaveProfile();
            EditorGUILayout.HelpBox(statusMessage, MessageType.None);
        }

        /// <summary>
        /// Lets the owner choose an existing JSON world profile.
        /// </summary>
        private void DrawProfilePicker()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Profile", string.IsNullOrEmpty(profilePath) ? "(none)" : Path.GetFileName(profilePath));
            if (GUILayout.Button("Open", GUILayout.Width(64f)))
                LoadProfileFromPicker();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Opens a profile file from the project's world-profile directory.
        /// </summary>
        private void LoadProfileFromPicker()
        {
            string folder = Path.GetFullPath(ProfilesFolder);
            Directory.CreateDirectory(folder);
            string selectedPath = EditorUtility.OpenFilePanel("Select world profile", folder, "json");
            if (!string.IsNullOrEmpty(selectedPath))
                LoadProfile(selectedPath);
        }

        /// <summary>
        /// Loads and migrates the selected profile's module configuration.
        /// </summary>
        private void LoadProfile(string path)
        {
            try
            {
                SphericalWorldProfile loaded =
                    JsonUtility.FromJson<SphericalWorldProfile>(File.ReadAllText(path));
                if (loaded == null) throw new InvalidDataException("The profile contains no data.");
                if (loaded.schemaVersion != 1)
                    throw new InvalidDataException($"Unsupported world profile schema version: {loaded.schemaVersion}");
                loaded.NormalizeModuleConfiguration();
                profile = loaded;
                profilePath = path;
                statusMessage = $"Loaded {profile.displayName}.";
            }
            catch (Exception exception)
            {
                profile = null;
                statusMessage = exception.Message;
                Debug.LogError($"Could not load world module profile '{path}': {exception}");
            }
        }

        /// <summary>
        /// Draws independent toggles for each currently defined optional capability.
        /// </summary>
        private void DrawModuleOptions()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Optional capabilities", EditorStyles.boldLabel);
            profile.modules.terrainPresentationEnabled =
                EditorGUILayout.Toggle("3D terrain presentation", profile.modules.terrainPresentationEnabled);
            profile.modules.biomeClassificationEnabled =
                EditorGUILayout.Toggle("Biome classification", profile.modules.biomeClassificationEnabled);
            profile.modules.biomeGroundPresentationEnabled =
                EditorGUILayout.Toggle("Biome ground presentation", profile.modules.biomeGroundPresentationEnabled);
            profile.modules.environmentAssetsEnabled =
                EditorGUILayout.Toggle("Environment assets", profile.modules.environmentAssetsEnabled);
            profile.modules.playerEnabled =
                EditorGUILayout.Toggle("Spawn player", profile.modules.playerEnabled);
            profile.modules.pokemonEnabled =
                EditorGUILayout.Toggle("Pokémon module", profile.modules.pokemonEnabled);
        }

        /// <summary>
        /// Writes the selected module options back to the same profile file.
        /// </summary>
        private void SaveProfile()
        {
            try
            {
                profile.moduleConfigurationVersion = 1;
                File.WriteAllText(profilePath, JsonUtility.ToJson(profile, true));
                AssetDatabase.Refresh();
                statusMessage = $"Saved module configuration for {profile.displayName}.";
            }
            catch (Exception exception)
            {
                statusMessage = exception.Message;
                Debug.LogError($"Could not save world module profile '{profilePath}': {exception}");
            }
        }
    }
}
#endif
