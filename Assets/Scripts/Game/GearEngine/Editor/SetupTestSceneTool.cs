using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation;
using Scaffold.Navigation;

namespace GearEngine.GearEngine.Editor
{
    public static class SetupTestSceneTool
    {
        [MenuItem("GearEngine/Step 2: Generate VContainer Test Scene")]
        public static void GenerateScene()
        {
            BuildGearComposableScene("Assets/Scenes/GearEngine_TestScene.unity", "TestCanvas");
        }

        [MenuItem("GearEngine/Create Gear_Clean Scene")]
        public static void CreateGearCleanScene()
        {
            BuildGearComposableScene("Assets/Scenes/Gear_Clean.unity", "Canvas");
        }

        private static void BuildGearComposableScene(string scenePath, string canvasObjectName)
        {
            UnityEngine.SceneManagement.Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera cam = CreateSceneCameraAndLight();
            RunGearComposableSceneContent(scenePath, canvasObjectName, newScene, cam);
        }

        private static void RunGearComposableSceneContent(string scenePath, string canvasObjectName, UnityEngine.SceneManagement.Scene newScene, Camera cam)
        {
            SetupBasicConfigsTool.GenerateConfigs();
            GearEngineNavigationAssetGenerator.Generate();
            GearSceneAssets assets = LoadGearSceneAssets();
            GameObject gearRootObj = CreateGearRootWithGrid(assets);
            GearTestSceneBootstrap testBootstrap = gearRootObj.AddComponent<GearTestSceneBootstrap>();
            SerializeGearEngineStartData(testBootstrap, assets.BoardConfig, assets.Core, assets.Loadout);
            CreateEventSystem();
            GameObject canvasObj = CreateGearCanvas(assets.ConfigFolder, canvasObjectName, cam, gearRootObj, out SimulationControlView simViewDef, out GearInventoryView invViewDef, out BoardView boardView);
            WireGearEngineViewFields(canvasObj.GetComponent<GearEngineView>(), simViewDef, invViewDef, boardView);
            NavigationSettings navigationSettings = AssetDatabase.LoadAssetAtPath<NavigationSettings>(GearEngineNavigationAssetGenerator.NavigationSettingsPath);
            ApplyGearMechanicsScopeReferences(gearRootObj.GetComponent<GearMechanicsScope>(), assets.BoardConfig, testBootstrap, navigationSettings, gearRootObj.transform);
            SaveGearSceneAndLog(newScene, scenePath);
        }

        private static void SaveGearSceneAndLog(UnityEngine.SceneManagement.Scene newScene, string scenePath)
        {
            EnsureSceneDirectoryExists();
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"<color=#33ff33>[GearEngine]</color> Composable gear scene saved at: {scenePath}");
        }

        private static Camera CreateSceneCameraAndLight()
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cameraObj.transform.position = new Vector3(0, 0, -10f);

            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            return cam;
        }

        private static GearSceneAssets LoadGearSceneAssets()
        {
            string folderPath = "Assets/Game/GearEngine/Configs";
            string prefabPath = "Assets/Game/GearEngine/Prefabs";
            GearConfig core = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/CoreGearConfig.asset");
            GearConfig baseGear = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/BaseGearConfig_Level1.asset");
            GameObject emptySlot = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/EmptySlotView.prefab");
            BoardConfigSO boardConfig = AssetDatabase.LoadAssetAtPath<BoardConfigSO>($"{folderPath}/BasicBoardConfig.asset");
            GearInventoryLoadoutSO loadout = AssetDatabase.LoadAssetAtPath<GearInventoryLoadoutSO>($"{folderPath}/GearInventoryLoadout.asset");
            return new GearSceneAssets(core, baseGear, emptySlot, boardConfig, loadout, folderPath);
        }

        private static GameObject CreateGearRootWithGrid(GearSceneAssets assets)
        {
            GameObject gearRootObj = new GameObject("GearEngine_Root");
            gearRootObj.AddComponent<GearMechanicsScope>();
            GameObject gridRootObj = new GameObject("GearGrid_Root");
            gridRootObj.transform.SetParent(gearRootObj.transform, false);
            PopulateEmptyGridSlots(assets.BoardConfig, assets.EmptySlot, gridRootObj.transform);
            return gearRootObj;
        }

        private static void PopulateEmptyGridSlots(BoardConfigSO boardConfig, GameObject emptySlot, Transform gridRoot)
        {
            if (boardConfig == null || emptySlot == null)
            {
                return;
            }

            for (int x = 0; x < boardConfig.GridWidth; x++)
            {
                for (int y = 0; y < boardConfig.GridHeight; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    GameObject slotView = Object.Instantiate(emptySlot, gridRoot);
                    slotView.transform.localPosition = boardConfig.GetWorldPosition(pos, 0.5f);
                    slotView.name = $"EmptySlot_{x}_{y}";
                }
            }
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private static GameObject CreateGearCanvas(string folderPath, string canvasObjectName, Camera cam, GameObject gearRootObj, out SimulationControlView simViewDef, out GearInventoryView invViewDef, out BoardView boardView)
        {
            GameObject canvasObj = new GameObject(canvasObjectName);
            Canvas testCanvas = canvasObj.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            testCanvas.worldCamera = cam;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasObj.transform.SetParent(gearRootObj.transform, false);
            canvasObj.AddComponent<GearEngineView>();
            simViewDef = BuildSimulationControlBlock(canvasObj.transform);
            invViewDef = BuildInventoryBlock(canvasObj.transform, folderPath);
            boardView = BuildBoardColliderAndViews(folderPath, gearRootObj.transform);
            return canvasObj;
        }

        private static SimulationControlView BuildSimulationControlBlock(Transform canvasTransform)
        {
            SimulationControlView simViewDef = CreateSimulationControlShell(canvasTransform, out UnityEngine.UI.Button simBtn);
            TMPro.TextMeshProUGUI simTxt = CreateSimulationLabel(simViewDef.transform);
            BindSimulationControlSerialized(simViewDef, simBtn, simTxt);
            return simViewDef;
        }

        private static SimulationControlView CreateSimulationControlShell(Transform canvasTransform, out UnityEngine.UI.Button simBtn)
        {
            GameObject simViewObj = new GameObject("SimulationControlView");
            RectTransform simRt = simViewObj.AddComponent<RectTransform>();
            simRt.SetParent(canvasTransform, false);
            simRt.anchorMin = new Vector2(0.5f, 1f);
            simRt.anchorMax = new Vector2(0.5f, 1f);
            simRt.pivot = new Vector2(0.5f, 1f);
            simRt.anchoredPosition = new Vector2(0, -20f);
            simRt.sizeDelta = new Vector2(250, 60);

            UnityEngine.UI.Image simImage = simViewObj.AddComponent<UnityEngine.UI.Image>();
            simImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            SimulationControlView simViewDef = simViewObj.AddComponent<SimulationControlView>();
            simBtn = simViewObj.AddComponent<UnityEngine.UI.Button>();
            simBtn.targetGraphic = simImage;
            return simViewDef;
        }

        private static TMPro.TextMeshProUGUI CreateSimulationLabel(Transform simRoot)
        {
            GameObject simTxtObj = new GameObject("Text");
            RectTransform txtRt = simTxtObj.AddComponent<RectTransform>();
            txtRt.SetParent(simRoot, false);
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            TMPro.TextMeshProUGUI simTxt = simTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
            simTxt.text = "Toggle simulation";
            simTxt.alignment = TMPro.TextAlignmentOptions.Center;
            simTxt.color = Color.white;
            return simTxt;
        }

        private static void BindSimulationControlSerialized(SimulationControlView simViewDef, UnityEngine.UI.Button simBtn, TMPro.TextMeshProUGUI simTxt)
        {
            SerializedObject simSo = new SerializedObject(simViewDef);
            AssignIfPresent(simSo, "toggleButton", simBtn);
            AssignIfPresent(simSo, "buttonText", simTxt);
            simSo.ApplyModifiedProperties();
        }

        private static GearInventoryView BuildInventoryBlock(Transform canvasTransform, string folderPath)
        {
            GearInventoryView invViewDef = CreateInventoryShell(canvasTransform, out RectTransform itemsRt);
            BindInventorySerialized(invViewDef, itemsRt, folderPath);
            return invViewDef;
        }

        private static GearInventoryView CreateInventoryShell(Transform canvasTransform, out RectTransform itemsRt)
        {
            GameObject invViewObj = new GameObject("GearInventoryView");
            RectTransform invRt = invViewObj.AddComponent<RectTransform>();
            invRt.SetParent(canvasTransform, false);
            invRt.anchorMin = new Vector2(0.5f, 0f);
            invRt.anchorMax = new Vector2(0.5f, 0f);
            invRt.pivot = new Vector2(0.5f, 0f);
            invRt.anchoredPosition = new Vector2(0, 50f);
            invRt.sizeDelta = new Vector2(800, 150f);
            itemsRt = CreateHorizontalItemsStrip(invRt);
            return invViewObj.AddComponent<GearInventoryView>();
        }

        private static RectTransform CreateHorizontalItemsStrip(RectTransform invRt)
        {
            GameObject itemsContainerObj = new GameObject("ItemsContainer");
            RectTransform itemsRt = itemsContainerObj.AddComponent<RectTransform>();
            itemsRt.SetParent(invRt, false);
            itemsRt.anchorMin = new Vector2(0, 0);
            itemsRt.anchorMax = new Vector2(1, 1);
            itemsRt.offsetMin = Vector2.zero;
            itemsRt.offsetMax = Vector2.zero;

            UnityEngine.UI.HorizontalLayoutGroup hlG = itemsContainerObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            hlG.childAlignment = TextAnchor.MiddleCenter;
            hlG.spacing = 15f;
            hlG.childControlWidth = false;
            hlG.childControlHeight = false;
            return itemsRt;
        }

        private static void BindInventorySerialized(GearInventoryView invViewDef, RectTransform itemsRt, string folderPath)
        {
            SerializedObject invSo = new SerializedObject(invViewDef);
            AssignIfPresent(invSo, "itemsContainer", itemsRt);
            TagSO gridBoardTagRef = AssetDatabase.LoadAssetAtPath<TagSO>($"{folderPath}/Tag/GridBoard_Tag.asset");
            SerializedProperty tagProp = invSo.FindProperty("gridBoardTag");
            if (tagProp != null && gridBoardTagRef != null)
            {
                tagProp.objectReferenceValue = gridBoardTagRef;
            }

            invSo.ApplyModifiedProperties();
        }

        private static BoardView BuildBoardColliderAndViews(string folderPath, Transform gearRoot)
        {
            GameObject floorGrid = new GameObject("GridBoardCollider");
            BoxCollider boxCol = floorGrid.AddComponent<BoxCollider>();
            BoardConfigSO boardConfig = AssetDatabase.LoadAssetAtPath<BoardConfigSO>($"{folderPath}/BasicBoardConfig.asset");
            ApplyBoardColliderSize(boxCol, boardConfig);
            floorGrid.transform.position = new Vector3(0, 0, 0.5f);
            floorGrid.transform.SetParent(gearRoot, false);
            BoardView boardView = floorGrid.AddComponent<BoardView>();
            WireBoardDragHandlerSerialized(boardView, floorGrid);
            AddGridBoardTagComponent(floorGrid, folderPath);
            return boardView;
        }

        private static void WireBoardDragHandlerSerialized(BoardView boardView, GameObject floorGrid)
        {
            GearBoardDragHandler boardDragHandler = floorGrid.AddComponent<GearBoardDragHandler>();
            SerializedObject boardSo = new SerializedObject(boardView);
            AssignIfPresent(boardSo, "dragHandler", boardDragHandler);
            boardSo.ApplyModifiedProperties();
        }

        private static void AddGridBoardTagComponent(GameObject floorGrid, string folderPath)
        {
            TagComponent tagComp = floorGrid.AddComponent<TagComponent>();
            TagSO gridBoardTag = AssetDatabase.LoadAssetAtPath<TagSO>($"{folderPath}/Tag/GridBoard_Tag.asset");
            if (gridBoardTag != null)
            {
                tagComp.AddTag(gridBoardTag);
            }
        }

        private static void ApplyBoardColliderSize(BoxCollider boxCol, BoardConfigSO boardConfig)
        {
            if (boardConfig != null)
            {
                boxCol.size = new Vector3(boardConfig.GridWidth * boardConfig.Spacing + 1f, boardConfig.GridHeight * boardConfig.Spacing + 1f, 0.5f);
            }
            else
            {
                boxCol.size = new Vector3(50, 50, 0.5f);
            }
        }

        private static void WireGearEngineViewFields(GearEngineView gearEngineView, SimulationControlView simViewDef, GearInventoryView invViewDef, BoardView boardView)
        {
            SerializedObject gearViewSo = new SerializedObject(gearEngineView);
            AssignIfPresent(gearViewSo, "simControlView", simViewDef);
            AssignIfPresent(gearViewSo, "inventoryView", invViewDef);
            AssignIfPresent(gearViewSo, "boardView", boardView);
            gearViewSo.ApplyModifiedProperties();
        }

        private static void AssignIfPresent(SerializedObject so, string propertyName, Object value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        private static void EnsureSceneDirectoryExists()
        {
            string sceneDir = "Assets/Scenes";
            if (!System.IO.Directory.Exists(sceneDir))
            {
                System.IO.Directory.CreateDirectory(sceneDir);
            }
        }

        private static void ApplyGearMechanicsScopeReferences(GearMechanicsScope scope, BoardConfigSO boardConfig, GearTestSceneBootstrap sceneBootstrap, NavigationSettings navigationSettings, Transform navigationViewHolder)
        {
            SerializedObject instSo = new SerializedObject(scope);
            AssignIfPresent(instSo, "boardConfig", boardConfig);
            AssignIfPresent(instSo, "sceneBootstrap", sceneBootstrap);
            AssignIfPresent(instSo, "navigationSettings", navigationSettings);
            AssignIfPresent(instSo, "navigationViewHolder", navigationViewHolder);
            instSo.ApplyModifiedProperties();
        }

        private static void SerializeGearEngineStartData(GearTestSceneBootstrap testBootstrap, BoardConfigSO boardConfig, GearConfig coreGear, GearInventoryLoadoutSO loadout)
        {
            SerializedObject tbSo = new SerializedObject(testBootstrap);
            SerializedProperty startDataProp = tbSo.FindProperty("startData");
            if (startDataProp == null)
            {
                tbSo.ApplyModifiedProperties();
                return;
            }

            WriteDefaultBoardPlacement(startDataProp, boardConfig, coreGear);
            CopyInventoryFromLoadout(startDataProp, loadout);
            tbSo.ApplyModifiedProperties();
        }

        private static void WriteDefaultBoardPlacement(SerializedProperty startDataProp, BoardConfigSO boardConfig, GearConfig coreGear)
        {
            SerializedProperty placementsProp = TryGetPlacementsProperty(startDataProp);
            if (placementsProp == null || boardConfig == null || coreGear == null)
            {
                return;
            }

            Vector2Int center = new Vector2Int(boardConfig.GridWidth / 2, boardConfig.GridHeight / 2);
            WriteSingleBoardPlacement(placementsProp, center, coreGear);
        }

        private static SerializedProperty TryGetPlacementsProperty(SerializedProperty startDataProp)
        {
            SerializedProperty boardLayoutProp = startDataProp.FindPropertyRelative("boardLayout");
            return boardLayoutProp != null ? boardLayoutProp.FindPropertyRelative("placements") : null;
        }

        private static void WriteSingleBoardPlacement(SerializedProperty placementsProp, Vector2Int position, GearConfig coreGear)
        {
            placementsProp.ClearArray();
            placementsProp.arraySize = 1;
            SerializedProperty p0 = placementsProp.GetArrayElementAtIndex(0);
            SerializedProperty posProp = p0.FindPropertyRelative("position");
            if (posProp != null)
            {
                posProp.vector2IntValue = position;
            }

            SerializedProperty gearProp = p0.FindPropertyRelative("gearConfig");
            if (gearProp != null)
            {
                gearProp.objectReferenceValue = coreGear;
            }
        }

        private static void CopyInventoryFromLoadout(SerializedProperty startDataProp, GearInventoryLoadoutSO loadout)
        {
            SerializedProperty invProp = startDataProp.FindPropertyRelative("inventoryGears");
            if (invProp == null || loadout == null || loadout.StartingGears == null)
            {
                return;
            }

            invProp.ClearArray();
            int n = loadout.StartingGears.Count;
            invProp.arraySize = n;
            for (int i = 0; i < n; i++)
            {
                invProp.GetArrayElementAtIndex(i).objectReferenceValue = loadout.StartingGears[i];
            }
        }
    }
}
