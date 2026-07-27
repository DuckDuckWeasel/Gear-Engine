using System;
using System.IO;
using Coffee.UIEffects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    [Category("Visual")]
    public sealed class UIEffectPatternRenderingTests
    {
        private const int k_captureResolution = 128;
        private const string k_artifactDirectory = "Artifacts/VisualTests/UIEffectPatternLayers";
        private const string k_testName =
            "GearEngine.GearEngine.Tests.Editor.UIEffectPatternRenderingTests.OrderedAlphaOver_DisabledAndZeroOpacityLayersDoNotContribute";
        private const string k_sampledAlphaTestName =
            "GearEngine.GearEngine.Tests.Editor.UIEffectPatternRenderingTests.SampledTextureAlpha_ScalesLayerOpacity";

        private GameObject cameraObject;
        private GameObject canvasObject;
        private RenderTexture renderTexture;
        private Texture2D readback;
        private Texture2D patternTexture;
        private Texture2D overlayTexture;

        [TearDown]
        public void TearDown()
        {
            if (cameraObject != null)
            {
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }

            if (canvasObject != null)
            {
                UnityEngine.Object.DestroyImmediate(canvasObject);
            }

            if (renderTexture != null)
            {
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }

            if (readback != null)
            {
                UnityEngine.Object.DestroyImmediate(readback);
            }

            if (patternTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(patternTexture);
            }

            if (overlayTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(overlayTexture);
            }
        }

        [Test]
        public void OrderedAlphaOver_DisabledAndZeroOpacityLayersDoNotContribute()
        {
            Camera camera = CreateCameraAndCanvas(out UIEffect effect);
            patternTexture = CreateTexture(Color.white);

            effect.SetPatternLayer(0, CreateReplaceLayer(Color.red, 1, patternTexture));
            effect.SetPatternLayer(1, CreateReplaceLayer(Color.blue, 0.5f, patternTexture));

            PatternLayer disabledLayer = CreateReplaceLayer(Color.green, 1, patternTexture);
            disabledLayer.m_Enabled = false;
            effect.SetPatternLayer(2, disabledLayer);
            effect.SetPatternLayer(3, CreateReplaceLayer(Color.yellow, 0, patternTexture));

            Color result = RenderCenterPixel(camera);

            Assert.That(result.r, Is.EqualTo(0.5f).Within(0.12f));
            Assert.That(result.g, Is.EqualTo(0).Within(0.12f));
            Assert.That(result.b, Is.EqualTo(0.5f).Within(0.12f));
            Assert.That(result.a, Is.EqualTo(1).Within(0.05f));
            CaptureEvidence(
                k_testName,
                "OrderedAlphaOver.png",
                "A red base pattern layer is overlaid by half-opacity blue while disabled and zero-opacity layers are ignored.",
                new[]
                {
                    "Layer 1 composites over layer 0 in index order.",
                    "The center pixel is approximately 50% red and 50% blue.",
                    "Disabled and zero-opacity layers do not change the output.",
                });
        }

        [Test]
        public void SampledTextureAlpha_ScalesLayerOpacity()
        {
            Camera camera = CreateCameraAndCanvas(out UIEffect effect);
            patternTexture = CreateTexture(Color.white);
            overlayTexture = CreateTexture(new Color(1, 1, 1, 0.75f));

            effect.SetPatternLayer(0, CreateReplaceLayer(Color.red, 1, patternTexture));
            effect.SetPatternLayer(1, CreateReplaceLayer(Color.blue, 1, overlayTexture));

            Color result = RenderCenterPixel(camera);

            Assert.That(result.r, Is.EqualTo(0.25f).Within(0.12f));
            Assert.That(result.g, Is.EqualTo(0).Within(0.12f));
            Assert.That(result.b, Is.EqualTo(0.75f).Within(0.12f));
            Assert.That(result.a, Is.EqualTo(1).Within(0.05f));
            CaptureEvidence(
                k_sampledAlphaTestName,
                "SampledTextureAlpha.png",
                "A fully opaque blue layer uses a texture with 75% sampled alpha over a red base layer.",
                new[]
                {
                    "Sampled texture alpha contributes to the layer alpha.",
                    "The center pixel is approximately 25% red and 75% blue.",
                });
        }

        private Camera CreateCameraAndCanvas(out UIEffect effect)
        {
            renderTexture = new RenderTexture(
                k_captureResolution,
                k_captureResolution,
                24,
                RenderTextureFormat.ARGB32);
            renderTexture.Create();

            cameraObject = new GameObject("PatternLayerTestCamera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.orthographic = true;
            camera.orthographicSize = k_captureResolution / 2f;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.targetTexture = renderTexture;

            canvasObject = new GameObject("PatternLayerTestCanvas", typeof(RectTransform), typeof(Canvas));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;

            GameObject imageObject = new GameObject(
                "PatternLayerTestImage",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            imageObject.transform.SetParent(canvasObject.transform, false);
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(k_captureResolution, k_captureResolution);

            Image image = imageObject.GetComponent<Image>();
            image.color = Color.white;
            effect = imageObject.AddComponent<UIEffect>();
            effect.transitionFilter = TransitionFilter.Pattern;
            return camera;
        }

        private static PatternLayer CreateReplaceLayer(Color color, float opacity, Texture texture)
        {
            return new PatternLayer
            {
                m_Enabled = true,
                m_Texture = texture,
                m_Opacity = opacity,
                m_Rate = 0.5f,
                m_ColorFilter = ColorFilter.Replace,
                m_Color = color,
                m_Area = PatternArea.All,
            };
        }

        private Color RenderCenterPixel(Camera camera)
        {
            Canvas.ForceUpdateCanvases();
            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            try
            {
                readback = new Texture2D(k_captureResolution, k_captureResolution, TextureFormat.RGBA32, false);
                readback.ReadPixels(new Rect(0, 0, k_captureResolution, k_captureResolution), 0, 0);
                readback.Apply();
            }
            finally
            {
                RenderTexture.active = previous;
            }

            return readback.GetPixel(k_captureResolution / 2, k_captureResolution / 2);
        }

        private void CaptureEvidence(string testName, string artifactFileName, string scenario, string[] criteria)
        {
            string artifactDirectory = Path.Combine(GetProjectPath(), k_artifactDirectory);
            Directory.CreateDirectory(artifactDirectory);

            string artifactPath = Path.Combine(artifactDirectory, artifactFileName);
            File.WriteAllBytes(artifactPath, readback.EncodeToPNG());

            PatternLayerEvidence evidence = new PatternLayerEvidence
            {
                test = testName,
                artifact = artifactFileName,
                scenario = scenario,
                criteria = criteria,
            };
            File.WriteAllText(artifactPath + ".evidence.json", JsonUtility.ToJson(evidence, true));
        }

        private static string GetProjectPath()
        {
            return Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unable to resolve the Unity project directory.");
        }

        private static Texture2D CreateTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        [Serializable]
        private sealed class PatternLayerEvidence
        {
            public string test;
            public string artifact;
            public string scenario;
            public string[] criteria;
        }
    }
}
