using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using GearEngine.GearEngine.Presentation;
using GearEngine.GearEngine.Presentation.UI;
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
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

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

            SetupBasicConfigsTool.GenerateConfigs();
            GearEngineNavigationAssetGenerator.Generate();

            string folderPath = "Assets/GearEngine/Data/Gear";
            string prefabPath = "Assets/GearEngine/Prefabs/Gears/Gears";

            GearConfig core = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/CoreGearConfig.asset");
            GearConfig baseGear = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/BaseGearConfig_Level1.asset");
            GameObject emptySlot = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/EmptySlotView.prefab");
            GameObject gearSlot = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/GearSlot.prefab");
            BoardConfigSO boardConfig = AssetDatabase.LoadAssetAtPath<BoardConfigSO>($"{folderPath}/BasicBoardConfig.asset");
            GearInventoryLoadoutSO loadout = AssetDatabase.LoadAssetAtPath<GearInventoryLoadoutSO>($"{folderPath}/GearInventoryLoadout.asset");
            GearEngineFeatureToggleSO featureToggle = AssetDatabase.LoadAssetAtPath<GearEngineFeatureToggleSO>($"{folderPath}/GearEngineFeatureToggle.asset");

            GameObject gearRootObj = new GameObject("GearEngine_Root");
            var scope = gearRootObj.AddComponent<GearMechanicsScope>();

            GameObject gridRootObj = new GameObject("GearGrid_Root");
            gridRootObj.transform.SetParent(gearRootObj.transform, false);

            if (boardConfig != null && emptySlot != null)
            {
                for (int x = 0; x < boardConfig.GridWidth; x++)
                {
                    for (int y = 0; y < boardConfig.GridHeight; y++)
                    {
                        var pos = new Vector2Int(x, y);
                        GameObject slotView = Object.Instantiate(emptySlot, gridRootObj.transform);
                        slotView.transform.localPosition = boardConfig.GetWorldPosition(pos, 0.5f);
                        slotView.name = $"EmptySlot_{x}_{y}";
                    }
                }
            }

            GearTestSceneBootstrap testBootstrap = gearRootObj.AddComponent<GearTestSceneBootstrap>();
            SerializeGearEngineStartData(testBootstrap, boardConfig, core, loadout);

            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            GameObject canvasObj = new GameObject(canvasObjectName);
            Canvas testCanvas = canvasObj.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            testCanvas.worldCamera = cam;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasObj.transform.SetParent(gearRootObj.transform, false);

            var gearEngineView = canvasObj.AddComponent<GearEngineView>();

            GameObject simViewObj = new GameObject("SimulationControlView");
            var simRt = simViewObj.AddComponent<RectTransform>();
            simRt.SetParent(canvasObj.transform, false);
            simRt.anchorMin = new Vector2(0.5f, 1f);
            simRt.anchorMax = new Vector2(0.5f, 1f);
            simRt.pivot = new Vector2(0.5f, 1f);
            simRt.anchoredPosition = new Vector2(0, -20f);
            simRt.sizeDelta = new Vector2(250, 60);

            var simImage = simViewObj.AddComponent<UnityEngine.UI.Image>();
            simImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var simViewDef = simViewObj.AddComponent<SimulationControlView>();
            var simBtn = simViewObj.AddComponent<UnityEngine.UI.Button>();
            simBtn.targetGraphic = simImage;

            var simTxtObj = new GameObject("Text");
            var txtRt = simTxtObj.AddComponent<RectTransform>();
            txtRt.SetParent(simRt, false);
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            var simTxt = simTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
            simTxt.text = "Toggle simulation";
            simTxt.alignment = TMPro.TextAlignmentOptions.Center;
            simTxt.color = Color.white;

            var simSo = new SerializedObject(simViewDef);
            var btnProp = simSo.FindProperty("toggleButton");
            if (btnProp != null)
            {
                btnProp.objectReferenceValue = simBtn;
            }

            var txtProp = simSo.FindProperty("buttonText");
            if (txtProp != null)
            {
                txtProp.objectReferenceValue = simTxt;
            }

            simSo.ApplyModifiedProperties();

            // Setup Board Limit Label
            var boardLabelObj = new GameObject("BoardCapacityLabel");
            var boardLabelRt = boardLabelObj.AddComponent<RectTransform>();
            boardLabelRt.SetParent(canvasObj.transform, false);
            boardLabelRt.anchorMin = new Vector2(0f, 1f);
            boardLabelRt.anchorMax = new Vector2(0f, 1f);
            boardLabelRt.pivot = new Vector2(0f, 1f);
            boardLabelRt.anchoredPosition = new Vector2(20f, -20f);
            boardLabelRt.sizeDelta = new Vector2(250f, 40f);
            
            var boardLabelTxt = boardLabelObj.AddComponent<TMPro.TextMeshProUGUI>();
            boardLabelTxt.text = "Board: 0/0";
            boardLabelTxt.alignment = TMPro.TextAlignmentOptions.TopLeft;
            boardLabelTxt.color = Color.white;
            boardLabelTxt.fontSize = 24f;

            // Setup Inventory Limit Label
            var invLabelObj = new GameObject("InventoryCapacityLabel");
            var invLabelRt = invLabelObj.AddComponent<RectTransform>();
            invLabelRt.SetParent(canvasObj.transform, false);
            invLabelRt.anchorMin = new Vector2(1f, 1f);
            invLabelRt.anchorMax = new Vector2(1f, 1f);
            invLabelRt.pivot = new Vector2(1f, 1f);
            invLabelRt.anchoredPosition = new Vector2(-20f, -20f);
            invLabelRt.sizeDelta = new Vector2(250f, 40f);
            
            var invLabelTxt = invLabelObj.AddComponent<TMPro.TextMeshProUGUI>();
            invLabelTxt.text = "Inventory: 0/0";
            invLabelTxt.alignment = TMPro.TextAlignmentOptions.TopRight;
            invLabelTxt.color = Color.white;
            invLabelTxt.fontSize = 24f;

            GameObject invViewObj = new GameObject("GearInventoryView");
            var invRt = invViewObj.AddComponent<RectTransform>();
            invRt.SetParent(canvasObj.transform, false);
            invRt.anchorMin = new Vector2(0.5f, 0f);
            invRt.anchorMax = new Vector2(0.5f, 0f);
            invRt.pivot = new Vector2(0.5f, 0f);
            invRt.anchoredPosition = new Vector2(0, 50f);
            invRt.sizeDelta = new Vector2(800, 150f);

            GameObject itemsContainerObj = new GameObject("ItemsContainer");
            var itemsRt = itemsContainerObj.AddComponent<RectTransform>();
            itemsRt.SetParent(invRt, false);
            itemsRt.anchorMin = new Vector2(0, 0);
            itemsRt.anchorMax = new Vector2(1, 1);
            itemsRt.offsetMin = Vector2.zero;
            itemsRt.offsetMax = Vector2.zero;

            var hlG = itemsContainerObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            hlG.childAlignment = TextAnchor.MiddleCenter;
            hlG.spacing = 15f;
            hlG.childControlWidth = false;
            hlG.childControlHeight = false;

            var invViewDef = invViewObj.AddComponent<GearInventoryView>();
            var invSo = new SerializedObject(invViewDef);
            var containerProp = invSo.FindProperty("itemsContainer");
            if (containerProp != null)
            {
                containerProp.objectReferenceValue = itemsRt;
            }

            var gridBoardTagRef = AssetDatabase.LoadAssetAtPath<TagSO>($"{folderPath}/Tag/GridBoard_Tag.asset");
            var tagProp = invSo.FindProperty("gridBoardTag");
            if (tagProp != null && gridBoardTagRef != null)
            {
                tagProp.objectReferenceValue = gridBoardTagRef;
            }

            var slotPrefabProp = invSo.FindProperty("slotPrefab");
            if (slotPrefabProp != null && gearSlot != null)
            {
                slotPrefabProp.objectReferenceValue = gearSlot;
            }

            invSo.ApplyModifiedProperties();

            GameObject floorGrid = new GameObject("GridBoardCollider");
            var boxCol = floorGrid.AddComponent<BoxCollider>();
            if (boardConfig != null)
            {
                boxCol.size = new Vector3(boardConfig.GridWidth * boardConfig.Spacing + 1f, boardConfig.GridHeight * boardConfig.Spacing + 1f, 0.5f);
            }
            else
            {
                boxCol.size = new Vector3(50, 50, 0.5f);
            }

            floorGrid.transform.position = new Vector3(0, 0, 0.5f);
            floorGrid.transform.SetParent(gearRootObj.transform, false);

            var boardView = floorGrid.AddComponent<BoardView>();
            var boardDragHandler = floorGrid.AddComponent<GearBoardDragHandler>();
            var boardSo = new SerializedObject(boardView);
            SerializedProperty dragHandlerProp = boardSo.FindProperty("dragHandler");
            if (dragHandlerProp != null)
            {
                dragHandlerProp.objectReferenceValue = boardDragHandler;
            }

            boardSo.ApplyModifiedProperties();

            // Wire trash zone tag to drag handler for tag-based discovery
            TagSO trashZoneTagRef = AssetDatabase.LoadAssetAtPath<TagSO>($"{folderPath}/Tag/TrashZone_Tag.asset");
            if (trashZoneTagRef != null)
            {
                var dragSo = new SerializedObject(boardDragHandler);
                var trashTagProp = dragSo.FindProperty("trashZoneTag");
                if (trashTagProp != null)
                {
                    trashTagProp.objectReferenceValue = trashZoneTagRef;
                }

                dragSo.ApplyModifiedProperties();
            }

            var tagComp = floorGrid.AddComponent<TagComponent>();
            TagSO gridBoardTag = AssetDatabase.LoadAssetAtPath<TagSO>($"{folderPath}/Tag/GridBoard_Tag.asset");
            if (gridBoardTag != null)
            {
                tagComp.AddTag(gridBoardTag);
            }

            var gearViewSo = new SerializedObject(gearEngineView);
            var sc = gearViewSo.FindProperty("simControlView");
            if (sc != null)
            {
                sc.objectReferenceValue = simViewDef;
            }

            var iv = gearViewSo.FindProperty("inventoryView");
            if (iv != null)
            {
                iv.objectReferenceValue = invViewDef;
            }

            var blProp = gearViewSo.FindProperty("boardLimitLabel");
            if (blProp != null) blProp.objectReferenceValue = boardLabelTxt;

            var ilProp = gearViewSo.FindProperty("inventoryLimitLabel");
            if (ilProp != null) ilProp.objectReferenceValue = invLabelTxt;

            var bv = gearViewSo.FindProperty("boardView");
            if (bv != null)
            {
                bv.objectReferenceValue = boardView;
            }

            gearViewSo.ApplyModifiedProperties();

            NavigationSettings navigationSettings =
                AssetDatabase.LoadAssetAtPath<NavigationSettings>(GearEngineNavigationAssetGenerator.NavigationSettingsPath);

            ApplyGearMechanicsScopeReferences(scope, boardConfig, testBootstrap, navigationSettings, gearRootObj.transform, featureToggle);

            string sceneDir = "Assets/Scenes";
            if (!System.IO.Directory.Exists(sceneDir))
            {
                System.IO.Directory.CreateDirectory(sceneDir);
            }

            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"<color=#33ff33>[GearEngine]</color> Composable gear scene saved at: {scenePath}");
        }

        private static void ApplyGearMechanicsScopeReferences(
            GearMechanicsScope scope,
            BoardConfigSO boardConfig,
            GearTestSceneBootstrap sceneBootstrap,
            NavigationSettings navigationSettings,
            Transform navigationViewHolder,
            GearEngineFeatureToggleSO featureToggle)
        {
            var instSo = new SerializedObject(scope);
            var bc = instSo.FindProperty("boardConfig");
            if (bc != null)
            {
                bc.objectReferenceValue = boardConfig;
            }

            var sb = instSo.FindProperty("sceneBootstrap");
            if (sb != null)
            {
                sb.objectReferenceValue = sceneBootstrap;
            }

            var ns = instSo.FindProperty("navigationSettings");
            if (ns != null)
            {
                ns.objectReferenceValue = navigationSettings;
            }

            var nh = instSo.FindProperty("navigationViewHolder");
            if (nh != null)
            {
                nh.objectReferenceValue = navigationViewHolder;
            }

            var ft = instSo.FindProperty("featureToggle");
            if (ft != null && featureToggle != null)
            {
                ft.objectReferenceValue = featureToggle;
            }

            instSo.ApplyModifiedProperties();
        }

        private static void SerializeGearEngineStartData(
            GearTestSceneBootstrap testBootstrap,
            BoardConfigSO boardConfig,
            GearConfig coreGear,
            GearInventoryLoadoutSO loadout)
        {
            var tbSo = new SerializedObject(testBootstrap);
            SerializedProperty startDataProp = tbSo.FindProperty("startData");
            if (startDataProp == null)
            {
                tbSo.ApplyModifiedProperties();
                return;
            }

            SerializedProperty boardLayoutProp = startDataProp.FindPropertyRelative("boardLayout");
            SerializedProperty placementsProp = boardLayoutProp != null ? boardLayoutProp.FindPropertyRelative("placements") : null;

            if (placementsProp != null && boardConfig != null && coreGear != null)
            {
                int centerX = boardConfig.GridWidth / 2;
                int centerY = boardConfig.GridHeight / 2;
                placementsProp.ClearArray();
                placementsProp.arraySize = 1;
                SerializedProperty p0 = placementsProp.GetArrayElementAtIndex(0);
                SerializedProperty posProp = p0.FindPropertyRelative("position");
                if (posProp != null)
                {
                    posProp.vector2IntValue = new Vector2Int(centerX, centerY);
                }

                SerializedProperty gearProp = p0.FindPropertyRelative("gearConfig");
                if (gearProp != null)
                {
                    gearProp.objectReferenceValue = coreGear;
                }
            }

            SerializedProperty invProp = startDataProp.FindPropertyRelative("inventoryGears");
            SerializedProperty maxSlotsProp = startDataProp.FindPropertyRelative("maxInventorySlots");
            
            if (maxSlotsProp != null && loadout != null)
            {
                maxSlotsProp.intValue = loadout.MaxInventorySlots;
            }
            if (invProp != null && loadout != null && loadout.StartingGears != null)
            {
                invProp.ClearArray();
                int n = loadout.StartingGears.Count;
                invProp.arraySize = n;
                for (int i = 0; i < n; i++)
                {
                    invProp.GetArrayElementAtIndex(i).objectReferenceValue = loadout.StartingGears[i];
                }
            }

            tbSo.ApplyModifiedProperties();
        }
    }
}
