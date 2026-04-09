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
            GearConfig core = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/CoreGearConfig.asset");
            GearConfig baseGear = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/BaseGearConfig_Level1.asset");
            
            if (core != null && baseGear != null)
            {
                var so = new SerializedObject(bootstrap);
                var prop = so.FindProperty("initialGears");
                prop.arraySize = 2;
                prop.GetArrayElementAtIndex(0).objectReferenceValue = core;
                prop.GetArrayElementAtIndex(1).objectReferenceValue = baseGear;
                so.ApplyModifiedProperties();
            }

            // Create Canvas and Event System for UI Testing
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            GameObject canvasObj = new GameObject("TestCanvas");
            Canvas testCanvas = canvasObj.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Setup Simulation View
            GameObject simViewObj = new GameObject("SimulationControlView");
            simViewObj.transform.SetParent(canvasObj.transform, false);
            var simViewDef = simViewObj.AddComponent<Game.GearEngine.Presentation.SimulationControlView>();
            // Add a mock button
            var simBtn = simViewObj.AddComponent<UnityEngine.UI.Button>();
            var simTxtObj = new GameObject("Text");
            simTxtObj.transform.SetParent(simViewObj.transform, false);
            var simTxt = simTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
            simTxt.text = "Toggle simulation";
            
            // Note: Scaffold MVVM Binding requires hooking the raw Rect/Button. 
            // In a real flow, a developer drags this into an inspector, this is just to guarantee the view is present in the scene.

            // Setup Board Collider to act as a hit target for DragHandler Raycasts 
            GameObject floorGrid = new GameObject("GridBoardCollider");
            floorGrid.tag = "GridBoard"; // Standard mapping
            var boxCol = floorGrid.AddComponent<BoxCollider>();
            boxCol.size = new Vector3(50, 50, 0.5f);
            floorGrid.transform.position = new Vector3(0, 0, 0.5f);
            
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
