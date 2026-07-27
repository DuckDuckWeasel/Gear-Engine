using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    internal sealed class CompositeExecutionRunner
    {
        public CompositeExecutionRunner(IReadOnlyList<ICompositeTask> tasks, Func<float> getRandomValue, IBlackboardLogger logger)
        {
            this.tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            orderBuilder = new CompositeOrderBuilder(tasks, getRandomValue);
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            executeNextSequentialTask = ExecuteNextSequentialTask;
            executeTask = ExecuteTask;
            completeRemovedTask = CompleteRemovedTask;
        }

        public bool IsExecuting { get; private set; }

        public ActionExecutionStatus LastExecutionStatus { get; private set; } = ActionExecutionStatus.Success;

        public int LastStartedTaskIndex { get; private set; } = -1;

        private readonly IReadOnlyList<ICompositeTask> tasks;
        private readonly CompositeOrderBuilder orderBuilder;
        private readonly IBlackboardLogger logger;
        private readonly Action executeNextSequentialTask;
        private readonly Action<int> executeTask;
        private readonly Action<int, ActionExecutionStatus> completeRemovedTask;
        private readonly HashSet<int> runningIndexes = new HashSet<int>();
        private readonly HashSet<int> failedUtilityIndexes = new HashSet<int>();
        private readonly Dictionary<int, ActionExecutionStatus> statuses = new Dictionary<int, ActionExecutionStatus>();
        private List<int> order = new List<int>();
        private Action<ActionExecutionStatus> completion;
        private ActionListExecutionMethod executionMethod;
        private ActionListAwaitMode awaitMode;
        private ActionListOrderMode orderMode;
        private int orderIndex;
        private int requestedOrderIndex = -1;
        private int startedCount;
        private int completedCount;
        private int version;
        private int activeUtilityIndex = -1;
        private bool isStartingParallel;
        private bool completionRequested;
        private bool hasParallelSuccess;
        private bool hasParallelFailure;
        private ActionExecutionStatus requestedCompletionStatus;

        public void Start(ActionListExecutionMethod method, ActionListAwaitMode await, ActionListOrderMode ordering, Action<ActionExecutionStatus> onComplete)
        {
            StartInternal(method, await, ordering, -1, onComplete);
        }

        public void StartWithoutRepeatingLast(ActionListExecutionMethod method, ActionListAwaitMode await, ActionListOrderMode ordering, int lastTaskIndex, Action<ActionExecutionStatus> onComplete)
        {
            StartInternal(method, await, ordering, lastTaskIndex, onComplete);
        }

        public void StartAt(ActionListExecutionMethod method, ActionListAwaitMode await, ActionListOrderMode ordering, int taskIndex, Action<ActionExecutionStatus> onComplete)
        {
            Stop();
            Configure(method, await, ordering, onComplete);
            ResetForStart();
            order = orderBuilder.Build(method, ordering);
            IsExecuting = true;
            if (taskIndex > 0 &&
                method == ActionListExecutionMethod.Sequence &&
                ordering == ActionListOrderMode.Ordered)
            {
                orderIndex = Math.Min(taskIndex, order.Count);
            }

            BeginExecution();
        }

        public void Tick()
        {
            if (!CanReevaluateUtility())
            {
                return;
            }

            int nextIndex = FindHighestUtilityIndex();
            ReevaluateUtility(nextIndex);
        }

        public bool IsTaskRunning(int taskIndex)
        {
            return runningIndexes.Contains(taskIndex);
        }

        public bool TryGetTaskStatus(int taskIndex, out ActionExecutionStatus status)
        {
            return statuses.TryGetValue(taskIndex, out status);
        }

        public void RequestNextTaskIndex(int taskIndex)
        {
            if (!IsExecuting || orderMode != ActionListOrderMode.Ordered)
            {
                return;
            }

            requestedOrderIndex = taskIndex == tasks.Count ? order.Count : order.IndexOf(taskIndex);
        }

        public int InterruptTasks(IReadOnlyList<int> taskIndexes, ActionExecutionStatus interruptionStatus)
        {
            List<int> indexes = FindRunningIndexes(taskIndexes);
            foreach (int taskIndex in indexes)
            {
                CompleteInterruptedTask(taskIndex, interruptionStatus);
            }

            return indexes.Count;
        }

        private void StartInternal(ActionListExecutionMethod method, ActionListAwaitMode await, ActionListOrderMode ordering, int lastTaskIndex, Action<ActionExecutionStatus> onComplete)
        {
            Stop();
            Configure(method, await, ordering, onComplete);
            ResetForStart();
            order = orderBuilder.Build(method, ordering);
            AvoidImmediateRepeat(lastTaskIndex);
            IsExecuting = true;
            BeginExecution();
        }

        public void Stop()
        {
            IsExecuting = false;
            version++;
            InterruptRunningTasks();
            runningIndexes.Clear();
            activeUtilityIndex = -1;
            completion = null;
            ResetTaskStatuses();
        }

        public void ResetTaskStatuses()
        {
            statuses.Clear();
        }

        private void Configure(ActionListExecutionMethod method, ActionListAwaitMode await, ActionListOrderMode ordering, Action<ActionExecutionStatus> onComplete)
        {
            executionMethod = method;
            awaitMode = await;
            orderMode = ordering;
            completion = onComplete ?? throw new ArgumentNullException(nameof(onComplete));
        }

        private void ResetForStart()
        {
            version++;
            orderIndex = 0;
            requestedOrderIndex = -1;
            LastStartedTaskIndex = -1;
            startedCount = 0;
            completedCount = 0;
            activeUtilityIndex = -1;
            completionRequested = false;
            hasParallelSuccess = false;
            hasParallelFailure = false;
            failedUtilityIndexes.Clear();
        }

        private void BeginExecution()
        {
            if (!HasExecutableTask())
            {
                Complete(GetEmptyStatus());
                return;
            }

            BeginNonEmptyExecution();
        }

        private void BeginNonEmptyExecution()
        {
            if (IsSequential())
            {
                ExecuteNextSequentialTask();
                return;
            }

            if (executionMethod == ActionListExecutionMethod.UtilitySelector)
            {
                ExecuteHighestUtilityTask();
                return;
            }

            ExecuteParallelTasks();
        }

        private void AvoidImmediateRepeat(int taskIndexToAvoid)
        {
            if (!CanAvoidRepeat(taskIndexToAvoid))
            {
                return;
            }

            int firstIndex = FindExecutableOrderIndex(0);
            int replacementIndex = FindExecutableOrderIndex(firstIndex + 1);
            if (firstIndex >= 0 && replacementIndex >= 0 && order[firstIndex] == taskIndexToAvoid)
            {
                SwapOrder(firstIndex, replacementIndex);
            }
        }

        private bool CanAvoidRepeat(int taskIndexToAvoid)
        {
            return taskIndexToAvoid >= 0 && CompositeExecutionDescription.SupportsOrder(executionMethod) && orderMode != ActionListOrderMode.Ordered;
        }

        private int FindExecutableOrderIndex(int startIndex)
        {
            for (int index = Math.Max(startIndex, 0); index < order.Count; index++)
            {
                if (CanExecuteTask(order[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        private void SwapOrder(int firstIndex, int secondIndex)
        {
            int value = order[firstIndex];
            order[firstIndex] = order[secondIndex];
            order[secondIndex] = value;
        }

        private bool CanReevaluateUtility()
        {
            return IsExecuting && executionMethod == ActionListExecutionMethod.UtilitySelector && activeUtilityIndex >= 0 && !tasks[activeUtilityIndex].BlockDuringExecution;
        }

        private void ReevaluateUtility(int nextIndex)
        {
            if (nextIndex == activeUtilityIndex)
            {
                return;
            }

            if (nextIndex < 0)
            {
                AbortActiveUtilityTask();
                Complete(ActionExecutionStatus.Failure);
                return;
            }

            SwitchUtilityTaskIfBetter(nextIndex);
        }

        private void SwitchUtilityTaskIfBetter(int nextIndex)
        {
            bool activeEligible = CanSelectUtilityTask(activeUtilityIndex);
            if (activeEligible && GetComparableUtility(nextIndex) <= GetComparableUtility(activeUtilityIndex))
            {
                return;
            }

            AbortActiveUtilityTask();
            StartUtilityTask(nextIndex);
        }

        private void ExecuteParallelTasks()
        {
            isStartingParallel = true;
            foreach (int taskIndex in order)
            {
                if (CanExecuteTask(taskIndex))
                {
                    ExecuteTask(taskIndex);
                }
            }

            isStartingParallel = false;
            FinishParallelStart();
        }

        private void FinishParallelStart()
        {
            if (awaitMode == ActionListAwaitMode.WaitNone)
            {
                Complete(ActionExecutionStatus.Success);
                return;
            }

            if (completionRequested)
            {
                CompleteRequestedParallel();
                return;
            }

            CompleteParallelIfFinished();
        }

        private void CompleteRequestedParallel()
        {
            ActionExecutionStatus status = requestedCompletionStatus;
            completionRequested = false;
            Complete(status);
        }

        private void CompleteParallelIfFinished()
        {
            if (startedCount == 0 || completedCount >= startedCount)
            {
                Complete(GetParallelStatus());
            }
        }

        private void ExecuteNextSequentialTask()
        {
            if (!IsExecuting)
            {
                return;
            }

            int taskIndex = FindNextSequentialTask();
            if (taskIndex >= 0)
            {
                ExecuteTask(taskIndex);
                return;
            }

            Complete(GetExhaustedSequentialStatus());
        }

        private int FindNextSequentialTask()
        {
            while (orderIndex < order.Count)
            {
                int taskIndex = order[orderIndex];
                if (CanExecuteTask(taskIndex))
                {
                    return taskIndex;
                }

                orderIndex++;
            }

            return -1;
        }

        private void ExecuteTask(int taskIndex)
        {
            LastStartedTaskIndex = taskIndex;
            startedCount++;
            runningIndexes.Add(taskIndex);
            int executionVersion = version;
            try
            {
                tasks[taskIndex].Execute(status => OnTaskComplete(taskIndex, status, executionVersion));
            }
            catch (Exception exception)
            {
                logger.Error($"Task {taskIndex} execution failed.", exception);
                OnTaskComplete(taskIndex, ActionExecutionStatus.Failure, executionVersion);
            }
        }

        private void OnTaskComplete(int taskIndex, ActionExecutionStatus status, int executionVersion)
        {
            if (executionVersion != version || !runningIndexes.Remove(taskIndex))
            {
                return;
            }

            CompleteRemovedTask(taskIndex, status);
        }

        private void CompleteRemovedTask(int taskIndex, ActionExecutionStatus status)
        {
            statuses[taskIndex] = status;
            if (!IsExecuting)
            {
                return;
            }

            RecordCompletion(status);
            RouteCompletion(taskIndex, status);
        }

        private void RecordCompletion(ActionExecutionStatus status)
        {
            completedCount++;
            hasParallelSuccess |= status == ActionExecutionStatus.Success;
            hasParallelFailure |= status != ActionExecutionStatus.Success;
        }

        private void RouteCompletion(int taskIndex, ActionExecutionStatus status)
        {
            if (executionMethod == ActionListExecutionMethod.UtilitySelector)
            {
                CompleteUtilityTask(taskIndex, status);
                return;
            }

            RouteNonUtilityCompletion(status);
        }

        private void RouteNonUtilityCompletion(ActionExecutionStatus status)
        {
            if (executionMethod == ActionListExecutionMethod.Sequence)
            {
                CompleteSequenceTask(status);
                return;
            }

            if (executionMethod == ActionListExecutionMethod.Selector)
            {
                CompleteSelectorTask(status);
                return;
            }

            CompleteParallelTask(status);
        }

        private void CompleteUtilityTask(int taskIndex, ActionExecutionStatus status)
        {
            activeUtilityIndex = -1;
            if (status == ActionExecutionStatus.Success)
            {
                Complete(ActionExecutionStatus.Success);
                return;
            }

            failedUtilityIndexes.Add(taskIndex);
            ExecuteHighestUtilityTask();
        }

        private void CompleteSequenceTask(ActionExecutionStatus status)
        {
            if (status != ActionExecutionStatus.Success)
            {
                Complete(ActionExecutionStatus.Failure);
                return;
            }

            AdvanceSequentialOrder();
            executeNextSequentialTask.Invoke();
        }

        private void CompleteSelectorTask(ActionExecutionStatus status)
        {
            if (status == ActionExecutionStatus.Success)
            {
                Complete(ActionExecutionStatus.Success);
                return;
            }

            AdvanceSequentialOrder();
            executeNextSequentialTask.Invoke();
        }

        private void AdvanceSequentialOrder()
        {
            if (requestedOrderIndex >= 0)
            {
                orderIndex = requestedOrderIndex;
                requestedOrderIndex = -1;
                return;
            }

            orderIndex++;
        }

        private void CompleteParallelTask(ActionExecutionStatus status)
        {
            if (awaitMode == ActionListAwaitMode.WaitAny)
            {
                RequestOrCompleteParallel(status);
                return;
            }

            if (!isStartingParallel && completedCount >= startedCount)
            {
                Complete(GetParallelStatus());
            }
        }

        private void RequestOrCompleteParallel(ActionExecutionStatus status)
        {
            if (isStartingParallel)
            {
                if (!completionRequested)
                {
                    requestedCompletionStatus = status;
                    completionRequested = true;
                }

                return;
            }

            Complete(status);
        }

        private void ExecuteHighestUtilityTask()
        {
            int taskIndex = FindHighestUtilityIndex();
            if (taskIndex < 0)
            {
                Complete(ActionExecutionStatus.Failure);
                return;
            }

            StartUtilityTask(taskIndex);
        }

        private void StartUtilityTask(int taskIndex)
        {
            activeUtilityIndex = taskIndex;
            executeTask.Invoke(taskIndex);
        }

        private int FindHighestUtilityIndex()
        {
            int selectedIndex = -1;
            float selectedUtility = float.NegativeInfinity;
            foreach (int taskIndex in order)
            {
                SelectHigherUtility(taskIndex, ref selectedIndex, ref selectedUtility);
            }

            return selectedIndex;
        }

        private void SelectHigherUtility(int taskIndex, ref int selectedIndex, ref float selectedUtility)
        {
            if (!CanSelectUtilityTask(taskIndex))
            {
                return;
            }

            float utility = GetComparableUtility(taskIndex);
            if (selectedIndex < 0 || utility > selectedUtility)
            {
                selectedIndex = taskIndex;
                selectedUtility = utility;
            }
        }

        private bool CanSelectUtilityTask(int taskIndex)
        {
            return !failedUtilityIndexes.Contains(taskIndex) && CanExecuteTask(taskIndex);
        }

        private float GetComparableUtility(int taskIndex)
        {
            float utility = tasks[taskIndex].Utility;
            return float.IsNaN(utility) ? float.NegativeInfinity : utility;
        }

        private void AbortActiveUtilityTask()
        {
            int taskIndex = activeUtilityIndex;
            activeUtilityIndex = -1;
            version++;
            runningIndexes.Remove(taskIndex);
            InterruptTask(taskIndex);
        }

        private bool IsSequential()
        {
            return executionMethod == ActionListExecutionMethod.Sequence || executionMethod == ActionListExecutionMethod.Selector;
        }

        private bool HasExecutableTask()
        {
            foreach (int taskIndex in order)
            {
                if (CanExecuteTask(taskIndex))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanExecuteTask(int taskIndex)
        {
            return taskIndex >= 0 && taskIndex < tasks.Count && tasks[taskIndex] != null && tasks[taskIndex].IsEnabled;
        }

        private ActionExecutionStatus GetParallelStatus()
        {
            if (executionMethod == ActionListExecutionMethod.ParallelSelector)
            {
                return hasParallelSuccess ? ActionExecutionStatus.Success : ActionExecutionStatus.Failure;
            }

            return hasParallelFailure ? ActionExecutionStatus.Failure : ActionExecutionStatus.Success;
        }

        private ActionExecutionStatus GetEmptyStatus()
        {
            bool selector = executionMethod == ActionListExecutionMethod.Selector || executionMethod == ActionListExecutionMethod.ParallelSelector || executionMethod == ActionListExecutionMethod.UtilitySelector;
            return selector ? ActionExecutionStatus.Failure : ActionExecutionStatus.Success;
        }

        private ActionExecutionStatus GetExhaustedSequentialStatus()
        {
            return executionMethod == ActionListExecutionMethod.Selector ? ActionExecutionStatus.Failure : ActionExecutionStatus.Success;
        }

        private void Complete(ActionExecutionStatus status)
        {
            if (!IsExecuting)
            {
                return;
            }

            IsExecuting = false;
            LastExecutionStatus = status;
            activeUtilityIndex = -1;
            Action<ActionExecutionStatus> callback = completion;
            completion = null;
            callback.Invoke(status);
        }

        private List<int> FindRunningIndexes(IReadOnlyList<int> requestedIndexes)
        {
            List<int> indexes = new List<int>();
            if (requestedIndexes == null)
            {
                return indexes;
            }

            foreach (int taskIndex in requestedIndexes)
            {
                AddRunningIndex(taskIndex, indexes);
            }

            return indexes;
        }

        private void AddRunningIndex(int taskIndex, ICollection<int> indexes)
        {
            if (runningIndexes.Contains(taskIndex) && !indexes.Contains(taskIndex))
            {
                indexes.Add(taskIndex);
            }
        }

        private void CompleteInterruptedTask(int taskIndex, ActionExecutionStatus status)
        {
            if (!runningIndexes.Remove(taskIndex))
            {
                return;
            }

            InterruptTask(taskIndex);
            completeRemovedTask.Invoke(taskIndex, status);
        }

        private void InterruptRunningTasks()
        {
            List<int> indexes = new List<int>(runningIndexes);
            runningIndexes.Clear();
            foreach (int taskIndex in indexes)
            {
                InterruptTask(taskIndex);
            }
        }

        private void InterruptTask(int taskIndex)
        {
            if (taskIndex < 0 || taskIndex >= tasks.Count || tasks[taskIndex] == null)
            {
                return;
            }

            TryInterruptTask(taskIndex);
        }

        private void TryInterruptTask(int taskIndex)
        {
            try
            {
                tasks[taskIndex].Interrupt();
            }
            catch (Exception exception)
            {
                logger.Error($"Task {taskIndex} interruption failed.", exception);
            }
        }
    }
}
