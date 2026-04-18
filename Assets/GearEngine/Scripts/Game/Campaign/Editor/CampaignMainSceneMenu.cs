using GearEngine.Campaign.Bootstrap;
using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Tracks;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation;
using GearEngine.GearEngine.Presentation.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GearEngine.Campaign.Editor
{
    public static class CampaignMainSceneMenu
    {
        private const string RaceScenePath = "Assets/GearEngine/Scenes/Race Scene.unity";
        private const string MainScenePath = "Assets/GearEngine/Scenes/Main Scene.unity";
        private const string DataFolder = "Assets/GearEngine/Data/Campaign";
        private const string PrefabFolder = "Assets/GearEngine/Prefabs/Campaign";
        private const string RaceViewConfigPath = "Assets/GearEngine/Data/Race/RaceViewConfig.asset";
        private const string NavigationSettingsPath = "Assets/Navigation/Navigation Settings.asset";
        private const string GearStubPath = "Assets/GearEngine/Prefabs/GearEngineView_NavigationStub.prefab";

        private const string GuidTrackPrefab = "a6f20e9e09e7715449856810b9a31e98";
        private const string GuidBoardConfig = "c8936b7a974414cf6aba78a5ac6be445";
        private const string GuidFeatureToggle = "e8f9a0b1c2d34432bcdef01234567891";
        private const string GuidTrackDefinition = "d2d2d2d2b2a3948576c5d4e3f2a1b0c8";
        private const string GuidCarDefinition = "b4e1eaf4e8dd58b418b4e6efc603f72e";
        private const string GuidGearA = "39144108699f44d10b422acf50e748ba";
        private const string GuidGearB = "3783c6d85510a4bf6b4f879d30ef249b";
        private const string GuidGearC = "b642772cacb3f48d7870118c94a3e3a0";

        private static readonly System.Type RaceScopeType =
            System.Type.GetType("GearEngine.Race.Bootstrap.RaceScope, Game.Race");

        private static readonly System.Type RaceBootstrapType =
            System.Type.GetType("GearEngine.Race.Bootstrap.RaceBootstrap, Game.Race");

        private static readonly System.Type CampaignScopeType =
            System.Type.GetType("GearEngine.Campaign.Bootstrap.CampaignScope, Game.Campaign");

        [MenuItem("GearEngine/Campaign/Build Main Scene And Assets")]
        public static void BuildAll()
        {
            EnsureFolder(DataFolder);
            EnsureFolder(PrefabFolder);
            string mainPrefab = BuildMainViewPrefab();
            string setupPrefab = BuildSetupViewPrefab();
            string racePrefab = BuildActiveRaceViewPrefab();
            string resultPrefab = BuildResultPopupPrefab();
            string roguePrefab = BuildRoguelikeViewPrefab(setupPrefab);
            CreateOrUpdateViewConfig("CampaignMainViewConfig", mainPrefab, typeof(MainView), typeof(MainViewModel));
            CreateOrUpdateViewConfig("CampaignSetupViewConfig", setupPrefab, typeof(SetupView), typeof(SetupViewModel));
            CreateOrUpdateViewConfig("CampaignActiveRaceViewConfig", racePrefab, typeof(ActiveRaceView), typeof(ActiveRaceViewModel));
            CreateOrUpdateViewConfig("CampaignResultPopupViewConfig", resultPrefab, typeof(ResultPopupView), typeof(ResultPopupViewModel));
            CreateOrUpdateViewConfig("CampaignRoguelikeViewConfig", roguePrefab, typeof(RoguelikeView), typeof(RoguelikeViewModel));
            AppendCampaignScreensToNavigation();
            BuildMainSceneFromRaceTemplate();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Campaign] Build Main Scene And Assets completed.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int lastSlash = path.LastIndexOf('/');
            string parent = lastSlash > 0 ? path.Substring(0, lastSlash) : "Assets";
            string name = lastSlash >= 0 ? path.Substring(lastSlash + 1) : path;
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }

        private static string BuildMainViewPrefab()
        {
            var root = new GameObject("MainView_Root");
            try
            {
                var trackAnchor = new GameObject("TrackAnchor");
                trackAnchor.transform.SetParent(root.transform, false);
                trackAnchor.transform.localPosition = new Vector3(0f, 4.87f, 0f);
                trackAnchor.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);
                trackAnchor.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                string trackPath = AssetDatabase.GUIDToAssetPath(GuidTrackPrefab);
                var trackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(trackPath);
                Object trackInstance = PrefabUtility.InstantiatePrefab(trackPrefab, trackAnchor.transform);
                ((GameObject)trackInstance).transform.localPosition = Vector3.zero;
                Track track = ((GameObject)trackInstance).GetComponentInChildren<Track>(true);
                GameObject ui = CreateCanvasUiRoot(root.transform, "MainUI", 10);
                var mainView = ui.AddComponent<MainView>();
                var statsGo = new GameObject("StatsPanel");
                statsGo.transform.SetParent(ui.transform, false);
                var statsRt = statsGo.AddComponent<RectTransform>();
                StretchFull(statsRt);
                var statsComp = statsGo.AddComponent<TrackStatsViewComponent>();
                CreateTmp(statsGo.transform, "TrackName", new Vector2(0f, 120f), out TextMeshProUGUI trackName);
                CreateTmp(statsGo.transform, "TargetLaps", new Vector2(0f, 80f), out TextMeshProUGUI laps);
                CreateTmp(statsGo.transform, "TargetTime", new Vector2(0f, 40f), out TextMeshProUGUI time);
                WireStatsSerialized(statsComp, trackName, laps, time);
                Button play = CreateButton(ui.transform, "PlayButton", "Play", new Vector2(0f, -140f));
                WireMainViewSerialized(mainView, track, play, statsComp);
                string dest = $"{PrefabFolder}/Campaign_MainView.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, dest);
                return dest;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void WireStatsSerialized(TrackStatsViewComponent stats, TextMeshProUGUI a, TextMeshProUGUI b, TextMeshProUGUI c)
        {
            var so = new SerializedObject(stats);
            so.FindProperty("trackNameLabel").objectReferenceValue = a;
            so.FindProperty("targetLapsLabel").objectReferenceValue = b;
            so.FindProperty("targetTimeLabel").objectReferenceValue = c;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireMainViewSerialized(MainView mainView, Track track, Button play, TrackStatsViewComponent stats)
        {
            var so = new SerializedObject(mainView);
            so.FindProperty("track").objectReferenceValue = track;
            so.FindProperty("playButton").objectReferenceValue = play;
            so.FindProperty("statsPanel").objectReferenceValue = stats;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string BuildActiveRaceViewPrefab()
        {
            var root = new GameObject("ActiveRace_Root");
            try
            {
                var trackAnchor = new GameObject("TrackAnchor");
                trackAnchor.transform.SetParent(root.transform, false);
                trackAnchor.transform.localPosition = new Vector3(0f, 4.87f, 0f);
                trackAnchor.transform.localScale = new Vector3(0.13f, 0.13f, 0.13f);
                trackAnchor.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                string trackPath = AssetDatabase.GUIDToAssetPath(GuidTrackPrefab);
                var trackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(trackPath);
                Object inst = PrefabUtility.InstantiatePrefab(trackPrefab, trackAnchor.transform);
                Track track = ((GameObject)inst).GetComponentInChildren<Track>(true);
                GameObject ui = CreateCanvasUiRoot(root.transform, "RaceUI", 10);
                var hudGo = new GameObject("Hud");
                hudGo.transform.SetParent(ui.transform, false);
                var hudRt = hudGo.AddComponent<RectTransform>();
                StretchFull(hudRt);
                var hud = hudGo.AddComponent<RaceHudViewComponent>();
                CreateTmp(hudGo.transform, "LapTime", new Vector2(-400f, 200f), out TextMeshProUGUI lapTime);
                CreateTmp(hudGo.transform, "LapCount", new Vector2(-400f, 160f), out TextMeshProUGUI lapCount);
                WireHudSerialized(hud, lapTime, lapCount);
                var raceView = ui.AddComponent<ActiveRaceView>();
                WireActiveRaceSerialized(raceView, track, hud);
                string dest = $"{PrefabFolder}/Campaign_ActiveRaceView.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, dest);
                return dest;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void WireHudSerialized(RaceHudViewComponent hud, TextMeshProUGUI lapTime, TextMeshProUGUI lapCount)
        {
            var so = new SerializedObject(hud);
            so.FindProperty("lapTimeLabel").objectReferenceValue = lapTime;
            so.FindProperty("lapCountLabel").objectReferenceValue = lapCount;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireActiveRaceSerialized(ActiveRaceView v, Track track, RaceHudViewComponent hud)
        {
            var so = new SerializedObject(v);
            so.FindProperty("track").objectReferenceValue = track;
            so.FindProperty("hud").objectReferenceValue = hud;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string BuildResultPopupPrefab()
        {
            var root = new GameObject("ResultPopup_Root");
            try
            {
                GameObject ui = CreateCanvasUiRoot(root.transform, "ResultUI", 20);
                var v = ui.AddComponent<ResultPopupView>();
                CreateTmp(ui.transform, "RaceTime", new Vector2(0f, 100f), out TextMeshProUGUI t1);
                CreateTmp(ui.transform, "Laps", new Vector2(0f, 60f), out TextMeshProUGUI t2);
                CreateTmp(ui.transform, "Score", new Vector2(0f, 20f), out TextMeshProUGUI t3);
                CreateTmp(ui.transform, "Gold", new Vector2(0f, -20f), out TextMeshProUGUI t4);
                Button upgrade = CreateButton(ui.transform, "Upgrade", "Upgrade", new Vector2(-120f, -100f));
                Button cont = CreateButton(ui.transform, "Continue", "Continue", new Vector2(120f, -100f));
                WireResultSerialized(v, t1, t2, t3, t4, upgrade, cont);
                string dest = $"{PrefabFolder}/Campaign_ResultPopupView.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, dest);
                return dest;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void WireResultSerialized(
            ResultPopupView v,
            TextMeshProUGUI a,
            TextMeshProUGUI b,
            TextMeshProUGUI c,
            TextMeshProUGUI d,
            Button u,
            Button k)
        {
            var so = new SerializedObject(v);
            so.FindProperty("raceTimeLabel").objectReferenceValue = a;
            so.FindProperty("lapCountLabel").objectReferenceValue = b;
            so.FindProperty("scoreLabel").objectReferenceValue = c;
            so.FindProperty("goldLabel").objectReferenceValue = d;
            so.FindProperty("upgradeButton").objectReferenceValue = u;
            so.FindProperty("continueButton").objectReferenceValue = k;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string BuildSetupViewPrefab()
        {
            string dest = $"{PrefabFolder}/Campaign_SetupView.prefab";
            AssetDatabase.CopyAsset(GearStubPath, dest);
            using (var scope = new PrefabUtility.EditPrefabContentsScope(dest))
            {
                GameObject prefabRoot = scope.prefabContentsRoot;
                Transform trackAnchor = new GameObject("TrackAnchor").transform;
                trackAnchor.SetParent(prefabRoot.transform, false);
                trackAnchor.localPosition = new Vector3(0f, 4.87f, 0f);
                trackAnchor.localScale = new Vector3(0.13f, 0.13f, 0.13f);
                trackAnchor.localRotation = Quaternion.Euler(90f, 0f, 0f);
                string trackPath = AssetDatabase.GUIDToAssetPath(GuidTrackPrefab);
                var tp = AssetDatabase.LoadAssetAtPath<GameObject>(trackPath);
                Object tInst = PrefabUtility.InstantiatePrefab(tp, trackAnchor);
                Track track = ((GameObject)tInst).GetComponentInChildren<Track>(true);
                GearEngineView old = prefabRoot.GetComponent<GearEngineView>();
                if (old != null)
                {
                    Object.DestroyImmediate(old, true);
                }

                var setup = prefabRoot.AddComponent<SetupView>();
                var board = prefabRoot.GetComponentInChildren<BoardViewComponent>(true);
                var inv = prefabRoot.GetComponentInChildren<GearInventoryViewComponent>(true);
                var trash = prefabRoot.GetComponentInChildren<TrashDropZoneViewComponent>(true);
                Transform canvasParent = board != null ? board.transform : prefabRoot.transform;
                Button race = CreateButton(FindOrCreateUiParent(prefabRoot), "RaceButton", "Race", new Vector2(0f, -200f));
                WireSetupSerialized(setup, track, board, inv, trash, race);
            }

            return dest;
        }

        private static Transform FindOrCreateUiParent(GameObject prefabRoot)
        {
            Transform ve = prefabRoot.transform.Find("View Elements");
            if (ve != null)
            {
                return ve;
            }

            Transform core = prefabRoot.transform.Find("GearEngineCoreViewComponent");
            if (core != null)
            {
                Transform inner = core.Find("View Elements");
                if (inner != null)
                {
                    return inner;
                }
            }

            return prefabRoot.transform;
        }

        private static void WireSetupSerialized(
            SetupView setup,
            Track track,
            BoardViewComponent board,
            GearInventoryViewComponent inv,
            TrashDropZoneViewComponent trash,
            Button race)
        {
            var so = new SerializedObject(setup);
            so.FindProperty("track").objectReferenceValue = track;
            so.FindProperty("boardView").objectReferenceValue = board;
            so.FindProperty("inventoryView").objectReferenceValue = inv;
            so.FindProperty("trashDropZone").objectReferenceValue = trash;
            so.FindProperty("raceButton").objectReferenceValue = race;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string BuildRoguelikeViewPrefab(string setupPrefabPath)
        {
            string dest = $"{PrefabFolder}/Campaign_RoguelikeView.prefab";
            AssetDatabase.CopyAsset(setupPrefabPath, dest);
            using (var scope = new PrefabUtility.EditPrefabContentsScope(dest))
            {
                GameObject root = scope.prefabContentsRoot;
                var setup = root.GetComponent<SetupView>();
                if (setup != null)
                {
                    Object.DestroyImmediate(setup, true);
                }

                var rogue = root.AddComponent<RoguelikeView>();
                var board = root.GetComponentInChildren<BoardViewComponent>(true);
                var inv = root.GetComponentInChildren<GearInventoryViewComponent>(true);
                var trash = root.GetComponentInChildren<TrashDropZoneViewComponent>(true);
                Transform uiParent = FindOrCreateUiParent(root);
                Object.DestroyImmediate(uiParent.Find("RaceButton")?.gameObject, true);
                Button confirm = CreateButton(uiParent, "ConfirmButton", "Confirm", new Vector2(0f, -260f));
                var cards = new CardOptionView[3];
                for (int i = 0; i < 3; i++)
                {
                    float x = (i - 1) * 180f;
                    cards[i] = CreateCardOption(uiParent, $"Card_{i}", new Vector2(x, -120f));
                }

                WireRoguelikeSerialized(rogue, board, inv, trash, cards, confirm);
            }

            return dest;
        }

        private static CardOptionView CreateCardOption(Transform parent, string name, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 200f);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            var hl = new GameObject("Highlight");
            hl.transform.SetParent(go.transform, false);
            var hlRt = hl.AddComponent<RectTransform>();
            StretchFull(hlRt);
            var hlImg = hl.AddComponent<Image>();
            hlImg.color = new Color(1f, 0.85f, 0.2f, 0.35f);
            hl.SetActive(false);
            var btnGo = new GameObject("Select");
            btnGo.transform.SetParent(go.transform, false);
            var btnRt = btnGo.AddComponent<RectTransform>();
            StretchFull(btnRt);
            var btn = btnGo.AddComponent<Button>();
            CreateTmp(go.transform, "Label", new Vector2(0f, 70f), out TextMeshProUGUI tmp);
            tmp.text = name;
            tmp.fontSize = 18;
            var card = go.AddComponent<CardOptionView>();
            WireCardOptionSerialized(card, tmp, hl, btn);
            return card;
        }

        private static void WireCardOptionSerialized(CardOptionView c, TextMeshProUGUI label, GameObject hl, Button btn)
        {
            var so = new SerializedObject(c);
            so.FindProperty("gearNameLabel").objectReferenceValue = label;
            so.FindProperty("selectedHighlight").objectReferenceValue = hl;
            so.FindProperty("selectButton").objectReferenceValue = btn;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireRoguelikeSerialized(
            RoguelikeView v,
            BoardViewComponent b,
            GearInventoryViewComponent i,
            TrashDropZoneViewComponent t,
            CardOptionView[] cards,
            Button confirm)
        {
            var so = new SerializedObject(v);
            so.FindProperty("boardView").objectReferenceValue = b;
            so.FindProperty("inventoryView").objectReferenceValue = i;
            so.FindProperty("trashDropZone").objectReferenceValue = t;
            so.FindProperty("confirmButton").objectReferenceValue = confirm;
            var arr = so.FindProperty("cardOptionViews");
            arr.arraySize = cards.Length;
            for (int j = 0; j < cards.Length; j++)
            {
                arr.GetArrayElementAtIndex(j).objectReferenceValue = cards[j];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateCanvasUiRoot(Transform parent, string name, int sortOrder)
        {
            var ui = new GameObject(name);
            ui.transform.SetParent(parent, false);
            var rt = ui.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var c = ui.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sortOrder;
            var sc = ui.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920f, 1080f);
            ui.AddComponent<GraphicRaycaster>();
            return ui;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void CreateTmp(Transform parent, string name, Vector2 pos, out TextMeshProUGUI tmp)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800f, 40f);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = name;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220f, 56f);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.45f, 0.85f, 1f);
            var btn = go.AddComponent<Button>();
            var child = new GameObject("Text");
            child.transform.SetParent(go.transform, false);
            var crt = child.AddComponent<RectTransform>();
            StretchFull(crt);
            var tmp = child.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            return btn;
        }

        private static void CreateOrUpdateViewConfig(string assetName, string prefabPath, System.Type viewType, System.Type vmType)
        {
            string dest = $"{DataFolder}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(dest) == null)
            {
                AssetDatabase.CopyAsset(RaceViewConfigPath, dest);
            }

            var cfg = AssetDatabase.LoadAssetAtPath<ScriptableObject>(dest);
            string viewAsm = $"{viewType.FullName}, {viewType.Assembly.GetName().Name}, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
            string vmAsm = $"{vmType.FullName}, {vmType.Assembly.GetName().Name}, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
            string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
            var so = new SerializedObject(cfg);
            so.FindProperty("viewType").FindPropertyRelative("serializedType").stringValue = $"\"{viewAsm}\"";
            so.FindProperty("controllerType").FindPropertyRelative("serializedType").stringValue = $"\"{vmAsm}\"";
            so.FindProperty("asset").FindPropertyRelative("m_AssetGUID").stringValue = prefabGuid;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AppendCampaignScreensToNavigation()
        {
            var nav = AssetDatabase.LoadAssetAtPath<ScriptableObject>(NavigationSettingsPath);
            var so = new SerializedObject(nav);
            SerializedProperty screens = so.FindProperty("screens");
            string[] names =
            {
                "CampaignMainViewConfig",
                "CampaignSetupViewConfig",
                "CampaignActiveRaceViewConfig",
                "CampaignResultPopupViewConfig",
                "CampaignRoguelikeViewConfig"
            };

            foreach (string n in names)
            {
                string path = $"{DataFolder}/{n}.asset";
                Object screen = AssetDatabase.LoadAssetAtPath<Object>(path);
                bool has = false;
                for (int i = 0; i < screens.arraySize; i++)
                {
                    if (screens.GetArrayElementAtIndex(i).objectReferenceValue == screen)
                    {
                        has = true;
                        break;
                    }
                }

                if (!has)
                {
                    screens.InsertArrayElementAtIndex(screens.arraySize);
                    screens.GetArrayElementAtIndex(screens.arraySize - 1).objectReferenceValue = screen;
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildMainSceneFromRaceTemplate()
        {
            Scene race = EditorSceneManager.OpenScene(RaceScenePath, OpenSceneMode.Single);
            EditorSceneManager.SaveScene(race, MainScenePath);
            Scene main = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            GameObject rootGo = null;
            foreach (GameObject go in main.GetRootGameObjects())
            {
                if (go.name == "Race_Root")
                {
                    rootGo = go;
                    break;
                }
            }

            if (rootGo == null)
            {
                Debug.LogError("[Campaign] Race_Root not found in Main Scene copy.");
                return;
            }

            rootGo.name = "Main_Root";
            Component raceScope = RaceScopeType != null ? rootGo.GetComponent(RaceScopeType) : null;
            Transform navHolder = rootGo.transform;
            if (raceScope != null)
            {
                var rso = new SerializedObject(raceScope);
                navHolder = rso.FindProperty("navigationViewHolder").objectReferenceValue as Transform ?? rootGo.transform;
                Object.DestroyImmediate(raceScope, true);
            }

            if (RaceBootstrapType != null)
            {
                Component raceBoot = rootGo.GetComponent(RaceBootstrapType);
                if (raceBoot != null)
                {
                    Object.DestroyImmediate(raceBoot, true);
                }
            }

            Component campaignScope = CampaignScopeType != null ? rootGo.AddComponent(CampaignScopeType) : null;
            if (campaignScope == null)
            {
                Debug.LogError("[Campaign] CampaignScope type not found (Game.Campaign assembly).");
                return;
            }

            var boot = rootGo.AddComponent<CampaignBootstrap>();
            var cso = new SerializedObject(campaignScope);
            cso.FindProperty("navigationSettings").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(NavigationSettingsPath);
            cso.FindProperty("navigationViewHolder").objectReferenceValue = navHolder != null ? navHolder : rootGo.transform;
            cso.FindProperty("boardConfig").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<BoardConfigSO>(AssetDatabase.GUIDToAssetPath(GuidBoardConfig));
            cso.FindProperty("featureToggle").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GearEngineFeatureToggleSO>(AssetDatabase.GUIDToAssetPath(GuidFeatureToggle));
            cso.FindProperty("sceneBootstrap").objectReferenceValue = boot;
            SerializedProperty tracks = cso.FindProperty("tracks");
            tracks.arraySize = 1;
            SerializedProperty e0 = tracks.GetArrayElementAtIndex(0);
            e0.FindPropertyRelative("track").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<TrackDefinition>(AssetDatabase.GUIDToAssetPath(GuidTrackDefinition));
            e0.FindPropertyRelative("car").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<CarDefinition>(AssetDatabase.GUIDToAssetPath(GuidCarDefinition));
            SerializedProperty pool = cso.FindProperty("roguelikeCardPool");
            pool.arraySize = 3;
            pool.GetArrayElementAtIndex(0).objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GearConfig>(AssetDatabase.GUIDToAssetPath(GuidGearA));
            pool.GetArrayElementAtIndex(1).objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GearConfig>(AssetDatabase.GUIDToAssetPath(GuidGearB));
            pool.GetArrayElementAtIndex(2).objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GearConfig>(AssetDatabase.GUIDToAssetPath(GuidGearC));
            cso.ApplyModifiedPropertiesWithoutUndo();
            DestroyObjectInSceneIfExists(main, "RaceView");

            EditorSceneManager.MarkSceneDirty(main);
            EditorSceneManager.SaveScene(main);
        }

        private static void DestroyObjectInSceneIfExists(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.gameObject.name == objectName)
                    {
                        Object.DestroyImmediate(t.gameObject);
                        return;
                    }
                }
            }
        }
    }
}
