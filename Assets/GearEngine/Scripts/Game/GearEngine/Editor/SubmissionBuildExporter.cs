using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GearEngine.GearEngine.Editor
{
    public static class SubmissionBuildExporter
    {
        private const string k_buildAddressablesMenuPath = "Tools/Gear Engine/Build Settings/Build Addressables With Submission";
        private const string k_buildAddressablesPreferenceKey = "GearEngine.SubmissionBuild.BuildAddressables";
        private const string k_buildPathEnvironmentVariable = "GEAR_ENGINE_BUILD_PATH";
        private const string k_mainScenePath = "Assets/GearEngine/Scenes/Main Scene.unity";
        private const string k_submissionProductName = "Gear Engine";
        private const string k_webGlTemplate = "PROJECT:GearEngine";

        [MenuItem("Tools/Gear Engine/Build WebGL Submission")]
        public static void BuildWebGl()
        {
            try
            {
                string buildPath = ResolveBuildPath();
                Directory.CreateDirectory(buildPath);
                if (ShouldBuildAddressables())
                {
                    BuildAddressableContent();
                }
                else
                {
                    Debug.Log("[SubmissionBuild] Addressables content build skipped by the editor toggle.");
                }

                BuildPlayerOptions options = new BuildPlayerOptions
                {
                    scenes = new[] { k_mainScenePath },
                    locationPathName = buildPath,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None,
                };

                BuildReport report = BuildWithSubmissionSettings(options);
                BuildSummary summary = report.summary;
                if (summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException($"WebGL build failed with result {summary.result} and {summary.totalErrors} errors.");
                }

                Debug.Log($"[SubmissionBuild] WebGL build completed at {buildPath}. Size: {summary.totalSize} bytes.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SubmissionBuild] Failed to create the WebGL submission build. {exception}");
                throw;
            }
        }

        [MenuItem(k_buildAddressablesMenuPath)]
        private static void ToggleBuildAddressables()
        {
            bool enabled = !ShouldBuildAddressables();
            EditorPrefs.SetBool(k_buildAddressablesPreferenceKey, enabled);
            Menu.SetChecked(k_buildAddressablesMenuPath, enabled);
        }

        [MenuItem(k_buildAddressablesMenuPath, true)]
        private static bool ValidateBuildAddressablesToggle()
        {
            Menu.SetChecked(k_buildAddressablesMenuPath, ShouldBuildAddressables());
            return true;
        }

        private static void BuildAddressableContent()
        {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                throw new InvalidOperationException($"Addressables build failed: {result.Error}");
            }

            Debug.Log($"[SubmissionBuild] Addressables content built with {result.LocationCount} locations in {result.Duration:F2} seconds.");
        }

        private static BuildReport BuildWithSubmissionSettings(BuildPlayerOptions options)
        {
            string previousProductName = PlayerSettings.productName;
            WebGLCompressionFormat previousCompressionFormat = PlayerSettings.WebGL.compressionFormat;
            bool previousDecompressionFallback = PlayerSettings.WebGL.decompressionFallback;
            bool previousDataCaching = PlayerSettings.WebGL.dataCaching;
            bool previousNameFilesAsHashes = PlayerSettings.WebGL.nameFilesAsHashes;
            string previousTemplate = PlayerSettings.WebGL.template;
            try
            {
                PlayerSettings.productName = k_submissionProductName;
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.decompressionFallback = true;
                PlayerSettings.WebGL.dataCaching = true;
                PlayerSettings.WebGL.nameFilesAsHashes = true;
                PlayerSettings.WebGL.template = k_webGlTemplate;
                return BuildPipeline.BuildPlayer(options);
            }
            finally
            {
                PlayerSettings.productName = previousProductName;
                PlayerSettings.WebGL.compressionFormat = previousCompressionFormat;
                PlayerSettings.WebGL.decompressionFallback = previousDecompressionFallback;
                PlayerSettings.WebGL.dataCaching = previousDataCaching;
                PlayerSettings.WebGL.nameFilesAsHashes = previousNameFilesAsHashes;
                PlayerSettings.WebGL.template = previousTemplate;
            }
        }

        private static bool ShouldBuildAddressables()
        {
            return EditorPrefs.GetBool(k_buildAddressablesPreferenceKey, true);
        }

        private static string ResolveBuildPath()
        {
            string configuredPath = Environment.GetEnvironmentVariable(k_buildPathEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return Path.GetFullPath(configuredPath);
            }

            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Artifacts", "Submission", "Build", "GearEngineWebGL"));
        }
    }
}
