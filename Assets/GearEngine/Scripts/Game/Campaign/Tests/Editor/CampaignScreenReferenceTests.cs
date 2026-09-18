using System;
using System.Linq;
using System.Reflection;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.Campaign.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class CampaignScreenReferenceTests
    {
        [Test]
        public void MainScene_HasOneTrackNavigationControl()
        {
            const string scenePath = "Assets/GearEngine/Scenes/Main Scene.unity";
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool wasAlreadyLoaded = scene.isLoaded;
            if (!wasAlreadyLoaded)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            try
            {
                MainView mainView = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<MainView>(true))
                    .Single();
                int navigationRootCount = mainView.GetComponentsInChildren<Transform>(true)
                    .Count(child => child.name == "TrackNavigation" || child.name == "select_track");

                Assert.That(navigationRootCount, Is.EqualTo(1),
                    "Home must render one previous/next track navigation control.");
            }
            finally
            {
                if (!wasAlreadyLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [Test]
        public void RaceBoardLayout_ReservesHudSpaceAndRestoresSharedBoard()
        {
            GameObject root = new GameObject("RaceLayoutTest");
            root.SetActive(false);
            GameObject boardObject = new GameObject("Board", typeof(RectTransform),
                typeof(GearEngine.Presentation.UI.BoardViewComponent));
            boardObject.transform.SetParent(root.transform);
            GearEngine.Presentation.UI.BoardView shared = root.AddComponent<GearEngine.Presentation.UI.BoardView>();
            SerializedObject sharedData = new SerializedObject(shared);
            sharedData.FindProperty("board").objectReferenceValue =
                boardObject.GetComponent<GearEngine.Presentation.UI.BoardViewComponent>();
            sharedData.ApplyModifiedPropertiesWithoutUndo();
            Presentation.ActiveRaceView view = root.AddComponent<Presentation.ActiveRaceView>();
            SerializedObject viewData = new SerializedObject(view);
            viewData.FindProperty("board").objectReferenceValue = shared;
            viewData.ApplyModifiedPropertiesWithoutUndo();
            RectTransform rect = (RectTransform)boardObject.transform;
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.4f);
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic;
            try
            {
                typeof(Presentation.ActiveRaceView).GetMethod("ApplyRaceBoardLayout", flags).Invoke(view, null);
                Assert.That(rect.anchorMin.y, Is.EqualTo(0.08f).Within(0.001f));
                Assert.That(rect.anchorMax.y, Is.EqualTo(0.38f).Within(0.001f));
                typeof(Presentation.ActiveRaceView).GetMethod("ApplyRaceBoardLayout", flags).Invoke(view, null);
                typeof(Presentation.ActiveRaceView).GetMethod("RestoreBoardLayout", flags).Invoke(view, null);
                Assert.That(rect.anchorMin, Is.EqualTo(new Vector2(0.1f, 0.1f)));
                Assert.That(rect.anchorMax, Is.EqualTo(new Vector2(0.9f, 0.4f)));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RaceHud_SitsBetweenTrackAndGearBoardAndLabelsRpm()
        {
            const float referenceHeight = 1920f;
            const float boardTop = 0.38f * referenceHeight;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/Race View.prefab");
            RectTransform trackViewport = prefab.transform.Find("TrackViewport") as RectTransform;
            RectTransform hud = prefab.transform.Find("Container/CarStatusHud") as RectTransform;
            ActiveRaceView view = prefab.GetComponent<ActiveRaceView>();
            SerializedObject serializedView = new SerializedObject(view);
            TMP_Text rpmText = (TMP_Text)serializedView.FindProperty("currentRpmText").objectReferenceValue;
            TMP_Text rpmLabel = (TMP_Text)serializedView.FindProperty("rpmLabelText").objectReferenceValue;
            TMP_Text scoreLabel = (TMP_Text)serializedView.FindProperty("scoreLabelText").objectReferenceValue;
            RectTransform rpmPanel = rpmText.transform.parent as RectTransform;
            RectTransform scorePanel = prefab.transform.Find("Container/CarStatusHud/Score_PanelRace") as RectTransform;

            Assert.That(trackViewport, Is.Not.Null);
            Assert.That(hud, Is.Not.Null);
            Assert.That(hud.anchorMin.y, Is.EqualTo(0.48f).Within(0.001f));
            Assert.That(hud.anchorMin.y, Is.LessThan(trackViewport.anchorMin.y),
                "HUD anchor must remain below the track region.");

            float hudBottom = (hud.anchorMin.y * referenceHeight) + hud.anchoredPosition.y -
                (hud.sizeDelta.y * hud.pivot.y);
            Assert.That(hudBottom, Is.GreaterThan(boardTop), "HUD must remain above the gear board.");
            Assert.That(rpmLabel.gameObject.activeSelf, Is.True);
            Assert.That(rpmLabel.text, Is.EqualTo("RPM"));
            Assert.That(rpmLabel.rectTransform.anchoredPosition.y,
                Is.GreaterThan(rpmText.rectTransform.anchoredPosition.y));
            Assert.That(rpmText.text, Is.EqualTo("0"));
            Assert.That(rpmPanel.sizeDelta.x, Is.GreaterThanOrEqualTo(680f));
            Assert.That(scorePanel.sizeDelta.x, Is.GreaterThanOrEqualTo(680f));
            AssertHudLabelOutline(view, rpmLabel);
            AssertHudLabelOutline(view, scoreLabel);
            Assert.That(serializedView.FindProperty("currentVelocityText").objectReferenceValue, Is.Not.Null);
            Assert.That(serializedView.FindProperty("currentGearText").objectReferenceValue, Is.Not.Null);
            Assert.That(serializedView.FindProperty("rpmSegments").arraySize, Is.EqualTo(3));
        }

        private static void AssertHudLabelOutline(ActiveRaceView view, TMP_Text label)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(ActiveRaceView).GetMethod("ConfigureHudLabels", flags).Invoke(view, null);
            Assert.That(label, Is.Not.Null);
            Assert.That(label.outlineWidth, Is.GreaterThan(0f));
            Assert.That(label.outlineColor, Is.EqualTo(new Color32(245, 239, 226, 255)));
        }

        [Test]
        public void RaceScore_UpdatesWhileDriftPointsAccumulate()
        {
            CarDefinition carDefinition = ScriptableObject.CreateInstance<CarDefinition>();
            TrackDefinition trackDefinition = ScriptableObject.CreateInstance<TrackDefinition>();
            RaceState session = new RaceState(new CarEntity(carDefinition), trackDefinition, new RaceSessionConfig())
            {
                Phase = SimulationLifecycleState.Running,
                TotalDriftScore = 40,
            };
            CarViewModel car = new CarViewModel(session, new StubSimulationRunner(), false)
            {
                IsDrifting = true,
            };
            RaceDriftScoreViewModel viewModel = new RaceDriftScoreViewModel(session, car, 100f, 0.1f);
            GameObject racePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/Race View.prefab");
            GameObject instance = Object.Instantiate(racePrefab);
            RaceDriftScoreView view = instance.GetComponentInChildren<RaceDriftScoreView>(true);
            SerializedObject serializedView = new SerializedObject(view);
            TMP_Text totalScore = (TMP_Text)serializedView.FindProperty("totalScoreText").objectReferenceValue;
            TMP_Text points = (TMP_Text)serializedView.FindProperty("pointsText").objectReferenceValue;
            TMP_Text multiplier = (TMP_Text)serializedView.FindProperty("multiplierText").objectReferenceValue;
            RectTransform driftPoints = view.transform.Find("DriftPoints") as RectTransform;
            Assert.That(driftPoints, Is.Not.Null);
            RectTransform score = driftPoints.Find("Score") as RectTransform;
            RectTransform multiplierBackground = driftPoints.Find("bg_multiplier") as RectTransform;
            HorizontalLayoutGroup obsoleteLayout = driftPoints.GetComponent<HorizontalLayoutGroup>();
            Assert.That(score, Is.Not.Null);
            Assert.That(multiplierBackground, Is.Not.Null);
            Assert.That(obsoleteLayout, Is.Not.Null);

            try
            {
                view.Bind(viewModel);
                viewModel.Tick(0.5f);
                Assert.That(totalScore.text, Is.EqualTo("90"),
                    "The score panel must include unbanked drift points while the value changes.");
                Assert.That(points.text, Is.EqualTo("+50"));
                Assert.That(multiplier.text, Is.EqualTo("1x"));
                Color expectedHudColor = new Color32(44, 57, 69, 255);
                Assert.That(points.color, Is.EqualTo(expectedHudColor));
                Assert.That(multiplier.color, Is.EqualTo(expectedHudColor));
                Assert.That(driftPoints.sizeDelta, Is.EqualTo(new Vector2(-650f, 150f)));
                Assert.That(score.sizeDelta, Is.EqualTo(new Vector2(255f, 105f)));
                Assert.That(multiplierBackground.sizeDelta, Is.EqualTo(new Vector2(170f, 150f)));
                Assert.That(multiplierBackground.anchoredPosition, Is.EqualTo(new Vector2(130f, 0f)));
                Assert.That(multiplierBackground.GetSiblingIndex(), Is.LessThan(score.GetSiblingIndex()),
                    "The multiplier badge must render behind the multiplier without covering the score.");
                Assert.That(obsoleteLayout.enabled, Is.False,
                    "The score and multiplier use explicit compact positions inside one badge.");
            }
            finally
            {
                Object.DestroyImmediate(instance);
                Object.DestroyImmediate(trackDefinition);
                Object.DestroyImmediate(carDefinition);
            }
        }

        [TestCase("Campaign_ResultPopupView", "ResultPopupView")]
        [TestCase("PFB_ReceivedRewardsView", "ReceivedRewardsView")]
        [TestCase("PFB_RaceProgressView", "RaceProgressView")]
        public void PostRacePrefab_HasWiredScreenReferences(string name, string viewType)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Assets/GearEngine/Prefabs/Campaign/{name}.prefab");
            Component view = System.Array.Find(prefab.GetComponents<Component>(), c => c.GetType().Name == viewType);
            Assert.That(view, Is.Not.Null);
            SerializedObject data = new SerializedObject(view);
            SerializedProperty property = data.GetIterator();
            while (property.NextVisible(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Assert.That(property.objectReferenceValue, Is.Not.Null, $"{name}: {property.propertyPath}");
                }
            }
        }

        [TestCase("Main View")]
        [TestCase("Setup View")]
        [TestCase("Race View")]
        [TestCase("Campaign_ResultPopupView")]
        [TestCase("PFB_ReceivedRewardsView")]
        [TestCase("PFB_RaceProgressView")]
        [TestCase("Campaign_RoguelikeView")]
        [TestCase("Items View")]
        [TestCase("ItemPopup View")]
        public void ScreenPrefab_HasNoMissingScripts(string name)
        {
            string path = $"Assets/GearEngine/Prefabs/Campaign/{name}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
            {
                int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
                Assert.That(missing, Is.Zero, $"{path}: {child.name} has missing scripts.");
            }
        }

        private sealed class StubSimulationRunner : ISimulationRunnerService
        {
            public event Action<CarEntity> OnLapCompleted
            {
                add { }
                remove { }
            }

            public void InitializeRun(ISimulationInitParams initParams) { }

            public void SetPaused(CarEntity entity, bool paused) { }

            public bool GetTelemetry(CarEntity entity, out CarTelemetryData data)
            {
                data = default;
                return false;
            }

            public void ApplyJerk(CarEntity entity, float severity) { }

            public void RemoveDriver(CarEntity entity) { }

            public void TriggerCinematicFinish(CarEntity entity) { }

            public void Tick() { }
        }
    }
}
