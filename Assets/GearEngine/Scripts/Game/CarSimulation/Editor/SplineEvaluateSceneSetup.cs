using System.IO;
using GearEngine.CarSimulation.SplineSimulation;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

namespace GearEngine.CarSimulation.Editor
{
    /// <summary>
    /// Editor menu tool that populates the currently open scene with all the
    /// GameObjects and ScriptableObject assets required for a working
    /// spline-evaluate test scene. Idempotent — can be re-run safely.
    /// <para>
    /// Menu: <b>GearEngine / Spline Evaluate / Setup Test Scene</b>
    /// </para>
    /// </summary>
    public static class SplineEvaluateSceneSetup
    {
        private const string AssetFolder = "Assets/GearEngine/Scripts/Game/SplineEvaluate/Data";
        private const string ConfigAssetName = "SplineDriverConfig.asset";
        private const string LaneProfileAssetName = "DefaultLaneProfile.asset";

        [MenuItem("GearEngine/Spline Evaluate/Setup Test Scene", false, 100)]
        public static void SetupScene()
        {
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Spline Evaluate Scene Setup");

            // ── 1. Ensure data assets exist ─────────────────────────────
            SplineDriverConfig config = FindOrCreateAsset<SplineDriverConfig>(ConfigAssetName);
            LaneProfile laneProfile = FindOrCreateAsset<LaneProfile>(LaneProfileAssetName);

            // ── 2. Track (SplineContainer with oval track) ──────────────
            SplineContainer splineContainer = FindOrCreateTrack();

            // ── 3. Scope (LifetimeScope) ────────────────────────────────
            SplineEvaluateScope scope = FindOrCreateScope(config, laneProfile);

            // ── 4. Bootstrap ────────────────────────────────────────────
            SplineEvaluateBootstrap bootstrap = FindOrCreateBootstrap(scope, splineContainer);

            // ── 5. Wire scope → bootstrap ───────────────────────────────
            WireScopeBootstrap(scope, bootstrap);

            // ── 6. Camera ───────────────────────────────────────────────
            SetupCamera(splineContainer);

            // ── 7. HUD Canvas ───────────────────────────────────────────
            SetupHUD(bootstrap);

            // ── 8. Directional Light ────────────────────────────────────
            EnsureLight();

            // ── 9. Ground plane ─────────────────────────────────────────
            EnsureGround();

            // ── 10. Gizmos ──────────────────────────────────────────────
            SetupGizmos(splineContainer, bootstrap);

            Undo.CollapseUndoOperations(undoGroup);
            EditorUtility.SetDirty(scope);

            Debug.Log(
                "[SplineEvaluateSceneSetup] Scene setup complete.\n" +
                "TODO:\n" +
                "  1. Assign a CarDefinition on the SplineEvaluateBootstrap\n" +
                "  2. Enter Play Mode and click Start\n" +
                "  3. Toggle Gizmos ON in the Scene/Game view to see track + car debug visuals");
        }

        // ================================================================
        // Asset creation
        // ================================================================

        private static T FindOrCreateAsset<T>(string fileName) where T : ScriptableObject
        {
            string fullPath = $"{AssetFolder}/{fileName}";
            T asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (asset != null) return asset;

            if (!Directory.Exists(AssetFolder))
            {
                Directory.CreateDirectory(AssetFolder);
                AssetDatabase.Refresh();
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SplineEvaluateSceneSetup] Created asset: {fullPath}");
            return asset;
        }

        // ================================================================
        // Track
        // ================================================================

        private static SplineContainer FindOrCreateTrack()
        {
            SplineContainer existing = Object.FindFirstObjectByType<SplineContainer>();
            if (existing != null) return existing;

            var trackGo = new GameObject("Track_Spline");
            Undo.RegisterCreatedObjectUndo(trackGo, "Create Track Spline");

            var container = trackGo.AddComponent<SplineContainer>();
            BuildOvalTrack(container);

            return container;
        }

        /// <summary>
        /// Builds a smooth oval race track (closed loop) with straights and corners.
        /// </summary>
        private static void BuildOvalTrack(SplineContainer container)
        {
            Spline spline = container.Spline;
            spline.Clear();

            float straightLength = 40f;
            float radius = 15f;
            int curveSegments = 6;

            // Bottom straight (left to right)
            spline.Add(new BezierKnot(new float3(-straightLength / 2f, 0f, -radius)));
            spline.Add(new BezierKnot(new float3(straightLength / 2f, 0f, -radius)));

            // Right semicircle
            for (int i = 1; i <= curveSegments; i++)
            {
                float angle = Mathf.PI * i / (curveSegments + 1);
                float x = straightLength / 2f + Mathf.Sin(angle) * radius;
                float z = -Mathf.Cos(angle) * radius;
                spline.Add(new BezierKnot(new float3(x, 0f, z)));
            }

            // Top straight (right to left)
            spline.Add(new BezierKnot(new float3(straightLength / 2f, 0f, radius)));
            spline.Add(new BezierKnot(new float3(-straightLength / 2f, 0f, radius)));

            // Left semicircle
            for (int i = 1; i <= curveSegments; i++)
            {
                float angle = Mathf.PI + Mathf.PI * i / (curveSegments + 1);
                float x = -straightLength / 2f + Mathf.Sin(angle) * radius;
                float z = -Mathf.Cos(angle) * radius;
                spline.Add(new BezierKnot(new float3(x, 0f, z)));
            }

            spline.Closed = true;

            for (int i = 0; i < spline.Count; i++)
            {
                spline.SetTangentMode(i, TangentMode.AutoSmooth);
            }
        }

        // ================================================================
        // Scope
        // ================================================================

        private static SplineEvaluateScope FindOrCreateScope(
            SplineDriverConfig config,
            LaneProfile laneProfile)
        {
            SplineEvaluateScope existing = Object.FindFirstObjectByType<SplineEvaluateScope>();
            if (existing != null) return existing;

            var scopeGo = new GameObject("SplineEvaluateScope");
            Undo.RegisterCreatedObjectUndo(scopeGo, "Create SplineEvaluateScope");

            var scope = scopeGo.AddComponent<SplineEvaluateScope>();

            var so = new SerializedObject(scope);
            SetField(so, "driverConfig", config);
            SetField(so, "defaultLaneProfile", laneProfile);

            // Wire NavigationSettings from project assets
            Object navSettings = FindNavigationSettings();
            if (navSettings != null)
            {
                SetField(so, "navigationSettings", navSettings);
            }
            else
            {
                Debug.LogWarning("[SplineEvaluateSceneSetup] NavigationSettings asset not found. " +
                                 "Drag Assets/Navigation/Navigation Settings.asset onto the Scope manually.");
            }

            // Navigation view holder — create a child transform
            var viewHolder = new GameObject("NavigationViewHolder");
            viewHolder.transform.SetParent(scopeGo.transform, false);
            Undo.RegisterCreatedObjectUndo(viewHolder, "Create NavigationViewHolder");

            SetField(so, "navigationViewHolder", viewHolder.transform);
            so.ApplyModifiedPropertiesWithoutUndo();

            return scope;
        }

        private static Object FindNavigationSettings()
        {
            // Search by type name (Scaffold.Navigation.NavigationSettings)
            string[] guids = AssetDatabase.FindAssets("t:NavigationSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<Object>(path);
            }

            // Fallback: try known path
            var fallback = AssetDatabase.LoadAssetAtPath<Object>("Assets/Navigation/Navigation Settings.asset");
            return fallback;
        }

        // ================================================================
        // Bootstrap
        // ================================================================

        private static SplineEvaluateBootstrap FindOrCreateBootstrap(
            SplineEvaluateScope scope,
            SplineContainer splineContainer)
        {
            SplineEvaluateBootstrap existing = Object.FindFirstObjectByType<SplineEvaluateBootstrap>();
            if (existing != null) return existing;

            var bootstrapGo = new GameObject("SplineEvaluateBootstrap");
            bootstrapGo.transform.SetParent(scope.transform, false);
            Undo.RegisterCreatedObjectUndo(bootstrapGo, "Create SplineEvaluateBootstrap");

            var bootstrap = bootstrapGo.AddComponent<SplineEvaluateBootstrap>();

            var so = new SerializedObject(bootstrap);
            SetField(so, "splineContainer", splineContainer);
            so.ApplyModifiedPropertiesWithoutUndo();

            return bootstrap;
        }

        private static void WireScopeBootstrap(SplineEvaluateScope scope, SplineEvaluateBootstrap bootstrap)
        {
            var so = new SerializedObject(scope);
            SetField(so, "sceneBootstrap", bootstrap);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ================================================================
        // Camera
        // ================================================================

        private static void SetupCamera(SplineContainer splineContainer)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                mainCam = camGo.AddComponent<Camera>();
                Undo.RegisterCreatedObjectUndo(camGo, "Create Main Camera");
            }

            Vector3 center = Vector3.zero;
            if (splineContainer != null && splineContainer.Spline != null && splineContainer.Spline.Count > 0)
            {
                Spline spline = splineContainer.Spline;
                Vector3 sum = Vector3.zero;
                int samples = Mathf.Min(spline.Count, 20);
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / samples;
                    sum += (Vector3)SplineUtility.EvaluatePosition(spline, t);
                }
                center = sum / samples;
            }

            mainCam.transform.position = center + new Vector3(0f, 60f, -40f);
            mainCam.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.12f, 0.12f, 0.15f);
        }

        // ================================================================
        // HUD
        // ================================================================

        private static void SetupHUD(SplineEvaluateBootstrap bootstrap)
        {
            if (Object.FindFirstObjectByType<SplineEvaluateHUD>() != null) return;

            var canvasGo = new GameObject("SplineEvaluateHUD_Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create HUD Canvas");

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGo.AddComponent<GraphicRaycaster>();

            var hud = canvasGo.AddComponent<SplineEvaluateHUD>();
            var hudSo = new SerializedObject(hud);
            SetField(hudSo, "bootstrap", bootstrap);

            // ── Telemetry panel (top-left) ──────────────────────────────
            var telemetryPanel = CreatePanel(canvasGo.transform, "TelemetryPanel",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(10f, -10f), new Vector2(300f, 120f));

            // Speed label
            var speedGo = CreateTMPLabel(telemetryPanel.transform, "SpeedLabel", "0 km/h", 28);
            SetField(hudSo, "speedLabel", speedGo);

            // Lap label
            var lapGo = CreateTMPLabel(telemetryPanel.transform, "LapLabel", "Lap 1 | 0.0%", 20);
            SetField(hudSo, "lapLabel", lapGo);

            // State label
            var stateGo = CreateTMPLabel(telemetryPanel.transform, "StateLabel", "COAST", 18);
            SetField(hudSo, "stateLabel", stateGo);

            // Curve Mode label
            var curveModeGo = CreateTMPLabel(telemetryPanel.transform, "CurveModeLabel", "Curve Mode: None", 16);
            SetField(hudSo, "curveModeLabel", curveModeGo);

            // ── Start/Stop button (bottom-center) ───────────────────────
            var buttonGo = CreateUIButton(canvasGo.transform, "StartStopButton", "Start",
                new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(200f, 50f));

            SetField(hudSo, "startStopButton", buttonGo.GetComponent<Button>());
            // The button label TMP component
            var btnLabels = buttonGo.GetComponentsInChildren<Component>();
            foreach (var comp in btnLabels)
            {
                if (comp.GetType().Name == "TextMeshProUGUI")
                {
                    SetField(hudSo, "startStopLabel", comp);
                    break;
                }
            }

            // ── Stat sliders panel (right side) ─────────────────────────
            var slidersPanel = CreatePanel(canvasGo.transform, "SlidersPanel",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-10f, 0f), new Vector2(280f, 350f));

            var layoutGroup = slidersPanel.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 8f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);

            string[] sliderNames = { "Speed Capability", "Cornering Skill", "Drift", "Precision", "Smoothness" };
            string[] sliderFields = { "speedCapabilitySlider", "corneringSkillSlider", "driftSlider", "precisionSlider", "smoothnessSlider" };

            for (int i = 0; i < sliderNames.Length; i++)
            {
                Slider slider = CreateLabeledSlider(slidersPanel.transform, sliderNames[i]);
                SetField(hudSo, sliderFields[i], slider);
            }

            hudSo.ApplyModifiedPropertiesWithoutUndo();
        }

        // ================================================================
        // Light & Ground
        // ================================================================

        private static void EnsureLight()
        {
            if (Object.FindFirstObjectByType<Light>() != null) return;

            var lightGo = new GameObject("Directional Light");
            Undo.RegisterCreatedObjectUndo(lightGo, "Create Directional Light");

            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void EnsureGround()
        {
            if (GameObject.Find("Ground") != null) return;

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(15f, 1f, 15f);
            ground.transform.position = new Vector3(0f, -0.01f, 0f);
            Undo.RegisterCreatedObjectUndo(ground, "Create Ground");

            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.25f, 0.28f, 0.25f);
                renderer.sharedMaterial = mat;
            }
        }

        // ================================================================
        // Gizmos
        // ================================================================

        private static void SetupGizmos(SplineContainer splineContainer, SplineEvaluateBootstrap bootstrap)
        {
            if (Object.FindFirstObjectByType<SplineEvaluateGizmos>() != null) return;

            // Add gizmos to the track spline object
            var gizmos = splineContainer.gameObject.AddComponent<SplineEvaluateGizmos>();

            var so = new SerializedObject(gizmos);
            SetField(so, "splineContainer", splineContainer);
            SetField(so, "bootstrap", bootstrap);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ================================================================
        // UI Helpers
        // ================================================================

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.6f);

            return go;
        }

        /// <summary>Creates a TMP label using reflection to avoid direct TMPro dependency in the Editor asmdef.</summary>
        private static Component CreateTMPLabel(Transform parent, string name, string text, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(10f, 0f);
            rt.offsetMax = new Vector2(-10f, 0f);

            // Add TextMeshProUGUI via type lookup
            System.Type tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (tmpType == null)
            {
                Debug.LogWarning($"[SplineEvaluateSceneSetup] TMPro not found, creating fallback Text for {name}.");
                var fallback = go.AddComponent<Text>();
                fallback.text = text;
                fallback.fontSize = fontSize;
                fallback.color = Color.white;

                var le = go.AddComponent<LayoutElement>();
                le.preferredHeight = fontSize + 10;
                return fallback;
            }

            var tmp = go.AddComponent(tmpType);
            // Set text, fontSize, color via reflection
            tmpType.GetProperty("text")?.SetValue(tmp, text);
            tmpType.GetProperty("fontSize")?.SetValue(tmp, (float)fontSize);
            tmpType.GetProperty("color")?.SetValue(tmp, Color.white);

            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = fontSize + 10;

            return tmp;
        }

        private static GameObject CreateUIButton(Transform parent, string name, string label,
            Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 1f, 0.9f);

            go.AddComponent<Button>();

            // Label child
            var labelComp = CreateTMPLabel(go.transform, "Label", label, 22);
            var labelRt = labelComp.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            return go;
        }

        private static Slider CreateLabeledSlider(Transform parent, string labelText)
        {
            var container = new GameObject(labelText + "_Container", typeof(RectTransform));
            container.transform.SetParent(parent, false);

            var le = container.AddComponent<LayoutElement>();
            le.preferredHeight = 50f;

            // Label
            CreateTMPLabel(container.transform, "Label", labelText, 14);

            // Slider GO
            var sliderGo = new GameObject("Slider", typeof(RectTransform));
            sliderGo.transform.SetParent(container.transform, false);
            var sliderRt = sliderGo.GetComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0f, 0f);
            sliderRt.anchorMax = new Vector2(1f, 0.5f);
            sliderRt.offsetMin = new Vector2(5f, 2f);
            sliderRt.offsetMax = new Vector2(-5f, -2f);

            // Background
            var bgGo = new GameObject("Background", typeof(RectTransform));
            bgGo.transform.SetParent(sliderGo.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.25f);
            bgRt.anchorMax = new Vector2(1f, 0.75f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            bgGo.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);

            // Fill
            var fillAreaGo = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGo.transform.SetParent(sliderGo.transform, false);
            var fillAreaRt = fillAreaGo.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRt.offsetMin = Vector2.zero;
            fillAreaRt.offsetMax = Vector2.zero;

            var fillGo = new GameObject("Fill", typeof(RectTransform));
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fillGo.AddComponent<Image>().color = new Color(0.3f, 0.7f, 1f);

            // Handle
            var handleAreaGo = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaGo.transform.SetParent(sliderGo.transform, false);
            var handleAreaRt = handleAreaGo.GetComponent<RectTransform>();
            handleAreaRt.anchorMin = Vector2.zero;
            handleAreaRt.anchorMax = Vector2.one;
            handleAreaRt.offsetMin = new Vector2(10f, 0f);
            handleAreaRt.offsetMax = new Vector2(-10f, 0f);

            var handleGo = new GameObject("Handle", typeof(RectTransform));
            handleGo.transform.SetParent(handleAreaGo.transform, false);
            handleGo.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 0f);
            var handleImg = handleGo.AddComponent<Image>();
            handleImg.color = Color.white;

            // Wire slider
            var slider = sliderGo.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = handleGo.GetComponent<RectTransform>();
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 10f;
            slider.value = 5f;
            slider.wholeNumbers = false;

            return slider;
        }

        // ================================================================
        // SerializedObject field helper
        // ================================================================

        private static void SetField(SerializedObject so, string fieldName, Object value)
        {
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
            else
            {
                Debug.LogWarning($"[SplineEvaluateSceneSetup] Could not find field '{fieldName}' on {so.targetObject.GetType().Name}.");
            }
        }
    }
}
