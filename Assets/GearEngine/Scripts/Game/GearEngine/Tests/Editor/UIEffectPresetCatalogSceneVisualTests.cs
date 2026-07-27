using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Coffee.UIEffects;
using GearEngine.GearEngine.Presentation.UI.Input;
using NUnit.Framework;
using Scaffold;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    [Category("Visual")]
    public sealed class UIEffectPresetCatalogSceneVisualTests
    {
        private const string k_scenePath = "Assets/GearEngine/Scenes/Test/UIEffectsForEachDemo.unity";
        private const string k_buttonName = "CycleUIEffectButton";
        private const string k_labelName = "EffectDescription";
        private const string k_artifactDirectory = "Artifacts/VisualTests/UIEffectPresetCatalog";
        private const string k_artifactFileName = "AllPresetsContactSheet.png";
        private const int k_thumbnailWidth = 256;
        private const int k_thumbnailHeight = 144;
        private const int k_columns = 8;
        private const string k_testName =
            "GearEngine.GearEngine.Tests.Editor.UIEffectPresetCatalogSceneVisualTests.CatalogScene_ButtonClick_AppliesAndRendersEveryPreset";

        private SceneSetup[] previousSceneSetup;
        private string previousActiveScenePath;
        private Camera camera;
        private Canvas canvas;
        private RenderTexture originalTargetTexture;
        private RenderMode originalRenderMode;
        private Camera originalWorldCamera;
        private float originalPlaneDistance;
        private RenderTexture thumbnailTarget;
        private Texture2D thumbnail;
        private Texture2D contactSheet;

        [TearDown]
        public void TearDown()
        {
            RestoreCanvasAndCamera();

            if (thumbnailTarget != null)
            {
                thumbnailTarget.Release();
                UnityEngine.Object.DestroyImmediate(thumbnailTarget);
            }

            if (thumbnail != null)
            {
                UnityEngine.Object.DestroyImmediate(thumbnail);
            }

            if (contactSheet != null)
            {
                UnityEngine.Object.DestroyImmediate(contactSheet);
            }

            if (previousSceneSetup != null && !string.IsNullOrEmpty(previousActiveScenePath))
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSceneSetup);
            }
        }

        [Test]
        public void CatalogScene_ButtonClick_AppliesAndRendersEveryPreset()
        {
            previousSceneSetup = EditorSceneManager.GetSceneManagerSetup();
            previousActiveScenePath = SceneManager.GetActiveScene().path;
            Scene scene = EditorSceneManager.OpenScene(k_scenePath, OpenSceneMode.Single);
            Assert.That(scene.isLoaded, Is.True, $"Unable to load '{k_scenePath}'.");

            Button button = FindRequiredComponent<Button>(k_buttonName);
            Text label = FindRequiredComponent<Text>(k_labelName);
            ButtonClicked buttonClicked = UnityEngine.Object.FindAnyObjectByType<ButtonClicked>();
            InvokeActionCommand command = UnityEngine.Object.FindAnyObjectByType<InvokeActionCommand>();
            Assert.That(buttonClicked, Is.Not.Null);
            Assert.That(command, Is.Not.Null);

            CycleUIEffectPreset cycleAction = command.actions.OfType<CycleUIEffectPreset>().Single();
            List<UIEffectPreset> presets = GetPresets(cycleAction);
            Assert.That(presets, Is.Not.Empty);
            Assert.That(presets.All(preset => preset != null), Is.True);

            ConfigureCameraCapture();
            buttonClicked.Start();

            int rows = Mathf.CeilToInt(presets.Count / (float)k_columns);
            contactSheet = new Texture2D(
                k_columns * k_thumbnailWidth,
                rows * k_thumbnailHeight,
                TextureFormat.RGBA32,
                false);

            for (int index = 0; index < presets.Count; index++)
            {
                button.onClick.Invoke();
                Canvas.ForceUpdateCanvases();
                CaptureThumbnail();

                UIEffectPreset preset = presets[index];
                Assert.That(label.text, Does.StartWith(preset.name), $"Preset {index} was not applied by the click path.");
                CopyThumbnailToContactSheet(index, rows);
            }

            contactSheet.Apply();
            Assert.That(CountVisiblePixels(contactSheet), Is.GreaterThan(0));
            WriteEvidence(presets.Count, rows);
        }

        private static T FindRequiredComponent<T>(string objectName) where T : Component
        {
            GameObject target = GameObject.Find(objectName);
            Assert.That(target, Is.Not.Null, $"Scene object '{objectName}' was not found.");
            T component = target.GetComponent<T>();
            Assert.That(component, Is.Not.Null, $"Scene object '{objectName}' is missing {typeof(T).Name}.");
            return component;
        }

        private static List<UIEffectPreset> GetPresets(CycleUIEffectPreset cycleAction)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo field = typeof(CycleUIEffectPreset).GetField("presets", flags);
            Assert.That(field, Is.Not.Null);
            return field.GetValue(cycleAction) as List<UIEffectPreset>;
        }

        private void ConfigureCameraCapture()
        {
            camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            Assert.That(camera, Is.Not.Null);
            Assert.That(canvas, Is.Not.Null);

            originalTargetTexture = camera.targetTexture;
            originalRenderMode = canvas.renderMode;
            originalWorldCamera = canvas.worldCamera;
            originalPlaneDistance = canvas.planeDistance;

            thumbnailTarget = new RenderTexture(k_thumbnailWidth, k_thumbnailHeight, 24, RenderTextureFormat.ARGB32);
            thumbnailTarget.Create();
            thumbnail = new Texture2D(k_thumbnailWidth, k_thumbnailHeight, TextureFormat.RGBA32, false);

            camera.targetTexture = thumbnailTarget;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
        }

        private void CaptureThumbnail()
        {
            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = thumbnailTarget;
            try
            {
                thumbnail.ReadPixels(new Rect(0, 0, k_thumbnailWidth, k_thumbnailHeight), 0, 0);
                thumbnail.Apply();
            }
            finally
            {
                RenderTexture.active = previous;
            }
        }

        private void CopyThumbnailToContactSheet(int index, int rows)
        {
            int column = index % k_columns;
            int row = rows - 1 - index / k_columns;
            contactSheet.SetPixels(
                column * k_thumbnailWidth,
                row * k_thumbnailHeight,
                k_thumbnailWidth,
                k_thumbnailHeight,
                thumbnail.GetPixels());
        }

        private void WriteEvidence(int presetCount, int rows)
        {
            string outputDirectory = Path.Combine(GetProjectPath(), k_artifactDirectory);
            Directory.CreateDirectory(outputDirectory);

            string artifactPath = Path.Combine(outputDirectory, k_artifactFileName);
            File.WriteAllBytes(artifactPath, contactSheet.EncodeToPNG());

            ContactSheetEvidence evidence = new ContactSheetEvidence
            {
                test = k_testName,
                artifact = k_artifactFileName,
                scenario = "Each configured UIEffect preset is applied by the scene Blackboard's button-click path and captured in one contact sheet.",
                criteria = new[]
                {
                    $"All {presetCount} configured presets are reached through Button.onClick.",
                    "Each thumbnail contains the rendered scene after its corresponding preset was applied.",
                    $"The contact sheet uses {k_columns} columns and {rows} rows.",
                },
            };
            File.WriteAllText(artifactPath + ".evidence.json", JsonUtility.ToJson(evidence, true));
        }

        private void RestoreCanvasAndCamera()
        {
            if (camera != null)
            {
                camera.targetTexture = originalTargetTexture;
            }

            if (canvas != null)
            {
                canvas.renderMode = originalRenderMode;
                canvas.worldCamera = originalWorldCamera;
                canvas.planeDistance = originalPlaneDistance;
            }
        }

        private static int CountVisiblePixels(Texture2D texture)
        {
            return texture.GetPixels().Count(color => color.a > 0.01f && color.maxColorComponent > 0.01f);
        }

        private static string GetProjectPath()
        {
            return Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unable to resolve the Unity project directory.");
        }

        [Serializable]
        private sealed class ContactSheetEvidence
        {
            public string test;
            public string artifact;
            public string scenario;
            public string[] criteria;
        }
    }
}
