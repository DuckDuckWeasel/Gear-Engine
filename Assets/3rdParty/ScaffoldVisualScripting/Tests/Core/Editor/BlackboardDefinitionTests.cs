using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Scaffold.VisualScripting.Authoring;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scaffold.VisualScripting.Tests
{
    public sealed class BlackboardDefinitionTests
    {
        private readonly List<Object> objectsToDestroy = new List<Object>();
        private SerializedGraphCloner cloner;
        private BlackboardDefinitionValidator validator;

        [SetUp]
        public void SetUp()
        {
            cloner = new SerializedGraphCloner();
            validator = new BlackboardDefinitionValidator();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object unityObject in objectsToDestroy)
            {
                Object.DestroyImmediate(unityObject);
            }

            objectsToDestroy.Clear();
        }

        [Test]
        public void CoreAssembly_ContainsNoMonoBehaviourTypes()
        {
            Type[] coreTypes = typeof(BlackboardDefinition).Assembly.GetTypes();

            Assert.That(
                coreTypes.Where(type => typeof(MonoBehaviour).IsAssignableFrom(type)),
                Is.Empty);
        }

        [Test]
        public void DirectReference_InstantiatesIndependentGraphsWithStableDefinitionIds()
        {
            BlackboardDefinition source = CreateDefinition(out TestActionDefinition sourceAction);
            BlackboardDefinitionReference reference = new BlackboardDefinitionReference();
            reference.SetDirect(source);

            BlackboardDefinitionClone first = reference.Instantiate(cloner, validator);
            BlackboardDefinitionClone second = reference.Instantiate(cloner, validator);
            TestActionDefinition firstAction = GetFirstAction(first.Definition);
            TestActionDefinition secondAction = GetFirstAction(second.Definition);

            Assert.That(first.Definition, Is.Not.SameAs(source));
            Assert.That(first.Definition, Is.Not.SameAs(second.Definition));
            Assert.That(
                first.Definition.Blocks[0],
                Is.Not.SameAs(second.Definition.Blocks[0]));
            Assert.That(firstAction, Is.Not.SameAs(sourceAction));
            Assert.That(firstAction, Is.Not.SameAs(secondAction));
            Assert.That(firstAction.Values, Is.Not.SameAs(secondAction.Values));
            Assert.That(first.Definition.DefinitionId, Is.EqualTo(source.DefinitionId));
            Assert.That(firstAction.DefinitionId, Is.EqualTo(sourceAction.DefinitionId));
            Assert.That(
                first.RuntimeInstanceId,
                Is.Not.EqualTo(second.RuntimeInstanceId));

            firstAction.Values.Add(99);
            Assert.That(sourceAction.Values, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(secondAction.Values, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void ScriptableObjectReference_ClonesManagedStateAndPreservesUnityObjectIdentity()
        {
            Texture2D texture = Track(new Texture2D(1, 1));
            BlackboardDefinition source = CreateDefinition(out TestActionDefinition sourceAction);
            sourceAction.UnityReference = texture;
            BlackboardDefinitionAsset asset = Track(
                ScriptableObject.CreateInstance<BlackboardDefinitionAsset>());
            asset.Definition = source;
            BlackboardDefinitionReference reference = new BlackboardDefinitionReference();
            reference.SetScriptableObject(asset);

            BlackboardDefinitionClone clone = reference.Instantiate(cloner, validator);
            TestActionDefinition clonedAction = GetFirstAction(clone.Definition);

            Assert.That(clone.Definition, Is.Not.SameAs(asset.Definition));
            Assert.That(clonedAction, Is.Not.SameAs(sourceAction));
            Assert.That(clonedAction.UnityReference, Is.SameAs(texture));
        }

        [Test]
        public void BlackboardVariableReference_UsesRunningSourceAndCreatesIndependentInstances()
        {
            BlackboardDefinition source = CreateDefinition(out _);
            BlackboardDefinitionVariable variable = new BlackboardDefinitionVariable(source);
            TestVariableSource variableSource = new TestVariableSource(variable);
            BlackboardDefinitionReference reference = new BlackboardDefinitionReference();
            reference.SetBlackboardVariable(variable.DefinitionId);

            BlackboardDefinitionClone first = reference.Instantiate(
                cloner,
                validator,
                variableSource);
            BlackboardDefinitionClone second = reference.Instantiate(
                cloner,
                validator,
                variableSource);

            Assert.That(first.Definition, Is.Not.SameAs(source));
            Assert.That(first.Definition, Is.Not.SameAs(second.Definition));
            Assert.That(
                first.Definition.Blocks[0],
                Is.Not.SameAs(second.Definition.Blocks[0]));
            Assert.That(
                first.RuntimeInstanceId,
                Is.Not.EqualTo(second.RuntimeInstanceId));
        }

        [Test]
        public void CloneGraph_PreservesManagedCycles()
        {
            BlackboardDefinition source = CreateDefinition(out TestActionDefinition sourceAction);
            sourceAction.LinkedAction = sourceAction;

            BlackboardDefinitionClone clone = cloner.Clone(source);
            TestActionDefinition clonedAction = GetFirstAction(clone.Definition);

            Assert.That(clonedAction, Is.Not.SameAs(sourceAction));
            Assert.That(clonedAction.LinkedAction, Is.SameAs(clonedAction));
            Assert.DoesNotThrow(() => validator.ValidateOrThrow(clone.Definition));
        }

        [Test]
        public void CloneGraph_ResetsDelegatesAndTransientState()
        {
            BlackboardDefinition source = CreateDefinition(out TestActionDefinition sourceAction);
            sourceAction.SetTransientState(42, true);
            sourceAction.Callback = () => { };

            BlackboardDefinitionClone clone = cloner.Clone(source);
            TestActionDefinition clonedAction = GetFirstAction(clone.Definition);

            Assert.That(clonedAction.Cache, Is.Zero);
            Assert.That(clonedAction.IsExecuting, Is.False);
            Assert.That(clonedAction.Callback, Is.Null);
        }

        [Test]
        public void Instantiate_MissingDirectTemplate_ThrowsActionableError()
        {
            BlackboardDefinitionReference reference = new BlackboardDefinitionReference();
            reference.SetDirect(null);

            BlackboardDefinitionResolutionException exception = Assert.Throws<
                BlackboardDefinitionResolutionException>(
                () => reference.Instantiate(cloner, validator));

            StringAssert.Contains("Direct", exception.Message);
        }

        [Test]
        public void Instantiate_MissingAsset_ThrowsActionableError()
        {
            BlackboardDefinitionReference reference = new BlackboardDefinitionReference();
            reference.SetScriptableObject(null);

            BlackboardDefinitionResolutionException exception = Assert.Throws<
                BlackboardDefinitionResolutionException>(
                () => reference.Instantiate(cloner, validator));

            StringAssert.Contains("asset", exception.Message.ToLowerInvariant());
        }

        [Test]
        public void Instantiate_VariableWithoutRunningSource_ThrowsStartupError()
        {
            BlackboardDefinitionReference reference = new BlackboardDefinitionReference();
            reference.SetBlackboardVariable(DefinitionId.New());

            BlackboardDefinitionResolutionException exception = Assert.Throws<
                BlackboardDefinitionResolutionException>(
                () => reference.Instantiate(cloner, validator));

            StringAssert.Contains("already-running", exception.Message);
        }

        [Test]
        public void Instantiate_UnresolvedVariable_ThrowsActionableError()
        {
            DefinitionId requestedId = DefinitionId.New();
            BlackboardDefinitionReference reference = new BlackboardDefinitionReference();
            reference.SetBlackboardVariable(requestedId);

            BlackboardDefinitionResolutionException exception = Assert.Throws<
                BlackboardDefinitionResolutionException>(
                () => reference.Instantiate(
                    cloner,
                    validator,
                    new EmptyVariableSource()));

            StringAssert.Contains(requestedId.ToString(), exception.Message);
        }

        [Test]
        public void Validate_NullAction_ReportsItsGraphPath()
        {
            BlackboardDefinition definition = CreateDefinition(out _);
            definition.Blocks[0].Tracks[0].ActionList.Actions.Add(null);

            BlackboardValidationException exception = Assert.Throws<
                BlackboardValidationException>(
                () => validator.ValidateOrThrow(definition));

            StringAssert.Contains("Actions[1]", exception.Message);
            StringAssert.Contains("null", exception.Message.ToLowerInvariant());
        }

        [Test]
        public void Validate_DuplicateDefinitionId_ReportsBothOwners()
        {
            BlackboardDefinition definition = CreateDefinition(out _);
            BlockDefinition duplicate = cloner.CloneGraph(definition.Blocks[0]);
            definition.Blocks.Add(duplicate);

            BlackboardValidationException exception = Assert.Throws<
                BlackboardValidationException>(
                () => validator.ValidateOrThrow(definition));

            StringAssert.Contains("already used", exception.Message);
            StringAssert.Contains("Blocks[1]", exception.Message);
        }

        [Test]
        public void Validate_NestedTemplateCycle_ThrowsDeterministically()
        {
            BlackboardDefinition definition = CreateDefinition(out _);
            definition.Variables.Add(new BlackboardDefinitionVariable(definition));

            BlackboardValidationException exception = Assert.Throws<
                BlackboardValidationException>(
                () => validator.ValidateOrThrow(definition));

            StringAssert.Contains("reference cycle", exception.Message);
        }

        [Test]
        public void EditorDuplication_RegeneratesDefinitionIdsWithoutBreakingCycles()
        {
            BlackboardDefinition source = CreateDefinition(out TestActionDefinition sourceAction);
            sourceAction.LinkedAction = sourceAction;
            BlackboardDefinition duplicate = cloner.Clone(source).Definition;
            TestActionDefinition duplicateAction = GetFirstAction(duplicate);
            DefinitionIdRegenerator regenerator = new DefinitionIdRegenerator();

            regenerator.Regenerate(duplicate);

            Assert.That(duplicate.DefinitionId, Is.Not.EqualTo(source.DefinitionId));
            Assert.That(
                duplicate.Blocks[0].DefinitionId,
                Is.Not.EqualTo(source.Blocks[0].DefinitionId));
            Assert.That(
                duplicateAction.DefinitionId,
                Is.Not.EqualTo(sourceAction.DefinitionId));
            Assert.That(duplicateAction.LinkedAction, Is.SameAs(duplicateAction));
            Assert.DoesNotThrow(() => validator.ValidateOrThrow(duplicate));
        }

        private T Track<T>(T unityObject)
            where T : Object
        {
            objectsToDestroy.Add(unityObject);
            return unityObject;
        }

        private static BlackboardDefinition CreateDefinition(
            out TestActionDefinition action)
        {
            action = new TestActionDefinition();
            action.Values.Add(1);
            action.Values.Add(2);

            ActionListDefinition actionList = new ActionListDefinition();
            actionList.Actions.Add(action);
            ActionTrackDefinition track = new ActionTrackDefinition
            {
                ActionList = actionList,
            };
            BlockDefinition block = new BlockDefinition();
            block.Tracks.Add(track);
            BlackboardDefinition definition = new BlackboardDefinition();
            definition.Blocks.Add(block);
            return definition;
        }

        private static TestActionDefinition GetFirstAction(
            BlackboardDefinition definition)
        {
            return (TestActionDefinition)definition
                .Blocks[0]
                .Tracks[0]
                .ActionList
                .Actions[0];
        }

        [Serializable]
        private sealed class TestActionDefinition : ActionDefinition
        {
            [SerializeField] private List<int> values = new List<int>();
            [SerializeField] private Texture2D unityReference;
            [SerializeReference] private TestActionDefinition linkedAction;
            [BlackboardTransient] private int cache;
            [NonSerialized] private bool isExecuting;

            public Action Callback;

            public List<int> Values => values;

            public Texture2D UnityReference
            {
                get => unityReference;
                set => unityReference = value;
            }

            public TestActionDefinition LinkedAction
            {
                get => linkedAction;
                set => linkedAction = value;
            }

            public int Cache => cache;

            public bool IsExecuting => isExecuting;

            public void SetTransientState(int cachedValue, bool executing)
            {
                cache = cachedValue;
                isExecuting = executing;
            }
        }

        private sealed class TestVariableSource : IBlackboardDefinitionVariableSource
        {
            private readonly BlackboardDefinitionVariable variable;

            public TestVariableSource(BlackboardDefinitionVariable variable)
            {
                this.variable = variable;
            }

            public bool TryGetBlackboardDefinition(
                DefinitionId variableId,
                out BlackboardDefinition definition)
            {
                if (variable.DefinitionId == variableId)
                {
                    definition = variable.Value;
                    return true;
                }

                definition = null;
                return false;
            }
        }

        private sealed class EmptyVariableSource : IBlackboardDefinitionVariableSource
        {
            public bool TryGetBlackboardDefinition(
                DefinitionId variableId,
                out BlackboardDefinition definition)
            {
                definition = null;
                return false;
            }
        }
    }
}
