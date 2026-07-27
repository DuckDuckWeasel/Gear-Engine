using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Scaffold.VisualScripting.Tests
{
    public sealed class ActionRuntimeTests
    {
        [Test]
        public void Sequence_ExecutesInOrderAndCompletesSuccessfully()
        {
            List<string> log = new List<string>();
            TestAction first = CreateAction("First", log, ActionExecutionStatus.Success);
            TestAction second = CreateAction("Second", log, ActionExecutionStatus.Success);
            ActionExecutionStatus status = ExecuteList(CreateList(first, second));

            Assert.That(status, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(log, Is.EqualTo(new[] { "First", "Second" }));
        }

        [Test]
        public void Sequence_StopsAtFirstFailure()
        {
            List<string> log = new List<string>();
            TestAction first = CreateAction("First", log, ActionExecutionStatus.Failure);
            TestAction second = CreateAction("Second", log, ActionExecutionStatus.Success);
            ActionExecutionStatus status = ExecuteList(CreateList(first, second));

            Assert.That(status, Is.EqualTo(ActionExecutionStatus.Failure));
            Assert.That(log, Is.EqualTo(new[] { "First" }));
        }

        [Test]
        public void Selector_StopsAtFirstSuccess()
        {
            List<string> log = new List<string>();
            ActionListDefinition definition = CreateList(
                CreateAction("Failure", log, ActionExecutionStatus.Failure),
                CreateAction("Success", log, ActionExecutionStatus.Success),
                CreateAction("Skipped", log, ActionExecutionStatus.Success));
            definition.ExecutionMethod = ActionListExecutionMethod.Selector;

            ActionExecutionStatus status = ExecuteList(definition);

            Assert.That(status, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(log, Is.EqualTo(new[] { "Failure", "Success" }));
        }

        [Test]
        public void Parallel_WaitAllCompletesAfterEveryAction()
        {
            TestAction first = CreateDeferredAction("First");
            TestAction second = CreateDeferredAction("Second");
            ActionList list = CreateRuntimeList(CreateParallelList(ActionListAwaitMode.WaitAll, first, second));
            ActionExecutionStatus? result = null;

            list.Execute(status => result = status);
            first.CompleteNow(ActionExecutionStatus.Success);

            Assert.That(result, Is.Null);
            second.CompleteNow(ActionExecutionStatus.Success);
            Assert.That(result, Is.EqualTo(ActionExecutionStatus.Success));
        }

        [Test]
        public void ParallelSelector_SucceedsWhenAnyActionSucceeds()
        {
            ActionListDefinition definition = CreateParallelList(
                ActionListAwaitMode.WaitAll,
                CreateImmediateAction(ActionExecutionStatus.Failure),
                CreateImmediateAction(ActionExecutionStatus.Success));
            definition.ExecutionMethod = ActionListExecutionMethod.ParallelSelector;

            Assert.That(ExecuteList(definition), Is.EqualTo(ActionExecutionStatus.Success));
        }

        [Test]
        public void WaitAny_CompletesListAndStopInterruptsDetachedAction()
        {
            TestAction first = CreateDeferredAction("First");
            TestAction second = CreateDeferredAction("Second");
            ActionList list = CreateRuntimeList(CreateParallelList(ActionListAwaitMode.WaitAny, first, second));
            ActionExecutionStatus? result = null;

            list.Execute(status => result = status);
            first.CompleteNow(ActionExecutionStatus.Success);
            Assert.That(result, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(second.IsActive, Is.True);

            list.Stop();
            Assert.That(second.InterruptionCount, Is.EqualTo(1));
        }

        [Test]
        public void WaitNone_CompletesImmediatelyAndStopInterruptsDetachedActions()
        {
            TestAction first = CreateDeferredAction("First");
            TestAction second = CreateDeferredAction("Second");
            ActionList list = CreateRuntimeList(CreateParallelList(ActionListAwaitMode.WaitNone, first, second));
            ActionExecutionStatus? result = null;

            list.Execute(status => result = status);

            Assert.That(result, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(first.IsActive, Is.True);
            Assert.That(second.IsActive, Is.True);
            list.Stop();
            Assert.That(first.InterruptionCount, Is.EqualTo(1));
            Assert.That(second.InterruptionCount, Is.EqualTo(1));
        }

        [Test]
        public void WeightedRandom_PrioritizesSelectedWeight()
        {
            List<string> log = new List<string>();
            TestAction first = CreateAction("First", log, ActionExecutionStatus.Success);
            TestAction second = CreateAction("Second", log, ActionExecutionStatus.Success);
            first.HasWeightOverride = true;
            first.Weight = 10f;
            second.HasWeightOverride = true;
            second.Weight = 90f;
            ActionListDefinition definition = CreateList(first, second);
            definition.OrderMode = ActionListOrderMode.Random;

            ExecuteList(definition, () => 0.95f);

            Assert.That(log, Is.EqualTo(new[] { "Second", "First" }));
        }

        [Test]
        public void Shuffle_UsesInjectedRandomSource()
        {
            List<string> log = new List<string>();
            ActionListDefinition definition = CreateList(
                CreateAction("First", log, ActionExecutionStatus.Success),
                CreateAction("Second", log, ActionExecutionStatus.Success),
                CreateAction("Third", log, ActionExecutionStatus.Success));
            definition.OrderMode = ActionListOrderMode.Shuffle;

            ExecuteList(definition, () => 0f);

            Assert.That(log, Is.EqualTo(new[] { "Second", "Third", "First" }));
        }

        [Test]
        public void AvoidRepeat_DoesNotStartThePreviousRandomAction()
        {
            List<string> log = new List<string>();
            ActionListDefinition definition = CreateList(
                CreateAction("First", log, ActionExecutionStatus.Success),
                CreateAction("Second", log, ActionExecutionStatus.Success));
            definition.ExecutionMethod = ActionListExecutionMethod.Selector;
            definition.OrderMode = ActionListOrderMode.Random;
            definition.AvoidRepeatingLastAction = true;
            ActionList list = CreateRuntimeList(definition, () => 0f);

            list.Execute(_ => { });
            list.Execute(_ => { });

            Assert.That(log, Is.EqualTo(new[] { "First", "Second" }));
        }

        [Test]
        public void UtilitySelector_ReevaluatesAndInterruptsForHigherUtility()
        {
            TestAction first = CreateDeferredAction("First");
            first.Utility = 10f;
            TestAction second = CreateDeferredAction("Second");
            second.Utility = 5f;
            ActionListDefinition definition = CreateList(first, second);
            definition.ExecutionMethod = ActionListExecutionMethod.UtilitySelector;
            ActionList list = CreateRuntimeList(definition);

            list.Execute(_ => { });
            second.Utility = 20f;
            list.Tick();

            Assert.That(first.InterruptionCount, Is.EqualTo(1));
            Assert.That(second.IsActive, Is.True);
        }

        [Test]
        public void UtilitySelector_BlockDuringExecutionPreventsReevaluation()
        {
            TestAction first = CreateDeferredAction("First");
            first.Utility = 10f;
            first.BlockDuringExecution = true;
            TestAction second = CreateDeferredAction("Second");
            second.Utility = 5f;
            ActionListDefinition definition = CreateList(first, second);
            definition.ExecutionMethod = ActionListExecutionMethod.UtilitySelector;
            ActionList list = CreateRuntimeList(definition);

            list.Execute(_ => { });
            second.Utility = 20f;
            list.Tick();

            Assert.That(first.InterruptionCount, Is.Zero);
            Assert.That(second.IsActive, Is.False);
        }

        [Test]
        public void FlowJump_SkipsToRequestedAction()
        {
            List<string> log = new List<string>();
            TestAction first = CreateAction("First", log, ActionExecutionStatus.Success);
            first.JumpTarget = 2;
            ActionListDefinition definition = CreateList(
                first,
                CreateAction("Skipped", log, ActionExecutionStatus.Success),
                CreateAction("Third", log, ActionExecutionStatus.Success));

            ActionExecutionStatus status = ExecuteList(definition);

            Assert.That(status, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(log, Is.EqualTo(new[] { "First", "Third" }));
        }

        [Test]
        public void Block_FlattensMultipleTracksUsingSharedCompositeSemantics()
        {
            List<string> log = new List<string>();
            BlockDefinition definition = new BlockDefinition();
            definition.ExecutionMethod = ActionListExecutionMethod.Sequence;
            definition.Tracks.Add(CreateTrack(CreateAction("TrackOne", log, ActionExecutionStatus.Success)));
            definition.Tracks.Add(CreateTrack(CreateAction("TrackTwo", log, ActionExecutionStatus.Success)));
            Block block = new Block(CreateBlackboard(), definition, () => 0f);
            ActionExecutionStatus? result = null;

            block.Execute(status => result = status);

            Assert.That(result, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(log, Is.EqualTo(new[] { "TrackOne", "TrackTwo" }));
            Assert.That(block.Tracks[0].ExecutionId.IsEmpty, Is.False);
            Assert.That(block.Tracks[1].ExecutionId.IsEmpty, Is.False);
        }

        [Test]
        public void Context_ExposesRuntimeServicesAndStableExecutionIdentifiers()
        {
            TestAction action = CreateImmediateAction(ActionExecutionStatus.Success);
            Blackboard blackboard = CreateBlackboard();
            ActionList list = CreateRuntimeList(CreateList(action), () => 0f, blackboard);

            list.Execute(_ => { });

            Assert.That(action.LastContext.Blackboard, Is.SameAs(blackboard));
            Assert.That(action.LastContext.Scheduler, Is.SameAs(blackboard.Scheduler));
            Assert.That(action.LastContext.RuntimeInstanceId, Is.EqualTo(blackboard.RuntimeInstanceId));
            Assert.That(action.LastContext.BlockExecutionId.IsEmpty, Is.False);
            Assert.That(action.LastContext.TrackExecutionId.IsEmpty, Is.False);
            Assert.That(action.LastContext.ActionListExecutionId.IsEmpty, Is.False);
            Assert.That(action.LastContext.ActionExecutionId.IsEmpty, Is.False);
        }

        [Test]
        public void InterruptActions_RecordsFeedbackAndResetClearsIt()
        {
            TestAction action = CreateDeferredAction("Deferred");
            ActionList list = CreateRuntimeList(CreateList(action));
            list.Execute(_ => { });

            int interrupted = list.InterruptActions(
                new[] { 0 },
                ActionExecutionStatus.Interrupted);

            Assert.That(interrupted, Is.EqualTo(1));
            Assert.That(action.InterruptionCount, Is.EqualTo(1));
            Assert.That(
                list.TryGetActionStatus(0, out ActionExecutionStatus status),
                Is.True);
            Assert.That(status, Is.EqualTo(ActionExecutionStatus.Interrupted));
            list.ResetExecutionFeedback();
            Assert.That(list.TryGetActionStatus(0, out _), Is.False);
        }

        private static TestAction CreateAction(
            string name,
            ICollection<string> log,
            ActionExecutionStatus status)
        {
            return new TestAction(name, log, status);
        }

        private static TestAction CreateImmediateAction(ActionExecutionStatus status)
        {
            return new TestAction("Action", null, status);
        }

        private static TestAction CreateDeferredAction(string name)
        {
            return new TestAction(name, null, null);
        }

        private static ActionListDefinition CreateList(params IAction[] actions)
        {
            ActionListDefinition definition = new ActionListDefinition();
            definition.Actions.AddRange(actions);
            return definition;
        }

        private static ActionListDefinition CreateParallelList(
            ActionListAwaitMode awaitMode,
            params IAction[] actions)
        {
            ActionListDefinition definition = CreateList(actions);
            definition.ExecutionMethod = ActionListExecutionMethod.Parallel;
            definition.AwaitMode = awaitMode;
            return definition;
        }

        private static ActionTrackDefinition CreateTrack(params IAction[] actions)
        {
            return new ActionTrackDefinition
            {
                ActionList = CreateList(actions),
            };
        }

        private static ActionExecutionStatus ExecuteList(
            ActionListDefinition definition,
            Func<float> random = null)
        {
            ActionExecutionStatus? result = null;
            ActionList list = CreateRuntimeList(definition, random);
            list.Execute(status => result = status);
            Assert.That(result.HasValue, Is.True);
            return result.Value;
        }

        private static ActionList CreateRuntimeList(
            ActionListDefinition definition,
            Func<float> random = null,
            Blackboard blackboard = null)
        {
            return new ActionList(
                blackboard ?? CreateBlackboard(),
                null,
                null,
                definition,
                random ?? (() => 0f));
        }

        private static Blackboard CreateBlackboard()
        {
            BlackboardRuntimeInstanceId runtimeId = BlackboardRuntimeInstanceId.New();
            BlackboardVariableSet variables = new BlackboardVariableSet(
                runtimeId,
                Array.Empty<VariableDefinitionBase>(),
                new PublicVariableRegistry(),
                new GlobalVariableStore());
            return new Blackboard(
                runtimeId,
                variables,
                new TestScheduler(),
                new TestTimeSource(),
                new BlackboardEventBus(),
                new TestSaveService(),
                new TestLogger());
        }

        private sealed class TestAction : ActionBase
        {
            public TestAction(
                string name,
                ICollection<string> log,
                ActionExecutionStatus? result)
            {
                this.name = name;
                this.log = log;
                this.result = result;
            }

            public ActionExecutionContext LastContext { get; private set; }

            public int InterruptionCount { get; private set; }

            public int JumpTarget { get; set; } = -1;

            public bool IsActive => IsExecutionActive;

            private readonly string name;
            private readonly ICollection<string> log;
            private readonly ActionExecutionStatus? result;

            public void CompleteNow(ActionExecutionStatus status)
            {
                Complete(status);
            }

            protected override void OnExecute()
            {
                LastContext = Context;
                log?.Add(name);
                if (JumpTarget >= 0)
                {
                    JumpTo(JumpTarget);
                    return;
                }

                if (result.HasValue)
                {
                    Complete(result.Value);
                }
            }

            protected override void OnInterrupted()
            {
                InterruptionCount++;
            }
        }

        private sealed class TestScheduler : IFrameScheduler
        {
            public IDisposable ScheduleNextFrame(Action callback)
            {
                return new TestDisposable();
            }

            public IDisposable Schedule(TimeSpan delay, Action callback)
            {
                return new TestDisposable();
            }

            public IDisposable ScheduleRoutine(IEnumerator routine)
            {
                return new TestDisposable();
            }

            public void Tick(float deltaTime)
            {
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
            public Task SaveAsync(
                string slot,
                BlackboardSaveData data,
                CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public Task<BlackboardSaveData> LoadAsync(
                string slot,
                BlackboardRuntimeInstanceId runtimeInstanceId,
                CancellationToken cancellationToken)
            {
                return Task.FromResult<BlackboardSaveData>(null);
            }

            public Task DeleteAsync(
                string slot,
                BlackboardRuntimeInstanceId runtimeInstanceId,
                CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class TestLogger : IBlackboardLogger
        {
            public void Info(string message)
            {
            }

            public void Warning(string message)
            {
            }

            public void Error(string message, Exception exception = null)
            {
            }
        }

        private sealed class TestDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
