using System.IO;
using System.Linq;
using System.Reflection;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class GearWorkspaceAssetTests
    {
        private const string WorkspacePath =
            "Assets/GearEngine/Prefabs/Gears/PFB_GearWorkspace.prefab";
        private const string SetupPath =
            "Assets/GearEngine/Prefabs/Campaign/Setup View.prefab";
        private const string RoguelikePath =
            "Assets/GearEngine/Prefabs/Campaign/Campaign_RoguelikeView.prefab";
        private const string RacePath =
            "Assets/GearEngine/Prefabs/Campaign/Race View.prefab";
        private const string MainScenePath =
            "Assets/GearEngine/Scenes/Main Scene.unity";

        [Test]
        public void BaseGearPrefab_PreservesGuid()
        {
            string guid = AssetDatabase.AssetPathToGUID(
                "Assets/GearEngine/Prefabs/Gears/Gears/BaseGearView.prefab");

            Assert.That(guid, Is.EqualTo("16426345c7f1e4cfcbe7499d444b3f6d"));
        }

        [Test]
        public void Workspace_IsCanvaslessAndContainsNoWorldInteractionComponents()
        {
            GameObject workspace = LoadPrefab(WorkspacePath);

            Assert.That(workspace.GetComponentsInChildren<Canvas>(true), Is.Empty);
            Assert.That(workspace.GetComponentsInChildren<SpriteRenderer>(true), Is.Empty);
            Assert.That(workspace.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(workspace.GetComponentsInChildren<Collider2D>(true), Is.Empty);
            Assert.That(workspace.GetComponentsInChildren<SortingGroup>(true), Is.Empty);
            Assert.That(workspace.GetComponentsInChildren<PhysicsRaycaster>(true), Is.Empty);
            Assert.That(workspace.GetComponentsInChildren<Physics2DRaycaster>(true), Is.Empty);
        }

        [Test]
        public void ConfiguredGearPrefabs_UseUiRenderingOnly()
        {
            GearView[] referencedPrefabs = AssetDatabase.FindAssets("t:GearItem")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<GearItem>)
                .Where(item => item != null)
                .Select(item => item.CreateRuntimeData().ViewPrefab)
                .Where(view => view != null)
                .Distinct()
                .ToArray();

            Assert.That(referencedPrefabs, Is.Not.Empty);
            foreach (GearView view in referencedPrefabs)
            {
                Assert.IsInstanceOf<RectTransform>(view.transform, view.name);
                Assert.That(view.GetComponentsInChildren<Image>(true), Is.Not.Empty, view.name);
                Assert.That(view.GetComponentsInChildren<SpriteRenderer>(true), Is.Empty, view.name);
                Assert.That(view.GetComponentsInChildren<Collider>(true), Is.Empty, view.name);
                Assert.That(view.GetComponentsInChildren<Collider2D>(true), Is.Empty, view.name);
            }
        }

        [TestCase(SetupPath, GearWorkspaceMode.Interactive)]
        [TestCase(RoguelikePath, GearWorkspaceMode.Interactive)]
        [TestCase(RacePath, GearWorkspaceMode.ReadOnly)]
        public void CampaignScreen_OwnsWorkspaceWithExpectedMode(
            string prefabPath,
            GearWorkspaceMode expectedMode)
        {
            GameObject screen = LoadPrefab(prefabPath);
            GearWorkspaceView workspace =
                screen.GetComponentInChildren<GearWorkspaceView>(includeInactive: true);

            Assert.IsNotNull(workspace, $"{prefabPath} must own a GearWorkspaceView.");
            Assert.That(workspace.Mode, Is.EqualTo(expectedMode));
        }

        [Test]
        public void MainScene_DoesNotReferenceStandaloneWorkspaceParts()
        {
            string sceneYaml = File.ReadAllText(MainScenePath);

            Assert.That(sceneYaml, Does.Not.Contain("1bb691964ce722a4ba6aed0bf6fb73c1"));
            Assert.That(sceneYaml, Does.Not.Contain("ba83461cca946410a94c4343a45a483e"));
            Assert.That(sceneYaml, Does.Not.Contain("5c0f2c75741614be083b530936f65100"));
        }

        [Test]
        public void TrashPlacement_AlignsAboveTopRightBoardCell()
        {
            GameObject parentObject = new GameObject("Workspace", typeof(RectTransform));
            RectTransform parent = parentObject.GetComponent<RectTransform>();
            parent.sizeDelta = new Vector2(1000f, 1000f);
            RectTransform cell = CreateRect(
                "TopRightCell",
                parent,
                new Vector2(100f, 100f),
                new Vector2(200f, 100f));
            RectTransform trashRect = CreateRect(
                "Trash",
                parent,
                new Vector2(180f, 96f),
                Vector2.zero);
            TrashDropZoneViewComponent trash =
                trashRect.gameObject.AddComponent<TrashDropZoneViewComponent>();
            trash.SetReferences(trashRect, null, null, null);
            BoardLayoutSO layout = ScriptableObject.CreateInstance<BoardLayoutSO>();
            layout.TrashZoneYOffset = 24f;
            trash.SetBoardPresentation(layout);

            FieldInfo topRightCellField = typeof(TrashDropZoneViewComponent)
                .GetField("topRightCell", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(topRightCellField);
            topRightCellField.SetValue(trash, cell);
            MethodInfo placementMethod = typeof(TrashDropZoneViewComponent)
                .GetMethod(
                    "ApplyScreenSpacePlacement",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(placementMethod);

            placementMethod.Invoke(trash, null);

            Assert.That(trashRect.anchoredPosition.x, Is.EqualTo(200f).Within(0.01f));
            Assert.That(trashRect.anchoredPosition.y, Is.EqualTo(222f).Within(0.01f));

            Object.DestroyImmediate(layout);
            Object.DestroyImmediate(parentObject);
        }

        private static RectTransform CreateRect(
            string objectName,
            Transform parent,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            return rect;
        }

        private static GameObject LoadPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Prefab could not be loaded: {path}");
            return prefab;
        }
    }
}
