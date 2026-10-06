using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.SplineSimulation;
using GearEngine.CarSimulation.Tracks;
using GearEngine.Campaign.Presentation;
using GearEngine.FrustumFit;
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

        [Test, Category("Biome")]
        public void MainScene_TrackPropsAndDesertGroundAreVisible()
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
                Transform generator = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Single(child => child.name == "PropGenerator");
                Material ground = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/GearEngine/Art/Materials/TrackThemes/M_ThemeDesertGround.mat");

                Assert.That(generator.gameObject.activeInHierarchy, Is.True,
                    "Generated track props must not be parented under an inactive object.");
                Assert.That(ground, Is.Not.Null);
                Assert.That(ground.GetTexture("_BaseMap"), Is.Not.Null,
                    "The desert ground texture must be included in Editor and player builds.");
                Assert.That(ground.GetTextureScale("_BaseMap"), Is.EqualTo(Vector2.one * 4f));
                Assert.That(AssetDatabase.GetAssetPath(ground.GetTexture("_BaseMap")),
                    Is.EqualTo("Assets/GearEngine/Art/Textures/TrackThemes/T_DesertGroundSoft.png"));
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
            GameObject root = new GameObject("RaceLayoutTest", typeof(RectTransform), typeof(Canvas));
            root.SetActive(false);
            Canvas raceCanvas = root.GetComponent<Canvas>();
            raceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            raceCanvas.sortingOrder = 20;
            GameObject boardRoot = new GameObject("SharedBoard", typeof(RectTransform), typeof(Canvas),
                typeof(GearEngine.Presentation.UI.BoardView));
            boardRoot.transform.SetParent(root.transform);
            GameObject cameraObject = new GameObject("BoardCamera", typeof(Camera));
            cameraObject.transform.SetParent(root.transform);
            Camera boardCamera = cameraObject.GetComponent<Camera>();
            Canvas boardCanvas = boardRoot.GetComponent<Canvas>();
            boardCanvas.worldCamera = boardCamera;
            boardCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            boardCanvas.sortingOrder = 11;
            GameObject boardObject = new GameObject("Board", typeof(RectTransform),
                typeof(GearEngine.Presentation.UI.BoardViewComponent));
            boardObject.transform.SetParent(boardRoot.transform);
            GearEngine.Presentation.UI.BoardView shared = boardRoot.GetComponent<GearEngine.Presentation.UI.BoardView>();
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
                Assert.That(rect.anchorMin.y, Is.EqualTo(0.02f).Within(0.001f));
                Assert.That(rect.anchorMax.y, Is.EqualTo(0.32f).Within(0.001f));
                Assert.That(boardCanvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
                Assert.That(boardCanvas.sortingOrder, Is.GreaterThan(raceCanvas.sortingOrder));

                boardCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                boardCanvas.worldCamera = boardCamera;
                boardCanvas.sortingOrder = 0;
                typeof(Presentation.ActiveRaceView).GetMethod("ApplyRaceBoardLayout", flags).Invoke(view, null);
                Assert.That(boardCanvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
                Assert.That(boardCanvas.worldCamera, Is.Null);
                Assert.That(boardCanvas.sortingOrder, Is.GreaterThan(raceCanvas.sortingOrder));

                typeof(Presentation.ActiveRaceView).GetMethod("RestoreBoardLayout", flags).Invoke(view, null);
                Assert.That(rect.anchorMin, Is.EqualTo(new Vector2(0.1f, 0.1f)));
                Assert.That(rect.anchorMax, Is.EqualTo(new Vector2(0.9f, 0.4f)));
                Assert.That(boardCanvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceCamera));
                Assert.That(boardCanvas.worldCamera, Is.SameAs(boardCamera));
                Assert.That(boardCanvas.sortingOrder, Is.EqualTo(11));

                typeof(Presentation.ActiveRaceView).GetMethod("ApplyRaceBoardLayout", flags).Invoke(view, null);
                Assert.That(boardCanvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
                Assert.That(boardCanvas.worldCamera, Is.Null);
                Assert.That(boardCanvas.sortingOrder, Is.GreaterThan(raceCanvas.sortingOrder));
                typeof(Presentation.ActiveRaceView).GetMethod("RestoreBoardLayout", flags).Invoke(view, null);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RaceView_HidesSharedCapacityChipOnEveryEntry()
        {
            GameObject root = new GameObject("RaceCapacityTest", typeof(RectTransform));
            GameObject boardRoot = new GameObject("SharedBoard", typeof(RectTransform),
                typeof(GearEngine.Presentation.UI.BoardView));
            boardRoot.transform.SetParent(root.transform);
            GameObject boardObject = new GameObject("Board", typeof(RectTransform),
                typeof(GearEngine.Presentation.UI.BoardViewComponent));
            boardObject.transform.SetParent(boardRoot.transform);
            GameObject capacityChip = new GameObject("chips_cogs", typeof(RectTransform));
            capacityChip.transform.SetParent(boardObject.transform);
            GameObject upgradeCapacityChip = new GameObject("chips_cogs", typeof(RectTransform));
            upgradeCapacityChip.transform.SetParent(boardObject.transform);
            GearEngine.Presentation.UI.BoardView shared = boardRoot.GetComponent<GearEngine.Presentation.UI.BoardView>();
            SerializedObject sharedData = new SerializedObject(shared);
            sharedData.FindProperty("board").objectReferenceValue =
                boardObject.GetComponent<GearEngine.Presentation.UI.BoardViewComponent>();
            sharedData.ApplyModifiedPropertiesWithoutUndo();
            Presentation.ActiveRaceView view = root.AddComponent<Presentation.ActiveRaceView>();
            SerializedObject viewData = new SerializedObject(view);
            viewData.FindProperty("board").objectReferenceValue = shared;
            viewData.ApplyModifiedPropertiesWithoutUndo();
            MethodInfo hideCapacity = typeof(Presentation.ActiveRaceView).GetMethod(
                "HideBoardCapacityChip", BindingFlags.Instance | BindingFlags.NonPublic);

            try
            {
                hideCapacity.Invoke(view, null);
                Assert.That(capacityChip.activeSelf, Is.False);
                Assert.That(upgradeCapacityChip.activeSelf, Is.False);

                capacityChip.SetActive(true);
                upgradeCapacityChip.SetActive(true);
                hideCapacity.Invoke(view, null);
                Assert.That(capacityChip.activeSelf, Is.False,
                    "The shared capacity chip must be hidden again when a later race reuses the board.");
                Assert.That(upgradeCapacityChip.activeSelf, Is.False,
                    "A capacity chip reparented by the upgrade screen must also stay hidden during the next race.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase(1680f)]
        [TestCase(1920f)]
        [TestCase(2280f)]
        public void RaceHud_SitsBetweenTrackAndGearBoardAndHidesNumericRpm(float referenceHeight)
        {
            float boardTop = 0.32f * referenceHeight;
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
            Assert.That(hud.anchorMin.y, Is.EqualTo(0.42f).Within(0.001f));
            Assert.That(hud.anchorMin.y, Is.LessThan(trackViewport.anchorMin.y),
                "HUD anchor must remain below the track region.");

            float hudBottom = (hud.anchorMin.y * referenceHeight) + hud.anchoredPosition.y -
                (hud.sizeDelta.y * hud.pivot.y);
            Assert.That(hudBottom, Is.GreaterThan(boardTop), "HUD must remain above the gear board.");
            Assert.That(rpmLabel.gameObject.activeSelf, Is.True);
            Assert.That(rpmLabel.text, Is.EqualTo("RPM"));
            Assert.That(rpmLabel.rectTransform.anchoredPosition.y,
                Is.GreaterThan(rpmText.rectTransform.anchoredPosition.y));
            Assert.That(rpmText.gameObject.activeSelf, Is.False,
                "The numeric RPM value must stay hidden while the RPM label and bar remain visible.");
            Assert.That(rpmText.text, Is.EqualTo("0"));
            Assert.That(rpmPanel.sizeDelta.x, Is.GreaterThanOrEqualTo(680f));
            Assert.That(scorePanel.sizeDelta.x, Is.GreaterThanOrEqualTo(680f));
            AssertHudLabelOutline(view, rpmLabel);
            AssertHudLabelOutline(view, scoreLabel);
            Assert.That(serializedView.FindProperty("currentVelocityText").objectReferenceValue, Is.Not.Null);
            Assert.That(serializedView.FindProperty("currentGearText").objectReferenceValue, Is.Not.Null);
            Assert.That(serializedView.FindProperty("rpmSegments").arraySize, Is.EqualTo(3));
        }

        [Test]
        public void RaceViewport_FitsTheSharedTrackOnScreen()
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
                ActiveRaceView view = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<ActiveRaceView>(true))
                    .Single();
                typeof(ActiveRaceView).GetMethod("ConfigureTrackViewport",
                    BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, null);
                RectTransform viewport = view.transform.Find("TrackViewport") as RectTransform;
                Assert.That(viewport, Is.Not.Null);
                Assert.That(viewport.localScale, Is.EqualTo(Vector3.one));
                Assert.That(viewport.anchoredPosition, Is.EqualTo(Vector2.zero));
                Assert.That(viewport.sizeDelta, Is.EqualTo(Vector2.zero));
                Assert.That(viewport.anchorMin.y, Is.EqualTo(0.58f).Within(0.001f));
                Assert.That(viewport.anchorMax.y, Is.EqualTo(0.9f).Within(0.001f));

                FrustumFitAnchor anchor = viewport.GetComponent<FrustumFitAnchor>();
                SerializedObject serializedView = new SerializedObject(view);
                Transform track = ((Component)serializedView.FindProperty("track").objectReferenceValue).transform;
                Assert.That(anchor.TargetTransform, Is.SameAs(track));
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
        public void RaceGrid_UsesTwoRowsBehindFinishLine()
        {
            MethodInfo method = typeof(ActiveRaceView).GetMethod("GetGridPlacement",
                BindingFlags.Static | BindingFlags.NonPublic);
            Vector2[] placements = Enumerable.Range(1, 4)
                .Select(position => (Vector2)method.Invoke(null, new object[] { position, 100f }))
                .ToArray();

            Assert.That(placements.All(placement => placement.x < 0f), Is.True);
            Assert.That(placements[0].x, Is.EqualTo(placements[1].x));
            Assert.That(placements[2].x, Is.EqualTo(placements[3].x));
            Assert.That(placements[2].x, Is.LessThan(placements[0].x));
            Assert.That(placements[0].y, Is.LessThan(0f));
            Assert.That(placements[1].y, Is.GreaterThan(0f));
            Assert.That(placements[2].y, Is.EqualTo(placements[0].y));
            Assert.That(placements[3].y, Is.EqualTo(placements[1].y));
        }

        [Test]
        public void RaceLapLabel_StartsAtOneAndCapsAtTotal()
        {
            MethodInfo method = typeof(ActiveRaceView).GetMethod("GetDisplayedLap",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method.Invoke(null, new object[] { 0, 3 }), Is.EqualTo(1));
            Assert.That(method.Invoke(null, new object[] { 1, 3 }), Is.EqualTo(2));
            Assert.That(method.Invoke(null, new object[] { 2, 3 }), Is.EqualTo(3));
            Assert.That(method.Invoke(null, new object[] { 3, 3 }), Is.EqualTo(3));
        }

        [Test]
        public void RaceLapLabel_UsesYellowForFinalLapAndKeepsFinishedColorAfterCrossing()
        {
            MethodInfo method = typeof(ActiveRaceView).GetMethod("GetLapColor",
                BindingFlags.Static | BindingFlags.NonPublic);
            Color normalColor = new Color32(44, 57, 69, 255);
            Color finalLapColor = new Color32(255, 204, 0, 255);
            Color finishedLapColor = new Color32(239, 70, 78, 255);

            Assert.That(method.Invoke(null, new object[] { 1, 3, normalColor }), Is.EqualTo(normalColor));
            Assert.That(method.Invoke(null, new object[] { 2, 3, normalColor }),
                Is.EqualTo(finalLapColor));
            Assert.That(method.Invoke(null, new object[] { 3, 3, normalColor }),
                Is.EqualTo(finishedLapColor));
        }

        [Test]
        public void RacePositionMarker_FormatsOrdinalWithoutUnsupportedArrowGlyphs()
        {
            MethodInfo method = typeof(ActiveRaceView).GetMethod("GetRacePositionLabel",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.That(method.Invoke(null, new object[] { 1 }), Is.EqualTo("1st"));
            Assert.That(method.Invoke(null, new object[] { 2 }), Is.EqualTo("2nd"));
            Assert.That(method.Invoke(null, new object[] { 3 }), Is.EqualTo("3rd"));
            Assert.That(method.Invoke(null, new object[] { 4 }), Is.EqualTo("4th"));
            Assert.That(method.Invoke(null, new object[] { 11 }), Is.EqualTo("11th"));
        }

        [Test]
        public void RacePositionMarker_UsesReadableScreenSpaceOverlay()
        {
            const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
            Type viewType = typeof(ActiveRaceView);
            float offset = (float)viewType.GetField("k_positionMarkerScreenOffset", staticFlags).GetRawConstantValue();
            GameObject root = new GameObject("RaceView", typeof(RectTransform), typeof(Canvas));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            try
            {
                GameObject marker = (GameObject)viewType.GetMethod("CreatePositionMarkerVisual", staticFlags)
                    .Invoke(null, new object[]
                    {
                        root.GetComponent<RectTransform>(), TMP_Settings.defaultFontAsset, 1, true
                    });
                RectTransform markerRect = marker.GetComponent<RectTransform>();
                TextMeshProUGUI label = marker.GetComponentInChildren<TextMeshProUGUI>();
                Transform arrow = marker.transform.Find("PositionArrow");
                Image leftFill = arrow.Find("LeftFill").GetComponent<Image>();
                Image rightFill = arrow.Find("RightFill").GetComponent<Image>();
                Image leftOutline = arrow.Find("LeftOutline").GetComponent<Image>();
                Image rightOutline = arrow.Find("RightOutline").GetComponent<Image>();

                Assert.That(offset, Is.InRange(16f, 24f),
                    "The marker must stay close enough to identify its car on a phone screen.");
                Assert.That(marker.transform.parent, Is.SameAs(root.transform));
                Assert.That(marker.GetComponent<Canvas>(), Is.Null,
                    "The marker must inherit the screen overlay canvas instead of creating a flickering world canvas.");
                Assert.That(markerRect.localScale, Is.EqualTo(Vector3.one));
                Assert.That(markerRect.sizeDelta.x, Is.InRange(104f, 120f));
                Assert.That(markerRect.sizeDelta.y, Is.InRange(84f, 96f));
                Assert.That(markerRect.pivot, Is.EqualTo(new Vector2(0.5f, 0f)));
                Assert.That(label.fontSize, Is.GreaterThanOrEqualTo(48f));
                Assert.That(label.fontStyle.HasFlag(FontStyles.Bold), Is.True);
                Assert.That(label.outlineWidth, Is.GreaterThanOrEqualTo(0.2f));
                Assert.That(label.text, Is.EqualTo("1st"));
                Assert.That(label.color, Is.EqualTo((Color)new Color32(255, 213, 64, 255)));
                Color outlineColor = new Color32(53, 65, 74, 255);
                Assert.That((Color32)label.outlineColor, Is.EqualTo((Color32)outlineColor));
                Color playerColor = new Color32(255, 213, 64, 255);
                Assert.That(leftFill.color, Is.EqualTo(playerColor));
                Assert.That(rightFill.color, Is.EqualTo(playerColor));
                Assert.That(leftOutline.color, Is.EqualTo(outlineColor));
                Assert.That(rightOutline.color, Is.EqualTo(outlineColor));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RacePositionMarker_UsesGoldForPlayerAndCreamForOpponents()
        {
            const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
            Type viewType = typeof(ActiveRaceView);
            MethodInfo createMethod = viewType.GetMethod("CreatePositionMarkerVisual", staticFlags);
            GameObject root = new GameObject("RaceView", typeof(RectTransform), typeof(Canvas));

            try
            {
                GameObject opponent = (GameObject)createMethod.Invoke(null, new object[]
                {
                    root.GetComponent<RectTransform>(), TMP_Settings.defaultFontAsset, 1, false
                });
                GameObject player = (GameObject)createMethod.Invoke(null, new object[]
                {
                    root.GetComponent<RectTransform>(), TMP_Settings.defaultFontAsset, 4, true
                });

                Assert.That(opponent.GetComponentInChildren<TextMeshProUGUI>().color,
                    Is.EqualTo((Color)new Color32(248, 239, 220, 255)));
                Assert.That(opponent.transform.Find("PositionArrow"), Is.Null,
                    "Opponent labels must not add arrows to the track view.");
                Assert.That(player.GetComponentInChildren<TextMeshProUGUI>().color,
                    Is.EqualTo((Color)new Color32(255, 213, 64, 255)));
                Assert.That(player.transform.Find("PositionArrow/LeftFill").GetComponent<Image>().color,
                    Is.EqualTo((Color)new Color32(255, 213, 64, 255)));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RacePositionMarker_RenderMaterialHasNavyOutlineAndUntintedFace()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/GearEngine/Art/Fonts/BricolageGrotesque-VariableFont_opsz,wdth,wght SDF.asset");
            GameObject root = new GameObject("MarkerCanvas", typeof(RectTransform), typeof(Canvas));
            try
            {
                MethodInfo create = typeof(ActiveRaceView).GetMethod("CreatePositionMarkerVisual", BindingFlags.Static | BindingFlags.NonPublic);
                foreach (bool isPlayer in new[] { false, true })
                {
                    GameObject marker = (GameObject)create.Invoke(null, new object[] { root.transform, font, 3, isPlayer });
                    TextMeshProUGUI label = marker.GetComponentInChildren<TextMeshProUGUI>();
                    label.ForceMeshUpdate(true);
                    Assert.That(label.fontSharedMaterial.GetColor("_OutlineColor"),
                        Is.EqualTo((Color)new Color32(53, 65, 74, 255)),
                        "Check the shader material, not TMP's cached outlineColor property.");
                    Assert.That(label.fontSharedMaterial.GetColor("_FaceColor"), Is.EqualTo(Color.white));
                    Assert.That(label.fontSharedMaterial, Is.Not.SameAs(font.material));
                    Transform arrow = marker.transform.Find("PositionArrow");
                    if (isPlayer)
                    {
                        Assert.That(label.color, Is.EqualTo(arrow.Find("LeftFill").GetComponent<Image>().color));
                    }
                    else
                    {
                        Assert.That(arrow, Is.Null);
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RacePositionMarker_FinishOrderStaysLockedAfterCarsKeepMoving(bool sameTick)
        {
            Type markerType = typeof(ActiveRaceView).GetNestedType("RacePositionMarker", BindingFlags.NonPublic);
            MethodInfo update = typeof(ActiveRaceView).GetMethod("UpdatePositionStandings", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(update, Is.Not.Null);
            GameObject root = new GameObject("RaceView", typeof(RectTransform));
            SplineDriverConfig config = ScriptableObject.CreateInstance<SplineDriverConfig>();
            try
            {
                ActiveRaceView view = root.AddComponent<ActiveRaceView>();
                System.Collections.IList markers = (System.Collections.IList)typeof(ActiveRaceView)
                    .GetField("rankedPositionMarkers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
                SplineEvaluateDriver first = new global::GearEngine.CarSimulation.SplineSimulation.SplineEvaluateDriver(config, null);
                SplineEvaluateDriver second = new global::GearEngine.CarSimulation.SplineSimulation.SplineEvaluateDriver(config, null);
                object firstMarker = Activator.CreateInstance(markerType, new object[]
                {
                    root, root.transform, first, null, null, null, Array.Empty<Renderer>(), 4, true
                });
                object secondMarker = Activator.CreateInstance(markerType, new object[]
                {
                    root, root.transform, second, null, null, null, Array.Empty<Renderer>(), 1, false
                });
                markers.Add(secondMarker);
                markers.Add(firstMarker);
                first.State.CompletedLaps = 3;
                first.State.PreviousT = 0.99f;
                first.State.T = 0.01f;
                second.State.CompletedLaps = sameTick ? 3 : 2;
                second.State.PreviousT = 0.95f;
                second.State.T = sameTick ? 0.01f : 0.99f;
                update.Invoke(view, new object[] { 3 });
                Assert.That(markerType.GetProperty("FinishedPosition").GetValue(firstMarker), Is.EqualTo(1));
                second.State.CompletedLaps = 3;
                second.State.T = 0.8f;
                first.State.T = 0.001f;
                update.Invoke(view, new object[] { 3 });
                Assert.That(markers[0], Is.SameAs(firstMarker));
                Assert.That(markerType.GetProperty("FinishedPosition").GetValue(firstMarker), Is.EqualTo(1));
                Assert.That(markerType.GetProperty("FinishedPosition").GetValue(secondMarker), Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(config);
            }
        }

        [TestCase(-140f, -90f)]
        [TestCase(0f, 0f)]
        [TestCase(140f, 90f)]
        public void RacePositionMarker_StaysDirectlyAboveCar(float x, float y)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            Type viewType = typeof(ActiveRaceView);
            MethodInfo anchorMethod = viewType.GetMethod("CalculatePositionMarkerAnchor", flags);
            float offset = (float)viewType.GetField("k_positionMarkerScreenOffset", flags).GetRawConstantValue();
            Vector2 carAnchor = new Vector2(x, y);
            Vector2 markerSize = new Vector2(112f, 90f);
            Rect container = new Rect(-500f, -500f, 1000f, 1000f);

            Vector2 markerAnchor = (Vector2)anchorMethod.Invoke(null, new object[]
            {
                carAnchor,
                markerSize,
                new Vector2(0.5f, 0f),
                container
            });

            Assert.That(markerAnchor.x, Is.EqualTo(carAnchor.x).Within(0.001f));
            Assert.That(markerAnchor.y, Is.EqualTo(carAnchor.y + offset).Within(0.001f));
        }

        [Test]
        public void RacePositionMarker_RemainsAboveCarWhenRoadTopCannotContainFullMarker()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            Type viewType = typeof(ActiveRaceView);
            MethodInfo anchorMethod = viewType.GetMethod("CalculatePositionMarkerAnchor", flags);
            float offset = (float)viewType.GetField("k_positionMarkerScreenOffset", flags).GetRawConstantValue();
            Vector2 carTopAnchor = new Vector2(0f, 80f);

            Vector2 markerAnchor = (Vector2)anchorMethod.Invoke(null, new object[]
            {
                carTopAnchor,
                new Vector2(112f, 90f),
                new Vector2(0.5f, 0f),
                new Rect(-100f, -100f, 200f, 200f)
            });

            Assert.That(markerAnchor.y, Is.GreaterThanOrEqualTo(carTopAnchor.y + offset));
        }

        [Test]
        public void RacePositionMarker_UsesRenderedCarTopAsScreenAnchor()
        {
            MethodInfo highestYMethod = typeof(ActiveRaceView).GetMethod(
                "ToHighestVisibleScreenY", BindingFlags.Static | BindingFlags.NonPublic);
            GameObject cameraObject = new GameObject("RaceCamera", typeof(Camera));
            GameObject car = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.aspect = 1f;
                camera.pixelRect = new Rect(0f, 0f, 1000f, 1000f);
                camera.transform.position = new Vector3(0f, 0f, -10f);
                Renderer renderer = car.GetComponent<Renderer>();
                float centerScreenY = camera.WorldToScreenPoint(car.transform.position).y;

                float highestScreenY = (float)highestYMethod.Invoke(null, new object[]
                {
                    new[] { renderer }, camera, centerScreenY
                });

                Assert.That(highestScreenY, Is.GreaterThan(centerScreenY));
                Assert.That(highestScreenY, Is.EqualTo(camera.WorldToScreenPoint(renderer.bounds.max).y).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(car);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void RacePositionMarker_PlayerArrowRemainsFixedWhenCarRotates()
        {
            Type markerType = typeof(ActiveRaceView).GetNestedType("RacePositionMarker", BindingFlags.NonPublic);
            MethodInfo create = typeof(ActiveRaceView).GetMethod(
                "CreatePositionMarkerVisual", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo layout = typeof(ActiveRaceView).GetMethod("UpdatePositionMarkerLayout",
                BindingFlags.Static | BindingFlags.NonPublic);
            GameObject root = new GameObject("RaceMarkers", typeof(RectTransform), typeof(Canvas));
            root.SetActive(false);
            try
            {
                GameObject visual = (GameObject)create.Invoke(null, new object[]
                {
                    root.transform,
                    TMP_Settings.defaultFontAsset,
                    1,
                    true
                });
                Transform arrow = visual.transform.Find("PositionArrow");
                GameObject car = new GameObject("PlayerCar");
                car.transform.SetParent(root.transform, false);
                object marker = Activator.CreateInstance(markerType, new object[]
                {
                    visual,
                    car.transform,
                    null,
                    visual.GetComponentInChildren<TextMeshProUGUI>(),
                    arrow.Find("LeftFill").GetComponent<Image>(),
                    arrow.Find("RightFill").GetComponent<Image>(),
                    Array.Empty<Renderer>(),
                    1,
                    true
                });
                Vector2 carAnchor = new Vector2(64f, -80f);
                markerType.GetProperty("CarAnchoredPosition").SetValue(marker, carAnchor);

                foreach (float angle in new[] { 0f, 90f, 180f, 270f })
                {
                    car.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                    layout.Invoke(null, new object[] { marker, new Rect(-300f, -300f, 600f, 600f) });
                    RectTransform rect = (RectTransform)visual.transform;
                    Assert.That(rect.anchoredPosition.x, Is.EqualTo(carAnchor.x).Within(0.001f));
                    Assert.That(rect.anchoredPosition.y, Is.GreaterThan(carAnchor.y));
                    Assert.That(Quaternion.Angle(rect.localRotation, Quaternion.identity), Is.LessThan(0.001f));
                    Assert.That(Quaternion.Angle(arrow.localRotation, Quaternion.identity), Is.LessThan(0.001f));
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase("Assets/GearEngine/Prefabs/Campaign/Campaign_ResultPopupView.prefab")]
        [TestCase("Assets/GearEngine/Prefabs/Campaign/PFB_ReceivedRewardsView.prefab")]
        public void PostRaceScreens_ScrollSeamlessPatternDiagonallyWithoutReversing(string prefabPath)
        {
            GameObject root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
            try
            {
                Image background = root.GetComponentsInChildren<Image>(true).Single(image => image.name == "Background");
                MonoBehaviour scroll = background.GetComponents<MonoBehaviour>()
                    .SingleOrDefault(component => component.GetType().Name == "UiPatternParallax");
                Assert.That(scroll, Is.Not.Null, "Post-race screens require continuous diagonal background motion.");

                Texture2D baseTexture = (Texture2D)scroll.GetType()
                    .GetField("backgroundTexture", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(scroll);
                Texture2D patternTexture = (Texture2D)scroll.GetType()
                    .GetField("patternTexture", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(scroll);
                Assert.That(AssetDatabase.GetAssetPath(baseTexture),
                    Is.EqualTo("Assets/GearEngine/Art/UI/Background&Pattern/Background_blank.png"));
                Assert.That(AssetDatabase.GetAssetPath(patternTexture),
                    Is.EqualTo("Assets/GearEngine/Art/UI/Background&Pattern/Pattern_TrophyandCog.png"));

                MethodInfo initialize = scroll.GetType().GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
                initialize.Invoke(scroll, null);

                RawImage baseLayer = background.GetComponentsInChildren<RawImage>(true)
                    .Single(image => image.name == "PatternBase");
                RawImage scrollingLayer = background.GetComponentsInChildren<RawImage>(true)
                    .Single(image => image.name == "PatternScroll");
                Assert.That(background.enabled, Is.False,
                    "The former full-screen composite must be replaced by independently rendered layers.");
                Assert.That(baseLayer.texture, Is.SameAs(baseTexture));
                Assert.That(scrollingLayer.texture, Is.SameAs(patternTexture));
                Assert.That(patternTexture.wrapModeU, Is.EqualTo(TextureWrapMode.Repeat));
                Assert.That(patternTexture.wrapModeV, Is.EqualTo(TextureWrapMode.Repeat));
                Assert.That(scrollingLayer.uvRect.width, Is.GreaterThan(1f));
                Assert.That(scrollingLayer.uvRect.height, Is.GreaterThan(1f));
                Assert.That(background.GetComponents<MonoBehaviour>()
                    .Any(component => component.GetType().Name == "UIEffect"), Is.False,
                    "The scroll must use RawImage UV translation without a shader distortion effect.");

                MonoBehaviour oscillator = background.GetComponents<MonoBehaviour>()
                    .SingleOrDefault(component => component.GetType().Name == "SmoothOscillateBehaviour");
                Assert.That(oscillator, Is.Null,
                    "The pattern must not reverse direction or oscillate like a waving flag.");

                Rect before = scrollingLayer.uvRect;
                MethodInfo advance = scroll.GetType().GetMethod("Advance", BindingFlags.Instance | BindingFlags.NonPublic);
                advance.Invoke(scroll, new object[] { 1f });
                Rect after = scrollingLayer.uvRect;
                Assert.That(after.x, Is.Not.EqualTo(before.x));
                Assert.That(after.y, Is.Not.EqualTo(before.y));
                Assert.That(after.width, Is.EqualTo(before.width));
                Assert.That(after.height, Is.EqualTo(before.height));

                advance.Invoke(scroll, new object[] { 10000f });
                Rect wrapped = scrollingLayer.uvRect;
                Assert.That(wrapped.x, Is.InRange(0f, 1f));
                Assert.That(wrapped.y, Is.InRange(0f, 1f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ItemCards_UseTheSameReadableTitleLayoutInSelectionAndPopup()
        {
            GameObject root = new GameObject("ItemCard", typeof(RectTransform), typeof(ItemSlotView));
            GameObject labelObject = new GameObject("Title", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(root.transform, false);
            ItemSlotView item = root.GetComponent<ItemSlotView>();
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            typeof(ItemSlotView).GetField("nameLabel", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(item, label);

            try
            {
                typeof(ItemSlotView).GetMethod("ConfigureNameLabel", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(item, new object[] { null });

                Assert.That(label.color, Is.EqualTo((Color)new Color32(44, 57, 69, 255)));
                Assert.That(label.enableAutoSizing, Is.True);
                Assert.That(label.fontSizeMin, Is.EqualTo(20f));
                Assert.That(label.fontSizeMax, Is.EqualTo(30f));
                Assert.That(label.rectTransform.anchorMin, Is.EqualTo(Vector2.up));
                Assert.That(label.rectTransform.anchorMax, Is.EqualTo(Vector2.one));
                Assert.That(label.rectTransform.anchoredPosition, Is.EqualTo(new Vector2(0f, -75f)));
                Assert.That(label.rectTransform.sizeDelta, Is.EqualTo(new Vector2(-80f, 105f)));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ItemPopupCard_PrefabUsesSelectionCardProportionAndPlacesTitleAboveIcon()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/ItemPopup View.prefab");
            GameObject instance = Object.Instantiate(prefab);

            try
            {
                ItemSlotView slot = instance.GetComponentInChildren<ItemSlotView>(true);
                SerializedObject serializedSlot = new SerializedObject(slot);
                TMP_Text title = (TMP_Text)serializedSlot.FindProperty("nameLabel").objectReferenceValue;
                Image icon = (Image)serializedSlot.FindProperty("iconImage").objectReferenceValue;

                typeof(ItemSlotView).GetMethod("ConfigureNameLabel", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(slot, new object[] { null });

                RectTransform cardRect = (RectTransform)slot.transform;
                RectTransform titleRect = title.rectTransform;
                RectTransform iconRect = icon.rectTransform;
                float titleCenter = cardRect.InverseTransformPoint(
                    titleRect.TransformPoint(titleRect.rect.center)).y;
                float iconCenter = cardRect.InverseTransformPoint(
                    iconRect.TransformPoint(iconRect.rect.center)).y;

                Assert.That(cardRect.sizeDelta, Is.EqualTo(new Vector2(300f, 400f)));
                Assert.That(cardRect.sizeDelta.x / cardRect.sizeDelta.y, Is.EqualTo(0.75f));
                Assert.That(serializedSlot.FindProperty("useAuthoredTitleLayout").boolValue, Is.True);
                Assert.That(titleRect.anchorMin.y, Is.EqualTo(1f));
                Assert.That(titleRect.anchorMax.y, Is.EqualTo(1f));
                Assert.That(titleCenter, Is.GreaterThan(iconCenter));
                Assert.That(iconRect.sizeDelta, Is.EqualTo(new Vector2(225f, 225f)));
                Assert.That(icon.preserveAspect, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void SetupClose_HidesSharedInventoryAndCapacityChipBeforeRace()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/Setup View.prefab");
            GameObject instance = Object.Instantiate(prefab);
            instance.SetActive(false);
            SetupView view = instance.GetComponent<SetupView>();
            SerializedObject data = new SerializedObject(view);
            Component inventory = (Component)data.FindProperty("inventory").objectReferenceValue;
            GearEngine.Presentation.UI.BoardCapacityChipView chip =
                (GearEngine.Presentation.UI.BoardCapacityChipView)data.FindProperty("boardCapacityChip").objectReferenceValue;
            inventory.gameObject.SetActive(true);
            chip.CapacityLabel.transform.parent.gameObject.SetActive(true);

            try
            {
                typeof(SetupView).GetMethod("OnClose", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(view, new object[] { true });

                Assert.That(inventory.gameObject.activeSelf, Is.False);
                Assert.That(chip.CapacityLabel.transform.parent.gameObject.activeSelf, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void SetupTrackViewport_TargetsTheBoundTrack()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/Setup View.prefab");
            GameObject instance = Object.Instantiate(prefab);
            instance.SetActive(false);
            GameObject trackObject = new GameObject("SetupTrack");
            trackObject.transform.SetParent(instance.transform);
            TrackViewComponent track = trackObject.AddComponent<TrackViewComponent>();
            SetupView view = instance.GetComponent<SetupView>();
            SerializedObject viewData = new SerializedObject(view);
            viewData.FindProperty("track").objectReferenceValue = track;
            viewData.ApplyModifiedPropertiesWithoutUndo();

            try
            {
                typeof(SetupView).GetMethod("ConfigureTrackViewport", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(view, null);

                SerializedProperty anchors = viewData.FindProperty("openTransitionAnchors");
                FrustumFitAnchor anchor = (FrustumFitAnchor)anchors.GetArrayElementAtIndex(0).objectReferenceValue;
                SerializedObject anchorData = new SerializedObject(anchor);
                Assert.That(
                    anchorData.FindProperty("targetTransform").objectReferenceValue,
                    Is.EqualTo(track.transform));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void SetupPanels_UseSharedNineSlicedRoundedCorners()
        {
            const string scenePath = "Assets/GearEngine/Scenes/Main Scene.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                SetupView setup = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<SetupView>(true)).Single();
                SerializedObject setupData = new SerializedObject(setup);
                Component board = (Component)setupData.FindProperty("boardView").objectReferenceValue;
                Component inventory = (Component)setupData.FindProperty("inventory").objectReferenceValue;
                Image boardPanel = board.GetComponentsInChildren<Image>(true)
                    .Single(image => image.name == "GridBoardViewComponent");
                Image inventoryPanel = inventory.GetComponent<Image>();
                Sprite roundedPanel = AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/GearEngine/Art/UI/ClubSport/T_RoundedPanel.png");

                Assert.That(roundedPanel, Is.Not.Null);
                Assert.That(roundedPanel.border.x, Is.GreaterThan(0f));
                Assert.That(boardPanel.sprite, Is.SameAs(roundedPanel));
                Assert.That(inventoryPanel.sprite, Is.SameAs(roundedPanel));
                Assert.That(boardPanel.type, Is.EqualTo(Image.Type.Sliced));
                Assert.That(inventoryPanel.type, Is.EqualTo(Image.Type.Sliced));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void SetupHeader_AlignsWithHomeScreenTitleHierarchy()
        {
            GameObject setupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/Setup View.prefab");
            GameObject homePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/Main View.prefab");
            GameObject setup = Object.Instantiate(setupPrefab);
            GameObject home = Object.Instantiate(homePrefab);

            try
            {
                TMP_Text setupEyebrow = setup.GetComponentsInChildren<TMP_Text>(true)
                    .Single(label => label.text == "BUILD YOUR ENGINE");
                TMP_Text setupTitle = setup.GetComponentsInChildren<TMP_Text>(true)
                    .Single(label => label.text == "Place the cogs.");
                TMP_Text homeEyebrow = home.GetComponentsInChildren<TMP_Text>(true)
                    .Single(label => label.name == "Track");
                TMP_Text homeTitle = home.GetComponentsInChildren<TMP_Text>(true)
                    .Single(label => label.name == "CountryPlace");

                float setupSpacing = (setupEyebrow.rectTransform.anchoredPosition.y -
                    setupTitle.rectTransform.anchoredPosition.y) * setupEyebrow.rectTransform.parent.localScale.y;
                float homeSpacing = (homeEyebrow.rectTransform.anchoredPosition.y -
                    homeTitle.rectTransform.anchoredPosition.y) * homeEyebrow.rectTransform.parent.localScale.y;

                Assert.That(setupSpacing, Is.EqualTo(homeSpacing + 16f).Within(0.1f),
                    "The compressed setup eyebrow needs extra separation to align visually with the home title.");
            }
            finally
            {
                Object.DestroyImmediate(setup);
                Object.DestroyImmediate(home);
            }
        }

        [Test]
        public void RacePositionRanking_TreatsStartingGridAsBeforeFinishLine()
        {
            MethodInfo method = typeof(ActiveRaceView).GetMethod("CalculateRaceProgress",
                BindingFlags.Static | BindingFlags.NonPublic);

            float waitingAtGrid = (float)method.Invoke(null, new object[] { 0, 0.9f, true });
            float crossedStartLine = (float)method.Invoke(null, new object[] { 0, 0.05f, false });
            float nextLap = (float)method.Invoke(null, new object[] { 1, 0.2f, false });

            Assert.That(waitingAtGrid, Is.EqualTo(-0.1f).Within(0.001f));
            Assert.That(crossedStartLine, Is.EqualTo(0.05f).Within(0.001f));
            Assert.That(nextLap, Is.EqualTo(1.2f).Within(0.001f));
        }

        [Test]
        public void RaceBackdrop_StaysAboveOtherScreensAndRestoresGround()
        {
            MethodInfo scaleMethod = typeof(ActiveRaceView).GetMethod("GetRaceGroundScale",
                BindingFlags.Static | BindingFlags.NonPublic);
            Vector3 wideTrackScale = (Vector3)scaleMethod.Invoke(null, new object[]
            {
                new Vector3(100f, 0f, 50f), new Vector3(10f, 0f, 10f), 30f
            });
            Assert.That(wideTrackScale.x, Is.EqualTo(13.6f).Within(0.001f));
            Assert.That(wideTrackScale.z, Is.EqualTo(8.6f).Within(0.001f));

            const string scenePath = "Assets/GearEngine/Scenes/Main Scene.unity";
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool wasAlreadyLoaded = scene.isLoaded;
            if (!wasAlreadyLoaded)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }
            try
            {
                ActiveRaceView view = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<ActiveRaceView>(true))
                    .Single();
                SerializedObject serializedView = new SerializedObject(view);
                Transform track = ((Component)serializedView.FindProperty("track").objectReferenceValue).transform;
                Transform ground = track.Find("Floor");
                Canvas canvas = view.GetComponent<Canvas>();
                Vector3 originalScale = ground.localScale;
                Vector3 originalPosition = ground.localPosition;
                int originalOrder = canvas.sortingOrder;
                MethodInfo configure = typeof(ActiveRaceView).GetMethod("ConfigureRaceBackdrop",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo restore = typeof(ActiveRaceView).GetMethod("RestoreRaceBackdrop",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                try
                {
                    configure.Invoke(view, null);
                    Assert.That(ground.localScale.x, Is.LessThan(originalScale.x));
                    Assert.That(ground.localScale.x, Is.GreaterThan(originalScale.x * 0.4f));
                    Assert.That(ground.localScale.z, Is.LessThan(originalScale.z));
                    Assert.That(canvas.sortingOrder, Is.GreaterThan(10));
                    RectTransform backdrop = view.transform.Find("RaceHudBackdrop") as RectTransform;
                    Assert.That(backdrop, Is.Not.Null);
                    Assert.That(backdrop.anchorMin, Is.EqualTo(new Vector2(0.5f, 0.5f)));
                    Assert.That(backdrop.anchorMax, Is.EqualTo(backdrop.anchorMin));
                    Assert.That(backdrop.sizeDelta.x, Is.GreaterThan(0f));
                    Assert.That(backdrop.sizeDelta.y, Is.GreaterThan(0f));
                    Assert.That(backdrop.GetSiblingIndex(),
                        Is.LessThan(view.transform.Find("Container").GetSiblingIndex()));
                    Image surface = backdrop.Find("Surface").GetComponent<Image>();
                    Image shadow = backdrop.Find("Shadow").GetComponent<Image>();
                    Assert.That(surface.type, Is.EqualTo(Image.Type.Sliced));
                    Assert.That(surface.sprite.border.x, Is.GreaterThan(0f));
                    Assert.That(surface.raycastTarget, Is.False);
                    Assert.That(shadow.raycastTarget, Is.False);
                    Assert.That(shadow.rectTransform.anchoredPosition.y, Is.LessThan(0f));
                    RectTransform scorePanel = view.transform.Find("Container/CarStatusHud/Score_PanelRace") as RectTransform;
                    TMP_Text velocity = (TMP_Text)serializedView.FindProperty("currentVelocityText").objectReferenceValue;
                    AssertCardContains(backdrop, scorePanel);
                    AssertCardContains(backdrop, velocity.rectTransform.parent as RectTransform);
                }
                finally
                {
                    restore.Invoke(view, null);
                }

                Assert.That(ground.localScale, Is.EqualTo(originalScale));
                Assert.That(ground.localPosition, Is.EqualTo(originalPosition));
                Assert.That(canvas.sortingOrder, Is.EqualTo(originalOrder));
                Assert.That(view.transform.Find("RaceHudBackdrop"), Is.Null);
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
        public void SetupAfterRace_KeepsControlsAndSharedBoardAboveTrack()
        {
            const string scenePath = "Assets/GearEngine/Scenes/Main Scene.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            try
            {
                ActiveRaceView race = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<ActiveRaceView>(true)).Single();
                SetupView setup = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<SetupView>(true)).Single();
                SerializedObject setupData = new SerializedObject(setup);
                Component board = (Component)setupData.FindProperty("boardView").objectReferenceValue;
                Canvas boardCanvas = board.GetComponent<Canvas>();
                for (int run = 0; run < 2; run++)
                {
                    typeof(ActiveRaceView).GetMethod("ApplyRaceBoardLayout", flags).Invoke(race, null);
                    typeof(ActiveRaceView).GetMethod("OnClose", flags).Invoke(race, new object[] { false });
                    Assert.That(setup.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay),
                        "Setup controls must render above track geometry on every return.");
                    Assert.That(boardCanvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay),
                        "Race cleanup must restore an overlay board for the next setup.");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void RaceClose_ReleasesCarsBeforeSharedTrackIsReused()
        {
            GameObject root = new GameObject("RaceCloseTest", typeof(RectTransform), typeof(Canvas));
            root.SetActive(false);
            GameObject carRoot = new GameObject("PreviousRaceCar");
            carRoot.SetActive(false);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            try
            {
                ActiveRaceView race = root.AddComponent<ActiveRaceView>();
                CarView car = carRoot.AddComponent<CarView>();
                List<CarView> cars = (List<CarView>)typeof(ActiveRaceView)
                    .GetField("spawnedCars", flags).GetValue(race);
                cars.Add(car);
                typeof(ActiveRaceView).GetMethod("OnClose", flags).Invoke(race, new object[] { false });
                Assert.That(cars, Is.Empty, "Closing a race must release cars before setup reactivates the shared track.");
                Assert.That(car == null, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
                if (carRoot != null)
                {
                    Object.DestroyImmediate(carRoot);
                }
            }
        }

        [Test]
        public void RaceView_HidingRestoresSharedScenePresentation()
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
                ActiveRaceView view = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<ActiveRaceView>(true))
                    .Single();
                SerializedObject serializedView = new SerializedObject(view);
                GameObject track = ((Component)serializedView.FindProperty("track").objectReferenceValue).gameObject;
                GameObject board = ((Component)serializedView.FindProperty("board").objectReferenceValue).gameObject;
                GameObject telemetry = ((Component)serializedView.FindProperty("telemetry").objectReferenceValue).gameObject;
                Transform ground = track.transform.Find("Floor");
                Canvas canvas = view.GetComponent<Canvas>();
                bool trackWasActive = track.activeSelf;
                bool boardWasActive = board.activeSelf;
                bool telemetryWasActive = telemetry.activeSelf;
                Vector3 groundScale = ground.localScale;
                Vector3 groundPosition = ground.localPosition;
                int canvasOrder = canvas.sortingOrder;
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                MethodInfo configure = typeof(ActiveRaceView).GetMethod("ConfigureRaceBackdrop", flags);
                MethodInfo setRootsActive = typeof(ActiveRaceView).GetMethod("SetRaceSceneRootsActive", flags);
                MethodInfo close = typeof(ActiveRaceView).GetMethod("OnClose", flags);
                MethodInfo restore = typeof(ActiveRaceView).GetMethod("RestoreRaceBackdrop", flags);

                try
                {
                    configure.Invoke(view, null);
                    setRootsActive.Invoke(view, new object[] { true });
                    close.Invoke(view, new object[] { true });

                    Assert.That(track.activeSelf, Is.False);
                    Assert.That(board.activeSelf, Is.False);
                    Assert.That(telemetry.activeSelf, Is.False);
                    Assert.That(ground.localScale, Is.EqualTo(groundScale));
                    Assert.That(ground.localPosition, Is.EqualTo(groundPosition));
                    Assert.That(canvas.sortingOrder, Is.EqualTo(canvasOrder));
                    Assert.That(view.transform.Find("RaceHudBackdrop"), Is.Null);
                }
                finally
                {
                    restore.Invoke(view, null);
                    track.SetActive(trackWasActive);
                    board.SetActive(boardWasActive);
                    telemetry.SetActive(telemetryWasActive);
                }
            }
            finally
            {
                if (!wasAlreadyLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void AssertCardContains(RectTransform card, RectTransform content)
        {
            Assert.That(content, Is.Not.Null);
            Vector3[] corners = new Vector3[4];
            content.GetWorldCorners(corners);
            foreach (Vector3 corner in corners)
            {
                Vector3 local = card.InverseTransformPoint(corner);
                Assert.That(local.x, Is.InRange(card.rect.xMin, card.rect.xMax));
                Assert.That(local.y, Is.InRange(card.rect.yMin, card.rect.yMax));
            }
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
                Assert.That(multiplier.color, Is.EqualTo((Color)new Color32(245, 239, 226, 255)));
                Assert.That(driftPoints.sizeDelta, Is.EqualTo(new Vector2(-650f, 150f)));
                Assert.That(score.sizeDelta, Is.EqualTo(new Vector2(255f, 105f)));
                Assert.That(multiplierBackground.sizeDelta, Is.EqualTo(new Vector2(170f, 150f)));
                Assert.That(multiplierBackground.anchoredPosition, Is.EqualTo(new Vector2(130f, 0f)));
                Assert.That(multiplierBackground.GetSiblingIndex(), Is.LessThan(score.GetSiblingIndex()),
                    "The multiplier badge must render behind the multiplier without covering the score.");
                Assert.That(obsoleteLayout.enabled, Is.False,
                    "The score and multiplier use explicit compact positions inside one badge.");

                string[] badgeNames = { "basic", "commum", "rare", "epic", "Legendary", "Legendary" };
                Color[] textColors =
                {
                    new Color32(245, 239, 226, 255), expectedHudColor,
                    new Color32(245, 239, 226, 255), new Color32(245, 239, 226, 255),
                    expectedHudColor, expectedHudColor,
                };
                for (int multiplierValue = 1; multiplierValue <= badgeNames.Length; multiplierValue++)
                {
                    viewModel.CurrentMultiplier = multiplierValue;
                    Sprite expectedBadge = AssetDatabase.LoadAllAssetsAtPath(
                        $"Assets/GearEngine/Art/UI/Panel&Bars/Panel_{badgeNames[multiplierValue - 1]}_multiplier.png")
                        .OfType<Sprite>().Single();
                    Assert.That(multiplier.text, Is.EqualTo($"{multiplierValue}x"));
                    Assert.That(multiplierBackground.GetComponent<Image>().sprite, Is.SameAs(expectedBadge));
                    Assert.That(multiplier.color, Is.EqualTo(textColors[multiplierValue - 1]));
                }
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
