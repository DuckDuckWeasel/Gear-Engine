using System.IO;
using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class BoardCapacityChipVisualTests
    {
        private const string k_artifactDirectory =
            "Artifacts/VisualTests/BoardCapacityChip";
        private const string k_setupPrefabPath =
            "Assets/GearEngine/Prefabs/Campaign/Setup View.prefab";

        [TestCase("Baseline", 1080, 1920, 80, 80)]
        [TestCase("Tall", 1080, 2400, 100, 60)]
        [TestCase("Short", 1080, 1680, 40, 40)]
        public void SetupHeader_PortraitResolution_RendersCapacityInsideSafeArea(
            string scenario,
            int width,
            int height,
            int topInset,
            int bottomInset)
        {
            GameObject cameraObject = null;
            GameObject setupInstance = null;
            RenderTexture renderTexture = null;
            Texture2D capture = null;
            try
            {
                Camera camera = CreateCamera(out cameraObject);
                renderTexture = new RenderTexture(
                    width,
                    height,
                    24,
                    RenderTextureFormat.ARGB32);
                camera.targetTexture = renderTexture;
                setupInstance = CreateSetupInstance(camera);
                BoardCapacityChipView chip =
                    setupInstance.GetComponent<BoardCapacityChipView>();
                Assert.IsNotNull(chip);
                Assert.IsNotNull(chip.CapacityLabel);
                chip.CapacityLabel.text = "6/6";

                string artifactPath = Capture(
                    scenario,
                    camera,
                    renderTexture,
                    width,
                    height,
                    out capture);
                WriteEvidence(artifactPath, scenario, width, height);

                Assert.That(chip.CapacityLabel.text, Is.EqualTo("6/6"));
                AssertRectInsideSafeArea(
                    chip.CapacityLabel.transform.parent as RectTransform,
                    camera,
                    width,
                    height,
                    topInset,
                    bottomInset);
                Assert.That(new FileInfo(artifactPath).Length, Is.GreaterThan(10_000));
            }
            finally
            {
                RenderTexture.active = null;
                if (cameraObject != null)
                {
                    cameraObject.GetComponent<Camera>().targetTexture = null;
                }

                if (renderTexture != null)
                {
                    renderTexture.Release();
                    Object.DestroyImmediate(renderTexture);
                }

                Object.DestroyImmediate(capture);
                Object.DestroyImmediate(setupInstance);
                Object.DestroyImmediate(cameraObject);
            }
        }

        private static Camera CreateCamera(out GameObject cameraObject)
        {
            cameraObject = new GameObject("CapacityCaptureCamera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.984f, 0.969f, 0.941f, 1f);
            camera.orthographic = true;
            return camera;
        }

        private static GameObject CreateSetupInstance(Camera camera)
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(k_setupPrefabPath);
            Assert.IsNotNull(prefab);
            GameObject instance = Object.Instantiate(prefab);
            instance.SetActive(true);

            RectTransform root = instance.transform as RectTransform;
            root.localScale = Vector3.one;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            Canvas canvas = instance.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            CanvasScaler scaler = instance.GetComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
            return instance;
        }

        private static string Capture(
            string scenario,
            Camera camera,
            RenderTexture renderTexture,
            int width,
            int height,
            out Texture2D capture)
        {
            string directory = Path.GetFullPath(k_artifactDirectory);
            Directory.CreateDirectory(directory);
            string artifactPath = Path.Combine(directory, $"{scenario}.png");
            capture = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                mipChain: false);
            Canvas.ForceUpdateCanvases();
            camera.Render();

            RenderTexture.active = renderTexture;
            capture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            capture.Apply();
            File.WriteAllBytes(artifactPath, capture.EncodeToPNG());
            return artifactPath;
        }

        private static void AssertRectInsideSafeArea(
            RectTransform rect,
            Camera camera,
            int width,
            int height,
            int topInset,
            int bottomInset)
        {
            Assert.IsNotNull(rect);
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 screenPoint = camera.WorldToScreenPoint(corners[i]);
                Assert.That(screenPoint.x, Is.InRange(0f, (float)width));
                Assert.That(
                    screenPoint.y,
                    Is.InRange((float)bottomInset, height - (float)topInset));
            }
        }

        private static void WriteEvidence(
            string artifactPath,
            string scenario,
            int width,
            int height)
        {
            string evidence =
                "{\n" +
                "  \"test\": \"GearEngine.GearEngine.Tests.Editor." +
                "BoardCapacityChipVisualTests." +
                "SetupHeader_PortraitResolution_RendersCapacityInsideSafeArea\",\n" +
                $"  \"artifact\": \"{Path.GetFileName(artifactPath)}\",\n" +
                $"  \"scenario\": \"{scenario} Setup header at {width}x{height}\",\n" +
                "  \"criteria\": \"The Setup cog chip displays 6/6 and remains inside " +
                "the configured portrait Safe Area.\"\n" +
                "}\n";
            File.WriteAllText(artifactPath + ".evidence.json", evidence);
        }
    }
}
