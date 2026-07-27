using System.IO;
using System.Linq;
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

        private static GameObject LoadPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"Prefab could not be loaded: {path}");
            return prefab;
        }
    }
}
