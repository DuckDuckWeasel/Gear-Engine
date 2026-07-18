using System.Collections.Generic;
using Scaffold;
using UnityEngine;
using GearEngine.Core.Actions;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    /// <summary>
    /// Defines how an Invoke Action command starts its nested actions.
    /// </summary>
    public enum InvokeActionExecutionMethod
    {
        /// <summary> Runs one action after another. </summary>
        Sequence,
        /// <summary> Starts every action together and completes after all of them finish. </summary>
        AllAtSameTime,
    }

    /// <summary>
    /// A generic wrapper command for Scaffold that executes decoupled pure C# IActions.
    /// Supports a list of actions executed in sequence or together within a single Scaffold block.
    /// </summary>
    [CommandInfo("Generic", "Invoke Action", "Executes pure C# actions independently of MonoBehaviours.")]
    [AddComponentMenu("")]
    public class InvokeActionCommand : Command
    {
        [Tooltip("The pure C# actions to execute.")]
        [SerializeReference]
        public List<IAction> actions = new List<IAction>();

        [SerializeField] private List<bool> actionEnabled = new List<bool>();

        [SerializeField] private bool displayAsGroup;

        [Tooltip("Controls whether nested actions run one after another or together.")]
        [SerializeField] private InvokeActionExecutionMethod executionMethod = InvokeActionExecutionMethod.Sequence;

        private int _currentActionIndex;
        private int _startedActionCount;
        private int _completedActionCount;
        private bool _isStartingActions;
        private bool _completionRequested;
        private bool _isExecutingActions;

        public InvokeActionExecutionMethod ExecutionMethod
        {
            get => executionMethod;
            set => executionMethod = value;
        }

        public bool DisplayAsGroup
        {
            get => displayAsGroup;
            set => displayAsGroup = value;
        }

        public bool IsActionEnabled(int index)
        {
            return index >= 0 && index < actions.Count &&
                   (index >= actionEnabled.Count || actionEnabled[index]);
        }

        public void SetActionEnabled(int index, bool enabled)
        {
            EnsureActionEnabledStates();
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

            EnsureActionEnabledStates();
            var sourceActions = new List<IAction>(actions);
            var sourceEnabledStates = new List<bool>(actionEnabled);
            for (int i = 0; i < sourceIndices.Count; i++)
            {
                int sourceIndex = sourceIndices[i];
                if (sourceIndex < 0 || sourceIndex >= sourceActions.Count)
                {
                    return;
                }

                actions[i] = sourceActions[sourceIndex];
                actionEnabled[i] = sourceEnabledStates[sourceIndex];
            }
        }

        public bool TryRemoveAction(int index, out IAction action, out bool enabled)
        {
            action = null;
            enabled = true;
            EnsureActionEnabledStates();

            if (index < 0 || index >= actions.Count)
            {
                return false;
            }

            action = actions[index];
            enabled = actionEnabled[index];
            if (actions.Count > 1)
            {
                displayAsGroup = true;
            }
            actions.RemoveAt(index);
            actionEnabled.RemoveAt(index);
            return true;
        }

        public void InsertAction(int index, IAction action, bool enabled)
        {
            EnsureActionEnabledStates();
            index = Mathf.Clamp(index, 0, actions.Count);
            actions.Insert(index, action);
            actionEnabled.Insert(index, enabled);
            if (actions.Count > 1)
            {
                displayAsGroup = true;
            }
        }

        public bool TryMoveAction(int sourceIndex, int destinationIndex)
        {
            EnsureActionEnabledStates();
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
            actions.RemoveAt(sourceIndex);
            actionEnabled.RemoveAt(sourceIndex);
            actions.Insert(destinationIndex, action);
            actionEnabled.Insert(destinationIndex, enabled);
            return true;
        }

        public override void OnEnter()
        {
            if (actions == null || actions.Count == 0)
            {
                Continue();
                return;
            }

            _currentActionIndex = 0;
            _startedActionCount = 0;
            _completedActionCount = 0;
            _completionRequested = false;
            _isExecutingActions = true;
            EnsureActionEnabledStates();

            if (executionMethod == InvokeActionExecutionMethod.Sequence)
            {
                ExecuteNextAction();
                return;
            }

            _isStartingActions = true;
            for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                ExecuteAction(actions[actionIndex], actionIndex);
            }
            _isStartingActions = false;

            if (_startedActionCount == 0 ||
                _completionRequested)
            {
                CompleteActions();
            }
        }

        private void ExecuteNextAction()
        {
            if (!_isExecutingActions)
            {
                return;
            }

            if (_currentActionIndex >= actions.Count)
            {
                CompleteActions();
                return;
            }

            ExecuteAction(actions[_currentActionIndex], _currentActionIndex);
        }

        private void ExecuteAction(IAction action, int actionIndex)
        {
            if (action == null || !IsActionEnabled(actionIndex))
            {
                if (action == null)
                {
                    Debug.LogWarning("[InvokeActionCommand] Null action found. Skipping.");
                }
                OnActionComplete();
                return;
            }

            _startedActionCount++;

            // Inject contexts if needed
            if (action is IMonoBehaviourConsumer mbConsumer)
            {
                mbConsumer.SetHost(this);
            }
            if (action is IFlowchartConsumer fcConsumer)
            {
                fcConsumer.SetFlowchart(GetFlowchart());
            }
            if (action is ICommandContextConsumer ccConsumer)
            {
                ccConsumer.SetCommandContext(this);
            }

            try
            {
                action.Execute(OnActionComplete);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InvokeActionCommand] Exception during action execution: {e}");
                OnActionComplete();
            }
        }

        private void OnActionComplete()
        {
            if (!_isExecutingActions)
            {
                return;
            }

            _completedActionCount++;

            if (executionMethod == InvokeActionExecutionMethod.Sequence)
            {
                _currentActionIndex++;
                ExecuteNextAction();
                return;
            }

            bool shouldComplete = _completedActionCount >= _startedActionCount;
            if (!shouldComplete)
            {
                return;
            }

            if (_isStartingActions)
            {
                _completionRequested = true;
                return;
            }

            CompleteActions();
        }

        private void CompleteActions()
        {
            if (!_isExecutingActions)
            {
                return;
            }

            _isExecutingActions = false;
            Continue();
        }

        /// <summary>
        /// Allows an IAction (e.g. flow control) to cancel the internal execution of this sequence
        /// so it can delegate jumping to the parent Block.
        /// </summary>
        public void CancelSequence()
        {
            _isExecutingActions = false;
        }

        private void EnsureActionEnabledStates()
        {
            if (actionEnabled == null)
            {
                actionEnabled = new List<bool>();
            }

            while (actionEnabled.Count < actions.Count)
            {
                actionEnabled.Add(true);
            }

            if (actionEnabled.Count > actions.Count)
            {
                actionEnabled.RemoveRange(actions.Count, actionEnabled.Count - actions.Count);
            }
        }

        public override string GetSummary()
        {
            if (actions == null || actions.Count == 0)
                return "Empty";

            if (actions.Count == 1)
                return actions[0] != null ? actions[0].GetType().Name : "None";

            return $"{actions.Count} Actions";
        }

        public override bool OpenBlock()
        {
            if (actions == null) return false;
            foreach (var action in actions)
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
            if (actions == null) return false;
            foreach (var action in actions)
            {
                if (action is ActionBase actionBase && actionBase.CloseBlock())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
