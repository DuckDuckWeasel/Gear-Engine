using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GearEngine.GearEngine.Editor
{
    public static class SubmissionBuildExporter
    {
        private const string k_buildPathEnvironmentVariable = "GEAR_ENGINE_BUILD_PATH";
        private const string k_mainScenePath = "Assets/GearEngine/Scenes/Main Scene.unity";
        private const string k_submissionProductName = "Gear Engine";
        private const string k_webGlTemplate = "PROJECT:GearEngine";
        private const int k_webGlInitialMemoryMb = 256;
        private const int k_webGlMaximumMemoryMb = 512;

        [MenuItem("Tools/Gear Engine/Build WebGL Submission")]
        public static void BuildWebGl()
        {
            try
            {
                string buildPath = ResolveBuildPath();
                Directory.CreateDirectory(buildPath);
                BuildOfflineAddressableContent();

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

        private static void BuildOfflineAddressableContent()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                throw new InvalidOperationException("Addressables settings are missing.");
            }

            if (settings.BuildRemoteCatalog)
            {
                throw new InvalidOperationException(
                    "WebGL submission builds require a local Addressables catalog. Disable Build Remote Catalog.");
            }

            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                throw new InvalidOperationException($"Addressables build failed: {result.Error}");
            }

            Debug.Log(
                $"[SubmissionBuild] Local Addressables content built with {result.LocationCount} locations in {result.Duration:F2} seconds.");
        }

        private static BuildReport BuildWithSubmissionSettings(BuildPlayerOptions options)
        {
            string previousProductName = PlayerSettings.productName;
            WebGLCompressionFormat previousCompressionFormat = PlayerSettings.WebGL.compressionFormat;
            bool previousDecompressionFallback = PlayerSettings.WebGL.decompressionFallback;
            bool previousDataCaching = PlayerSettings.WebGL.dataCaching;
            bool previousNameFilesAsHashes = PlayerSettings.WebGL.nameFilesAsHashes;
            int previousInitialMemorySize = PlayerSettings.WebGL.initialMemorySize;
            int previousMaximumMemorySize = PlayerSettings.WebGL.maximumMemorySize;
            string previousTemplate = PlayerSettings.WebGL.template;
            try
            {
                PlayerSettings.productName = k_submissionProductName;
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.decompressionFallback = true;
                PlayerSettings.WebGL.dataCaching = true;
                PlayerSettings.WebGL.nameFilesAsHashes = true;
                PlayerSettings.WebGL.initialMemorySize = k_webGlInitialMemoryMb;
                PlayerSettings.WebGL.maximumMemorySize = k_webGlMaximumMemoryMb;
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
                PlayerSettings.WebGL.initialMemorySize = previousInitialMemorySize;
                PlayerSettings.WebGL.maximumMemorySize = previousMaximumMemorySize;
                PlayerSettings.WebGL.template = previousTemplate;
            }
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
