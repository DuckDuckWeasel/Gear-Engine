using System;
using System.Linq;
using System.Collections;
using UnityEngine.TestTools;
using UnityEditor.SceneManagement;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class ClubSportHudTests
    {
        [TestCase(0, 0f, 0f, 0f)]
        [TestCase(653, 0.8706667f, 0f, 0f)]
        [TestCase(750, 1f, 0f, 0f)]
        [TestCase(1500, 1f, 0.5f, 0f)]
        [TestCase(2250, 1f, 1f, 0f)]
        [TestCase(3000, 1f, 1f, 0.5f)]
        [TestCase(3750, 1f, 1f, 1f)]
        [TestCase(4000, 1f, 1f, 1f)]
        public void Stars_FillSequentiallyAcrossScoreIntervals(
            int score,
            float firstFill,
            float secondFill,
            float thirdFill)
        {
            int[] targets = { 750, 2250, 3750 };
            float[] expected = { firstFill, secondFill, thirdFill };
            ClubSportHudViewModel state = new ClubSportHudViewModel(targets, 0);
            state.SetBankedScore(score);
            for (int i = 0; i < targets.Length; i++)
            {
                Assert.That(state.Fill(i, score), Is.EqualTo(expected[i]).Within(.0001f));
            }
        }

        [Test]
        public void Stars_MultipleAwardsCelebrateOnceAndRebindingDoesNotReplay()
        {
            int[] targets = { 800, 2400, 4000 };
            ClubSportHudViewModel state = new ClubSportHudViewModel(targets, 0);
            state.SetBankedScore(4100);
            Assert.That(state.ConsumeCelebration(0, 799), Is.False);
            for (int i = 0; i < 3; i++)
            {
                Assert.That(state.ConsumeCelebration(i, 4100), Is.True);
                Assert.That(state.ConsumeCelebration(i, 4100), Is.False);
            }
            state = new ClubSportHudViewModel(targets, 4100);
            Assert.That(state.ConsumeCelebration(2, 4100), Is.False);
            state = new ClubSportHudViewModel(targets, 0);
            state.SetBankedScore(800);
            Assert.That(state.ConsumeCelebration(0, 800), Is.True);
        }

        [Test]
        public void Targets_RejectInvalidConfigurationWithoutFallbackRewards()
        {
            Assert.Throws<ArgumentException>(() => new ClubSportHudViewModel(new[] { 0 }, 0));
            Assert.Throws<ArgumentException>(() => new ClubSportHudViewModel(new[] { -1 }, 0));
            Assert.Throws<ArgumentException>(() => new ClubSportHudViewModel(new[] { 2400, 800 }, 0));
            Assert.Throws<ArgumentException>(() => new ClubSportHudViewModel(new[] { 800, 800 }, 0));
            Assert.That(new ClubSportHudViewModel(Array.Empty<int>(), 9000).StarCount, Is.Zero);
        }

        [Test]
        public void Overtake_ConfirmsImprovementAndRejectsGridFinishAndBriefFluctuations()
        {
            ClubSportHudViewModel state = new ClubSportHudViewModel(Array.Empty<int>(), 0);
            state.UpdateStanding(4, true, true, false, 1);
            state.UpdateStanding(2, true, true, false, 1);
            Assert.That(state.OvertakeAlpha, Is.Zero);
            state.UpdateStanding(4, true, false, false, 1);
            state.UpdateStanding(3, true, false, false, .1f);
            state.UpdateStanding(4, true, false, false, .1f);
            Assert.That(state.OvertakeAlpha, Is.Zero);
            state.UpdateStanding(3, true, false, false, .1f);
            Assert.That(state.OvertakeAlpha, Is.Zero);
            state.UpdateStanding(3, true, false, false, .1f);
            Assert.That(state.OvertakeAlpha, Is.EqualTo(1));
            state.UpdateStanding(3, true, false, false, 1.5f);
            Assert.That(state.OvertakeAlpha, Is.EqualTo(1).Within(.001f));
            state.UpdateStanding(3, true, false, false, .1f);
            Assert.That(state.OvertakeAlpha, Is.EqualTo(.5f).Within(.001f));
            state.UpdateStanding(1, true, false, true, .3f);
            Assert.That(state.OvertakeAlpha, Is.Zero);
        }

        [Test]
        public void LapValue_UsesNormalYellowAndRedAcrossRaceProgress()
        {
            const string path = "Assets/GearEngine/Prefabs/Campaign/PFB_ClubSportHud.prefab";
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            CarDefinition definition = ScriptableObject.CreateInstance<CarDefinition>();
            TrackDefinition track = AssetDatabase.LoadAssetAtPath<TrackDefinition>(
                "Assets/GearEngine/Data/Track/Tracks/PeanutTrack.asset");
            RaceState session = new RaceState(new CarEntity(definition), track, new RaceSessionConfig());
            CarViewModel car = new CarViewModel(session, new HudRunner(), false);
            RaceDriftScoreViewModel combo = new RaceDriftScoreViewModel(session, car);
            ClubSportHudView hud = instance.GetComponent<ClubSportHudView>();
            TMP_Text laps = instance.transform.Find("Content/Header/Laps/Value").GetComponent<TMP_Text>();
            Color normalLapColor = new Color32(248, 239, 220, 255);
            Color finalLapColor = new Color32(255, 204, 0, 255);
            Color finishedLapColor = new Color32(239, 70, 78, 255);
            try
            {
                hud.Bind(session, combo);
                Assert.That(laps.color, Is.EqualTo(normalLapColor));
                session.CurrentLap = session.TotalLaps - 1;
                hud.Tick(0f);
                Assert.That(laps.color, Is.EqualTo(finalLapColor));
                session.CurrentLap = session.TotalLaps;
                hud.Tick(0f);
                Assert.That(laps.color, Is.EqualTo(finishedLapColor));
            }
            finally
            {
                Object.DestroyImmediate(instance);
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void ProductionPrefab_HasNoDevDependenciesAndReopensWithBankedScoreOnly()
        {
            const string path = "Assets/GearEngine/Prefabs/Campaign/PFB_ClubSportHud.prefab";
            Assert.That(AssetDatabase.GetDependencies(path).Any(value => value.StartsWith("Assets/_Dev/")), Is.False);
            GameObject canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), canvas.transform);
            CarDefinition definition = ScriptableObject.CreateInstance<CarDefinition>();
            TrackDefinition track = AssetDatabase.LoadAssetAtPath<TrackDefinition>("Assets/GearEngine/Data/Track/Tracks/PeanutTrack.asset");
            RaceState session = new RaceState(new CarEntity(definition), track, new RaceSessionConfig()) { TotalDriftScore = 2114 };
            CarViewModel car = new CarViewModel(session, new HudRunner(), false);
            RaceDriftScoreViewModel combo = new RaceDriftScoreViewModel(session, car);
            ClubSportHudView hud = instance.GetComponent<ClubSportHudView>();
            try
            {
                hud.Bind(session, combo);
                TMP_Text scoreTitle = instance.transform.Find("Content/Feedback/ScoreTitle").GetComponent<TMP_Text>();
                CanvasGroup activeCombo = instance.transform.Find("Content/Feedback/ActiveCombo").GetComponent<CanvasGroup>();
                Assert.That(scoreTitle.text, Is.EqualTo("SCORE"));
                Assert.That(scoreTitle.gameObject.activeInHierarchy, Is.True);
                Assert.That(activeCombo.alpha, Is.Zero, "The SCORE heading must remain visible while active combo details are hidden.");
                Image multiplierPanel = instance.transform.Find("Content/Feedback/ActiveCombo/Multiplier").GetComponent<Image>();
                TMP_Text multiplier = multiplierPanel.transform.Find("Value").GetComponent<TMP_Text>();
                combo.DisplayPoints = 50; combo.CurrentMultiplier = 2; combo.IsDisplayingScore = true;
                hud.Tick(0);
                Assert.That(multiplierPanel.color, Is.EqualTo((Color)new Color32(255, 102, 105, 255)));
                Assert.That(multiplier.color, Is.EqualTo((Color)new Color32(248, 239, 220, 255)));
                combo.CurrentMultiplier = 3;
                hud.Tick(0);
                Assert.That(multiplierPanel.color, Is.EqualTo((Color)new Color32(255, 213, 64, 255)));
                Assert.That(multiplier.color, Is.EqualTo((Color)new Color32(53, 65, 74, 255)));
                combo.DisplayPoints = 177; combo.CurrentMultiplier = 9; combo.IsDisplayingScore = true;
                hud.Tick(0);
                Assert.That(hud.State.BankedScore, Is.EqualTo(2114));
                TMP_Text total = instance.transform.Find("Content/Feedback/BankedTotal").GetComponent<TMP_Text>();
                Assert.That(total.text, Is.EqualTo("2,114"));
                Assert.That(total.fontSizeMax, Is.EqualTo(31f));
                Assert.That(total.color, Is.EqualTo((Color)new Color32(255, 213, 64, 255)));
                Assert.That(instance.GetComponentsInChildren<TMP_Text>().Any(text => text.text == "9x"), Is.True);
                RectTransform multiplierBadge = multiplier.transform.parent as RectTransform;
                Assert.That(multiplier.fontSize, Is.EqualTo(18f));
                Assert.That(multiplier.fontWeight, Is.EqualTo(FontWeight.Bold));
                Assert.That(multiplierBadge.anchorMax.y - multiplierBadge.anchorMin.y, Is.GreaterThan(0.85f));
                Assert.That(hud.ControlsDisabled, Is.True);
                Image pulsePanel = instance.transform.Find("Content/Dashboard/Pulse").GetComponent<Image>();
                Image turboPanel = instance.transform.Find("Content/Dashboard/Turbo").GetComponent<Image>();
                Image lapsPanel = instance.transform.Find("Content/Header/Laps").GetComponent<Image>();
                Image positionPanel = instance.transform.Find("Content/Header/Position").GetComponent<Image>();
                Image goldStar = instance.transform.Find("Content/Feedback/Star0/Gold").GetComponent<Image>();
                Color ink = new Color32(53, 65, 74, 255);
                Color coral = new Color32(255, 102, 105, 255);
                Color gold = new Color32(255, 213, 64, 255);
                Assert.That(pulsePanel.color, Is.EqualTo(ink));
                Assert.That(lapsPanel.color, Is.EqualTo(ink));
                Assert.That(turboPanel.color, Is.EqualTo(coral));
                Assert.That(positionPanel.color, Is.EqualTo(coral));
                Assert.That(goldStar.color, Is.EqualTo(gold));
                Sprite pulseShape = pulsePanel.sprite;
                Sprite turboShape = turboPanel.sprite;
                Assert.That(pulseShape, Is.Not.Null);
                Assert.That(turboShape, Is.SameAs(pulseShape));
                Assert.That(AssetDatabase.GetAssetPath(pulseShape), Does.EndWith("T_RoundedPanel.png"));
                Assert.That(lapsPanel.sprite, Is.SameAs(pulseShape));
                Assert.That(pulseShape.border.x, Is.GreaterThan(0), "The shared panel needs a nine-slice border to preserve circular corners.");
                Assert.That(pulsePanel.type, Is.EqualTo(Image.Type.Sliced));
                Assert.That(turboPanel.type, Is.EqualTo(Image.Type.Sliced));
                Assert.That(lapsPanel.type, Is.EqualTo(Image.Type.Sliced));
                string laps = instance.transform.Find("Content/Header/Laps/Value").GetComponent<TMP_Text>().text;
                Assert.That(laps, Does.Not.Contain(" / "));
                Assert.That(laps, Does.Contain("/"));
                foreach (Button button in instance.GetComponentsInChildren<Button>())
                {
                    Assert.That(button.colors.disabledColor, Is.EqualTo(Color.white));
                    int calls = 0; button.onClick.AddListener(() => calls++);
                    button.OnSubmit(new BaseEventData(null));
                    Assert.That(calls, Is.Zero);
                    button.onClick.RemoveAllListeners();
                }
                hud.Unbind();
                instance.SetActive(false);
                Assert.That(hud.State, Is.Null);
                instance.SetActive(true);
                hud.Bind(session, combo);
                Assert.That(hud.State.ConsumeCelebration(0, 2114), Is.False);
                session.Phase = SimulationLifecycleState.Completed;
                session.TotalDriftScore = 4000;
                hud.Tick(0);
                Assert.That(total.text, Is.EqualTo("4,000"));
                Assert.That(hud.GetComponentsInChildren<ClubSportStarView>().All(star => star.FillAmount == 1), Is.True);
                session.Reset(); hud.Bind(session, combo);
                Assert.That(hud.State.BankedScore, Is.Zero);
                Assert.That(total.text, Is.EqualTo("0"));
            }
            finally { Object.DestroyImmediate(canvas); Object.DestroyImmediate(definition); }
        }

        [Test]
        public void ProductionCarLiveries_AreDistinctAndUseProductionAssets()
        {
            const string path = "Assets/GearEngine/Prefabs/Tracks/CarView.prefab";
            Assert.That(AssetDatabase.GetDependencies(path).Any(value => value.StartsWith("Assets/_Dev/")), Is.False);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Texture[] textures = new Texture[4];
            GameObject[] cars = new GameObject[4];
            try
            {
                for (int i = 0; i < cars.Length; i++)
                {
                    cars[i] = Object.Instantiate(prefab);
                    ClubSportCarLiveryView livery = cars[i].GetComponent<ClubSportCarLiveryView>();
                    Assert.That(livery, Is.Not.Null);
                    livery.Apply(i, i == 0);
                    MeshRenderer body = cars[i].GetComponentsInChildren<MeshRenderer>(true)
                        .Single(renderer => renderer.name == "Body");
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    body.GetPropertyBlock(block, 0);
                    textures[i] = block.GetTexture("_BaseMap");
                    Assert.That(textures[i], Is.Not.Null);
                    Assert.That(AssetDatabase.GetAssetPath(textures[i]), Does.StartWith("Assets/GearEngine/Art/Textures/ClubSport/"));
                    Assert.That(body.transform.Find("ClubSportRoofStripe"), Is.Not.Null);
                }

                Assert.That(textures.Distinct().Count(), Is.EqualTo(4));
            }
            finally
            {
                foreach (GameObject car in cars)
                {
                    Object.DestroyImmediate(car);
                }
            }
        }

        [TestCase(390, 780)]
        [TestCase(390, 635)]
        [TestCase(320, 680)]
        [TestCase(640, 480)]
        public void Layout_ReservesRoadAndBoardEvenWhenTheOpeningAnimationHasZeroScale(int width, int height)
        {
            GameObject root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GearEngine/Prefabs/Campaign/PFB_ClubSportHud.prefab"));
            try
            {
                ClubSportHudLayout layout = root.GetComponent<ClubSportHudLayout>();
                layout.ApplySize(new Vector2(width, height), width);
                root.transform.localScale = Vector3.zero;
                Rect road = layout.RoadAnchors();
                Rect board = layout.BoardAnchors();
                Assert.That(road.width, Is.EqualTo(.84f).Within(.001f));
                Assert.That(road.height, Is.GreaterThan(.3f));
                Assert.That(board.height, Is.GreaterThan(.17f));
                Assert.That(board.yMin, Is.Zero);
                Assert.That(road.yMin, Is.GreaterThan(board.yMax));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void SharedBoard_RestoresAnchorsScaleAndCapacityAfterTwoRaceLayouts()
        {
            GameObject boardObject = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GearEngine/Prefabs/Campaign/PFB_BoardView.prefab"));
            GameObject hudObject = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GearEngine/Prefabs/Campaign/PFB_ClubSportHud.prefab"));
            BoardViewComponent board = boardObject.GetComponentInChildren<BoardViewComponent>();
            RectTransform rect = (RectTransform)board.transform;
            Vector2 min = rect.anchorMin, max = rect.anchorMax, position = rect.anchoredPosition, size = rect.sizeDelta;
            Vector3 scale = rect.localScale, gridScale = board.GetBoardSpaceRoot().localScale;
            ClubSportBoardLayout binding = new ClubSportBoardLayout();
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    ClubSportHudLayout layout = hudObject.GetComponent<ClubSportHudLayout>();
                    layout.ApplySize(new Vector2(390, 780));
                    binding.Apply(board, layout);
                    Assert.That(rect.anchorMax.y, Is.GreaterThan(rect.anchorMin.y));
                    binding.Restore();
                    Assert.That(rect.anchorMin, Is.EqualTo(min)); Assert.That(rect.anchorMax, Is.EqualTo(max));
                    Assert.That(rect.anchoredPosition, Is.EqualTo(position)); Assert.That(rect.sizeDelta, Is.EqualTo(size));
                    Assert.That(rect.localScale, Is.EqualTo(scale)); Assert.That(board.GetBoardSpaceRoot().localScale, Is.EqualTo(gridScale));
                }
            }
            finally { Object.DestroyImmediate(boardObject); Object.DestroyImmediate(hudObject); }
        }

        [Test]
        public void SetupBoard_UsesClubSportSevenByFourGridFitAndRestoresIt()
        {
            GameObject boardObject = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/PFB_BoardView.prefab"));
            BoardViewComponent board = boardObject.GetComponentInChildren<BoardViewComponent>();
            RectTransform root = (RectTransform)board.transform;
            RectTransform grid = board.GetBoardSpaceRoot();
            Vector3 originalScale = grid.localScale;
            ClubSportBoardLayout binding = new ClubSportBoardLayout();
            try
            {
                Canvas.ForceUpdateCanvases();
                binding.ApplyToCurrentRegion(board);
                float spacing = board.BoardLayout.CellSpacing;
                float expected = Mathf.Min(root.rect.width * 0.86f / (7f * spacing),
                    root.rect.height * 0.88f / (4f * spacing));

                Assert.That(grid.localScale, Is.EqualTo(Vector3.one * expected));
                Assert.That(grid.rect.width * grid.localScale.x,
                    Is.GreaterThanOrEqualTo(root.rect.width * 0.84f));

                binding.Restore();
                Assert.That(grid.localScale, Is.EqualTo(originalScale));
            }
            finally
            {
                Object.DestroyImmediate(boardObject);
            }
        }

        [UnityTest]
        public IEnumerator AnimatedScore_RetargetsAndDisableCancelsOwnedPresentation()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            GameObject canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GearEngine/Prefabs/Campaign/PFB_ClubSportHud.prefab"), canvas.transform);
            ClubSportHudView hud = instance.GetComponent<ClubSportHudView>();
            CarDefinition definition = ScriptableObject.CreateInstance<CarDefinition>();
            TrackDefinition track = AssetDatabase.LoadAssetAtPath<TrackDefinition>("Assets/GearEngine/Data/Track/Tracks/PeanutTrack.asset");
            RaceState session = new RaceState(new CarEntity(definition), track, new RaceSessionConfig());
            RaceDriftScoreViewModel combo = new RaceDriftScoreViewModel(session, new CarViewModel(session, new HudRunner(), false));
            hud.Bind(session, combo);
            session.TotalDriftScore = 4100; hud.Tick(0);
            yield return new WaitForSeconds(.15f);
            TMP_Text text = instance.transform.Find("Content/Feedback/BankedTotal").GetComponent<TMP_Text>();
            int intermediate = int.Parse(text.text.Replace(",", ""));
            Assert.That(intermediate, Is.InRange(1, 4099));
            session.TotalDriftScore = 4500; hud.Tick(0);
            Assert.That(int.Parse(text.text.Replace(",", "")), Is.EqualTo(intermediate));
            float deadline = Time.realtimeSinceStartup + 8f;
            while (text.text != "4,500" && Time.realtimeSinceStartup < deadline) { yield return null; }
            Assert.That(text.text, Is.EqualTo("4,500"));
            hud.SetReducedMotion(true); session.TotalDriftScore = 4600; hud.Tick(0);
            Assert.That(text.text, Is.EqualTo("4,600"));
            instance.SetActive(false);
            Assert.That(hud.State, Is.Null);
            yield return new WaitForSeconds(.2f);
            Assert.That(text.transform.localScale, Is.EqualTo(Vector3.one));
            instance.SetActive(true); hud.Bind(session, combo);
            Assert.That(hud.State.ConsumeCelebration(2, 4600), Is.False);
            Object.Destroy(canvas); Object.Destroy(definition);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator ReviewControls_AnimateConnectedGearsWithoutDrivingTheCars()
        {
            EditorSceneManager.OpenScene("Assets/_Dev/ClubSport/Review/ClubSportPeanutHudReview.unity");
            yield return new EnterPlayMode();
            ClubSportHudView hud = Object.FindAnyObjectByType<ClubSportHudView>();
            Canvas canvas = hud.GetComponentInParent<Canvas>();
            Assert.That(canvas.GetComponent<GraphicRaycaster>(), Is.Not.Null);
            Assert.That(EventSystem.current, Is.Not.Null);
            Button pulse = hud.transform.Find("Content/Dashboard/Pulse").GetComponent<Button>();
            Button turbo = hud.transform.Find("Content/Dashboard/Turbo").GetComponent<Button>();
            Assert.That(pulse.interactable && turbo.interactable, Is.True);
            Transform board = hud.transform.Find("Content/BoardArea/OriginalSevenByFourLayout");
            Transform core = board.Find("Cell9");
            GameObject disconnected = Object.Instantiate(board.Find("Cell8").gameObject, board);
            disconnected.name = "Cell27";
            CarView[] cars = Object.FindObjectsByType<CarView>();
            Vector3[] positions = cars.Select(car => car.transform.position).ToArray();
            pulse.OnSubmit(new BaseEventData(EventSystem.current));
            float deadline = Time.realtimeSinceStartup + 5;
            while (core.localScale.x < 1.01f && Time.realtimeSinceStartup < deadline) { yield return null; }
            Assert.That(core.localScale.x, Is.GreaterThan(1.01f));
            Assert.That(disconnected.transform.localScale, Is.EqualTo(Vector3.one));
            for (int i = 0; i < cars.Length; i++) { Assert.That(cars[i].transform.position, Is.EqualTo(positions[i])); }
            turbo.OnSubmit(new BaseEventData(EventSystem.current));
            yield return null;
            canvas.gameObject.SetActive(false);
            Assert.That(hud.State, Is.Null);
            Assert.That(core.localScale, Is.EqualTo(Vector3.one));
            Assert.That(pulse.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(turbo.transform.localScale, Is.EqualTo(Vector3.one));
            Object.Destroy(disconnected);
            yield return new ExitPlayMode();
        }

        private sealed class HudRunner : ISimulationRunnerService
        {
            public event Action<CarEntity> OnLapCompleted { add { } remove { } }
            public void InitializeRun(ISimulationInitParams parameters) { }
            public void SetPaused(CarEntity entity, bool paused) { }
            public bool GetTelemetry(CarEntity entity, out CarTelemetryData data) { data = default; return false; }
            public void ApplyJerk(CarEntity entity, float severity) { }
            public void RemoveDriver(CarEntity entity) { }
            public void TriggerCinematicFinish(CarEntity entity) { }
            public void Tick() { }
        }
    }
}
