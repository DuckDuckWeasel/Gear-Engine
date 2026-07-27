using System.IO;
using System.Linq;
using System.Reflection;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class GearWorkspaceAssetTests
    {
        private const string k_workspacePath =
            "Assets/GearEngine/Prefabs/Gears/PFB_GearWorkspace.prefab";
        private const string k_setupPath =
            "Assets/GearEngine/Prefabs/Campaign/Setup View.prefab";
        private const string k_boardViewPath =
            "Assets/GearEngine/Prefabs/Campaign/PFB_BoardView.prefab";
        private const string k_roguelikePath =
            "Assets/GearEngine/Prefabs/Campaign/Campaign_RoguelikeView.prefab";
        private const string k_racePath =
            "Assets/GearEngine/Prefabs/Campaign/Race View.prefab";
        private const string k_mainScenePath =
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
            GameObject workspace = LoadPrefab(k_workspacePath);

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

        [TestCase(k_roguelikePath, GearWorkspaceMode.Interactive)]
        [TestCase(k_racePath, GearWorkspaceMode.ReadOnly)]
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
        public void SetupPrefab_OwnsOnlyInventoryWorkspaceContent()
        {
            GameObject setup = LoadPrefab(k_setupPath);
            GearInventoryViewComponent inventory =
                setup.GetComponentInChildren<GearInventoryViewComponent>(true);
            MonoBehaviour setupView = setup.GetComponents<MonoBehaviour>()
                .Single(component =>
                    component != null && component.GetType().Name == "SetupView");
            SerializedObject serializedView = new SerializedObject(setupView);

            Assert.IsNotNull(inventory);
            Assert.IsTrue(inventory.gameObject.activeSelf);
            Assert.That(inventory.transform.localScale.x, Is.GreaterThan(0.01f));
            Assert.That(inventory.transform.localScale.y, Is.GreaterThan(0.01f));
            Assert.That(inventory.transform.localScale.z, Is.GreaterThan(0.01f));
            Assert.That(
                serializedView.FindProperty("inventory").objectReferenceValue,
                Is.SameAs(inventory));
            Assert.That(
                serializedView.FindProperty("boardView").objectReferenceValue,
                Is.Null);
            Assert.That(
                setup.GetComponentsInChildren<BoardViewComponent>(true),
                Is.Empty);
            Assert.That(
                setup.GetComponentsInChildren<TrashDropZoneViewComponent>(true),
                Is.Empty);
        }

        [Test]
        public void BoardViewPrefab_OwnsBoardTrashAndOverlay()
        {
            GameObject prefab = LoadPrefab(k_boardViewPath);
            BoardView boardView = prefab.GetComponent<BoardView>();
            SerializedObject serializedView = new SerializedObject(boardView);

            Assert.IsNotNull(boardView);
            Assert.IsNotNull(prefab.GetComponent<Canvas>());
            Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one));
            BoardViewComponent grid =
                prefab.GetComponentInChildren<BoardViewComponent>(true);
            Assert.IsNotNull(grid);
            Assert.That(grid.transform.localScale.x, Is.EqualTo(1.1f));
            Assert.That(grid.transform.localScale.y, Is.EqualTo(1.1f));
            Assert.That(
                serializedView.FindProperty("board").objectReferenceValue,
                Is.Not.Null);
            Assert.That(
                serializedView.FindProperty("trash").objectReferenceValue,
                Is.Not.Null);
            Assert.That(
                serializedView.FindProperty("dragOverlay").objectReferenceValue,
                Is.Not.Null);
        }

        [Test]
        public void MainScene_OwnsBoardViewBesideSetupView()
        {
            Scene scene = EditorSceneManager.OpenScene(k_mainScenePath, OpenSceneMode.Additive);
            try
            {
                BoardView boardView = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<BoardView>(true))
                    .Single();
                MonoBehaviour setupView = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                    .Single(component =>
                        component != null && component.GetType().Name == "SetupView");
                MonoBehaviour activeRaceView = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                    .Single(component =>
                        component != null && component.GetType().Name == "ActiveRaceView");
                SerializedProperty boardViewProperty =
                    new SerializedObject(setupView).FindProperty("boardView");
                SerializedProperty activeRaceBoardProperty =
                    new SerializedObject(activeRaceView).FindProperty("board");

                Assert.IsNotNull(boardViewProperty);
                Assert.IsNotNull(activeRaceBoardProperty);
                Assert.That(boardViewProperty.objectReferenceValue, Is.SameAs(boardView));
                Assert.That(activeRaceBoardProperty.objectReferenceValue, Is.SameAs(boardView));
                Assert.That(boardView.transform.parent, Is.SameAs(setupView.transform.parent));
                Assert.IsFalse(
                    boardView.gameObject.activeSelf,
                    "The shared BoardView must remain inactive on initial Main Scene load.");
                Assert.That(boardView.transform.localScale.x, Is.GreaterThan(0.01f));
                Assert.That(boardView.transform.localScale.y, Is.GreaterThan(0.01f));
                Assert.That(boardView.transform.localScale.z, Is.GreaterThan(0.01f));
                Assert.That(
                    boardView.GetComponent<Canvas>().worldCamera,
                    Is.SameAs(setupView.GetComponent<Canvas>().worldCamera));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, removeScene: true);
            }
        }

        [Test]
        public void SetupClose_KeepsSharedBoardVisibleForRace()
        {
            GameObject setupInstance = Object.Instantiate(LoadPrefab(k_setupPath));
            GameObject boardInstance = Object.Instantiate(LoadPrefab(k_boardViewPath));
            MonoBehaviour setupView = setupInstance.GetComponents<MonoBehaviour>()
                .Single(component =>
                    component != null && component.GetType().Name == "SetupView");
            SerializedObject serializedView = new SerializedObject(setupView);
            serializedView.FindProperty("boardView").objectReferenceValue =
                boardInstance.GetComponent<BoardView>();
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            boardInstance.SetActive(true);

            MethodInfo onClose = setupView.GetType().GetMethod(
                "OnClose",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(onClose);
            onClose.Invoke(setupView, new object[] { false });

            Assert.IsTrue(boardInstance.activeSelf);

            Object.DestroyImmediate(boardInstance);
            Object.DestroyImmediate(setupInstance);
        }

        [Test]
        public void MainScene_HasSingleInputSystemEventSystem()
        {
            Scene scene = EditorSceneManager.OpenScene(k_mainScenePath, OpenSceneMode.Additive);
            try
            {
                EventSystem[] eventSystems = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<EventSystem>(true))
                    .ToArray();

                Assert.That(eventSystems, Has.Length.EqualTo(1));
                Assert.IsNotNull(
                    eventSystems[0].GetComponent<InputSystemUIInputModule>());
                Assert.IsNull(
                    eventSystems[0].GetComponent<StandaloneInputModule>());
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, removeScene: true);
            }
        }

        [Test]
        public void TrashPlacement_IsAuthoredByBoardViewPrefab()
        {
            GameObject prefab = LoadPrefab(k_boardViewPath);
            TrashDropZoneViewComponent prefabTrash =
                prefab.GetComponentInChildren<TrashDropZoneViewComponent>(true);
            GameObject instance = Object.Instantiate(prefab);
            TrashDropZoneViewComponent instanceTrash =
                instance.GetComponentInChildren<TrashDropZoneViewComponent>(true);

            Assert.IsNotNull(prefabTrash);
            Assert.IsNotNull(instanceTrash);
            Assert.That(
                ((RectTransform)instanceTrash.transform).anchoredPosition,
                Is.EqualTo(((RectTransform)prefabTrash.transform).anchoredPosition));
            Assert.That(
                typeof(TrashDropZoneViewComponent)
                    .GetMethods()
                    .Any(method => method.Name == "SetBoardPresentation"),
                Is.False);

            Object.DestroyImmediate(instance);
        }

        private static GameObject LoadPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Prefab could not be loaded: {path}");
            return prefab;
        }
    }
}
