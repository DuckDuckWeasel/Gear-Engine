using System.Reflection;
using GearEngine.App.Bootstrap.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class BootstrapLoadingManagerTests
    {
        [Test]
        public void ShowFallback_DisplaysConfiguredTexture_WhenVideoIsUnavailable()
        {
            GameObject root = CreateInactiveLoadingRoot(out RawImage loadingVisual, out BootstrapLoadingManager manager);
            var fallbackTexture = new Texture2D(2, 2);

            try
            {
                SetPrivateField(manager, "loadingVisual", loadingVisual);
                SetPrivateField(manager, "fallbackTexture", fallbackTexture);

                InvokePrivateMethod(manager, "ShowFallback");

                Assert.That(loadingVisual.texture, Is.SameAs(fallbackTexture));
                Assert.That(GetPrivateField<bool>(manager, "hasVideoFrame"), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(fallbackTexture);
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ConfigureLoadingVideo_UsesMutedLoopingUrlPlayback()
        {
            GameObject root = CreateInactiveLoadingRoot(out _, out BootstrapLoadingManager manager);
            VideoPlayer videoPlayer = root.AddComponent<VideoPlayer>();

            try
            {
                SetPrivateField(manager, "loadingVideoPlayer", videoPlayer);
                SetPrivateField(manager, "loadingVideoFileName", "GearEngineLoadingLoop.mp4");

                InvokePrivateMethod(manager, "ConfigureLoadingVideo");

                Assert.That(videoPlayer.source, Is.EqualTo(VideoSource.Url));
                Assert.That(videoPlayer.url, Does.EndWith("/GearEngineLoadingLoop.mp4"));
                Assert.That(videoPlayer.audioOutputMode, Is.EqualTo(VideoAudioOutputMode.None));
                Assert.That(videoPlayer.isLooping, Is.True);
                Assert.That(videoPlayer.playOnAwake, Is.False);
                Assert.That(videoPlayer.waitForFirstFrame, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateInactiveLoadingRoot(
            out RawImage loadingVisual,
            out BootstrapLoadingManager manager)
        {
            var root = new GameObject("LoadingRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            root.SetActive(false);
            loadingVisual = root.GetComponent<RawImage>();
            manager = root.AddComponent<BootstrapLoadingManager>();
            return root;
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(target, null);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (T)field.GetValue(target);
        }
    }
}
