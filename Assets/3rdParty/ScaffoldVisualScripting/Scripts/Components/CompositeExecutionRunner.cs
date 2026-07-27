using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Owns one composite state machine shared by Block tracks and Invoke Action children.
    /// The state transitions intentionally remain centralized in this class so synchronous
    /// callbacks, detached parallel tasks, interruption, and utility reevaluation cannot
    /// diverge between the two hosts. Individual task types stay isolated behind ICompositeTask.
    /// </summary>
    public sealed class CompositeExecutionRunner
    {
        private readonly IReadOnlyList<ICompositeTask> tasks;
        private readonly Func<float> getRandomValue;
        private readonly List<int> executionOrder = new List<int>();
        private readonly HashSet<int> runningTaskIndexes = new HashSet<int>();
        private readonly HashSet<int> failedUtilityTaskIndexes = new HashSet<int>();
        private readonly Dictionary<int, CompositeExecutionStatus> completedTaskStatuses =
            new Dictionary<int, CompositeExecutionStatus>();

        private Action<CompositeExecutionStatus> onComplete;
        private CompositeExecutionMethod executionMethod;
        private CompositeAwaitMode awaitMode;
        private CompositeOrderMode orderMode;
        private int executionOrderIndex;
        private int requestedExecutionOrderIndex = -1;
        private int firstTaskIndexToAvoid = -1;
        private int startedTaskCount;
        private int completedTaskCount;
        private int executionVersion;
        private int activeUtilityTaskIndex = -1;
        private bool isStartingTasks;
        private bool completionRequested;
        private bool hasParallelSuccess;
        private bool hasParallelFailure;
        private CompositeExecutionStatus requestedCompletionStatus;

        public CompositeExecutionRunner(
            IReadOnlyList<ICompositeTask> tasks,
            Func<float> getRandomValue)
        {
            this.tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            this.getRandomValue = getRandomValue ?? throw new ArgumentNullException(nameof(getRandomValue));
        }

        public bool IsExecuting { get; private set; }

        public CompositeExecutionStatus LastExecutionStatus { get; private set; } =
            CompositeExecutionStatus.Success;

        public int LastStartedTaskIndex { get; private set; } = -1;

        public void Start(
            CompositeExecutionMethod executionMethod,
            CompositeAwaitMode awaitMode,
            CompositeOrderMode orderMode,
            Action<CompositeExecutionStatus> onComplete)
        {
            StartInternal(executionMethod, awaitMode, orderMode, -1, onComplete);
        }

        public void StartWithoutRepeatingLast(
            CompositeExecutionMethod executionMethod,
            CompositeAwaitMode awaitMode,
            CompositeOrderMode orderMode,
            int lastExecutedTaskIndex,
            Action<CompositeExecutionStatus> onComplete)
        {
            StartInternal(
                executionMethod,
                awaitMode,
                orderMode,
                lastExecutedTaskIndex,
                onComplete);
        }

        private void StartInternal(
            CompositeExecutionMethod executionMethod,
            CompositeAwaitMode awaitMode,
            CompositeOrderMode orderMode,
            int lastExecutedTaskIndex,
            Action<CompositeExecutionStatus> onComplete)
        {
            Stop();
            this.executionMethod = executionMethod;
            this.awaitMode = awaitMode;
            this.orderMode = orderMode;
            this.onComplete = onComplete;
            executionVersion++;
            executionOrderIndex = 0;
            requestedExecutionOrderIndex = -1;
            firstTaskIndexToAvoid = lastExecutedTaskIndex;
            LastStartedTaskIndex = -1;
            startedTaskCount = 0;
            completedTaskCount = 0;
            activeUtilityTaskIndex = -1;
            isStartingTasks = false;
            completionRequested = false;
            hasParallelSuccess = false;
            hasParallelFailure = false;
            failedUtilityTaskIndexes.Clear();
            IsExecuting = true;
            BuildExecutionOrder();
            AvoidImmediateTaskRepeat();

            if (!HasExecutableTask())
            {
                CompleteAfterAll(GetEmptyCompositeStatus());
                return;
            }

            if (IsSequentialExecution())
            {
                ExecuteNextSequentialTask();
                return;
            }

            if (executionMethod == CompositeExecutionMethod.UtilitySelector)
            {
                ExecuteHighestUtilityTask();
                return;
            }

            ExecuteParallelTasks();
        }

        public void Tick()
        {
            if (!IsExecuting ||
                executionMethod != CompositeExecutionMethod.UtilitySelector ||
                activeUtilityTaskIndex < 0)
            {
                return;
            }

            if (tasks[activeUtilityTaskIndex].BlockDuringExecution)
            {
                return;
            }

            int highestUtilityTaskIndex = FindHighestUtilityTaskIndex();
            if (highestUtilityTaskIndex == activeUtilityTaskIndex)
            {
                return;
            }

            if (highestUtilityTaskIndex < 0)
            {
                AbortActiveUtilityTask();
                CompleteAfterAll(CompositeExecutionStatus.Failure);
                return;
            }

            bool activeTaskIsEligible = CanSelectUtilityTask(activeUtilityTaskIndex);
            if (activeTaskIsEligible &&
                GetComparableUtility(highestUtilityTaskIndex) <=
                GetComparableUtility(activeUtilityTaskIndex))
            {
                return;
            }

            AbortActiveUtilityTask();
            StartUtilityTask(highestUtilityTaskIndex);
        }

        public bool IsTaskRunning(int taskIndex)
        {
            return runningTaskIndexes.Contains(taskIndex);
        }

        public bool TryGetTaskStatus(
            int taskIndex,
            out CompositeExecutionStatus status)
        {
            return completedTaskStatuses.TryGetValue(taskIndex, out status);
        }

        public void ResetTaskStatuses()
        {
            completedTaskStatuses.Clear();
        }

        public void RequestNextTaskIndex(int taskIndex)
        {
            if (!IsExecuting || orderMode != CompositeOrderMode.Ordered)
            {
                return;
            }

            if (taskIndex == tasks.Count)
            {
                requestedExecutionOrderIndex = executionOrder.Count;
                return;
            }

            requestedExecutionOrderIndex = executionOrder.IndexOf(taskIndex);
        }

        public int InterruptTasks(
            IReadOnlyList<int> taskIndexes,
            CompositeExecutionStatus interruptionStatus)
        {
            if (taskIndexes == null || taskIndexes.Count == 0)
            {
                return 0;
            }

            int interruptedTaskCount = 0;
            int currentExecutionVersion = executionVersion;
            List<int> uniqueTaskIndexes = new List<int>();
            foreach (int taskIndex in taskIndexes)
            {
                if (runningTaskIndexes.Contains(taskIndex) && !uniqueTaskIndexes.Contains(taskIndex))
                {
                    uniqueTaskIndexes.Add(taskIndex);
                }
            }

            foreach (int taskIndex in uniqueTaskIndexes)
            {
                if (currentExecutionVersion != executionVersion)
                {
                    break;
                }

                InterruptTask(taskIndex);
                OnTaskComplete(taskIndex, interruptionStatus, currentExecutionVersion);
                interruptedTaskCount++;
            }

            return interruptedTaskCount;
        }

        public void Stop()
        {
            IsExecuting = false;
            executionVersion++;
            InterruptRunningTasks();
            runningTaskIndexes.Clear();
            activeUtilityTaskIndex = -1;
            onComplete = null;
            ResetTaskStatuses();
        }

        private void BuildExecutionOrder()
        {
            executionOrder.Clear();
            for (int taskIndex = 0; taskIndex < tasks.Count; taskIndex++)
            {
                executionOrder.Add(taskIndex);
            }

            if (!CompositeExecutionDescription.SupportsOrder(executionMethod) ||
                orderMode == CompositeOrderMode.Ordered)
            {
                return;
            }

            if (orderMode == CompositeOrderMode.Shuffle)
            {
                ShuffleExecutionOrder();
                return;
            }

            BuildWeightedExecutionOrder();
        }

        private void ShuffleExecutionOrder()
        {
            for (int sourceIndex = executionOrder.Count - 1; sourceIndex > 0; sourceIndex--)
            {
                int destinationIndex = GetRandomIndex(sourceIndex + 1);
                int taskIndex = executionOrder[sourceIndex];
                executionOrder[sourceIndex] = executionOrder[destinationIndex];
                executionOrder[destinationIndex] = taskIndex;
            }
        }

        private void BuildWeightedExecutionOrder()
        {
            List<int> remainingTaskIndexes = new List<int>(executionOrder);
            executionOrder.Clear();
            while (remainingTaskIndexes.Count > 0)
            {
                int selectedListIndex = SelectWeightedListIndex(remainingTaskIndexes);
                executionOrder.Add(remainingTaskIndexes[selectedListIndex]);
                remainingTaskIndexes.RemoveAt(selectedListIndex);
            }
        }

        private void AvoidImmediateTaskRepeat()
        {
            if (firstTaskIndexToAvoid < 0 ||
                !CompositeExecutionDescription.SupportsOrder(executionMethod) ||
                orderMode == CompositeOrderMode.Ordered)
            {
                return;
            }

            int firstExecutableOrderIndex = FindExecutableOrderIndex(0);
            if (firstExecutableOrderIndex < 0 ||
                executionOrder[firstExecutableOrderIndex] != firstTaskIndexToAvoid)
            {
                return;
            }

            int replacementOrderIndex = FindExecutableOrderIndex(firstExecutableOrderIndex + 1);
            if (replacementOrderIndex < 0)
            {
                return;
            }

            int replacementTaskIndex = executionOrder[replacementOrderIndex];
            executionOrder[replacementOrderIndex] = executionOrder[firstExecutableOrderIndex];
            executionOrder[firstExecutableOrderIndex] = replacementTaskIndex;
        }

        private int FindExecutableOrderIndex(int startOrderIndex)
        {
            for (int orderIndex = startOrderIndex; orderIndex < executionOrder.Count; orderIndex++)
            {
                if (CanExecuteTask(executionOrder[orderIndex]))
                {
                    return orderIndex;
                }
            }

            return -1;
        }

        private int SelectWeightedListIndex(IReadOnlyList<int> taskIndexes)
        {
            float totalWeight = 0f;
            for (int listIndex = 0; listIndex < taskIndexes.Count; listIndex++)
            {
                totalWeight += Mathf.Clamp(tasks[taskIndexes[listIndex]].Weight, 0f, 100f);
            }

            if (totalWeight <= 0f)
            {
                return GetRandomIndex(taskIndexes.Count);
            }

            float targetWeight = GetClampedRandomValue() * totalWeight;
            float cumulativeWeight = 0f;
            for (int listIndex = 0; listIndex < taskIndexes.Count; listIndex++)
            {
                cumulativeWeight += Mathf.Clamp(tasks[taskIndexes[listIndex]].Weight, 0f, 100f);
                if (targetWeight < cumulativeWeight)
                {
                    return listIndex;
                }
            }

            return taskIndexes.Count - 1;
        }

        private int GetRandomIndex(int count)
        {
            if (count <= 1)
            {
                return 0;
            }

            return Mathf.Min(Mathf.FloorToInt(GetClampedRandomValue() * count), count - 1);
        }

        private float GetClampedRandomValue()
        {
            return Mathf.Clamp(getRandomValue(), 0f, 0.999999f);
        }

        private bool HasExecutableTask()
        {
            foreach (int taskIndex in executionOrder)
            {
                if (CanExecuteTask(taskIndex))
                {
                    return true;
                }
            }

            return false;
        }

        private void ExecuteParallelTasks()
        {
            isStartingTasks = true;
            foreach (int taskIndex in executionOrder)
            {
                if (!CanExecuteTask(taskIndex))
                {
                    continue;
                }

                ExecuteTask(taskIndex);
            }
            isStartingTasks = false;

            if (awaitMode == CompositeAwaitMode.WaitNone)
            {
                CompleteAndDetachRemaining(CompositeExecutionStatus.Success);
                return;
            }

            if (completionRequested)
            {
                CompositeExecutionStatus completionStatus = requestedCompletionStatus;
                completionRequested = false;
                CompleteAndDetachRemaining(completionStatus);
                return;
            }

            if (startedTaskCount == 0 || completedTaskCount >= startedTaskCount)
            {
                CompleteAfterAll(GetCompletedParallelStatus());
            }
        }

        private void ExecuteNextSequentialTask()
        {
            if (!IsExecuting)
            {
                return;
            }

            while (executionOrderIndex < executionOrder.Count)
            {
                int taskIndex = executionOrder[executionOrderIndex];
                if (CanExecuteTask(taskIndex))
                {
                    ExecuteTask(taskIndex);
                    return;
                }

                executionOrderIndex++;
            }

            CompositeExecutionStatus status = executionMethod == CompositeExecutionMethod.Selector
                ? CompositeExecutionStatus.Failure
                : CompositeExecutionStatus.Success;
            CompleteAfterAll(status);
        }

        private void ExecuteTask(int taskIndex)
        {
            LastStartedTaskIndex = taskIndex;
            startedTaskCount++;
            runningTaskIndexes.Add(taskIndex);
            int currentExecutionVersion = executionVersion;
            try
            {
                tasks[taskIndex].Execute(status =>
                    OnTaskComplete(taskIndex, status, currentExecutionVersion));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[CompositeExecutionRunner] Task execution failed: {exception}");
                OnTaskComplete(taskIndex, CompositeExecutionStatus.Failure, currentExecutionVersion);
            }
        }

        private void OnTaskComplete(
            int taskIndex,
            CompositeExecutionStatus status,
            int currentExecutionVersion)
        {
            if (currentExecutionVersion != executionVersion ||
                !runningTaskIndexes.Remove(taskIndex))
            {
                return;
            }

            completedTaskStatuses[taskIndex] = status;

            if (!IsExecuting)
            {
                return;
            }

            completedTaskCount++;
            hasParallelSuccess |= status == CompositeExecutionStatus.Success;
            hasParallelFailure |= status == CompositeExecutionStatus.Failure;

            if (executionMethod == CompositeExecutionMethod.UtilitySelector)
            {
                CompleteUtilityTask(taskIndex, status);
                return;
            }

            if (executionMethod == CompositeExecutionMethod.Sequence)
            {
                CompleteSequenceTask(status);
                return;
            }

            if (executionMethod == CompositeExecutionMethod.Selector)
            {
                CompleteSelectorTask(status);
                return;
            }

            CompleteParallelTask(status);
        }

        private void CompleteUtilityTask(int taskIndex, CompositeExecutionStatus status)
        {
            activeUtilityTaskIndex = -1;
            if (status == CompositeExecutionStatus.Success)
            {
                CompleteAfterAll(CompositeExecutionStatus.Success);
                return;
            }

            failedUtilityTaskIndexes.Add(taskIndex);
            ExecuteHighestUtilityTask();
        }

        private void CompleteSequenceTask(CompositeExecutionStatus status)
        {
            if (status == CompositeExecutionStatus.Failure)
            {
                CompleteAfterAll(CompositeExecutionStatus.Failure);
                return;
            }

            AdvanceSequentialOrder();
            ExecuteNextSequentialTask();
        }

        private void CompleteSelectorTask(CompositeExecutionStatus status)
        {
            if (status == CompositeExecutionStatus.Success)
            {
                CompleteAfterAll(CompositeExecutionStatus.Success);
                return;
            }

            AdvanceSequentialOrder();
            ExecuteNextSequentialTask();
        }

        private void AdvanceSequentialOrder()
        {
            if (requestedExecutionOrderIndex >= 0)
            {
                executionOrderIndex = requestedExecutionOrderIndex;
                requestedExecutionOrderIndex = -1;
                return;
            }

            executionOrderIndex++;
        }

        private void CompleteParallelTask(CompositeExecutionStatus status)
        {
            if (awaitMode == CompositeAwaitMode.WaitAny)
            {
                if (isStartingTasks)
                {
                    if (!completionRequested)
                    {
                        requestedCompletionStatus = status;
                        completionRequested = true;
                    }
                    return;
                }

                CompleteAndDetachRemaining(status);
                return;
            }

            if (!isStartingTasks && completedTaskCount >= startedTaskCount)
            {
                CompleteAfterAll(GetCompletedParallelStatus());
            }
        }

        private void ExecuteHighestUtilityTask()
        {
            int highestUtilityTaskIndex = FindHighestUtilityTaskIndex();
            if (highestUtilityTaskIndex < 0)
            {
                CompleteAfterAll(CompositeExecutionStatus.Failure);
                return;
            }

            StartUtilityTask(highestUtilityTaskIndex);
        }

        private void StartUtilityTask(int taskIndex)
        {
            activeUtilityTaskIndex = taskIndex;
            ExecuteTask(taskIndex);
        }

        private int FindHighestUtilityTaskIndex()
        {
            int highestUtilityTaskIndex = -1;
            float highestUtility = float.NegativeInfinity;
            foreach (int taskIndex in executionOrder)
            {
                if (!CanSelectUtilityTask(taskIndex))
                {
                    continue;
                }

                float utility = GetComparableUtility(taskIndex);
                if (highestUtilityTaskIndex < 0 || utility > highestUtility)
                {
                    highestUtilityTaskIndex = taskIndex;
                    highestUtility = utility;
                }
            }

            return highestUtilityTaskIndex;
        }

        private bool CanSelectUtilityTask(int taskIndex)
        {
            return !failedUtilityTaskIndexes.Contains(taskIndex) && CanExecuteTask(taskIndex);
        }

        private float GetComparableUtility(int taskIndex)
        {
            float utility = tasks[taskIndex].Utility;
            return float.IsNaN(utility) ? float.NegativeInfinity : utility;
        }

        private void AbortActiveUtilityTask()
        {
            int interruptedTaskIndex = activeUtilityTaskIndex;
            activeUtilityTaskIndex = -1;
            executionVersion++;
            InterruptTask(interruptedTaskIndex);
            runningTaskIndexes.Remove(interruptedTaskIndex);
        }

        private bool CanExecuteTask(int taskIndex)
        {
            return taskIndex >= 0 &&
                   taskIndex < tasks.Count &&
                   tasks[taskIndex] != null &&
                   tasks[taskIndex].IsEnabled;
        }

        private bool IsSequentialExecution()
        {
            return executionMethod == CompositeExecutionMethod.Sequence ||
                   executionMethod == CompositeExecutionMethod.Selector;
        }

        private CompositeExecutionStatus GetCompletedParallelStatus()
        {
            if (executionMethod == CompositeExecutionMethod.ParallelSelector)
            {
                return hasParallelSuccess
                    ? CompositeExecutionStatus.Success
                    : CompositeExecutionStatus.Failure;
            }

            return hasParallelFailure
                ? CompositeExecutionStatus.Failure
                : CompositeExecutionStatus.Success;
        }

        private CompositeExecutionStatus GetEmptyCompositeStatus()
        {
            return executionMethod == CompositeExecutionMethod.Selector ||
                   executionMethod == CompositeExecutionMethod.ParallelSelector ||
                   executionMethod == CompositeExecutionMethod.UtilitySelector
                ? CompositeExecutionStatus.Failure
                : CompositeExecutionStatus.Success;
        }

        private void CompleteAfterAll(CompositeExecutionStatus status)
        {
            Complete(status);
        }

        private void CompleteAndDetachRemaining(CompositeExecutionStatus status)
        {
            Complete(status);
        }

        private void Complete(CompositeExecutionStatus status)
        {
            if (!IsExecuting)
            {
                return;
            }

            IsExecuting = false;
            LastExecutionStatus = status;
            activeUtilityTaskIndex = -1;
            Action<CompositeExecutionStatus> completion = onComplete;
            onComplete = null;
            completion?.Invoke(status);
        }

        private void InterruptRunningTasks()
        {
            List<int> taskIndexes = new List<int>(runningTaskIndexes);
            foreach (int taskIndex in taskIndexes)
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

            try
            {
                tasks[taskIndex].Interrupt();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[CompositeExecutionRunner] Task interruption failed: {exception}");
            }
        }
    }
}
