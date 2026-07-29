using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;

namespace Scaffold.VisualScripting.Unity.Tests
{
    public sealed class BlackboardBehaviourTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private IObjectResolver container;

        [TearDown]
        public void TearDown()
        {
            container?.Dispose();
            container = null;
            foreach (GameObject gameObject in objects)
            {
                if (gameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(gameObject);
                }
            }

            objects.Clear();
        }

        [UnityTest]
        public IEnumerator WrapperAndScriptFactory_UseEquivalentIsolatedRuntimes()
        {
            BlackboardFactory factory = CreateFactory();
            BlackboardDefinition definition = CreateDefinition(null, out IntegerVariableDefinition counter);
            Blackboard scriptRuntime = factory.Create(definition);
            scriptRuntime.Start();
            BlackboardBehaviour behaviour = CreateBehaviour(factory, definition);

            yield return null;

            Assert.That(behaviour.IsRuntimeAvailable, Is.True);
            Assert.That(behaviour.Runtime.HasStarted, Is.True);
            Assert.That(scriptRuntime.ExecuteBlock("Block"), Is.True);
            Assert.That(behaviour.ExecuteBlock("Block"), Is.True);
            Assert.That(GetValue(scriptRuntime, counter), Is.EqualTo(1));
            Assert.That(GetValue(behaviour.Runtime, counter), Is.EqualTo(1));
            Assert.That(scriptRuntime.RuntimeInstanceId, Is.Not.EqualTo(behaviour.Runtime.RuntimeInstanceId));
            Assert.That(scriptRuntime.Scheduler, Is.Not.SameAs(behaviour.Runtime.Scheduler));
            scriptRuntime.Dispose();
        }

        [UnityTest]
        public IEnumerator MissingFactory_DisablesWrapperWithActionableError()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("\\[BlackboardBehaviour\\] Failed to initialize"));
            GameObject host = Track(new GameObject("InvalidBlackboard"));
            host.SetActive(false);
            BlackboardBehaviour behaviour = host.AddComponent<BlackboardBehaviour>();

            host.SetActive(true);
            yield return null;

            Assert.That(behaviour.enabled, Is.False);
            Assert.That(behaviour.Runtime, Is.Null);
        }

        [UnityTest]
        public IEnumerator PointerRelay_ForwardsUnityCallbackAsLocalMessage()
        {
            BlackboardFactory factory = CreateFactory();
            BlackboardDefinition definition = CreateDefinition(new BlackboardMessageTriggerDefinition { MessageName = "PointerClick" }, out IntegerVariableDefinition counter);
            BlackboardBehaviour behaviour = CreateBehaviour(factory, definition);
            GameObject relayObject = Track(new GameObject("PointerRelay"));
            PointerCallbackRelay relay = relayObject.AddComponent<PointerCallbackRelay>();
            relay.Target = behaviour;

            yield return null;
            relay.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(null));

            Assert.That(GetValue(behaviour.Runtime, counter), Is.EqualTo(1));
        }

        [Test]
        public void ButtonSignalSource_DetachesListenerSymmetrically()
        {
            GameObject buttonObject = Track(new GameObject("Button", typeof(RectTransform), typeof(Button)));
            Button button = buttonObject.GetComponent<Button>();
            ButtonTriggerSignalSource source = new ButtonTriggerSignalSource { Target = button };
            int received = 0;
            IDisposable subscription = source.Subscribe(_ => received++);

            button.onClick.Invoke();
            subscription.Dispose();
            button.onClick.Invoke();

            Assert.That(received, Is.EqualTo(1));
        }

        [Test]
        public void UnityVariableSerializer_RoundTripsPrimitiveAndVector()
        {
            RecordingLogger logger = new RecordingLogger();
            UnityVariableValueSerializer serializer = new UnityVariableValueSerializer(logger);
            Vector3 expected = new Vector3(1f, 2f, 3f);

            string serializedInteger = serializer.Serialize(typeof(int), 42);
            string serializedVector = serializer.Serialize(typeof(Vector3), expected);

            Assert.That(serializer.Deserialize(typeof(int), serializedInteger), Is.EqualTo(42));
            Assert.That(serializer.Deserialize(typeof(Vector3), serializedVector), Is.EqualTo(expected));
            Assert.That(logger.Errors, Is.Empty);
        }

        private BlackboardFactory CreateFactory()
        {
            ContainerBuilder builder = new ContainerBuilder();
            new BlackboardRuntimeInstaller().Install(builder);
            container = builder.Build();
            return container.Resolve<BlackboardFactory>();
        }

        private BlackboardBehaviour CreateBehaviour(BlackboardFactory factory, BlackboardDefinition definition)
        {
            GameObject host = Track(new GameObject("BlackboardBehaviour"));
            host.SetActive(false);
            BlackboardBehaviour behaviour = host.AddComponent<BlackboardBehaviour>();
            behaviour.DefinitionReference.SetDirect(definition);
            behaviour.Construct(factory);
            host.SetActive(true);
            return behaviour;
        }

        private GameObject Track(GameObject gameObject)
        {
            objects.Add(gameObject);
            return gameObject;
        }

        private static BlackboardDefinition CreateDefinition(TriggerDefinition trigger, out IntegerVariableDefinition counter)
        {
            counter = new IntegerVariableDefinition { Key = "Counter", InitialValue = 0 };
            VariableReference reference = new VariableReference { DefinitionId = counter.DefinitionId, Scope = counter.Scope };
            ActionListDefinition actionList = new ActionListDefinition();
            actionList.Actions.Add(new IncrementAction(reference));
            ActionTrackDefinition track = new ActionTrackDefinition { Name = "Main", ActionList = actionList };
            BlockDefinition block = new BlockDefinition { Name = "Block", Trigger = trigger };
            block.Tracks.Add(track);
            BlackboardDefinition definition = new BlackboardDefinition { Name = "UnityTest" };
            definition.Blocks.Add(block);
            definition.Variables.Add(counter);
            return definition;
        }

        private static int GetValue(Blackboard runtime, IntegerVariableDefinition definition)
        {
            VariableReference reference = new VariableReference { DefinitionId = definition.DefinitionId, Scope = definition.Scope };
            return runtime.GetVariable<int>(reference).Value;
        }

        [Serializable]
        private sealed class IncrementAction : ActionBase
        {
            public IncrementAction(VariableReference target)
            {
                this.target = target;
            }

            [SerializeField] private VariableReference target;

            protected override void OnExecute()
            {
                VariableCell<int> cell = Context.Blackboard.GetVariable<int>(target);
                cell.Value++;
                Succeed();
            }
        }

        private sealed class RecordingLogger : IBlackboardLogger
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
    }
}
