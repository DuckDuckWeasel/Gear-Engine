using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Scaffold;
using Scaffold.VisualScripting;
using CoreActionExecutionStatus =
    Scaffold.VisualScripting.ActionExecutionStatus;
using CoreActionList = Scaffold.VisualScripting.ActionList;
using CoreBlackboard = Scaffold.VisualScripting.Blackboard;

namespace GearEngine.GearEngine.Tests.Editor
{
    public sealed class ActionContextBridgeTests
    {
        [Test]
        public void ReturnStatus_ExecutesThroughCoreContext()
        {
            ReturnActionStatus action = new ReturnActionStatus
            {
                Success = false,
            };

            CoreActionExecutionStatus status = Execute(action);

            Assert.That(status, Is.EqualTo(CoreActionExecutionStatus.Failure));
        }

        [Test]
        public void Wait_UsesInjectedSchedulerWithoutGameObject()
        {
            TestScheduler scheduler = new TestScheduler();
            CoreActionList list = CreateList(new Wait(), scheduler);
            CoreActionExecutionStatus? result = null;

            list.Execute(status => result = status);
            Assert.That(result, Is.Null);
            Assert.That(scheduler.ActiveCount, Is.EqualTo(1));

            scheduler.RunAll();
            Assert.That(result, Is.EqualTo(CoreActionExecutionStatus.Success));
        }

        [Test]
        public void InterruptingWait_DisposesScheduledWork()
        {
            TestScheduler scheduler = new TestScheduler();
            CoreActionList list = CreateList(new Wait(), scheduler);

            list.Execute(_ => { });
            list.Stop();

            Assert.That(scheduler.ActiveCount, Is.Zero);
        }

        [Test]
        public void WaitFrames_UsesInjectedRoutineSchedulerWithoutGameObject()
        {
            TestScheduler scheduler = new TestScheduler();
            CoreActionList list = CreateList(new WaitFrames(), scheduler);
            CoreActionExecutionStatus? result = null;

            list.Execute(status => result = status);
            Assert.That(result, Is.Null);
            Assert.That(scheduler.ActiveCount, Is.EqualTo(1));

            scheduler.RunAll();
            Assert.That(result, Is.EqualTo(CoreActionExecutionStatus.Success));
        }

        [Test]
        public void InterruptingWaitFrames_DisposesScheduledRoutine()
        {
            TestScheduler scheduler = new TestScheduler();
            CoreActionList list = CreateList(new WaitFrames(), scheduler);

            list.Execute(_ => { });
            list.Stop();

            Assert.That(scheduler.ActiveCount, Is.Zero);
        }

        [Test]
        public void RuntimeClone_PreservesGearActionDefinitionId()
        {
            ReturnActionStatus source = new ReturnActionStatus();
            SerializedGraphCloner cloner = new SerializedGraphCloner();

            ReturnActionStatus clone = cloner.CloneGraph(source);

            Assert.That(clone, Is.Not.SameAs(source));
            Assert.That(clone.DefinitionId, Is.EqualTo(source.DefinitionId));
        }

        [Test]
        public void LegacyActionExecution_RemainsAvailableDuringCutover()
        {
            ReturnActionStatus action = new ReturnActionStatus();
            bool completed = false;

            action.Execute(() => completed = true);

            Assert.That(completed, Is.True);
        }

        private static CoreActionExecutionStatus Execute(
            Scaffold.VisualScripting.IAction action)
        {
            CoreActionExecutionStatus? result = null;
            CoreActionList list = CreateList(action, new TestScheduler());
            list.Execute(status => result = status);
            Assert.That(result.HasValue, Is.True);
            return result.Value;
        }

        private static CoreActionList CreateList(
            Scaffold.VisualScripting.IAction action,
            IFrameScheduler scheduler)
        {
            ActionListDefinition definition = new ActionListDefinition();
            definition.Actions.Add(action);
            return new CoreActionList(
                CreateBlackboard(scheduler),
                null,
                null,
                definition,
                () => 0f);
        }

        private static CoreBlackboard CreateBlackboard(
            IFrameScheduler scheduler)
        {
            BlackboardRuntimeInstanceId runtimeId =
                BlackboardRuntimeInstanceId.New();
            BlackboardVariableSet variables = new BlackboardVariableSet(
                runtimeId,
                Array.Empty<VariableDefinitionBase>(),
                new PublicVariableRegistry(),
                new GlobalVariableStore());
            return new CoreBlackboard(
                runtimeId,
                variables,
                scheduler,
                new TestTimeSource(),
                new BlackboardEventBus(),
                new TestSaveService(),
                new TestLogger());
        }

        private sealed class TestScheduler : IFrameScheduler
        {
            public int ActiveCount
            {
                get
                {
                    int count = 0;
                    foreach (ScheduledHandle handle in handles)
                    {
                        count += handle.IsActive ? 1 : 0;
                    }

                    return count;
                }
            }

            private readonly List<ScheduledHandle> handles =
                new List<ScheduledHandle>();

            public IDisposable ScheduleNextFrame(Action callback)
            {
                return Add(callback);
            }

            public IDisposable Schedule(TimeSpan delay, Action callback)
            {
                return Add(callback);
            }

            public IDisposable ScheduleRoutine(IEnumerator routine)
            {
                return Add(() => CompleteRoutine(routine));
            }

            private void CompleteRoutine(IEnumerator routine)
            {
                while (routine.MoveNext())
                {
                }
            }

            public void Tick(float deltaTime)
            {
            }

            public void RunAll()
            {
                foreach (ScheduledHandle handle in handles.ToArray())
                {
                    handle.Run();
                }
            }

            private ScheduledHandle Add(Action callback)
            {
                ScheduledHandle handle = new ScheduledHandle(callback);
                handles.Add(handle);
                return handle;
            }
        }

        private sealed class ScheduledHandle : IDisposable
        {
            public ScheduledHandle(Action callback)
            {
                this.callback = callback ??
                    throw new ArgumentNullException(nameof(callback));
            }

            public bool IsActive => callback != null;

            private Action callback;

            public void Run()
            {
                Action action = callback;
                if (action == null)
                {
                    return;
                }

                callback = null;
                action.Invoke();
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
            public System.Threading.Tasks.Task SaveAsync(
                string slot,
                BlackboardSaveData data,
                CancellationToken cancellationToken)
            {
                return System.Threading.Tasks.Task.CompletedTask;
            }

            public System.Threading.Tasks.Task<BlackboardSaveData> LoadAsync(
                string slot,
                BlackboardRuntimeInstanceId runtimeInstanceId,
                CancellationToken cancellationToken)
            {
                return System.Threading.Tasks.Task.FromResult<BlackboardSaveData>(
                    null);
            }

            public System.Threading.Tasks.Task DeleteAsync(
                string slot,
                BlackboardRuntimeInstanceId runtimeInstanceId,
                CancellationToken cancellationToken)
            {
                return System.Threading.Tasks.Task.CompletedTask;
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
    }
}
