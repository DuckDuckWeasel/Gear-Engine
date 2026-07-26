using System;
using System.Collections.Generic;
using Scaffold;
using UnityEngine;
using GearEngine.Core.Actions;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    /// <summary>
    /// A generic wrapper command for Scaffold that executes decoupled pure C# IActions.
    /// Supports behavior-tree-style sequential, parallel, selector, and utility execution.
    /// </summary>
    [CommandInfo("Generic", "Action Invoker", "Executes pure C# actions independently of MonoBehaviours.")]
    [AddComponentMenu("")]
    public class InvokeActionCommand : Command, ICompositeExecutionStatusProvider
    {
        [Serializable]
        public struct ActionWrapper
        {
            [SerializeReference]
            public IAction action;

            public bool enabled;
            public string id;
            public InvokeActionUtilitySettings utilitySettings;

            public ActionWrapper(IAction action)
            {
                this.action = action;
                this.enabled = true;
                this.id = Guid.NewGuid().ToString("N");
                this.utilitySettings = new InvokeActionUtilitySettings(0f, false);
            }
        }

        [Tooltip("The pure C# actions to execute.")]
        public List<ActionWrapper> actions = new List<ActionWrapper>();

        [SerializeField] private bool displayAsGroup;

        [Tooltip("Controls the composite behavior used to execute nested actions.")]
        [SerializeField]
        private CompositeExecutionMethod executionMethod =
            CompositeExecutionMethod.Sequence;

        [Tooltip("Controls when a parallel Action Invoker returns.")]
        [SerializeField] private CompositeAwaitMode awaitMode = CompositeAwaitMode.WaitAll;

        [Tooltip("Controls the child order for Sequence and Selector.")]
        [SerializeField] private CompositeOrderMode orderMode = CompositeOrderMode.Ordered;

        [Tooltip("Prevents Random and Shuffle from starting with the action that executed last in the previous run.")]
        [SerializeField] private bool avoidRepeatingLastAction;

        private readonly List<ICompositeTask> compositeTasks = new List<ICompositeTask>();
        private CompositeExecutionRunner compositeRunner;
        private string lastExecutedActionId;

        public CompositeExecutionMethod ExecutionMethod
        {
            get => executionMethod;
            set => executionMethod = value;
        }

        public CompositeAwaitMode AwaitMode
        {
            get => awaitMode;
            set => awaitMode = value;
        }

        public CompositeOrderMode OrderMode
        {
            get => orderMode;
            set => orderMode = value;
        }

        public bool AvoidRepeatingLastAction
        {
            get => avoidRepeatingLastAction;
            set => avoidRepeatingLastAction = value;
        }

        public bool DisplayAsGroup
        {
            get => displayAsGroup;
            set => displayAsGroup = value;
        }

        public ActionExecutionStatus LastExecutionStatus { get; private set; } = ActionExecutionStatus.Success;

        public CompositeExecutionStatus LastCompositeExecutionStatus { get; private set; } =
            CompositeExecutionStatus.Success;

        public float GetActionUtility(int index)
        {
            EnsureActionMetadata();
            return index >= 0 && index < actions.Count
                ? actions[index].utilitySettings.Utility
                : float.NegativeInfinity;
        }

        public void SetActionUtility(int index, float utility)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actions.Count)
            {
                return;
            }

            ActionWrapper wrapper = actions[index];
            InvokeActionUtilitySettings settings = wrapper.utilitySettings;
            settings.SetUtility(utility);
            wrapper.utilitySettings = settings;
            actions[index] = wrapper;
        }

        public void SetActionUtilityData(int index, FloatData utility)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actions.Count)
            {
                return;
            }

            ActionWrapper wrapper = actions[index];
            InvokeActionUtilitySettings settings = wrapper.utilitySettings;
            settings.SetUtility(utility);
            wrapper.utilitySettings = settings;
            actions[index] = wrapper;
        }

        public bool IsUtilityBlockedDuringExecution(int index)
        {
            EnsureActionMetadata();
            return index >= 0 && index < actions.Count &&
                   actions[index].utilitySettings.BlockDuringExecution;
        }

        public void SetUtilityBlockedDuringExecution(int index, bool shouldBlock)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actions.Count)
            {
                return;
            }

            ActionWrapper wrapper = actions[index];
            InvokeActionUtilitySettings settings = wrapper.utilitySettings;
            settings.SetBlockDuringExecution(shouldBlock);
            wrapper.utilitySettings = settings;
            actions[index] = wrapper;
        }

        public float GetActionWeight(int index)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actions.Count || !IsActionEnabled(index))
            {
                return 0f;
            }

            GetEnabledActionWeightBalance(out float overrideTotal, out int automaticActionCount);

            InvokeActionUtilitySettings selectedSettings = actions[index].utilitySettings;
            if (overrideTotal >= 100f)
            {
                return selectedSettings.HasWeightOverride && overrideTotal > 0f
                    ? selectedSettings.Weight / overrideTotal * 100f
                    : 0f;
            }

            if (selectedSettings.HasWeightOverride)
            {
                return selectedSettings.Weight;
            }

            if (automaticActionCount > 0)
            {
                return (100f - overrideTotal) / automaticActionCount;
            }

            return overrideTotal > 0f
                ? selectedSettings.Weight / overrideTotal * 100f
                : 0f;
        }

        private void GetEnabledActionWeightBalance(
            out float overrideTotal,
            out int automaticActionCount)
        {
            overrideTotal = 0f;
            automaticActionCount = 0;
            for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                if (!IsActionEnabled(actionIndex))
                {
                    continue;
                }

                InvokeActionUtilitySettings settings = actions[actionIndex].utilitySettings;
                if (settings.HasWeightOverride)
                {
                    overrideTotal += settings.Weight;
                    continue;
                }

                automaticActionCount++;
            }
        }

        public void SetActionWeight(int index, float weight)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actions.Count)
            {
                return;
            }

            ActionWrapper wrapper = actions[index];
            InvokeActionUtilitySettings settings = wrapper.utilitySettings;
            settings.SetWeight(weight);
            wrapper.utilitySettings = settings;
            actions[index] = wrapper;
        }

        public bool HasActionWeightOverride(int index)
        {
            EnsureActionMetadata();
            return index >= 0 && index < actions.Count &&
                   actions[index].utilitySettings.HasWeightOverride;
        }

        public void ClearActionWeightOverride(int index)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actions.Count)
            {
                return;
            }

            ActionWrapper wrapper = actions[index];
            InvokeActionUtilitySettings settings = wrapper.utilitySettings;
            settings.ClearWeightOverride();
            wrapper.utilitySettings = settings;
            actions[index] = wrapper;
        }

        public bool IsActionEnabled(int index)
        {
            return index >= 0 && index < actions.Count &&
                   actions[index].enabled;
        }

        public void SetActionEnabled(int index, bool enabled)
        {
            EnsureActionMetadata();
            if (index >= 0 && index < actions.Count)
            {
                ActionWrapper wrapper = actions[index];
                wrapper.enabled = enabled;
                actions[index] = wrapper;
            }
        }

        public void ReorderActions(IReadOnlyList<int> sourceIndices)
        {
            if (sourceIndices == null || sourceIndices.Count != actions.Count)
            {
                return;
            }

            EnsureActionMetadata();
            List<ActionWrapper> sourceActions = new List<ActionWrapper>(actions);
            for (int i = 0; i < sourceIndices.Count; i++)
            {
                int sourceIndex = sourceIndices[i];
                if (sourceIndex < 0 || sourceIndex >= sourceActions.Count)
                {
                    return;
                }

                actions[i] = sourceActions[sourceIndex];
            }
        }

        public bool TryRemoveAction(int index, out IAction action, out bool enabled)
        {
            return TryRemoveAction(index, out action, out enabled, out _);
        }

        public bool TryRemoveAction(
            int index,
            out IAction action,
            out bool enabled,
            out InvokeActionUtilitySettings utilitySettings)
        {
            action = null;
            enabled = true;
            utilitySettings = new InvokeActionUtilitySettings(0f, false);
            EnsureActionMetadata();

            if (index < 0 || index >= actions.Count)
            {
                return false;
            }

            ActionWrapper wrapper = actions[index];
            action = wrapper.action;
            enabled = this.enabled && wrapper.enabled;
            utilitySettings = wrapper.utilitySettings;
            if (actions.Count > 1)
            {
                displayAsGroup = true;
            }
            actions.RemoveAt(index);
            return true;
        }

        public void InsertAction(int index, IAction action, bool enabled)
        {
            InsertAction(index, action, enabled, new InvokeActionUtilitySettings(0f, false));
        }

        public void InsertAction(
            int index,
            IAction action,
            bool enabled,
            InvokeActionUtilitySettings utilitySettings)
        {
            EnsureActionMetadata();
            index = Mathf.Clamp(index, 0, actions.Count);
            ActionWrapper wrapper = new ActionWrapper(action);
            wrapper.enabled = enabled;
            wrapper.utilitySettings = utilitySettings;
            actions.Insert(index, wrapper);
            if (actions.Count > 1)
            {
                displayAsGroup = true;
            }
        }

        public void InsertActionInGroup(int index, IAction action, bool enabled)
        {
            InsertAction(index, action, enabled);
            displayAsGroup = true;
        }

        public void InsertActionInGroup(
            int index,
            IAction action,
            bool enabled,
            InvokeActionUtilitySettings utilitySettings)
        {
            InsertAction(index, action, enabled, utilitySettings);
            displayAsGroup = true;
        }

        public bool TryMoveAction(int sourceIndex, int destinationIndex)
        {
            EnsureActionMetadata();
            if (sourceIndex < 0 || sourceIndex >= actions.Count ||
                destinationIndex < 0 || destinationIndex >= actions.Count)
            {
                return false;
            }

            if (sourceIndex == destinationIndex)
            {
                return true;
            }

            ActionWrapper wrapper = actions[sourceIndex];
            actions.RemoveAt(sourceIndex);
            actions.Insert(destinationIndex, wrapper);
            return true;
        }

        public string GetActionId(int actionIndex)
        {
            EnsureActionMetadata();
            return actionIndex >= 0 && actionIndex < actions.Count
                ? actions[actionIndex].id
                : string.Empty;
        }

        public bool IsActionRunning(int actionIndex)
        {
            return compositeRunner != null && compositeRunner.IsTaskRunning(actionIndex);
        }

        public bool TryGetActionExecutionStatus(
            int actionIndex,
            out CompositeExecutionStatus status)
        {
            status = default;
            return compositeRunner != null &&
                   compositeRunner.TryGetTaskStatus(actionIndex, out status);
        }

        public bool TryGetActionExecutionProgress(int actionIndex, out float progress)
        {
            progress = 0f;
            if (!IsActionRunning(actionIndex) ||
                actionIndex < 0 ||
                actionIndex >= actions.Count ||
                actions[actionIndex].action is not IActionProgressProvider progressProvider)
            {
                return false;
            }

            return progressProvider.TryGetExecutionProgress(out progress);
        }

        public bool TryGetExecutionProgress(out float progress)
        {
            progress = 0f;
            int runningActionIndex = -1;
            for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                if (!IsActionRunning(actionIndex))
                {
                    continue;
                }

                if (runningActionIndex >= 0)
                {
                    return false;
                }

                runningActionIndex = actionIndex;
            }

            return runningActionIndex >= 0 &&
                   TryGetActionExecutionProgress(runningActionIndex, out progress);
        }

        public bool EnsureActionMetadata()
        {
            bool metadataChanged = false;
            if (actions == null)
            {
                actions = new List<ActionWrapper>();
                metadataChanged = true;
            }
            for (int i = 0; i < actions.Count; i++)
            {
                ActionWrapper wrapper = actions[i];
                if (string.IsNullOrEmpty(wrapper.id))
                {
                    wrapper.id = CreateActionId();
                    metadataChanged = true;
                }
                if (wrapper.utilitySettings.MigrateWeightOverride())
                {
                    metadataChanged = true;
                }
                actions[i] = wrapper;
            }
            return metadataChanged;
        }

        public override void OnEnter()
        {
            CancelSequence();
            EnsureActionMetadata();
            LastExecutionStatus = ActionExecutionStatus.Success;
            LastCompositeExecutionStatus = CompositeExecutionStatus.Success;
            CreateCompositeRunner();
            int lastExecutedActionIndex = actions.FindIndex(w => w.id == lastExecutedActionId);
            if (ShouldAvoidRepeatingLastAction())
            {
                compositeRunner.StartWithoutRepeatingLast(
                    executionMethod,
                    awaitMode,
                    orderMode,
                    lastExecutedActionIndex,
                    OnCompositeComplete);
                return;
            }

            compositeRunner.Start(executionMethod, awaitMode, orderMode, OnCompositeComplete);
        }

        private void Update()
        {
            compositeRunner?.Tick();
        }

        public void ReevaluateUtilitySelection()
        {
            compositeRunner?.Tick();
        }

        public int InterruptActions(
            IReadOnlyList<string> targetActionIds,
            ActionExecutionStatus interruptionStatus)
        {
            if (compositeRunner == null || targetActionIds == null || targetActionIds.Count == 0)
            {
                return 0;
            }

            EnsureActionMetadata();
            List<int> targetIndexes = new List<int>();
            foreach (string targetActionId in targetActionIds)
            {
                int actionIndex = actions.FindIndex(w => w.id == targetActionId);
                if (actionIndex >= 0 && !targetIndexes.Contains(actionIndex))
                {
                    targetIndexes.Add(actionIndex);
                }
            }

            CompositeExecutionStatus status = interruptionStatus == ActionExecutionStatus.Success
                ? CompositeExecutionStatus.Success
                : CompositeExecutionStatus.Failure;
            return compositeRunner.InterruptTasks(targetIndexes, status);
        }

        /// <summary>
        /// Allows an IAction (e.g. flow control) to cancel the internal execution of this sequence
        /// so it can delegate jumping to the parent Block.
        /// </summary>
        public void CancelSequence()
        {
            RememberLastExecutedAction();
            compositeRunner?.Stop();
        }

        public override void OnStopExecuting()
        {
            CancelSequence();
        }

        public override void ResetExecutionFeedback()
        {
            compositeRunner?.ResetTaskStatuses();
        }

        protected virtual float GetRandomValue()
        {
            return UnityEngine.Random.value;
        }

        private void CreateCompositeRunner()
        {
            compositeTasks.Clear();
            for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                int capturedIndex = actionIndex;
                InvokeActionCompositeTask task = new InvokeActionCompositeTask(
                    actions[capturedIndex].action,
                    () => IsActionEnabled(capturedIndex),
                    () => GetActionUtility(capturedIndex),
                    () => GetActionWeight(capturedIndex),
                    () => IsUtilityBlockedDuringExecution(capturedIndex),
                    PrepareAction);
                compositeTasks.Add(task);
            }

            compositeRunner = new CompositeExecutionRunner(compositeTasks, GetRandomValue);
        }

        private void PrepareAction(IAction action)
        {
            if (action is IMonoBehaviourConsumer monoBehaviourConsumer)
            {
                monoBehaviourConsumer.SetHost(this);
            }

            if (action is IBlackboardConsumer blackboardConsumer)
            {
                blackboardConsumer.SetBlackboard(GetBlackboard());
            }

            if (action is ICommandContextConsumer commandContextConsumer)
            {
                commandContextConsumer.SetCommandContext(this);
            }
        }

        private void OnCompositeComplete(CompositeExecutionStatus status)
        {
            RememberLastExecutedAction();
            LastCompositeExecutionStatus = status;
            LastExecutionStatus = status == CompositeExecutionStatus.Success
                ? ActionExecutionStatus.Success
                : ActionExecutionStatus.Failure;
            Continue();
        }

        private bool ShouldAvoidRepeatingLastAction()
        {
            return avoidRepeatingLastAction &&
                   actions.Count > 1 &&
                   CompositeExecutionDescription.SupportsOrder(executionMethod) &&
                   orderMode != CompositeOrderMode.Ordered;
        }

        private void RememberLastExecutedAction()
        {
            if (compositeRunner == null)
            {
                return;
            }

            int lastExecutedActionIndex = compositeRunner.LastStartedTaskIndex;
            if (lastExecutedActionIndex >= 0 && lastExecutedActionIndex < actions.Count)
            {
                lastExecutedActionId = actions[lastExecutedActionIndex].id;
            }
        }

        private static string CreateActionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public override string GetSummary()
        {
            if (actions == null || actions.Count == 0)
            {
                return "Empty";
            }

            if (actions.Count == 1)
            {
                return actions[0].action != null ? actions[0].action.GetType().Name : "None";
            }

            return $"{actions.Count} Actions";
        }

        public override bool OpenBlock()
        {
            if (actions == null)
            {
                return false;
            }

            foreach (ActionWrapper wrapper in actions)
            {
                IAction action = wrapper.action;
                if (action is ActionBase actionBase && actionBase.OpenBlock())
                {
                    return true;
                }
            }
            return false;
        }

        public override bool CloseBlock()
        {
            if (actions == null)
            {
                return false;
            }

            foreach (ActionWrapper wrapper in actions)
            {
                IAction action = wrapper.action;
                if (action is ActionBase actionBase && actionBase.CloseBlock())
                {
                    return true;
                }
            }
            return false;
        }

        public override bool HasReference(Variable variable)
        {
            EnsureActionMetadata();
            foreach (ActionWrapper wrapper in actions)
            {
                if (wrapper.utilitySettings.HasReference(variable))
                {
                    return true;
                }

                if (wrapper.action is ActionBase actionBase && actionBase.HasReference(variable))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
