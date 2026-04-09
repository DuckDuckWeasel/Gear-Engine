using UnityEditor;
using UnityEngine;
using VContainer.Unity;
using UnityEditor.SceneManagement;
using Game.GearEngine.Presentation;

namespace Game.GearEngine.Editor
{
    public static class SetupTestSceneTool
    {
        [MenuItem("GearEngine/Step 2: Generate VContainer Test Scene")]
        public static void GenerateScene()
        {
            // Create a new empty scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Add basic lighting and camera
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cameraObj.transform.position = new Vector3(0, 0, -10f);

            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Setup VContainer LifetimeScope / Installer
            GameObject scopeObj = new GameObject("GearMechanics_LifetimeScope");
            GearMechanicsScope scope = scopeObj.AddComponent<GearMechanicsScope>();

            // Setup Bootstrap
            GearBootstrap bootstrap = scopeObj.AddComponent<GearBootstrap>();

            // Setup the required List of Configs (mock) for the bootstrap
            SetupBasicConfigsTool.GenerateConfigs(); // ensure configs exist
            
            // Try to assign configs to the bootstrap directly
            string folderPath = "Assets/Game/GearEngine/Configs";
            string prefabPath = "Assets/Game/GearEngine/Prefabs";
            
            GearConfig core = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/CoreGearConfig.asset");
            GearConfig baseGear = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/BaseGearConfig_Level1.asset");
            GameObject emptySlot = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/EmptySlotView.prefab");
            BoardConfigSO boardConfig = AssetDatabase.LoadAssetAtPath<BoardConfigSO>($"{folderPath}/BasicBoardConfig.asset");
            
            // Assign to scope
            if (boardConfig != null)
            {
                var scopeSo = new SerializedObject(scope);
                var boardConfigProp = scopeSo.FindProperty("boardConfig");
                if (boardConfigProp != null) boardConfigProp.objectReferenceValue = boardConfig;
                scopeSo.ApplyModifiedProperties();
            }

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
                
                GearConfig rockObs = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/ObstacleRockConfig.asset");
                GearConfig speedGear = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/SpeedBuffGearConfig.asset");
                GearConfig scoreGear = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/ScoreGearConfig.asset");
                GearConfig baseLevel2 = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/BaseGearConfig_Level2.asset");
                
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

            // Create Canvas and Event System for UI Testing
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            GameObject canvasObj = new GameObject("TestCanvas");
            Canvas testCanvas = canvasObj.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Setup Simulation View
            GameObject simViewObj = new GameObject("SimulationControlView");
            var simRt = simViewObj.AddComponent<RectTransform>();
            simRt.SetParent(canvasObj.transform, false);
            simRt.anchorMin = new Vector2(0.5f, 1f);
            simRt.anchorMax = new Vector2(0.5f, 1f);
            simRt.pivot = new Vector2(0.5f, 1f);
            simRt.anchoredPosition = new Vector2(0, -20f);
            simRt.sizeDelta = new Vector2(250, 60);

            // Button requires a target graphic to be clickable
            var simImage = simViewObj.AddComponent<UnityEngine.UI.Image>();
            simImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var simViewDef = simViewObj.AddComponent<Game.GearEngine.Presentation.SimulationControlView>();
            // Add a mock button
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
            
            // Assign references via SerializedObject to avoid manual drag-and-drop
            var simSo = new SerializedObject(simViewDef);
            var btnProp = simSo.FindProperty("toggleButton");
            if (btnProp != null) btnProp.objectReferenceValue = simBtn;
            var txtProp = simSo.FindProperty("buttonText");
            if (txtProp != null) txtProp.objectReferenceValue = simTxt;
            simSo.ApplyModifiedProperties();

            // Setup Inventory View at the bottom
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

            var invViewDef = invViewObj.AddComponent<Game.GearEngine.Presentation.GearInventoryView>();
            // Assign the itemsContainer to the View using reflection
            var invSo = new SerializedObject(invViewDef);
            var containerProp = invSo.FindProperty("itemsContainer");
            if (containerProp != null)
            {
                containerProp.objectReferenceValue = itemsRt;
            }
            invSo.ApplyModifiedProperties();

            // Setup Board Collider to act as a hit target for DragHandler Raycasts 
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

            // Attach BoardView which will process GearDroppedFromUIEvent
            floorGrid.AddComponent<Game.GearEngine.Presentation.BoardView>();
            
            // Attach the shiny new TagComponent for the DragHandler Validations
            var tagComp = floorGrid.AddComponent<Game.GearEngine.Presentation.TagComponent>();
            TagSO gridBoardTag = AssetDatabase.LoadAssetAtPath<TagSO>($"{folderPath}/GridBoard_Tag.asset");
            if (gridBoardTag != null)
            {
                tagComp.AddTag(gridBoardTag); // Validates the generic drag drops!
            }

            // Save Scene
            string sceneDir = "Assets/Scenes";
            if (!System.IO.Directory.Exists(sceneDir))
            {
                System.IO.Directory.CreateDirectory(sceneDir);
            }
            string scenePath = $"{sceneDir}/GearEngine_TestScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"<color=#33ff33>[GearEngine]</color> VContainer Test Scene generated and saved at: {scenePath}");
        }
    }
}
