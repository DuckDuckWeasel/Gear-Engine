using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace Scaffold.VisualScripting.Tests
{
    public sealed class BlackboardVariableRuntimeTests
    {
        [Test]
        public void ClonedBlackboards_DoNotShareLocalOrPublicCells()
        {
            CreateVariableSets(out BlackboardVariableSet first, out BlackboardVariableSet second, out IntegerVariableDefinition local, out IntegerVariableDefinition publicVariable, out _);

            first.Get<int>(CreateReference(local)).Value = 11;
            first.Get<int>(CreateReference(publicVariable)).Value = 12;

            Assert.That(second.Get<int>(CreateReference(local)).Value, Is.EqualTo(1));
            Assert.That(second.Get<int>(CreateReference(publicVariable)).Value, Is.EqualTo(2));
            Assert.That(first.Get<int>(CreateReference(local)), Is.Not.SameAs(second.Get<int>(CreateReference(local))));
        }

        [Test]
        public void PublicReference_CanAddressOneSpecificRunningBlackboard()
        {
            CreateVariableSets(out BlackboardVariableSet first, out BlackboardVariableSet second, out _, out IntegerVariableDefinition publicVariable, out _);
            first.Get<int>(CreateReference(publicVariable)).Value = 23;
            VariableReference reference = CreateReference(publicVariable);
            reference.SourceRuntimeInstanceId = first.RuntimeInstanceId;

            VariableCell<int> resolved = second.Get<int>(reference);

            Assert.That(resolved.Value, Is.EqualTo(23));
            Assert.That(resolved, Is.SameAs(first.Get<int>(CreateReference(publicVariable))));
        }

        [Test]
        public void InjectedGlobalReference_ExplicitlySharesOneCell()
        {
            CreateVariableSets(out BlackboardVariableSet first, out BlackboardVariableSet second, out _, out _, out IntegerVariableDefinition global);

            first.Get<int>(CreateReference(global)).Value = 31;

            Assert.That(second.Get<int>(CreateReference(global)).Value, Is.EqualTo(31));
            Assert.That(first.Get<int>(CreateReference(global)), Is.SameAs(second.Get<int>(CreateReference(global))));
        }

        [Test]
        public void Dispose_RemovesOnlyOwnedPublicRegistrations()
        {
            PublicVariableRegistry registry = new PublicVariableRegistry();
            GlobalVariableStore globalStore = new GlobalVariableStore();
            BlackboardDefinition definition = CreateDefinition(out _, out IntegerVariableDefinition publicVariable, out _);
            BlackboardDefinitionClone clone = new SerializedGraphCloner().Clone(definition);
            BlackboardVariableSet variables = new BlackboardVariableSet(clone.RuntimeInstanceId, clone.Definition.Variables, registry, globalStore);
            VariableAddress address = new VariableAddress(clone.RuntimeInstanceId, publicVariable.DefinitionId);

            variables.Dispose();

            Assert.That(registry.TryGet(address, out _), Is.False);
        }

        [Test]
        public void DefinitionVariableSource_ReturnsDefinitionNotRuntime()
        {
            BlackboardDefinition nested = new BlackboardDefinition();
            BlackboardDefinitionVariable variable = new BlackboardDefinitionVariable(nested)
            {
                Key = "Template",
            };
            BlackboardDefinition root = new BlackboardDefinition();
            root.Variables.Add(variable);
            BlackboardDefinitionClone clone = new SerializedGraphCloner().Clone(root);
            BlackboardVariableSet variables = new BlackboardVariableSet(clone.RuntimeInstanceId, clone.Definition.Variables, new PublicVariableRegistry(), new GlobalVariableStore());

            bool found = variables.TryGetBlackboardDefinition(variable.DefinitionId, out BlackboardDefinition resolved);

            Assert.That(found, Is.True);
            Assert.That(resolved, Is.Not.SameAs(nested));
            Assert.That(resolved.DefinitionId, Is.EqualTo(nested.DefinitionId));
        }

        [Test]
        public void Persistence_UsesRuntimeAndDefinitionIds()
        {
            BlackboardDefinition definition = CreateDefinition(out IntegerVariableDefinition local, out _, out _);
            BlackboardDefinitionClone clone = new SerializedGraphCloner().Clone(definition);
            BlackboardVariableSet variables = new BlackboardVariableSet(clone.RuntimeInstanceId, clone.Definition.Variables, new PublicVariableRegistry(), new GlobalVariableStore());
            VariableCell<int> cell = variables.Get<int>(CreateReference(local));
            cell.Value = 45;
            TestLogger logger = new TestLogger();
            BlackboardVariablePersistence persistence = new BlackboardVariablePersistence(new TestValueSerializer(), logger);

            BlackboardSaveData data = persistence.Capture(variables);
            cell.Value = 3;
            persistence.Apply(data, variables);

            Assert.That(data.RuntimeInstanceId, Is.EqualTo(clone.RuntimeInstanceId));
            Assert.That(data.Variables[0].DefinitionId, Is.EqualTo(local.DefinitionId));
            Assert.That(cell.Value, Is.EqualTo(45));
            Assert.That(logger.Errors, Is.Empty);
        }

        [Test]
        public void Persistence_RejectsAnotherRuntimeAndLogsTheFailure()
        {
            BlackboardDefinition definition = CreateDefinition(out _, out _, out _);
            BlackboardDefinitionClone clone = new SerializedGraphCloner().Clone(definition);
            BlackboardVariableSet variables = new BlackboardVariableSet(clone.RuntimeInstanceId, clone.Definition.Variables, new PublicVariableRegistry(), new GlobalVariableStore());
            TestLogger logger = new TestLogger();
            BlackboardVariablePersistence persistence = new BlackboardVariablePersistence(new TestValueSerializer(), logger);
            BlackboardSaveData captured = persistence.Capture(variables);
            BlackboardSaveData wrongRuntime = new BlackboardSaveData(BlackboardRuntimeInstanceId.New(), captured.Variables);

            Assert.Throws<InvalidOperationException>(() => persistence.Apply(wrongRuntime, variables));
            Assert.That(logger.Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void TextSubstitution_UsesInjectedVariableSet()
        {
            BlackboardDefinition definition = CreateDefinition(out IntegerVariableDefinition local, out _, out _);
            BlackboardDefinitionClone clone = new SerializedGraphCloner().Clone(definition);
            BlackboardVariableSet variables = new BlackboardVariableSet(clone.RuntimeInstanceId, clone.Definition.Variables, new PublicVariableRegistry(), new GlobalVariableStore());
            variables.Get<int>(CreateReference(local)).Value = 7;
            TestLogger logger = new TestLogger();
            TextSubstitutionService service = new TextSubstitutionService(logger);

            string result = service.Substitute("Value=${Local}", variables);

            Assert.That(result, Is.EqualTo("Value=7"));
            Assert.That(logger.Errors, Is.Empty);
        }

        [Test]
        public void EventBus_DisposalDetachesSubscriptionSymmetrically()
        {
            BlackboardEventBus eventBus = new BlackboardEventBus();
            int received = 0;
            IDisposable subscription = eventBus.Subscribe<BlackboardMessage>(_ => received++);

            eventBus.Publish(new BlackboardMessage(BlackboardRuntimeInstanceId.New(), "BeforeDispose"));
            subscription.Dispose();
            eventBus.Publish(new BlackboardMessage(BlackboardRuntimeInstanceId.New(), "AfterDispose"));

            Assert.That(received, Is.EqualTo(1));
        }

        [Test]
        public void BlackboardRegistry_UsesRuntimeInstanceIdentity()
        {
            BlackboardRegistry registry = new BlackboardRegistry();
            TestBlackboardHandle handle = new TestBlackboardHandle(BlackboardRuntimeInstanceId.New());

            registry.Register(handle);
            bool found = registry.TryGet(handle.RuntimeInstanceId, out IBlackboardHandle resolved);
            registry.Unregister(handle.RuntimeInstanceId);

            Assert.That(found, Is.True);
            Assert.That(resolved, Is.SameAs(handle));
            Assert.That(registry.TryGet(handle.RuntimeInstanceId, out _), Is.False);
        }

        [Test]
        public void UntypedAssignment_RejectsIncompatibleValues()
        {
            IntegerVariableDefinition definition = new IntegerVariableDefinition
            {
                InitialValue = 1,
            };
            BlackboardDefinition blackboardDefinition = new BlackboardDefinition();
            blackboardDefinition.Variables.Add(definition);
            BlackboardDefinitionClone clone = new SerializedGraphCloner().Clone(blackboardDefinition);
            BlackboardVariableSet variables = new BlackboardVariableSet(clone.RuntimeInstanceId, clone.Definition.Variables, new PublicVariableRegistry(), new GlobalVariableStore());
            VariableCellBase cell = variables.Get<int>(CreateReference(definition));

            VariableTypeMismatchException exception = Assert.Throws<VariableTypeMismatchException>(() => cell.UntypedValue = "wrong");

            StringAssert.Contains(typeof(int).FullName, exception.Message);
            StringAssert.Contains(typeof(string).FullName, exception.Message);
        }

        [Test]
        public void ClonedCollectionDefinitions_DoNotShareManagedItems()
        {
            VariableCollection collection = new VariableCollection();
            collection.Items.Add("source");
            CollectionVariableDefinition variable = new CollectionVariableDefinition
            {
                InitialValue = collection,
            };
            BlackboardDefinition source = new BlackboardDefinition();
            source.Variables.Add(variable);
            SerializedGraphCloner cloner = new SerializedGraphCloner();

            BlackboardDefinition first = cloner.Clone(source).Definition;
            BlackboardDefinition second = cloner.Clone(source).Definition;
            VariableCollection firstValue = ((CollectionVariableDefinition)first.Variables[0]).InitialValue;
            VariableCollection secondValue = ((CollectionVariableDefinition)second.Variables[0]).InitialValue;
            firstValue.Items.Add("first");

            Assert.That(firstValue, Is.Not.SameAs(secondValue));
            Assert.That(secondValue.Items, Is.EqualTo(new[] { "source" }));
            Assert.That(collection.Items, Is.EqualTo(new[] { "source" }));
        }

        [Test]
        public void CollectionCell_DoesNotAliasDefinitionAndResetRestoresPristineValue()
        {
            VariableCollection collection = new VariableCollection();
            collection.Items.Add("source");
            CollectionVariableDefinition variable = new CollectionVariableDefinition
            {
                InitialValue = collection,
            };
            BlackboardDefinition source = new BlackboardDefinition();
            source.Variables.Add(variable);
            BlackboardDefinitionClone clone = new SerializedGraphCloner().Clone(source);
            BlackboardVariableSet variables = new BlackboardVariableSet(clone.RuntimeInstanceId, clone.Definition.Variables, new PublicVariableRegistry(), new GlobalVariableStore());
            VariableCell<VariableCollection> cell = variables.Get<VariableCollection>(CreateReference(variable));

            cell.Value.Items.Add("runtime");
            cell.Reset();

            VariableCollection definitionValue = ((CollectionVariableDefinition)clone.Definition.Variables[0]).InitialValue;
            Assert.That(definitionValue.Items, Is.EqualTo(new[] { "source" }));
            Assert.That(cell.Value.Items, Is.EqualTo(new[] { "source" }));
            Assert.That(cell.Value, Is.Not.SameAs(definitionValue));
        }

        private static void CreateVariableSets(out BlackboardVariableSet first, out BlackboardVariableSet second, out IntegerVariableDefinition local, out IntegerVariableDefinition publicVariable, out IntegerVariableDefinition global)
        {
            BlackboardDefinition definition = CreateDefinition(out local, out publicVariable, out global);
            SerializedGraphCloner cloner = new SerializedGraphCloner();
            BlackboardDefinitionClone firstClone = cloner.Clone(definition);
            BlackboardDefinitionClone secondClone = cloner.Clone(definition);
            PublicVariableRegistry registry = new PublicVariableRegistry();
            GlobalVariableStore globalStore = new GlobalVariableStore();
            first = new BlackboardVariableSet(firstClone.RuntimeInstanceId, firstClone.Definition.Variables, registry, globalStore);
            second = new BlackboardVariableSet(secondClone.RuntimeInstanceId, secondClone.Definition.Variables, registry, globalStore);
        }

        private static BlackboardDefinition CreateDefinition(out IntegerVariableDefinition local, out IntegerVariableDefinition publicVariable, out IntegerVariableDefinition global)
        {
            local = CreateInteger("Local", VariableScope.Local, 1);
            publicVariable = CreateInteger("Public", VariableScope.Public, 2);
            global = CreateInteger("Global", VariableScope.InjectedGlobal, 3);
            BlackboardDefinition definition = new BlackboardDefinition();
            definition.Variables.Add(local);
            definition.Variables.Add(publicVariable);
            definition.Variables.Add(global);
            return definition;
        }

        private static IntegerVariableDefinition CreateInteger(string key, VariableScope scope, int value)
        {
            return new IntegerVariableDefinition
            {
                Key = key,
                Scope = scope,
                InitialValue = value,
            };
        }

        private static VariableReference CreateReference(VariableDefinitionBase definition)
        {
            return new VariableReference
            {
                Scope = definition.Scope,
                DefinitionId = definition.DefinitionId,
            };
        }

        private sealed class TestValueSerializer : IVariableValueSerializer
        {
            public string Serialize(Type valueType, object value)
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            public object Deserialize(Type valueType, string serializedValue)
            {
                return valueType == typeof(int) ? int.Parse(serializedValue, CultureInfo.InvariantCulture) : serializedValue;
            }
        }

        private sealed class TestLogger : IBlackboardLogger
        {
            public List<string> Errors { get; } = new List<string>();

            public void Info(string message)
            {
            }

            public void Warning(string message)
            {
            }

            public void Error(string message, Exception exception = null)
            {
                Errors.Add(message);
            }
        }

        private sealed class TestBlackboardHandle : IBlackboardHandle
        {
            public TestBlackboardHandle(BlackboardRuntimeInstanceId runtimeInstanceId)
            {
                RuntimeInstanceId = runtimeInstanceId;
            }

            public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }
        }
    }
}
