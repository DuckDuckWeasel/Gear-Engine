using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class WebGlStartupTests
    {
        [Test]
        public void LoadingTemplate_PlaysMutedLoopingVideoUntilUnitySignalsReady()
        {
            string template = ReadProjectFile("Assets/WebGLTemplates/GearEngine/index.html");

            Assert.That(template, Does.Contain("autoplay muted loop playsinline"));
            Assert.That(template, Does.Contain("window.gearEngineLoadingReady = completeLoading"));
            Assert.That(template, Does.Contain("StreamingAssets/GearEngineLoadingLoop.mp4"));
        }

        [Test]
        public void SubmissionBuild_UsesHashedFilesAndDecompressionFallback()
        {
            string exporter = ReadProjectFile(
                "Assets/GearEngine/Scripts/Game/GearEngine/Editor/SubmissionBuildExporter.cs");

            Assert.That(exporter, Does.Contain("PlayerSettings.WebGL.nameFilesAsHashes = true"));
            Assert.That(exporter, Does.Contain("PlayerSettings.WebGL.decompressionFallback = true"));
            Assert.That(exporter, Does.Contain("PlayerSettings.WebGL.template = k_webGlTemplate"));
        }

        [Test]
        public void SubmissionBuild_BuildsOfflineAddressablesContent()
        {
            string exporter = ReadProjectFile(
                "Assets/GearEngine/Scripts/Game/GearEngine/Editor/SubmissionBuildExporter.cs");
            string settings = ReadProjectFile(
                "Assets/AddressableAssetsData/AddressableAssetSettings.asset");

            Assert.That(exporter, Does.Contain("BuildPlayerContent"));
            Assert.That(exporter, Does.Contain("BuildRemoteCatalog"));
            Assert.That(exporter, Does.Not.Contain("Directory.Delete(addressablesPath, true)"));
            Assert.That(settings, Does.Contain("m_BuildRemoteCatalog: 0"));
            Assert.That(settings, Does.Contain("m_DisableCatalogUpdateOnStart: 1"));
        }

        [Test]
        public void RuntimeBootstrap_InstallsAddressablesRuntime()
        {
            string foundationLayer = ReadProjectFile(
                "Assets/GearEngine/Scripts/App/Bootstrap/Layers/FoundationLayer.cs");
            string sceneFoundation = ReadProjectFile(
                "Assets/GearEngine/Scripts/Core/SceneFoundation/Bootstrap/SceneFoundationScope.cs");

            Assert.That(foundationLayer, Does.Contain("new AddressablesInstaller"));
            Assert.That(sceneFoundation, Does.Contain("new AddressablesInstaller"));
        }

        [Test]
        public void UgsLayer_InitializesAnalyticsAfterUnityServices()
        {
            string layer = ReadProjectFile("Assets/GearEngine/Scripts/App/Bootstrap/Layers/UgsLayer.cs");
            string initializer = ReadProjectFile(
                "Assets/GearEngine/Scripts/App/Bootstrap/Layers/UgsAnalyticsInitializer.cs");

            Assert.That(layer, Does.Not.Contain("new AnalyticsInstaller"));
            Assert.That(layer, Does.Contain("UgsAnalyticsInitializer"));
            Assert.That(initializer.IndexOf("await ugs.InitializeAsync", System.StringComparison.Ordinal),
                Is.LessThan(initializer.IndexOf("analyticsService.Initialize", System.StringComparison.Ordinal)));
        }

        private static string ReadProjectFile(string relativePath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return File.ReadAllText(Path.Combine(projectRoot, relativePath));
        }
    }
}
