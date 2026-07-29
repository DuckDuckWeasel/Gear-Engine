using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public sealed class Block : IDisposable
    {
        public Block(Blackboard blackboard, BlockDefinition definition, Func<float> getRandomValue)
        {
            Blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.getRandomValue = getRandomValue ?? throw new ArgumentNullException(nameof(getRandomValue));
            CreateTracks();
            CreateTasks();
            runner = new CompositeExecutionRunner(tasks, getRandomValue, blackboard.Logger);
        }

        public Blackboard Blackboard { get; }

        public BlockDefinition Definition { get; }

        public string BlockName => Definition.Name;

        public IReadOnlyList<ActionTrack> Tracks => tracks;

        public ExecutionId ExecutionId { get; private set; }

        public BlockExecutionState State { get; private set; }

        public ActionExecutionStatus LastExecutionStatus => runner.LastExecutionStatus;

        public int ExecutionCount { get; private set; }

        private readonly Func<float> getRandomValue;
        private readonly List<ActionTrack> tracks = new List<ActionTrack>();
        private readonly List<BlockActionEntry> entries = new List<BlockActionEntry>();
        private readonly List<ICompositeTask> tasks = new List<ICompositeTask>();
        private readonly CompositeExecutionRunner runner;
        private Action<ActionExecutionStatus> completion;
        private int lastExecutedTaskIndex = -1;
        private int startTaskIndex;

        public void Execute(Action<ActionExecutionStatus> onComplete)
        {
            Execute(0, onComplete);
        }

        public void Execute(int firstTaskIndex, Action<ActionExecutionStatus> onComplete)
        {
            ThrowIfDisposed();
            if (State == BlockExecutionState.Executing)
            {
                throw new InvalidOperationException($"Block '{Definition.Name}' is already executing.");
            }

            startTaskIndex = Math.Max(firstTaskIndex, 0);
            BeginExecution(onComplete);
            ExecutionCount++;
            Blackboard.EventBus.Publish(
                new BlackboardBlockStartedEvent(
                    Blackboard.RuntimeInstanceId,
                    Definition.DefinitionId,
                    Definition.Name,
                    ExecutionId));
            StartRunner();
        }

        public void Tick()
        {
            ThrowIfDisposed();
            runner.Tick();
        }

        public void Stop()
        {
            if (State == BlockExecutionState.Disposed)
            {
                return;
            }

            RememberLastTask();
            runner.Stop();
            State = BlockExecutionState.Idle;
            completion = null;
        }

        public bool IsActionRunning(int taskIndex)
        {
            return runner.IsTaskRunning(taskIndex);
        }

        public bool IsExecuting()
        {
            return State == BlockExecutionState.Executing;
        }

        public bool TryGetActionStatus(int taskIndex, out ActionExecutionStatus status)
        {
            return runner.TryGetTaskStatus(taskIndex, out status);
        }

        public void ResetExecutionFeedback()
        {
            runner.ResetTaskStatuses();
        }

        public int InterruptActions(IReadOnlyList<int> taskIndexes, ActionExecutionStatus status)
        {
            ThrowIfDisposed();
            return runner.InterruptTasks(taskIndexes, status);
        }

        public void Dispose()
        {
            if (State == BlockExecutionState.Disposed)
            {
                return;
            }

            runner.Stop();
            DisposeTracks();
            State = BlockExecutionState.Disposed;
            completion = null;
        }

        internal void JumpTo(ActionTrack track, int actionIndex)
        {
            int taskIndex = FindTaskIndex(track, actionIndex);
            runner.RequestNextTaskIndex(taskIndex);
        }

        private void CreateTracks()
        {
            foreach (ActionTrackDefinition trackDefinition in Definition.Tracks)
            {
                tracks.Add(new ActionTrack(Blackboard, this, trackDefinition, getRandomValue));
            }
        }

        private void CreateTasks()
        {
            foreach (ActionTrack track in tracks)
            {
                AddTrackTasks(track);
            }
        }

        private void AddTrackTasks(ActionTrack track)
        {
            IReadOnlyList<IAction> actions = track.ActionList.Definition.Actions;
            for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                AddActionTask(track, actionIndex, actions[actionIndex]);
            }
        }

        private void AddActionTask(ActionTrack track, int actionIndex, IAction action)
        {
            BlockActionEntry entry = new BlockActionEntry(track, actionIndex, action);
            BlockFlowController flowController = new BlockFlowController(this, track);
            entries.Add(entry);
            tasks.Add(new ActionCompositeTask(action, () => CreateContext(entry, flowController)));
        }

        private ActionExecutionContext CreateContext(BlockActionEntry entry, IActionFlowController flowController)
        {
            return entry.Track.ActionList.CreateContext(entry.ActionIndex, flowController);
        }

        private void BeginExecution(Action<ActionExecutionStatus> onComplete)
        {
            completion = onComplete ?? throw new ArgumentNullException(nameof(onComplete));
            ExecutionId = ExecutionId.New();
            foreach (ActionTrack track in tracks)
            {
                track.BeginExecution(ExecutionId);
            }

            State = BlockExecutionState.Executing;
        }

        private void StartRunner()
        {
            RememberLastTask();
            if (startTaskIndex > 0)
            {
                StartAtConfiguredTask();
                return;
            }

            if (ShouldAvoidRepeatingLastAction())
            {
                runner.StartWithoutRepeatingLast(Definition.ExecutionMethod, Definition.AwaitMode, Definition.OrderMode, lastExecutedTaskIndex, Complete);
                return;
            }

            runner.Start(Definition.ExecutionMethod, Definition.AwaitMode, Definition.OrderMode, Complete);
        }

        private void StartAtConfiguredTask()
        {
            runner.StartAt(Definition.ExecutionMethod, Definition.AwaitMode, Definition.OrderMode, startTaskIndex, Complete);
        }

        private bool ShouldAvoidRepeatingLastAction()
        {
            return Definition.AvoidRepeatingLastAction && tasks.Count > 1 && CompositeExecutionDescription.SupportsOrder(Definition.ExecutionMethod) && Definition.OrderMode != ActionListOrderMode.Ordered;
        }

        private void Complete(ActionExecutionStatus status)
        {
            RememberLastTask();
            State = BlockExecutionState.Idle;
            Action<ActionExecutionStatus> callback = completion;
            completion = null;
            Blackboard.EventBus.Publish(
                new BlackboardBlockCompletedEvent(
                    Blackboard.RuntimeInstanceId,
                    Definition.DefinitionId,
                    Definition.Name,
                    ExecutionId,
                    status));
            callback.Invoke(status);
        }

        private void RememberLastTask()
        {
            if (runner.LastStartedTaskIndex >= 0)
            {
                lastExecutedTaskIndex = runner.LastStartedTaskIndex;
            }
        }

        private int FindTaskIndex(ActionTrack track, int actionIndex)
        {
            for (int taskIndex = 0; taskIndex < entries.Count; taskIndex++)
            {
                BlockActionEntry entry = entries[taskIndex];
                if (entry.Track == track && entry.ActionIndex >= actionIndex)
                {
                    return taskIndex;
                }
            }

            return tasks.Count;
        }

        private void DisposeTracks()
        {
            foreach (ActionTrack track in tracks)
            {
                track.Dispose();
            }
        }

        private void ThrowIfDisposed()
        {
            if (State == BlockExecutionState.Disposed)
            {
                throw new ObjectDisposedException(nameof(Block));
            }
        }
    }
}
