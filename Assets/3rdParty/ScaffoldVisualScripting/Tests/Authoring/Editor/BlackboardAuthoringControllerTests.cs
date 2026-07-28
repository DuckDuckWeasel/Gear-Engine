using System;
using System.Collections.Generic;
using NUnit.Framework;
using Scaffold.VisualScripting.Authoring;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor.Tests
{
    public sealed class BlackboardAuthoringControllerTests
    {
        private readonly List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
        private BlackboardDefinitionAsset asset;
        private BlackboardAuthoringController controller;

        [SetUp]
        public void SetUp()
        {
            Undo.ClearAll();
            asset = Track(ScriptableObject.CreateInstance<BlackboardDefinitionAsset>());
            BlackboardAuthoringTarget target = new BlackboardAuthoringTarget(asset, asset.Definition, asset.AuthoringMetadata, "Test");
            BlackboardAuthoringClipboard clipboard = new BlackboardAuthoringClipboard(new SerializedGraphCloner(), new DefinitionIdRegenerator());
            controller = new BlackboardAuthoringController(target, clipboard);
        }

        [TearDown]
        public void TearDown()
        {
            Undo.ClearAll();
            foreach (UnityEngine.Object value in objects)
            {
                if (value != null)
                {
                    UnityEngine.Object.DestroyImmediate(value);
                }
            }

            objects.Clear();
        }

        [Test]
        public void ManagedOperations_AuthorCompleteGraphWithoutComponents()
        {
            BlockDefinition block = controller.AddBlock("Start");
            ActionTrackDefinition track = block.Tracks[0];
            IAction action = controller.AddAction(track.DefinitionId, typeof(TestAction));
            TriggerDefinition trigger = controller.SetTrigger(block.DefinitionId, typeof(TestTrigger));
            VariableDefinitionBase variable = controller.AddVariable(typeof(IntegerVariableDefinition), "Score");

            Assert.That(asset, Is.Not.InstanceOf<Component>());
            Assert.That(block.Name, Is.EqualTo("Start"));
            Assert.That(track.ActionList.Actions, Is.EqualTo(new[] { action }));
            Assert.That(trigger, Is.TypeOf<TestTrigger>());
            Assert.That(variable.Key, Is.EqualTo("Score"));
        }

        [Test]
        public void DuplicateAndClipboard_RegenerateIdsAndPreserveUnityReferences()
        {
            Texture2D texture = Track(new Texture2D(1, 1));
            BlockDefinition block = controller.AddBlock("Clone");
            TestAction action = controller.AddAction(block.Tracks[0].DefinitionId, typeof(TestAction)) as TestAction;
            action.Reference = texture;

            BlockDefinition duplicate = controller.DuplicateBlock(block.DefinitionId);
            controller.CopyBlock(block.DefinitionId);
            BlockDefinition pasted = controller.PasteBlock();

            AssertDistinctGraphIds(block, duplicate);
            AssertDistinctGraphIds(block, pasted);
            Assert.That(((TestAction)duplicate.Tracks[0].ActionList.Actions[0]).Reference, Is.SameAs(texture));
            Assert.That(((TestAction)pasted.Tracks[0].ActionList.Actions[0]).Reference, Is.SameAs(texture));
        }

        [Test]
        public void UndoRedo_RestoresManagedDefinitionMutation()
        {
            controller.AddBlock("Undo");
            Assert.That(asset.Definition.Blocks, Has.Count.EqualTo(1));

            Undo.PerformUndo();
            Assert.That(asset.Definition.Blocks, Is.Empty);

            Undo.PerformRedo();
            Assert.That(asset.Definition.Blocks, Has.Count.EqualTo(1));
        }

        [Test]
        public void ReorderGroupingLayoutTintAndSelection_AreAuthoringOnly()
        {
            BlockDefinition first = controller.AddBlock("First");
            BlockDefinition second = controller.AddBlock("Second");
            ActionTrackDefinition track = first.Tracks[0];
            IAction firstAction = controller.AddAction(track.DefinitionId, typeof(TestAction));
            IAction secondAction = controller.AddAction(track.DefinitionId, typeof(TestAction));

            controller.MoveBlock(second.DefinitionId, 0);
            controller.MoveAction(secondAction.DefinitionId, 0);
            ActionGroupAuthoringMetadata group = controller.GroupActions(track.DefinitionId, new[] { firstAction.DefinitionId, secondAction.DefinitionId }, "Pair");
            controller.SetBlockTint(first.DefinitionId, true, Color.cyan);
            controller.AutoLayout();

            Assert.That(asset.Definition.Blocks[0], Is.SameAs(second));
            Assert.That(track.ActionList.Actions[0], Is.SameAs(secondAction));
            Assert.That(group.ActionIds, Has.Count.EqualTo(2));
            Assert.That(asset.AuthoringMetadata.BlockLayouts, Has.Count.EqualTo(2));
            Assert.That(asset.Definition.GetType().GetField("authoringMetadata"), Is.Null);
            Assert.That(controller.UngroupActions(group.GroupId), Is.True);
        }

        [Test]
        public void Resolver_NavigatesDirectAssetAndVariableTemplates()
        {
            BlackboardAuthoringTargetResolver resolver = new BlackboardAuthoringTargetResolver();
            BlackboardBehaviour direct = CreateBehaviour("Direct");
            direct.DefinitionReference.SetDirect(asset.Definition);
            BlackboardAuthoringTarget directTarget = resolver.Resolve(direct);

            BlackboardBehaviour assetBacked = CreateBehaviour("Asset");
            assetBacked.DefinitionReference.SetScriptableObject(asset);
            BlackboardAuthoringTarget assetTarget = resolver.Resolve(assetBacked);
            BlackboardAuthoringTarget variableTarget = ResolveVariableTarget(resolver, direct);

            Assert.That(directTarget.Definition, Is.SameAs(asset.Definition));
            Assert.That(assetTarget.Owner, Is.SameAs(asset));
            Assert.That(variableTarget.Definition.Name, Is.EqualTo("Nested"));
            Assert.That(variableTarget.Owner, Is.SameAs(direct));
        }

        [Test]
        public void DuplicationUtility_RegeneratesEveryDefinitionId()
        {
            Texture2D texture = Track(new Texture2D(1, 1));
            BlockDefinition block = controller.AddBlock("Duplicate");
            TestAction action = controller.AddAction(block.Tracks[0].DefinitionId, typeof(TestAction)) as TestAction;
            action.Reference = texture;

            BlackboardDefinition duplicate = BlackboardDefinitionDuplicationUtility.CloneWithNewIds(asset.Definition);

            AssertDistinctGraphIds(asset.Definition.Blocks[0], duplicate.Blocks[0]);
            Assert.That(((TestAction)duplicate.Blocks[0].Tracks[0].ActionList.Actions[0]).Reference, Is.SameAs(texture));
        }

        [Test]
        public void TypeCatalog_FindsPlainTriggersAndVariables()
        {
            Assert.That(BlackboardManagedTypeCatalog.GetTriggerTypes(), Does.Contain(typeof(GameStartedTriggerDefinition)));
            Assert.That(BlackboardManagedTypeCatalog.GetVariableTypes(), Does.Contain(typeof(IntegerVariableDefinition)));
        }

        [Test]
        public void TypeCatalog_ExcludesPrivateTestImplementations()
        {
            Assert.That(new List<Type>(BlackboardManagedTypeCatalog.GetActionTypes()).Contains(typeof(TestAction)), Is.False);
            Assert.That(new List<Type>(BlackboardManagedTypeCatalog.GetTriggerTypes()).Contains(typeof(TestTrigger)), Is.False);
        }

        [Test]
        public void TypeCatalog_RequiresActionMenuMetadata()
        {
            List<Type> types = new List<Type>(BlackboardManagedTypeCatalog.GetActionTypes());

            Assert.That(types.Exists(type => type.FullName == "Scaffold.Wait"), Is.True);
            Assert.That(types.Contains(typeof(UndocumentedAction)), Is.False);
        }

        [Test]
        public void ManagedPropertyExpansion_SurvivesSerializedObjectRecreation()
        {
            BlockDefinition block = controller.AddBlock("Foldout");
            IAction action = controller.AddAction(block.Tracks[0].DefinitionId, typeof(UndocumentedAction));
            SerializedObject firstSerialized = new SerializedObject(asset);
            SerializedProperty firstRoot = BlackboardSerializedPropertyRenderer.FindManagedReference(firstSerialized, action);
            SerializedProperty firstDetails = firstRoot.FindPropertyRelative("details");
            firstDetails.isExpanded = true;
            BlackboardSerializedPropertyRenderer.CaptureExpandedState(asset, action, firstDetails);

            SerializedObject rebuiltSerialized = new SerializedObject(asset);
            SerializedProperty rebuiltRoot = BlackboardSerializedPropertyRenderer.FindManagedReference(rebuiltSerialized, action);
            SerializedProperty rebuiltDetails = rebuiltRoot.FindPropertyRelative("details");
            rebuiltDetails.isExpanded = false;
            BlackboardSerializedPropertyRenderer.ApplyExpandedState(asset, action, rebuiltDetails);

            Assert.That(rebuiltDetails.isExpanded, Is.True);
        }

        [TestCase(typeof(GameStartedTriggerDefinition), "Game Started")]
        [TestCase(typeof(IntegerVariableDefinition), "Integer")]
        [TestCase(typeof(TestTrigger), "Test")]
        public void DisplayName_RemovesManagedImplementationSuffixes(Type type, string expected)
        {
            Assert.That(BlackboardEditorDisplay.GetName(type), Is.EqualTo(expected));
        }

        [Test]
        public void AuthoringMetadata_ZoomUsesLegacyRange()
        {
            asset.AuthoringMetadata.Zoom = 2f;
            Assert.That(asset.AuthoringMetadata.Zoom, Is.EqualTo(1f));

            asset.AuthoringMetadata.Zoom = 0.1f;
            Assert.That(asset.AuthoringMetadata.Zoom, Is.EqualTo(0.25f));
        }

        [Test]
        public void DetailPreviewSelection_RequiresExactlyOneSelectedAction()
        {
            BlockDefinition block = controller.AddBlock("Preview");
            ActionTrackDefinition track = block.Tracks[0];
            IAction first = controller.AddAction(track.DefinitionId, typeof(TestAction));
            IAction second = controller.AddAction(track.DefinitionId, typeof(TestAction));
            controller.SelectOnlyAction(track.DefinitionId, first.DefinitionId);

            bool resolved = BlackboardDetailPanel.TryGetSelectedActionPreview(controller, out ActionTrackDefinition selectedTrack, out IAction selectedAction);

            Assert.That(resolved, Is.True);
            Assert.That(selectedTrack, Is.SameAs(track));
            Assert.That(selectedAction, Is.SameAs(first));

            controller.SelectAllActions(track.DefinitionId);

            Assert.That(BlackboardDetailPanel.TryGetSelectedActionPreview(controller, out _, out _), Is.False);
            Assert.That(controller.Metadata.SelectedActionIds, Is.EqualTo(new[] { first.DefinitionId, second.DefinitionId }));
        }

        [TestCase(null, "wait-action", true)]
        [TestCase("wait-action", "debug-log-action", true)]
        [TestCase("wait-action", null, true)]
        [TestCase("wait-action", "wait-action", false)]
        [TestCase(null, null, false)]
        public void DetailPreviewFocus_ReleasesOnlyWhenPreviewActionChanges(string previousActionId, string nextActionId, bool expected)
        {
            Assert.That(
                BlackboardDetailPanel.ShouldReleasePreviewTextFocus(previousActionId, nextActionId),
                Is.EqualTo(expected));
        }

        [TestCase(PlayModeStateChange.ExitingEditMode, false)]
        [TestCase(PlayModeStateChange.EnteredPlayMode, true)]
        [TestCase(PlayModeStateChange.ExitingPlayMode, false)]
        [TestCase(PlayModeStateChange.EnteredEditMode, true)]
        public void PlayModeTransition_RebindsAuthoringTargetAfterContextChanges(
            PlayModeStateChange state,
            bool expected)
        {
            Assert.That(
                BlackboardDefinitionWindow.RequiresTargetRebind(state),
                Is.EqualTo(expected));
        }

        [Test]
        public void PlayFromSelected_ResolvesFlattenedActionIndexAcrossTracks()
        {
            BlockDefinition block = controller.AddBlock("Play");
            ActionTrackDefinition firstTrack = block.Tracks[0];
            controller.AddAction(firstTrack.DefinitionId, typeof(TestAction));
            ActionTrackDefinition secondTrack = controller.AddTrack(
                block.DefinitionId,
                "Second");
            IAction selected = controller.AddAction(
                secondTrack.DefinitionId,
                typeof(TestAction));
            controller.SelectOnlyAction(
                secondTrack.DefinitionId,
                selected.DefinitionId);

            bool resolved =
                BlackboardEditorExecutionController
                    .TryResolveSelectedActionStart(
                        controller,
                        out DefinitionId blockId,
                        out int taskIndex);

            Assert.That(resolved, Is.True);
            Assert.That(blockId, Is.EqualTo(block.DefinitionId));
            Assert.That(taskIndex, Is.EqualTo(1));
        }

        [Test]
        public void RemoveAction_ClearsDeletedSelectionAndPreservesRemainingActions()
        {
            BlockDefinition block = controller.AddBlock("Delete");
            ActionTrackDefinition track = block.Tracks[0];
            IAction deleted = controller.AddAction(
                track.DefinitionId,
                typeof(TestAction));
            IAction remaining = controller.AddAction(
                track.DefinitionId,
                typeof(TestAction));
            controller.SelectOnlyAction(
                track.DefinitionId,
                deleted.DefinitionId);

            bool removed = controller.RemoveAction(deleted.DefinitionId);

            Assert.That(removed, Is.True);
            Assert.That(track.ActionList.Actions, Is.EqualTo(new[] { remaining }));
            Assert.That(
                controller.Metadata.SelectedActionIds,
                Is.Empty);
        }

        [TestCase(100f, 1200f, 300f)]
        [TestCase(340f, 1200f, 340f)]
        [TestCase(600f, 1200f, 440f)]
        [TestCase(340f, 920f, 300f)]
        public void WorkspaceSidePanels_PreserveTheCenterBoard(
            float requested,
            float windowWidth,
            float expected)
        {
            Assert.That(
                BlackboardDefinitionWindow.ClampSidePanelWidth(
                    requested,
                    windowWidth),
                Is.EqualTo(expected));
        }

        [TestCase(false, false, 0.26f)]
        [TestCase(false, true, 0.38f)]
        [TestCase(true, false, 0.55f)]
        [TestCase(true, true, 0.62f)]
        public void ActionRowPresentation_HighlightsHoverAndKeepsControlsContextual(
            bool selected,
            bool hovered,
            float expectedAlpha)
        {
            Assert.That(
                BlackboardDetailPanel.GetActionRowAlpha(
                    selected,
                    hovered),
                Is.EqualTo(expectedAlpha));
            Assert.That(
                BlackboardDetailPanel.ShouldShowActionControls(hovered),
                Is.EqualTo(hovered));
        }

        [Test]
        public void MultiSelection_MoveGestureMovesEverySelectedBlockAndSupportsUndo()
        {
            BlockDefinition first = controller.AddBlock("First");
            BlockDefinition second = controller.AddBlock("Second");
            controller.SetBlockPosition(first.DefinitionId, new Vector2(10f, 20f));
            controller.SetBlockPosition(second.DefinitionId, new Vector2(50f, 80f));
            controller.SelectBlocks(new[] { first.DefinitionId, second.DefinitionId });

            controller.BeginBlockMove();
            controller.MoveSelectedBlocks(new Vector2(15f, -5f));
            controller.EndBlockMove();

            Assert.That(controller.GetLayout(first.DefinitionId).Position.position, Is.EqualTo(new Vector2(25f, 15f)));
            Assert.That(controller.GetLayout(second.DefinitionId).Position.position, Is.EqualTo(new Vector2(65f, 75f)));

            Undo.PerformUndo();

            Assert.That(controller.GetLayout(first.DefinitionId).Position.position, Is.EqualTo(new Vector2(10f, 20f)));
            Assert.That(controller.GetLayout(second.DefinitionId).Position.position, Is.EqualTo(new Vector2(50f, 80f)));
        }

        [Test]
        public void MultiBlockClipboard_RegeneratesIdsAndPreservesOrderedSelection()
        {
            BlockDefinition first = controller.AddBlock("First");
            BlockDefinition second = controller.AddBlock("Second");
            controller.SelectBlocks(new[] { first.DefinitionId, second.DefinitionId });
            controller.CopySelectedBlocks();

            IReadOnlyList<BlockDefinition> pasted = controller.PasteBlocks(new Vector2(300f, 140f));

            Assert.That(pasted, Has.Count.EqualTo(2));
            Assert.That(pasted[0].DefinitionId, Is.Not.EqualTo(first.DefinitionId));
            Assert.That(pasted[1].DefinitionId, Is.Not.EqualTo(second.DefinitionId));
            Assert.That(controller.Metadata.SelectedBlockIds, Is.EqualTo(new[] { pasted[0].DefinitionId, pasted[1].DefinitionId }));
            Assert.That(controller.GetLayout(pasted[0].DefinitionId).Position.position, Is.EqualTo(new Vector2(300f, 140f)));
            Assert.That(controller.GetLayout(pasted[1].DefinitionId).Position.position, Is.EqualTo(new Vector2(324f, 164f)));
        }

        [Test]
        public void RenameBlock_DoesNotChangeAnAlreadyUniqueName()
        {
            BlockDefinition block = controller.AddBlock("Stable");

            controller.RenameBlock(block.DefinitionId, "Stable");

            Assert.That(block.Name, Is.EqualTo("Stable"));
        }

        [Test]
        public void MoveActionAcrossTracks_CleansSourceGroupAndMovesSelection()
        {
            BlockDefinition block = controller.AddBlock("Actions");
            ActionTrackDefinition source = block.Tracks[0];
            ActionTrackDefinition destination = controller.AddTrack(block.DefinitionId, "Destination");
            IAction first = controller.AddAction(source.DefinitionId, typeof(TestAction));
            IAction moved = controller.AddAction(source.DefinitionId, typeof(TestAction));
            ActionGroupAuthoringMetadata group = controller.GroupActions(source.DefinitionId, new[] { first.DefinitionId, moved.DefinitionId }, "Pair");

            controller.MoveActionToTrack(moved.DefinitionId, destination.DefinitionId, 0);

            Assert.That(source.ActionList.Actions, Is.EqualTo(new[] { first }));
            Assert.That(destination.ActionList.Actions, Is.EqualTo(new[] { moved }));
            Assert.That(group.ActionIds, Is.EqualTo(new[] { first.DefinitionId }));
            Assert.That(controller.Metadata.SelectedTrackId, Is.EqualTo(destination.DefinitionId));
        }

        [Test]
        public void SelectActionRange_SelectsEveryActionBetweenAnchorAndDestination()
        {
            BlockDefinition block = controller.AddBlock("Range");
            ActionTrackDefinition track = block.Tracks[0];
            IAction first = controller.AddAction(track.DefinitionId, typeof(TestAction));
            IAction middle = controller.AddAction(track.DefinitionId, typeof(TestAction));
            IAction last = controller.AddAction(track.DefinitionId, typeof(TestAction));
            controller.SelectOnlyAction(track.DefinitionId, first.DefinitionId);

            controller.SelectActionRange(track.DefinitionId, last.DefinitionId);

            Assert.That(controller.Metadata.SelectedActionIds, Is.EqualTo(new[] { first.DefinitionId, middle.DefinitionId, last.DefinitionId }));
        }

        [Test]
        public void VariableDuplicateAndSort_PreserveManagedValues()
        {
            IntegerVariableDefinition second = controller.AddVariable(typeof(IntegerVariableDefinition), "Zulu") as IntegerVariableDefinition;
            second.InitialValue = 42;
            controller.AddVariable(typeof(StringVariableDefinition), "Alpha");

            IntegerVariableDefinition duplicate = controller.DuplicateVariable(second.DefinitionId) as IntegerVariableDefinition;
            controller.SortVariablesByName();

            Assert.That(duplicate.DefinitionId, Is.Not.EqualTo(second.DefinitionId));
            Assert.That(duplicate.InitialValue, Is.EqualTo(42));
            Assert.That(asset.Definition.Variables[0].Key, Is.EqualTo("Alpha"));
        }

        [Test]
        public void CompatibilityVariablePicker_FiltersDefinitionsByValueType()
        {
            FloatVariableDefinition speed =
                controller.AddVariable(
                    typeof(FloatVariableDefinition),
                    "Speed") as FloatVariableDefinition;
            controller.AddVariable(
                typeof(IntegerVariableDefinition),
                "Lives");

            IReadOnlyList<VariableDefinitionBase> compatible =
                BlackboardCompatibilityVariableDataDrawer
                    .GetCompatibleVariables(
                        asset.Definition,
                        typeof(float));

            Assert.That(compatible, Is.EqualTo(new[] { speed }));
        }

        [Test]
        public void CompatibilityVariablePicker_FiltersUnityObjectsByAssignableType()
        {
            GameObject target = Track(new GameObject("Target"));
            Texture2D texture = Track(new Texture2D(1, 1));
            UnityObjectVariableDefinition targetVariable =
                controller.AddVariable(
                    typeof(UnityObjectVariableDefinition),
                    "Target") as UnityObjectVariableDefinition;
            targetVariable.InitialValue = target;
            UnityObjectVariableDefinition textureVariable =
                controller.AddVariable(
                    typeof(UnityObjectVariableDefinition),
                    "Texture") as UnityObjectVariableDefinition;
            textureVariable.InitialValue = texture;

            IReadOnlyList<VariableDefinitionBase> compatible =
                BlackboardCompatibilityVariableDataDrawer
                    .GetCompatibleVariables(
                        asset.Definition,
                        typeof(GameObject));

            Assert.That(
                compatible,
                Is.EqualTo(new[] { targetVariable }));
        }

        [Test]
        public void ConnectionResolver_UsesManagedConnectionContract()
        {
            BlockDefinition source = controller.AddBlock("Source");
            BlockDefinition destination = controller.AddBlock("Destination");
            TestConnectionAction action = controller.AddAction(source.Tracks[0].DefinitionId, typeof(TestConnectionAction)) as TestConnectionAction;
            action.TargetName = destination.Name;

            IReadOnlyList<BlackboardGraphConnection> connections = new BlackboardGraphConnectionResolver().Resolve(asset.Definition);

            Assert.That(connections, Has.Count.EqualTo(1));
            Assert.That(connections[0].Source, Is.SameAs(source));
            Assert.That(connections[0].Destination, Is.SameAs(destination));
        }

        private BlackboardAuthoringTarget ResolveVariableTarget(BlackboardAuthoringTargetResolver resolver, BlackboardBehaviour source)
        {
            BlackboardDefinition nested = new BlackboardDefinition { Name = "Nested" };
            BlackboardDefinitionVariable variable = new BlackboardDefinitionVariable(nested) { Key = "Template" };
            source.DefinitionReference.DirectDefinition.Variables.Add(variable);
            BlackboardBehaviour child = CreateBehaviour("Variable");
            child.SourceBehaviour = source;
            child.DefinitionReference.SetBlackboardVariable(variable.DefinitionId);
            return resolver.Resolve(child);
        }

        private BlackboardBehaviour CreateBehaviour(string name)
        {
            GameObject gameObject = Track(new GameObject(name));
            return gameObject.AddComponent<BlackboardBehaviour>();
        }

        private T Track<T>(T value) where T : UnityEngine.Object
        {
            objects.Add(value);
            return value;
        }

        private static void AssertDistinctGraphIds(BlockDefinition first, BlockDefinition second)
        {
            Assert.That(second.DefinitionId, Is.Not.EqualTo(first.DefinitionId));
            Assert.That(second.Tracks[0].DefinitionId, Is.Not.EqualTo(first.Tracks[0].DefinitionId));
            Assert.That(second.Tracks[0].ActionList.DefinitionId, Is.Not.EqualTo(first.Tracks[0].ActionList.DefinitionId));
            Assert.That(second.Tracks[0].ActionList.Actions[0].DefinitionId, Is.Not.EqualTo(first.Tracks[0].ActionList.Actions[0].DefinitionId));
        }

        [Serializable]
        private sealed class TestAction : ActionBase
        {
            public UnityEngine.Object Reference
            {
                get => reference;
                set => reference = value;
            }

            [SerializeField] private UnityEngine.Object reference;

            protected override void OnExecute()
            {
                Succeed();
            }
        }

        [Serializable]
        private sealed class TestConnectionAction : ActionBase, IBlockConnectionSource
        {
            public string TargetName { get; set; }

            public void GetConnectedBlockNames(ICollection<string> blockNames)
            {
                if (!string.IsNullOrWhiteSpace(TargetName))
                {
                    blockNames.Add(TargetName);
                }
            }

            protected override void OnExecute()
            {
                Succeed();
            }
        }

        [Serializable]
        public sealed class UndocumentedAction : ActionBase
        {
            [SerializeField] private FoldoutDetails details = new FoldoutDetails();

            protected override void OnExecute()
            {
                Succeed();
            }
        }

        [Serializable]
        private sealed class FoldoutDetails
        {
            [SerializeField] private int value;
        }

        [Serializable]
        private sealed class TestTrigger : TriggerDefinition
        {
            public override ITriggerBinding CreateBinding(TriggerExecutionContext context)
            {
                return new TestBinding();
            }
        }

        private sealed class TestBinding : ITriggerBinding
        {
            public bool IsEnabled { get; private set; }

            public void Enable()
            {
                IsEnabled = true;
            }

            public void Disable()
            {
                IsEnabled = false;
            }

            public void Tick()
            {
            }

            public void Dispose()
            {
                IsEnabled = false;
            }
        }
    }
}
