using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BirdGame.Editor
{
    public static class BirdGameBuildAutomation
    {
        public static void BuildWindowsAndRun()
        {
            BirdGameSceneGenerator.GenerateMvpScenes();

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var buildDirectory = Path.Combine(projectRoot, "Builds");
            Directory.CreateDirectory(buildDirectory);

            var outputPath = Path.Combine(buildDirectory, "BirdGame.exe");
            var scenes = new[]
            {
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scenes/Game.unity"
            };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Build failed: {report.summary.result}");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"Build succeeded at: {outputPath}");
            EditorApplication.Exit(0);
        }
    }
}
