using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Scaffold;
using Scaffold.VisualScripting;
using SystemTask = System.Threading.Tasks.Task;

namespace Game.GearEngine.RuntimeTests
{
    public sealed class GearActionRuntimeTests
    {
        [Test]
        public void ReturnStatus_ExecutesWithoutGameObject()
        {
            ReturnActionStatus action = new ReturnActionStatus
            {
                Success = false,
            };
            TestScheduler scheduler = new TestScheduler();
            ActionList actionList = CreateActionList(action, scheduler);
            ActionExecutionStatus? result = null;

            actionList.Execute(status => result = status);

            Assert.That(result, Is.EqualTo(ActionExecutionStatus.Failure));
        }

        [Test]
        public void Wait_UsesInjectedSchedulerWithoutCoroutineHost()
        {
            TestScheduler scheduler = new TestScheduler();
            ActionList actionList = CreateActionList(new Wait(), scheduler);
            ActionExecutionStatus? result = null;

            actionList.Execute(status => result = status);
            Assert.That(result, Is.Null);

            scheduler.RunAll();

            Assert.That(result, Is.EqualTo(ActionExecutionStatus.Success));
        }

        private static ActionList CreateActionList(
            Scaffold.VisualScripting.IAction action,
            IFrameScheduler scheduler)
        {
            BlackboardRuntimeInstanceId runtimeId =
                BlackboardRuntimeInstanceId.New();
            BlackboardVariableSet variables = new BlackboardVariableSet(
                runtimeId,
                Array.Empty<VariableDefinitionBase>(),
                new PublicVariableRegistry(),
                new GlobalVariableStore());
            Scaffold.VisualScripting.Blackboard blackboard =
                new Scaffold.VisualScripting.Blackboard(
                runtimeId,
                variables,
                scheduler,
                new TestTimeSource(),
                new BlackboardEventBus(),
                new TestSaveService(),
                new TestLogger());
            ActionListDefinition definition = new ActionListDefinition();
            definition.Actions.Add(action);
            return new ActionList(
                blackboard,
                null,
                null,
                definition,
                () => 0f);
        }

        private sealed class TestScheduler : IFrameScheduler
        {
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
                return Add(
                    () =>
                    {
                        while (routine.MoveNext())
                        {
                        }
                    });
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

            private Action callback;

            public void Run()
            {
                Action action = callback;
                callback = null;
                action?.Invoke();
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
            public SystemTask SaveAsync(
                string slot,
                BlackboardSaveData data,
                CancellationToken cancellationToken)
            {
                return SystemTask.CompletedTask;
            }

            public System.Threading.Tasks.Task<BlackboardSaveData> LoadAsync(
                string slot,
                BlackboardRuntimeInstanceId runtimeInstanceId,
                CancellationToken cancellationToken)
            {
                return SystemTask.FromResult<BlackboardSaveData>(null);
            }

            public SystemTask DeleteAsync(
                string slot,
                BlackboardRuntimeInstanceId runtimeInstanceId,
                CancellationToken cancellationToken)
            {
                return SystemTask.CompletedTask;
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
