using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Pokemon.Project.DataServices.Loading;

namespace Pokemon.Project.Editor.WorldGeneration
{
    /// <summary>
    /// Includes the canonical source JSON in player builds without placing it in
    /// Resources or maintaining a second editable copy.
    /// </summary>
    public sealed class WorldGenerationConfigBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private static string SourcePath
        {
            get
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                return Path.Combine(projectRoot, "Assets",
                    WorldGenerationConfigLoader.RelativeSourcePath.Replace('/', Path.DirectorySeparatorChar));
            }
        }

        private static string DestinationPath =>
            Path.Combine(Application.streamingAssetsPath, WorldGenerationConfigLoader.StreamingAssetsFileName);

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!File.Exists(SourcePath))
                throw new BuildFailedException("Missing canonical world-generation JSON: " + SourcePath);

            string directory = Path.GetDirectoryName(DestinationPath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.Copy(SourcePath, DestinationPath, true);
            AssetDatabase.Refresh();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (File.Exists(DestinationPath))
            {
                File.Delete(DestinationPath);
                AssetDatabase.Refresh();
            }
        }
    }
}
