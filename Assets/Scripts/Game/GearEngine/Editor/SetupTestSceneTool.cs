using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using Game.GearEngine.Presentation;

namespace Game.GearEngine.Editor
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

            string folderPath = "Assets/Game/GearEngine/Configs";
            string prefabPath = "Assets/Game/GearEngine/Prefabs";

            GearConfig core = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/CoreGearConfig.asset");
            GearConfig baseGear = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/BaseGearConfig_Level1.asset");
            GameObject emptySlot = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/EmptySlotView.prefab");
            BoardConfigSO boardConfig = AssetDatabase.LoadAssetAtPath<BoardConfigSO>($"{folderPath}/BasicBoardConfig.asset");

            GameObject gearRootObj = new GameObject("GearEngine_Root");
            gearRootObj.AddComponent<GearMechanicsScope>();
            var installer = gearRootObj.AddComponent<GearMechanicsInstaller>();

            GameObject gridRootObj = new GameObject("GearGrid_Root");
            gridRootObj.transform.SetParent(gearRootObj.transform, false);
            GearBootstrap bootstrap = gridRootObj.AddComponent<GearBootstrap>();

            if (core != null && baseGear != null)
            {
                var so = new SerializedObject(bootstrap);

                var prop = so.FindProperty("gearConfigs");
                if (prop != null)
                {
                    prop.arraySize = 2;
                    prop.GetArrayElementAtIndex(0).objectReferenceValue = core;
                    prop.GetArrayElementAtIndex(1).objectReferenceValue = baseGear;
                }

                GearConfig rockObs = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/ObstacleRockConfig.asset");
                GearConfig speedGear = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/SpeedBuffGearConfig.asset");
                GearConfig scoreGear = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/ScoreGearConfig.asset");
                GearConfig baseLevel2 = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/BaseGearConfig_Level2.asset");

                var invProp = so.FindProperty("startingInventoryGears");
                if (invProp != null)
                {
                    invProp.arraySize = 6;
                    invProp.GetArrayElementAtIndex(0).objectReferenceValue = core != null ? core : baseGear;
                    invProp.GetArrayElementAtIndex(1).objectReferenceValue = baseGear;
                    invProp.GetArrayElementAtIndex(2).objectReferenceValue = baseLevel2 != null ? baseLevel2 : baseGear;
                    invProp.GetArrayElementAtIndex(3).objectReferenceValue = speedGear != null ? speedGear : baseGear;
                    invProp.GetArrayElementAtIndex(4).objectReferenceValue = rockObs != null ? rockObs : baseGear;
                    invProp.GetArrayElementAtIndex(5).objectReferenceValue = scoreGear != null ? scoreGear : baseGear;
                }

                var slotProp = so.FindProperty("emptySlotPrefab");
                if (slotProp != null && emptySlot != null)
                {
                    slotProp.objectReferenceValue = emptySlot;
                }

                so.ApplyModifiedProperties();
            }

            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            GameObject canvasObj = new GameObject(canvasObjectName);
            Canvas testCanvas = canvasObj.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            testCanvas.worldCamera = cam;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

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

            var boardView = floorGrid.AddComponent<BoardView>();

            var tagComp = floorGrid.AddComponent<TagComponent>();
            TagSO gridBoardTag = AssetDatabase.LoadAssetAtPath<TagSO>($"{folderPath}/Tag/GridBoard_Tag.asset");
            if (gridBoardTag != null)
            {
                tagComp.AddTag(gridBoardTag);
            }

            ApplyGearMechanicsInstallerReferences(installer, boardConfig, bootstrap, invViewDef, simViewDef, boardView);

            string sceneDir = "Assets/Scenes";
            if (!System.IO.Directory.Exists(sceneDir))
            {
                System.IO.Directory.CreateDirectory(sceneDir);
            }

            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"<color=#33ff33>[GearEngine]</color> Composable gear scene saved at: {scenePath}");
        }

        private static void ApplyGearMechanicsInstallerReferences(
            GearMechanicsInstaller installer,
            BoardConfigSO boardConfig,
            GearBootstrap bootstrap,
            GearInventoryView inventoryView,
            SimulationControlView simulationControlView,
            BoardView boardView)
        {
            var instSo = new SerializedObject(installer);
            var bc = instSo.FindProperty("boardConfig");
            if (bc != null)
            {
                bc.objectReferenceValue = boardConfig;
            }

            var bs = instSo.FindProperty("bootstrap");
            if (bs != null)
            {
                bs.objectReferenceValue = bootstrap;
            }

            var inv = instSo.FindProperty("inventoryView");
            if (inv != null)
            {
                inv.objectReferenceValue = inventoryView;
            }

            var sim = instSo.FindProperty("simControlView");
            if (sim != null)
            {
                sim.objectReferenceValue = simulationControlView;
            }

            var bv = instSo.FindProperty("boardView");
            if (bv != null)
            {
                bv.objectReferenceValue = boardView;
            }

            instSo.ApplyModifiedProperties();
        }
    }
}
