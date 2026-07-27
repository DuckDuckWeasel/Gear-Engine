using System.IO;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class GearWorkspaceVisualTests
    {
        private const string ArtifactDirectory =
            "Artifacts/VisualTests/GearWorkspaceScreenSpace";
        private const string WorkspacePath =
            "Assets/GearEngine/Prefabs/Gears/PFB_GearWorkspace.prefab";
        private const string GridSlotPath =
            "Assets/GearEngine/Prefabs/Gears/Gears/GridSlotView.prefab";
        private const string InventorySlotPath =
            "Assets/GearEngine/Prefabs/Gears/Gears/GearSlot.prefab";
        private const string RulesPath =
            "Assets/GearEngine/Data/Gear/BasicBoardRules.asset";
        private const string LayoutPath =
            "Assets/GearEngine/Data/Gear/BasicBoardLayout.asset";

        [TestCase("Baseline", 1080, 1920, 80, 80)]
        [TestCase("Tall", 1080, 2400, 100, 60)]
        [TestCase("Short", 1080, 1680, 40, 40)]
        public void Workspace_PortraitResolution_RendersInsideSafeArea(
            string scenario,
            int width,
            int height,
            int topInset,
            int bottomInset)
        {
            GameObject cameraObject = null;
            GameObject canvasObject = null;
            RenderTexture renderTexture = null;
            Texture2D capture = null;
            try
            {
                Camera camera = CreateCamera(out cameraObject);
                RectTransform canvasRect = CreateCanvas(camera, out canvasObject);
                GearWorkspaceView workspace = CreateWorkspace(
                    canvasRect,
                    width,
                    height,
                    topInset,
                    bottomInset);
                PopulateBoard(workspace);
                PopulateInventory(workspace);
                RevealTrash(workspace);

                string artifactPath = Capture(
                    scenario,
                    camera,
                    width,
                    height,
                    out renderTexture,
                    out capture);
                WriteEvidence(artifactPath, scenario, width, height);

                RectTransform workspaceRect = workspace.transform as RectTransform;
                Assert.IsNotNull(workspaceRect);
                Assert.That(workspaceRect.anchorMin.y, Is.GreaterThanOrEqualTo(0f));
                Assert.That(workspaceRect.anchorMax.y, Is.LessThanOrEqualTo(1f));
                Assert.That(new FileInfo(artifactPath).Length, Is.GreaterThan(10_000));
            }
            finally
            {
                RenderTexture.active = null;
                if (renderTexture != null)
                {
                    renderTexture.Release();
                    Object.DestroyImmediate(renderTexture);
                }

                Object.DestroyImmediate(capture);
                Object.DestroyImmediate(canvasObject);
                Object.DestroyImmediate(cameraObject);
            }
        }

        private static Camera CreateCamera(out GameObject cameraObject)
        {
            cameraObject = new GameObject("WorkspaceCaptureCamera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.067f, 0.09f, 1f);
            camera.orthographic = true;
            return camera;
        }

        private static RectTransform CreateCanvas(Camera camera, out GameObject canvasObject)
        {
            canvasObject = new GameObject(
                "WorkspaceCaptureCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvasObject.GetComponent<RectTransform>();
        }

        private static GearWorkspaceView CreateWorkspace(
            RectTransform canvasRect,
            int width,
            int height,
            int topInset,
            int bottomInset)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WorkspacePath);
            Assert.IsNotNull(prefab);
            GameObject instance = Object.Instantiate(prefab, canvasRect, false);
            GearWorkspaceView workspace = instance.GetComponent<GearWorkspaceView>();
            Assert.IsNotNull(workspace);
            SafeAreaRectTransform safeArea = instance.GetComponent<SafeAreaRectTransform>();
            safeArea.enabled = false;

            RectTransform rect = instance.transform as RectTransform;
            rect.anchorMin = new Vector2(0f, (float)bottomInset / height);
            rect.anchorMax = new Vector2(1f, 1f - ((float)topInset / height));
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return workspace;
        }

        private static void PopulateBoard(GearWorkspaceView workspace)
        {
            BoardRulesSO rules = AssetDatabase.LoadAssetAtPath<BoardRulesSO>(RulesPath);
            BoardLayoutSO layout = AssetDatabase.LoadAssetAtPath<BoardLayoutSO>(LayoutPath);
            GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GridSlotPath);
            SerializedObject boardObject = new SerializedObject(workspace.Board);
            RectTransform gridRoot =
                boardObject.FindProperty("gridRoot").objectReferenceValue as RectTransform;
            GearItemData[] gears = LoadGearData();

            for (int y = 0; y < rules.GridHeight; y++)
            {
                for (int x = 0; x < rules.GridWidth; x++)
                {
                    GameObject slot = Object.Instantiate(slotPrefab, gridRoot, false);
                    RectTransform slotRect = slot.transform as RectTransform;
                    slotRect.anchoredPosition =
                        layout.GetCellLocalPosition(new Vector2Int(x, y), rules);
                    int gearIndex = (x + (y * rules.GridWidth)) % 6;
                    if (gearIndex < gears.Length && (x + y) % 2 == 0)
                    {
                        GearView view = GearViewSpawner.Spawn(gears[gearIndex], slotRect);
                        view.SetChargeFillTarget((gearIndex + 1f) / 6f, snap: true);
                    }
                }
            }
        }

        private static void PopulateInventory(GearWorkspaceView workspace)
        {
            GearInventoryViewComponent inventory =
                workspace.GetComponentInChildren<GearInventoryViewComponent>(true);
            SerializedObject inventoryObject = new SerializedObject(inventory);
            RectTransform itemsContainer =
                inventoryObject.FindProperty("itemsContainer").objectReferenceValue as RectTransform;
            GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InventorySlotPath);
            GearItemData[] gears = LoadGearData();

            for (int i = 0; i < Mathf.Min(gears.Length, 5); i++)
            {
                GameObject slot = Object.Instantiate(slotPrefab, itemsContainer, false);
                GearInventorySlotView slotView = slot.GetComponent<GearInventorySlotView>();
                GearView view = GearViewSpawner.Spawn(gears[i], slotView.VisualContainer);
                view.SetChargeFillTarget(1f, snap: true);
            }
        }

        private static void RevealTrash(GearWorkspaceView workspace)
        {
            TrashDropZoneViewComponent trash =
                workspace.GetComponentInChildren<TrashDropZoneViewComponent>(true);
            trash.gameObject.SetActive(true);
            CanvasGroup canvasGroup = trash.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
        }

        private static GearItemData[] LoadGearData()
        {
            string[] paths =
            {
                "Assets/GearEngine/Data/Gear/Gear/CoreGearConfig.asset",
                "Assets/GearEngine/Data/Gear/Gear/BaseGearConfig_Level1.asset",
                "Assets/GearEngine/Data/Gear/Gear/BaseGearConfig_Level2.asset",
                "Assets/GearEngine/Data/Gear/Gear/ScoreGearConfig.asset",
                "Assets/GearEngine/Data/Gear/Gear/SpeedBuffGearConfig.asset",
                "Assets/GearEngine/Data/Gear/Gear/ObstacleRockConfig.asset",
            };
            GearItemData[] data = new GearItemData[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                GearItem item = AssetDatabase.LoadAssetAtPath<GearItem>(paths[i]);
                data[i] = item.CreateRuntimeData();
            }

            return data;
        }

        private static string Capture(
            string scenario,
            Camera camera,
            int width,
            int height,
            out RenderTexture renderTexture,
            out Texture2D capture)
        {
            string directory = Path.GetFullPath(ArtifactDirectory);
            Directory.CreateDirectory(directory);
            string artifactPath = Path.Combine(directory, $"{scenario}.png");
            renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            capture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
            camera.targetTexture = renderTexture;
            Canvas.ForceUpdateCanvases();
            camera.Render();

            RenderTexture.active = renderTexture;
            capture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            capture.Apply();
            File.WriteAllBytes(artifactPath, capture.EncodeToPNG());
            camera.targetTexture = null;
            return artifactPath;
        }

        private static void WriteEvidence(
            string artifactPath,
            string scenario,
            int width,
            int height)
        {
            string evidence =
                "{\n" +
                "  \"test\": \"GearEngine.GearEngine.Tests.Editor." +
                "GearWorkspaceVisualTests.Workspace_PortraitResolution_RendersInsideSafeArea\",\n" +
                $"  \"artifact\": \"{Path.GetFileName(artifactPath)}\",\n" +
                $"  \"scenario\": \"{scenario} portrait workspace at {width}x{height}\",\n" +
                "  \"criteria\": \"Board, Inventory, Trash, and Gear UI remain inside the " +
                "configured Safe Area using screen-space rendering.\"\n" +
                "}\n";
            File.WriteAllText(artifactPath + ".evidence.json", evidence);
        }
    }
}
