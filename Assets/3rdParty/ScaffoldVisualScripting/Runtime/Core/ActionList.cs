using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public sealed class ActionList : IActionFlowController, IDisposable
    {
        public ActionList(Blackboard blackboard, Block block, ActionTrack track, ActionListDefinition definition, Func<float> getRandomValue)
        {
            Blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            Block = block;
            Track = track;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            CreateTasks();
            runner = new CompositeExecutionRunner(tasks, getRandomValue, blackboard.Logger);
        }

        public Blackboard Blackboard { get; }

        public Block Block { get; }

        public ActionTrack Track { get; }

        public ActionListDefinition Definition { get; }

        public ExecutionId ExecutionId { get; private set; }

        public ActionExecutionStatus LastExecutionStatus => runner.LastExecutionStatus;

        public bool IsExecuting => runner.IsExecuting;

        private readonly List<ICompositeTask> tasks = new List<ICompositeTask>();
        private readonly CompositeExecutionRunner runner;
        private ExecutionId blockExecutionId;
        private ExecutionId trackExecutionId;
        private int lastExecutedActionIndex = -1;
        private bool disposed;

        public void Execute(Action<ActionExecutionStatus> onComplete)
        {
            ThrowIfDisposed();
            ExecutionId parentBlockExecutionId = ExecutionId.New();
            ExecutionId parentTrackExecutionId = ExecutionId.New();
            BeginExecution(parentBlockExecutionId, parentTrackExecutionId);
            StartRunner(onComplete ?? throw new ArgumentNullException(nameof(onComplete)));
        }

        public void Tick()
        {
            ThrowIfDisposed();
            runner.Tick();
        }

        public void JumpTo(int actionIndex)
        {
            ThrowIfDisposed();
            runner.RequestNextTaskIndex(actionIndex);
        }

        public void StopBlock()
        {
            if (Block != null)
            {
                Block.Stop();
                return;
            }

            Stop();
        }

        public void Stop()
        {
            RememberLastExecutedAction();
            runner.Stop();
        }

        public int InterruptActions(IReadOnlyList<int> actionIndexes, ActionExecutionStatus status)
        {
            ThrowIfDisposed();
            return runner.InterruptTasks(actionIndexes, status);
        }

        public bool IsActionRunning(int actionIndex)
        {
            return runner.IsTaskRunning(actionIndex);
        }

        public bool TryGetActionStatus(int actionIndex, out ActionExecutionStatus status)
        {
            return runner.TryGetTaskStatus(actionIndex, out status);
        }

        public void ResetExecutionFeedback()
        {
            runner.ResetTaskStatuses();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            runner.Stop();
        }

        internal void BeginExecution(ExecutionId parentBlockExecutionId, ExecutionId parentTrackExecutionId)
        {
            blockExecutionId = parentBlockExecutionId;
            trackExecutionId = parentTrackExecutionId;
            ExecutionId = ExecutionId.New();
        }

        private void CreateTasks()
        {
            for (int actionIndex = 0; actionIndex < Definition.Actions.Count; actionIndex++)
            {
                int capturedIndex = actionIndex;
                IAction action = Definition.Actions[capturedIndex];
                tasks.Add(new ActionCompositeTask(action, () => CreateContext(capturedIndex, this)));
            }
        }

        internal ActionExecutionContext CreateContext(int actionIndex, IActionFlowController flowController)
        {
            ExecutionId actionExecutionId = ExecutionId.New();
            return new ActionExecutionContext(Blackboard, Block, Track, this, flowController, blockExecutionId, trackExecutionId, ExecutionId, actionExecutionId);
        }

        private void StartRunner(Action<ActionExecutionStatus> onComplete)
        {
            RememberLastExecutedAction();
            if (ShouldAvoidRepeatingLastAction())
            {
                runner.StartWithoutRepeatingLast(Definition.ExecutionMethod, Definition.AwaitMode, Definition.OrderMode, lastExecutedActionIndex, status => Complete(status, onComplete));
                return;
            }

            runner.Start(Definition.ExecutionMethod, Definition.AwaitMode, Definition.OrderMode, status => Complete(status, onComplete));
        }

        private bool ShouldAvoidRepeatingLastAction()
        {
            return Definition.AvoidRepeatingLastAction && tasks.Count > 1 && CompositeExecutionDescription.SupportsOrder(Definition.ExecutionMethod) && Definition.OrderMode != ActionListOrderMode.Ordered;
        }

        private void Complete(ActionExecutionStatus status, Action<ActionExecutionStatus> onComplete)
        {
            RememberLastExecutedAction();
            onComplete.Invoke(status);
        }

        private void RememberLastExecutedAction()
        {
            if (runner.LastStartedTaskIndex >= 0)
            {
                lastExecutedActionIndex = runner.LastStartedTaskIndex;
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ActionList));
            }
        }
    }
}
