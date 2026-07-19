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
        [Tooltip("The pure C# actions to execute.")]
        [SerializeReference]
        public List<IAction> actions = new List<IAction>();

        [SerializeField] private List<bool> actionEnabled = new List<bool>();

        [SerializeField] private List<string> actionIds = new List<string>();

        [SerializeField]
        private List<InvokeActionUtilitySettings> actionUtilitySettings =
            new List<InvokeActionUtilitySettings>();

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
            return index >= 0 && index < actionUtilitySettings.Count
                ? actionUtilitySettings[index].Utility
                : float.NegativeInfinity;
        }

        public void SetActionUtility(int index, float utility)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actionUtilitySettings.Count)
            {
                return;
            }

            InvokeActionUtilitySettings settings = actionUtilitySettings[index];
            settings.SetUtility(utility);
            actionUtilitySettings[index] = settings;
        }

        public void SetActionUtilityData(int index, FloatData utility)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actionUtilitySettings.Count)
            {
                return;
            }

            InvokeActionUtilitySettings settings = actionUtilitySettings[index];
            settings.SetUtility(utility);
            actionUtilitySettings[index] = settings;
        }

        public bool IsUtilityBlockedDuringExecution(int index)
        {
            EnsureActionMetadata();
            return index >= 0 && index < actionUtilitySettings.Count &&
                   actionUtilitySettings[index].BlockDuringExecution;
        }

        public void SetUtilityBlockedDuringExecution(int index, bool shouldBlock)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actionUtilitySettings.Count)
            {
                return;
            }

            InvokeActionUtilitySettings settings = actionUtilitySettings[index];
            settings.SetBlockDuringExecution(shouldBlock);
            actionUtilitySettings[index] = settings;
        }

        public float GetActionWeight(int index)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actionUtilitySettings.Count || !IsActionEnabled(index))
            {
                return 0f;
            }

            GetEnabledActionWeightBalance(out float overrideTotal, out int automaticActionCount);

            InvokeActionUtilitySettings selectedSettings = actionUtilitySettings[index];
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

                InvokeActionUtilitySettings settings = actionUtilitySettings[actionIndex];
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
            if (index < 0 || index >= actionUtilitySettings.Count)
            {
                return;
            }

            InvokeActionUtilitySettings settings = actionUtilitySettings[index];
            settings.SetWeight(weight);
            actionUtilitySettings[index] = settings;
        }

        public bool HasActionWeightOverride(int index)
        {
            EnsureActionMetadata();
            return index >= 0 && index < actionUtilitySettings.Count &&
                   actionUtilitySettings[index].HasWeightOverride;
        }

        public void ClearActionWeightOverride(int index)
        {
            EnsureActionMetadata();
            if (index < 0 || index >= actionUtilitySettings.Count)
            {
                return;
            }

            InvokeActionUtilitySettings settings = actionUtilitySettings[index];
            settings.ClearWeightOverride();
            actionUtilitySettings[index] = settings;
        }

        public bool IsActionEnabled(int index)
        {
            return index >= 0 && index < actions.Count &&
                   (index >= actionEnabled.Count || actionEnabled[index]);
        }

        public void SetActionEnabled(int index, bool enabled)
        {
            EnsureActionMetadata();
            if (index >= 0 && index < actionEnabled.Count)
            {
                actionEnabled[index] = enabled;
            }
        }

        public void ReorderActions(IReadOnlyList<int> sourceIndices)
        {
            if (sourceIndices == null || sourceIndices.Count != actions.Count)
            {
                return;
            }

            EnsureActionMetadata();
            List<IAction> sourceActions = new List<IAction>(actions);
            List<bool> sourceEnabledStates = new List<bool>(actionEnabled);
            List<string> sourceActionIds = new List<string>(actionIds);
            List<InvokeActionUtilitySettings> sourceUtilitySettings =
                new List<InvokeActionUtilitySettings>(actionUtilitySettings);
            for (int i = 0; i < sourceIndices.Count; i++)
            {
                int sourceIndex = sourceIndices[i];
                if (sourceIndex < 0 || sourceIndex >= sourceActions.Count)
                {
                    return;
                }

                actions[i] = sourceActions[sourceIndex];
                actionEnabled[i] = sourceEnabledStates[sourceIndex];
                actionIds[i] = sourceActionIds[sourceIndex];
                actionUtilitySettings[i] = sourceUtilitySettings[sourceIndex];
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

            action = actions[index];
            enabled = this.enabled && actionEnabled[index];
            if (actions.Count > 1)
            {
                displayAsGroup = true;
            }
            actions.RemoveAt(index);
            actionEnabled.RemoveAt(index);
            actionIds.RemoveAt(index);
            utilitySettings = actionUtilitySettings[index];
            actionUtilitySettings.RemoveAt(index);
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
            actions.Insert(index, action);
            actionEnabled.Insert(index, enabled);
            actionIds.Insert(index, CreateActionId());
            actionUtilitySettings.Insert(index, utilitySettings);
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

            IAction action = actions[sourceIndex];
            bool enabled = actionEnabled[sourceIndex];
            string actionId = actionIds[sourceIndex];
            InvokeActionUtilitySettings utilitySettings = actionUtilitySettings[sourceIndex];
            actions.RemoveAt(sourceIndex);
            actionEnabled.RemoveAt(sourceIndex);
            actionIds.RemoveAt(sourceIndex);
            actionUtilitySettings.RemoveAt(sourceIndex);
            actions.Insert(destinationIndex, action);
            actionEnabled.Insert(destinationIndex, enabled);
            actionIds.Insert(destinationIndex, actionId);
            actionUtilitySettings.Insert(destinationIndex, utilitySettings);
            return true;
        }

        public string GetActionId(int actionIndex)
        {
            EnsureActionMetadata();
            return actionIndex >= 0 && actionIndex < actionIds.Count
                ? actionIds[actionIndex]
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
                actions[actionIndex] is not IActionProgressProvider progressProvider)
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
                actions = new List<IAction>();
                metadataChanged = true;
            }

            if (actionEnabled == null)
            {
                actionEnabled = new List<bool>();
                metadataChanged = true;
            }

            if (actionIds == null)
            {
                actionIds = new List<string>();
                metadataChanged = true;
            }

            if (actionUtilitySettings == null)
            {
                actionUtilitySettings = new List<InvokeActionUtilitySettings>();
                metadataChanged = true;
            }

            while (actionEnabled.Count < actions.Count)
            {
                actionEnabled.Add(true);
                metadataChanged = true;
            }

            while (actionIds.Count < actions.Count)
            {
                actionIds.Add(CreateActionId());
                metadataChanged = true;
            }

            while (actionUtilitySettings.Count < actions.Count)
            {
                actionUtilitySettings.Add(new InvokeActionUtilitySettings(0f, false));
                metadataChanged = true;
            }

            if (actionEnabled.Count > actions.Count)
            {
                actionEnabled.RemoveRange(actions.Count, actionEnabled.Count - actions.Count);
                metadataChanged = true;
            }

            if (actionIds.Count > actions.Count)
            {
                actionIds.RemoveRange(actions.Count, actionIds.Count - actions.Count);
                metadataChanged = true;
            }

            if (actionUtilitySettings.Count > actions.Count)
            {
                actionUtilitySettings.RemoveRange(
                    actions.Count,
                    actionUtilitySettings.Count - actions.Count);
                metadataChanged = true;
            }

            for (int actionIndex = 0; actionIndex < actionIds.Count; actionIndex++)
            {
                if (string.IsNullOrEmpty(actionIds[actionIndex]))
                {
                    actionIds[actionIndex] = CreateActionId();
                    metadataChanged = true;
                }

                InvokeActionUtilitySettings settings = actionUtilitySettings[actionIndex];
                if (settings.MigrateWeightOverride())
                {
                    actionUtilitySettings[actionIndex] = settings;
                    metadataChanged = true;
                }
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
            int lastExecutedActionIndex = actionIds.IndexOf(lastExecutedActionId);
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
                int actionIndex = actionIds.IndexOf(targetActionId);
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
                    actions[capturedIndex],
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
            if (lastExecutedActionIndex >= 0 && lastExecutedActionIndex < actionIds.Count)
            {
                lastExecutedActionId = actionIds[lastExecutedActionIndex];
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
                return actions[0] != null ? actions[0].GetType().Name : "None";
            }

            return $"{actions.Count} Actions";
        }

        public override bool OpenBlock()
        {
            if (actions == null)
            {
                return false;
            }

            foreach (IAction action in actions)
            {
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

            foreach (IAction action in actions)
            {
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
            foreach (InvokeActionUtilitySettings settings in actionUtilitySettings)
            {
                if (settings.HasReference(variable))
                {
                    return true;
                }
            }

            foreach (IAction action in actions)
            {
                if (action is ActionBase actionBase && actionBase.HasReference(variable))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
