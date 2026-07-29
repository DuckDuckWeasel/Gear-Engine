using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Scaffold.VisualScripting.Tests
{
    public sealed class BlackboardRuntimeLifecycleTests
    {
        private readonly List<Blackboard> runtimes = new List<Blackboard>();
        private TestFrameScheduler scheduler;
        private BlackboardEventBus eventBus;
        private BlackboardRegistry registry;
        private TestSaveService saveService;
        private TestLogger logger;
        private BlackboardFactory factory;

        [SetUp]
        public void SetUp()
        {
            scheduler = new TestFrameScheduler();
            eventBus = new BlackboardEventBus();
            registry = new BlackboardRegistry();
            saveService = new TestSaveService();
            logger = new TestLogger();
            BlackboardVariablePersistence persistence =
                new BlackboardVariablePersistence(
                    new TestValueSerializer(),
                    logger);
            BlackboardRuntimeServices services =
                new BlackboardRuntimeServices(
                    scheduler,
                    new TestTimeSource(),
                    eventBus,
                    saveService,
                    logger,
                    persistence,
                    new TextSubstitutionService(logger),
                    registry);
            factory = new BlackboardFactory(
                new SerializedGraphCloner(),
                new BlackboardDefinitionValidator(),
                new FixedBlackboardRuntimeServicesFactory(services),
                new PublicVariableRegistry(),
                new GlobalVariableStore(),
                new SystemRandomSource(17),
                logger);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Blackboard runtime in runtimes)
            {
                runtime.Dispose();
            }

            runtimes.Clear();
        }

        [Test]
        public void GameStarted_StartsOnScheduledFrameWithoutGameObject()
        {
            BlackboardDefinition definition = CreateIncrementingDefinition(
                new GameStartedTriggerDefinition
                {
                    WaitForFrames = 1,
                },
                out IntegerVariableDefinition counter);
            Blackboard runtime = CreateRuntime(definition);

            runtime.Start();

            Assert.That(GetValue<int>(runtime, counter), Is.Zero);
            runtime.Tick(0.016f);
            Assert.That(GetValue<int>(runtime, counter), Is.EqualTo(1));
        }

        [Test]
        public void Factory_CreatesIndependentRuntimeGraphs()
        {
            BlackboardDefinition definition = CreateIncrementingDefinition(
                null,
                out IntegerVariableDefinition counter);
            Blackboard first = CreateRuntime(definition);
            Blackboard second = CreateRuntime(definition);

            first.Start();
            second.Start();
            Assert.That(first.ExecuteBlock("Block"), Is.True);

            Assert.That(GetValue<int>(first, counter), Is.EqualTo(1));
            Assert.That(GetValue<int>(second, counter), Is.Zero);
            Assert.That(first.RuntimeInstanceId, Is.Not.EqualTo(second.RuntimeInstanceId));
            Assert.That(first.Blocks[0], Is.Not.SameAs(second.Blocks[0]));
            Assert.That(
                first.Definition.Blocks[0].Tracks[0].ActionList.Actions[0],
                Is.Not.SameAs(
                    second.Definition.Blocks[0].Tracks[0].ActionList.Actions[0]));
        }

        [Test]
        public void EnableDisable_AttachesTriggersAndCancelsScheduledWork()
        {
            BlackboardDefinition definition = CreateIncrementingDefinition(
                new BlackboardEnabledTriggerDefinition
                {
                    WaitForFrames = 1,
                },
                out IntegerVariableDefinition counter);
            Blackboard runtime = CreateRuntime(definition);

            runtime.Start();
            runtime.Disable();
            runtime.Tick(0.016f);
            Assert.That(GetValue<int>(runtime, counter), Is.Zero);

            runtime.Enable();
            runtime.Tick(0.016f);

            Assert.That(GetValue<int>(runtime, counter), Is.EqualTo(1));
        }

        [Test]
        public void LocalMessage_TargetsOnlyTheReceivingRuntime()
        {
            BlackboardDefinition definition = CreateIncrementingDefinition(
                new BlackboardMessageTriggerDefinition
                {
                    MessageName = "Advance",
                },
                out IntegerVariableDefinition counter);
            Blackboard first = CreateRuntime(definition);
            Blackboard second = CreateRuntime(definition);
            first.Start();
            second.Start();

            first.SendMessage("Advance");

            Assert.That(GetValue<int>(first, counter), Is.EqualTo(1));
            Assert.That(GetValue<int>(second, counter), Is.Zero);

            first.BroadcastMessage("Advance");

            Assert.That(GetValue<int>(first, counter), Is.EqualTo(2));
            Assert.That(GetValue<int>(second, counter), Is.EqualTo(1));
        }

        [Test]
        public void PollingTrigger_CanUsePlainVariableCondition()
        {
            BooleanVariableDefinition condition = new BooleanVariableDefinition
            {
                Key = "Condition",
                InitialValue = false,
            };
            VariableReference conditionReference = CreateReference(condition);
            PollingTriggerDefinition trigger = new PollingTriggerDefinition
            {
                Condition = new BooleanVariableCondition(conditionReference),
                FireMode = PollingTriggerFireMode.RisingEdge,
            };
            BlackboardDefinition definition = CreateIncrementingDefinition(
                trigger,
                out IntegerVariableDefinition counter);
            definition.Variables.Add(condition);
            Blackboard runtime = CreateRuntime(definition);
            runtime.Start();

            runtime.GetVariable<bool>(conditionReference).Value = true;
            runtime.Tick(0.016f);
            runtime.Tick(0.016f);
            runtime.GetVariable<bool>(conditionReference).Value = false;
            runtime.Tick(0.016f);
            runtime.GetVariable<bool>(conditionReference).Value = true;
            runtime.Tick(0.016f);

            Assert.That(GetValue<int>(runtime, counter), Is.EqualTo(2));
        }

        [Test]
        public void BindableTrigger_AttachesAndDetachesSymmetrically()
        {
            StringVariableDefinition value = new StringVariableDefinition
            {
                Key = "Value",
                InitialValue = string.Empty,
            };
            BindableTriggerDefinition trigger = new BindableTriggerDefinition
            {
                Source = new TestSignalSource(),
                ValueTarget = CreateReference(value),
            };
            BlackboardDefinition definition = CreateIncrementingDefinition(
                trigger,
                out IntegerVariableDefinition counter);
            definition.Variables.Add(value);
            Blackboard runtime = CreateRuntime(definition);
            runtime.Start();
            BindableTriggerDefinition runtimeTrigger =
                (BindableTriggerDefinition)runtime.Definition.Blocks[0].Trigger;
            TestSignalSource source =
                (TestSignalSource)runtimeTrigger.Source;

            Assert.That(source.SubscriberCount, Is.EqualTo(1));
            source.Emit("Received");
            Assert.That(GetValue<int>(runtime, counter), Is.EqualTo(1));
            Assert.That(GetValue<string>(runtime, value), Is.EqualTo("Received"));

            runtime.Disable();
            Assert.That(source.SubscriberCount, Is.Zero);
            runtime.Enable();
            Assert.That(source.SubscriberCount, Is.EqualTo(1));
            runtime.Dispose();
            Assert.That(source.SubscriberCount, Is.Zero);
        }

        [Test]
        public async Task SaveLoad_UsesRuntimeAndStableVariableIds()
        {
            BlackboardDefinition definition = CreateIncrementingDefinition(
                null,
                out IntegerVariableDefinition counter);
            Blackboard runtime = CreateRuntime(definition);
            runtime.Start();
            VariableReference reference = CreateReference(counter);
            runtime.GetVariable<int>(reference).Value = 41;

            await runtime.SaveAsync("Checkpoint");
            runtime.GetVariable<int>(reference).Value = 4;
            await runtime.LoadAsync("Checkpoint");

            Assert.That(runtime.GetVariable<int>(reference).Value, Is.EqualTo(41));
            Assert.That(saveService.LastSaved.RuntimeInstanceId, Is.EqualTo(runtime.RuntimeInstanceId));
            Assert.That(saveService.LastSaved.Variables[0].DefinitionId, Is.EqualTo(counter.DefinitionId));
        }

        [Test]
        public void Reset_StopsActionsAndRestoresVariables()
        {
            IntegerVariableDefinition counter = new IntegerVariableDefinition
            {
                Key = "Counter",
                InitialValue = 3,
            };
            PendingAction action = new PendingAction();
            BlackboardDefinition definition =
                CreateDefinition(null, action, counter);
            Blackboard runtime = CreateRuntime(definition);
            runtime.Start();
            VariableReference reference = CreateReference(counter);
            runtime.GetVariable<int>(reference).Value = 9;

            Assert.That(runtime.ExecuteBlock("Block"), Is.True);
            runtime.Reset();

            PendingAction runtimeAction =
                (PendingAction)runtime.Definition
                    .Blocks[0]
                    .Tracks[0]
                    .ActionList
                    .Actions[0];
            Assert.That(runtime.Blocks[0].State, Is.EqualTo(BlockExecutionState.Idle));
            Assert.That(runtimeAction.InterruptionCount, Is.EqualTo(1));
            Assert.That(runtime.GetVariable<int>(reference).Value, Is.EqualTo(3));
        }

        [Test]
        public void Registry_RemovesDisposedRuntime()
        {
            Blackboard runtime = CreateRuntime(
                CreateIncrementingDefinition(null, out _));
            BlackboardRuntimeInstanceId runtimeId = runtime.RuntimeInstanceId;

            Assert.That(registry.TryGet(runtimeId, out _), Is.True);
            runtime.Dispose();

            Assert.That(registry.TryGet(runtimeId, out _), Is.False);
        }

        [Test]
        public void Substitute_UsesRuntimeVariableValues()
        {
            BlackboardDefinition definition = CreateIncrementingDefinition(
                null,
                out IntegerVariableDefinition counter);
            Blackboard runtime = CreateRuntime(definition);
            runtime.Start();
            runtime.GetVariable<int>(CreateReference(counter)).Value = 12;

            Assert.That(
                runtime.Substitute("Count=${Counter}"),
                Is.EqualTo("Count=12"));
        }

        private Blackboard CreateRuntime(BlackboardDefinition definition)
        {
            Blackboard runtime = factory.Create(definition);
            runtimes.Add(runtime);
            return runtime;
        }

        private static BlackboardDefinition CreateIncrementingDefinition(
            TriggerDefinition trigger,
            out IntegerVariableDefinition counter)
        {
            counter = new IntegerVariableDefinition
            {
                Key = "Counter",
                InitialValue = 0,
            };
            return CreateDefinition(
                trigger,
                new IncrementIntegerAction(CreateReference(counter)),
                counter);
        }

        private static BlackboardDefinition CreateDefinition(
            TriggerDefinition trigger,
            IAction action,
            VariableDefinitionBase variable)
        {
            ActionListDefinition actionList = new ActionListDefinition();
            actionList.Actions.Add(action);
            ActionTrackDefinition track = new ActionTrackDefinition
            {
                Name = "Main",
                ActionList = actionList,
            };
            BlockDefinition block = new BlockDefinition
            {
                Name = "Block",
                Trigger = trigger,
            };
            block.Tracks.Add(track);
            BlackboardDefinition definition = new BlackboardDefinition
            {
                Name = "RuntimeTest",
            };
            definition.Blocks.Add(block);
            definition.Variables.Add(variable);
            return definition;
        }

        private static VariableReference CreateReference(
            VariableDefinitionBase definition)
        {
            return new VariableReference
            {
                DefinitionId = definition.DefinitionId,
                Scope = definition.Scope,
            };
        }

        private static T GetValue<T>(
            Blackboard runtime,
            VariableDefinitionBase definition)
        {
            return runtime.GetVariable<T>(CreateReference(definition)).Value;
        }

        [Serializable]
        private sealed class IncrementIntegerAction : ActionBase
        {
            public IncrementIntegerAction(VariableReference target)
            {
                this.target = target;
            }

            [SerializeField] private VariableReference target;

            protected override void OnExecute()
            {
                VariableCell<int> cell =
                    Context.Blackboard.GetVariable<int>(target);
                cell.Value++;
                Succeed();
            }
        }

        [Serializable]
        private sealed class PendingAction : ActionBase
        {
            public int InterruptionCount { get; private set; }

            protected override void OnExecute()
            {
            }

            protected override void OnInterrupted()
            {
                InterruptionCount++;
            }
        }

        [Serializable]
        private sealed class BooleanVariableCondition : ITriggerCondition
        {
            public BooleanVariableCondition(VariableReference reference)
            {
                this.reference = reference;
            }

            [SerializeField] private VariableReference reference;

            public bool Evaluate(TriggerExecutionContext context)
            {
                return context.Variables.Get<bool>(reference).Value;
            }
        }

        [Serializable]
        private sealed class TestSignalSource : ITriggerSignalSource
        {
            public int SubscriberCount => signal?.GetInvocationList().Length ?? 0;

            [NonSerialized] private Action<object> signal;

            public IDisposable Subscribe(Action<object> handler)
            {
                signal += handler;
                return new TestDisposable(() => signal -= handler);
            }

            public void Emit(object value)
            {
                signal?.Invoke(value);
            }
        }

        private sealed class TestFrameScheduler : IFrameScheduler
        {
            private readonly List<ScheduledCallback> nextFrame =
                new List<ScheduledCallback>();

            public IDisposable ScheduleNextFrame(Action callback)
            {
                ScheduledCallback scheduled = new ScheduledCallback(callback);
                nextFrame.Add(scheduled);
                return scheduled;
            }

            public IDisposable Schedule(TimeSpan delay, Action callback)
            {
                return ScheduleNextFrame(callback);
            }

            public IDisposable ScheduleRoutine(IEnumerator routine)
            {
                return new TestDisposable();
            }

            public void Tick(float deltaTime)
            {
                ScheduledCallback[] snapshot = nextFrame.ToArray();
                nextFrame.Clear();
                foreach (ScheduledCallback scheduled in snapshot)
                {
                    scheduled.Invoke();
                }
            }
        }

        private sealed class ScheduledCallback : IDisposable
        {
            public ScheduledCallback(Action callback)
            {
                this.callback =
                    callback ?? throw new ArgumentNullException(nameof(callback));
            }

            private Action callback;

            public void Invoke()
            {
                Action scheduled = callback;
                callback = null;
                scheduled?.Invoke();
            }

            public void Dispose()
            {
                callback = null;
            }
        }

        private sealed class TestTimeSource : ITimeSource
        {
            public float DeltaTime => 0f;

            public double ElapsedSeconds => 0d;

            public long Frame => 0L;
        }

        private sealed class TestSaveService : IBlackboardSaveService
        {
            public BlackboardSaveData LastSaved { get; private set; }

            public Task SaveAsync(
                string slot,
                BlackboardSaveData data,
                CancellationToken cancellationToken)
            {
                LastSaved = data;
                return Task.CompletedTask;
            }

            public Task<BlackboardSaveData> LoadAsync(
                string slot,
                BlackboardRuntimeInstanceId runtimeInstanceId,
                CancellationToken cancellationToken)
            {
                BlackboardSaveData result =
                    LastSaved?.RuntimeInstanceId == runtimeInstanceId
                        ? LastSaved
                        : null;
                return Task.FromResult(result);
            }

            public Task DeleteAsync(
                string slot,
                BlackboardRuntimeInstanceId runtimeInstanceId,
                CancellationToken cancellationToken)
            {
                LastSaved = null;
                return Task.CompletedTask;
            }
        }

        private sealed class TestValueSerializer : IVariableValueSerializer
        {
            public string Serialize(Type valueType, object value)
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            public object Deserialize(Type valueType, string serializedValue)
            {
                if (valueType == typeof(int))
                {
                    return int.Parse(
                        serializedValue,
                        CultureInfo.InvariantCulture);
                }

                if (valueType == typeof(bool))
                {
                    return bool.Parse(serializedValue);
                }

                return serializedValue;
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

        private sealed class TestDisposable : IDisposable
        {
            public TestDisposable(Action dispose = null)
            {
                this.dispose = dispose;
            }

            private Action dispose;

            public void Dispose()
            {
                Action callback = dispose;
                dispose = null;
                callback?.Invoke();
            }
        }
    }
}
