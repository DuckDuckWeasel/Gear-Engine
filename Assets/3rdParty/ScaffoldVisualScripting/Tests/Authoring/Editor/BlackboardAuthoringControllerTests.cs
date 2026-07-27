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
