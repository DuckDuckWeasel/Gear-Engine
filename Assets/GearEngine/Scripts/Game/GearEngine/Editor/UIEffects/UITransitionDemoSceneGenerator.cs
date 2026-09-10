using System.Collections.Generic;
using System.IO;
using Coffee.UIEffects;
using GearEngine.Presentation.UI.Effects;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Editor.UIEffects
{
    /// <summary>
    /// Generates the connected UI transition sample scenes.
    /// </summary>
    public static class UITransitionDemoSceneGenerator
    {
        private const string k_sceneFolder = "Assets/GearEngine/Scenes/Test/UITransitions";
        private const string k_galleryPath = k_sceneFolder + "/UITransitionsGallery.unity";
        private const string k_destinationPath = k_sceneFolder + "/UITransitionsDestination.unity";
        private const string k_presetFolder = "Assets/3rdParty/UIEffect/UIEffectPresets/";
        private const string k_visualArtifactFolder = "Artifacts/VisualTests/UITransitionDemos";

        private static readonly string[] s_presetNames =
        {
            "Transition-Fade.asset",
            "Transition-Burn.asset",
            "Transition-Dissolve.asset",
            "Transition (Pattern)-Square.asset",
            "Transition (Pattern)-Diamond.asset",
            "Transition (Pattern)-Stripe.asset",
            "Transition-Melt.asset",
            "Transition (Blaze)-Default.asset",
        };

        [MenuItem("GearEngine/Samples/Generate UI Transition Demo Scenes")]
        public static void Generate()
        {
            EnsureSceneFolder();
            List<UIEffectPreset> presets = LoadPresets();
            CreateScene(k_galleryPath, CreateGalleryTheme(), presets);
            CreateScene(k_destinationPath, CreateDestinationTheme(), presets);
            RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UITransitionDemoSceneGenerator] Generated both UI transition demo scenes.");
        }

        [MenuItem("GearEngine/Samples/Capture UI Transition Demo Evidence")]
        public static void CaptureVisualEvidence()
        {
            string outputFolder = Path.GetFullPath(k_visualArtifactFolder);
            Directory.CreateDirectory(outputFolder);
            CaptureScene(k_galleryPath, Path.Combine(outputFolder, "Gallery.png"), false);
            CaptureScene(k_galleryPath, Path.Combine(outputFolder, "SquareMidpoint.png"), true);
            CaptureScene(k_destinationPath, Path.Combine(outputFolder, "Destination.png"), false);
            Debug.Log($"[UITransitionDemoSceneGenerator] Captured demo evidence in '{outputFolder}'.");
        }

        private static void CaptureScene(
            string scenePath,
            string outputPath,
            bool showTransition)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            ConfigureCaptureTransition(showTransition);
            Canvas.ForceUpdateCanvases();
            Camera camera = Object.FindAnyObjectByType<Camera>();
            CaptureCamera(camera, outputPath);
        }

        private static void ConfigureCaptureTransition(bool showTransition)
        {
            GameObject surface = GameObject.Find("TransitionSurface");
            if (!showTransition)
            {
                surface.SetActive(false);
                return;
            }

            UIEffect effect = surface.GetComponent<UIEffect>();
            UIEffectPreset preset = AssetDatabase.LoadAssetAtPath<UIEffectPreset>(
                k_presetFolder + "Transition (Pattern)-Square.asset");
            effect.LoadPreset(preset, false);
            effect.transitionRate = 0.5f;
        }

        private static void CaptureCamera(Camera camera, string outputPath)
        {
            RenderTexture target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            Texture2D image = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = target;
            target.Create();
            camera.Render();
            ReadAndWriteCapture(target, image, outputPath);
            RestoreCapture(camera, target, image, previous);
        }

        private static void ReadAndWriteCapture(
            RenderTexture target,
            Texture2D image,
            string outputPath)
        {
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0f, 0f, target.width, target.height), 0, 0);
            image.Apply();
            File.WriteAllBytes(outputPath, image.EncodeToPNG());
        }

        private static void RestoreCapture(
            Camera camera,
            RenderTexture target,
            Texture2D image,
            RenderTexture previous)
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            target.Release();
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(image);
        }

        private static void CreateScene(
            string path,
            SceneTheme theme,
            List<UIEffectPreset> presets)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera(theme.BackgroundColor);
            Canvas canvas = CreateCanvas(camera);
            CreateBackground(canvas.transform, theme);
            CreateHeader(canvas.transform, theme);
            CreatePreviewContent(canvas.transform, theme);
            DemoControls controls = CreateControls(canvas.transform, theme);
            TransitionSurface surface = CreateTransitionSurface(canvas.transform, theme);
            CreateAndConfigureController(theme, presets, controls, surface);
            CreateEventSystem();
            EditorSceneManager.SaveScene(scene, path);
        }

        private static Camera CreateCamera(Color backgroundColor)
        {
            GameObject gameObject = new GameObject("Camera", typeof(Camera));
            Camera camera = gameObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backgroundColor;
            camera.orthographic = true;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            return camera;
        }

        private static Canvas CreateCanvas(Camera camera)
        {
            GameObject gameObject = new GameObject(
                "UITransitionDemoCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            ConfigureCanvasScaler(gameObject.GetComponent<CanvasScaler>());
            return canvas;
        }

        private static void ConfigureCanvasScaler(CanvasScaler scaler)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static void CreateBackground(Transform parent, SceneTheme theme)
        {
            CreateImage(parent, "Background", RectSpec.Full, theme.BackgroundColor);
            CreateImage(
                parent,
                "TopAccent",
                new RectSpec(0f, 0.985f, 1f, 1f),
                theme.AccentColor);
            CreateImage(
                parent,
                "BottomAccent",
                new RectSpec(0f, 0f, 1f, 0.012f),
                theme.SecondaryAccentColor);
        }

        private static void CreateHeader(Transform parent, SceneTheme theme)
        {
            CreateText(
                parent,
                "Kicker",
                theme.Kicker,
                new RectSpec(0.08f, 0.88f, 0.92f, 0.94f),
                28,
                theme.AccentColor,
                TextAnchor.MiddleLeft);
            CreateText(
                parent,
                "Title",
                theme.Title,
                new RectSpec(0.08f, 0.77f, 0.92f, 0.89f),
                62,
                Color.white,
                TextAnchor.MiddleLeft);
        }

        private static void CreatePreviewContent(Transform parent, SceneTheme theme)
        {
            Image card = CreateImage(
                parent,
                "ContentCard",
                new RectSpec(0.08f, 0.27f, 0.56f, 0.72f),
                theme.CardColor);
            AddOutline(card.gameObject, theme.AccentColor);
            CreateCardCopy(card.transform, theme);
            CreateFlagPreview(parent, theme);
        }

        private static void CreateCardCopy(Transform parent, SceneTheme theme)
        {
            CreateText(
                parent,
                "SceneName",
                "CURRENT SAMPLE",
                new RectSpec(0.08f, 0.72f, 0.92f, 0.88f),
                22,
                theme.AccentColor,
                TextAnchor.MiddleLeft);
            CreateText(
                parent,
                "Description",
                theme.Description,
                new RectSpec(0.08f, 0.25f, 0.92f, 0.7f),
                34,
                Color.white,
                TextAnchor.MiddleLeft);
        }

        private static void CreateFlagPreview(Transform parent, SceneTheme theme)
        {
            Image panel = CreateImage(
                parent,
                "RaceFlagPreview",
                new RectSpec(0.59f, 0.27f, 0.92f, 0.72f),
                new Color(0f, 0f, 0f, 0.18f));
            AddOutline(panel.gameObject, theme.SecondaryAccentColor);
            CreateFlagGrid(panel.transform, theme);
        }

        private static void CreateFlagGrid(Transform parent, SceneTheme theme)
        {
            const int columns = 8;
            const int rows = 5;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    CreateFlagCell(parent, theme, column, row, columns, rows);
                }
            }
        }

        private static void CreateFlagCell(
            Transform parent,
            SceneTheme theme,
            int column,
            int row,
            int columns,
            int rows)
        {
            float xMin = column / (float)columns;
            float yMin = row / (float)rows;
            RectSpec rect = new RectSpec(xMin, yMin, xMin + 1f / columns, yMin + 1f / rows);
            Color color = (column + row) % 2 == 0 ? theme.AccentColor : theme.SecondaryAccentColor;
            color.a = 0.82f;
            CreateImage(parent, $"Quad_{row}_{column}", rect, color);
        }

        private static DemoControls CreateControls(Transform parent, SceneTheme theme)
        {
            DemoControls controls = new DemoControls();
            controls.Previous = CreateButton(parent, "PreviousButton", "PREVIOUS", 0.08f, 0.25f, theme);
            controls.Replay = CreateButton(parent, "ReplayButton", "REPLAY TRANSITION", 0.27f, 0.51f, theme);
            controls.Next = CreateButton(parent, "NextButton", "NEXT", 0.53f, 0.7f, theme);
            controls.Destination = CreateButton(parent, "DestinationButton", theme.DestinationLabel, 0.72f, 0.92f, theme);
            controls.PresetLabel = CreatePresetLabel(parent, theme);
            controls.SceneLabel = CreateSceneLabel(parent, theme);
            return controls;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string label,
            float xMin,
            float xMax,
            SceneTheme theme)
        {
            Image image = CreateImage(
                parent,
                name,
                new RectSpec(xMin, 0.08f, xMax, 0.18f),
                theme.ButtonColor);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ConfigureButtonColors(button, theme);
            AddOutline(image.gameObject, theme.AccentColor);
            CreateText(image.transform, "Label", label, RectSpec.Full, 24, Color.white, TextAnchor.MiddleCenter);
            return button;
        }

        private static void ConfigureButtonColors(Button button, SceneTheme theme)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = theme.ButtonColor;
            colors.highlightedColor = theme.AccentColor;
            colors.pressedColor = theme.SecondaryAccentColor;
            colors.selectedColor = theme.AccentColor;
            colors.disabledColor = new Color(0.2f, 0.2f, 0.25f, 0.6f);
            button.colors = colors;
        }

        private static Text CreatePresetLabel(Transform parent, SceneTheme theme)
        {
            return CreateText(
                parent,
                "PresetLabel",
                "Transition-Fade",
                new RectSpec(0.08f, 0.205f, 0.66f, 0.255f),
                25,
                theme.SecondaryAccentColor,
                TextAnchor.MiddleLeft);
        }

        private static Text CreateSceneLabel(Transform parent, SceneTheme theme)
        {
            return CreateText(
                parent,
                "ActiveSceneLabel",
                theme.SceneName,
                new RectSpec(0.67f, 0.205f, 0.92f, 0.255f),
                21,
                new Color(1f, 1f, 1f, 0.55f),
                TextAnchor.MiddleRight);
        }

        private static TransitionSurface CreateTransitionSurface(Transform parent, SceneTheme theme)
        {
            Image image = CreateImage(parent, "TransitionSurface", RectSpec.Full, theme.CoverColor);
            image.raycastTarget = false;
            UIEffect effect = image.gameObject.AddComponent<UIEffect>();
            UIEffectTweener tweener = image.gameObject.AddComponent<UIEffectTweener>();
            image.transform.SetAsLastSibling();
            return new TransitionSurface(effect, tweener);
        }

        private static void CreateAndConfigureController(
            SceneTheme theme,
            List<UIEffectPreset> presets,
            DemoControls controls,
            TransitionSurface surface)
        {
            GameObject gameObject = new GameObject("UITransitionDemoController");
            UITransitionDemoController controller = gameObject.AddComponent<UITransitionDemoController>();
            ConfigureControllerProperties(controller, theme, presets, controls, surface);
            WireButtons(controller, controls);
        }

        private static void ConfigureControllerProperties(
            UITransitionDemoController controller,
            SceneTheme theme,
            List<UIEffectPreset> presets,
            DemoControls controls,
            TransitionSurface surface)
        {
            SerializedObject serializedController = new SerializedObject(controller);
            SetObject(serializedController, "transitionEffect", surface.Effect);
            SetObject(serializedController, "transitionTweener", surface.Tweener);
            SetObject(serializedController, "presetLabel", controls.PresetLabel);
            SetObject(serializedController, "sceneLabel", controls.SceneLabel);
            SetObjectList(serializedController, "controls", controls.All);
            SetObjectList(serializedController, "presets", presets);
            serializedController.FindProperty("destinationSceneName").stringValue = theme.DestinationSceneName;
            serializedController.FindProperty("revealOnStart").boolValue = theme.RevealOnStart;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireButtons(
            UITransitionDemoController controller,
            DemoControls controls)
        {
            UnityEventTools.AddPersistentListener(controls.Previous.onClick, controller.PreviousPreset);
            UnityEventTools.AddPersistentListener(controls.Replay.onClick, controller.PlayPreview);
            UnityEventTools.AddPersistentListener(controls.Next.onClick, controller.NextPreset);
            UnityEventTools.AddPersistentListener(controls.Destination.onClick, controller.LoadDestination);
        }

        private static Image CreateImage(
            Transform parent,
            string name,
            RectSpec rect,
            Color color)
        {
            GameObject gameObject = CreateRectObject(parent, name, rect);
            Image image = gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            string value,
            RectSpec rect,
            int fontSize,
            Color color,
            TextAnchor alignment)
        {
            GameObject gameObject = CreateRectObject(parent, name, rect);
            Text text = gameObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static GameObject CreateRectObject(
            Transform parent,
            string name,
            RectSpec rect)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform transform = gameObject.GetComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.anchorMin = new Vector2(rect.XMin, rect.YMin);
            transform.anchorMax = new Vector2(rect.XMax, rect.YMax);
            transform.offsetMin = Vector2.zero;
            transform.offsetMax = Vector2.zero;
            return gameObject;
        }

        private static void AddOutline(GameObject target, Color color)
        {
            Outline outline = target.AddComponent<Outline>();
            color.a = 0.75f;
            outline.effectColor = color;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;
        }

        private static void CreateEventSystem()
        {
            GameObject gameObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            gameObject.GetComponent<InputSystemUIInputModule>();
        }

        private static List<UIEffectPreset> LoadPresets()
        {
            List<UIEffectPreset> presets = new List<UIEffectPreset>();
            foreach (string presetName in s_presetNames)
            {
                UIEffectPreset preset = AssetDatabase.LoadAssetAtPath<UIEffectPreset>(k_presetFolder + presetName);
                if (preset == null)
                {
                    throw new MissingReferenceException($"Missing UIEffect preset '{presetName}'.");
                }

                presets.Add(preset);
            }

            return presets;
        }

        private static void SetObject(
            SerializedObject target,
            string propertyName,
            Object value)
        {
            target.FindProperty(propertyName).objectReferenceValue = value;
        }

        private static void SetObjectList<T>(
            SerializedObject target,
            string propertyName,
            IReadOnlyList<T> values)
            where T : Object
        {
            SerializedProperty property = target.FindProperty(propertyName);
            property.arraySize = values.Count;
            for (int index = 0; index < values.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }
        }

        private static void RegisterBuildScenes()
        {
            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            AddBuildSceneIfMissing(scenes, k_galleryPath);
            AddBuildSceneIfMissing(scenes, k_destinationPath);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddBuildSceneIfMissing(
            List<EditorBuildSettingsScene> scenes,
            string path)
        {
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.path == path)
                {
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        private static void EnsureSceneFolder()
        {
            if (!AssetDatabase.IsValidFolder(k_sceneFolder))
            {
                AssetDatabase.CreateFolder("Assets/GearEngine/Scenes/Test", "UITransitions");
            }
        }

        private static SceneTheme CreateGalleryTheme()
        {
            return new SceneTheme
            {
                SceneName = "UITransitionsGallery",
                Kicker = "COFFEE UIEFFECT / SAMPLE 01",
                Title = "UI TRANSITION LAB",
                Description = "Cycle the native transition presets, replay a full cover and reveal, then continue to the destination scene.",
                DestinationLabel = "OPEN DESTINATION",
                DestinationSceneName = "UITransitionsDestination",
                BackgroundColor = ParseColor("#0B1020"),
                CardColor = ParseColor("#171F36"),
                ButtonColor = ParseColor("#202C49"),
                AccentColor = ParseColor("#50E3C2"),
                SecondaryAccentColor = ParseColor("#FF4D8D"),
                CoverColor = ParseColor("#080B14"),
            };
        }

        private static SceneTheme CreateDestinationTheme()
        {
            return new SceneTheme
            {
                SceneName = "UITransitionsDestination",
                Kicker = "COFFEE UIEFFECT / SAMPLE 02",
                Title = "DESTINATION REVEALED",
                Description = "This scene starts fully covered and reveals itself with the same transition pipeline used by the gallery.",
                DestinationLabel = "RETURN TO GALLERY",
                DestinationSceneName = "UITransitionsGallery",
                RevealOnStart = true,
                BackgroundColor = ParseColor("#24102F"),
                CardColor = ParseColor("#351A48"),
                ButtonColor = ParseColor("#47255C"),
                AccentColor = ParseColor("#FFD166"),
                SecondaryAccentColor = ParseColor("#7B61FF"),
                CoverColor = ParseColor("#120818"),
            };
        }

        private static Color ParseColor(string html)
        {
            if (!ColorUtility.TryParseHtmlString(html, out Color color))
            {
                throw new System.ArgumentException($"Invalid HTML color '{html}'.", nameof(html));
            }

            return color;
        }

        private sealed class DemoControls
        {
            public Button Previous { get; set; }
            public Button Replay { get; set; }
            public Button Next { get; set; }
            public Button Destination { get; set; }
            public Text PresetLabel { get; set; }
            public Text SceneLabel { get; set; }

            public IReadOnlyList<Button> All
            {
                get
                {
                    return new[] { Previous, Replay, Next, Destination };
                }
            }
        }

        private readonly struct TransitionSurface
        {
            public TransitionSurface(UIEffect effect, UIEffectTweener tweener)
            {
                Effect = effect;
                Tweener = tweener;
            }

            public UIEffect Effect { get; }
            public UIEffectTweener Tweener { get; }
        }

        private readonly struct RectSpec
        {
            public static RectSpec Full
            {
                get
                {
                    return new RectSpec(0f, 0f, 1f, 1f);
                }
            }

            public RectSpec(float xMin, float yMin, float xMax, float yMax)
            {
                XMin = xMin;
                YMin = yMin;
                XMax = xMax;
                YMax = yMax;
            }

            public float XMin { get; }
            public float YMin { get; }
            public float XMax { get; }
            public float YMax { get; }
        }

        private sealed class SceneTheme
        {
            public string SceneName { get; set; }
            public string Kicker { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string DestinationLabel { get; set; }
            public string DestinationSceneName { get; set; }
            public bool RevealOnStart { get; set; }
            public Color BackgroundColor { get; set; }
            public Color CardColor { get; set; }
            public Color ButtonColor { get; set; }
            public Color AccentColor { get; set; }
            public Color SecondaryAccentColor { get; set; }
            public Color CoverColor { get; set; }
        }
    }
}
