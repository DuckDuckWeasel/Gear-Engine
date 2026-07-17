using System.Collections.Generic;
using Scaffold;
using UnityEngine;
using GearEngine.Core.Actions;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    /// <summary>
    /// A generic wrapper command for Scaffold that executes decoupled pure C# IActions.
    /// Supports a list of actions executed sequentially within a single Scaffold block.
    /// </summary>
    [CommandInfo("Generic", "Invoke Action", "Executes a sequence of pure C# actions independent of MonoBehaviours.")]
    [AddComponentMenu("")]
    public class InvokeActionCommand : Command
    {
        [Tooltip("The pure C# actions to execute sequentially.")]
        [SerializeReference]
        public List<IAction> actions = new List<IAction>();

        private int currentActionIndex = 0;
        private bool isExecutingSequence = false;

        public override void OnEnter()
        {
            if (actions == null || actions.Count == 0)
            {
                Continue();
                return;
            }

            currentActionIndex = 0;
            isExecutingSequence = true;
            ExecuteNextAction();
        }

        private void ExecuteNextAction()
        {
            if (!isExecutingSequence) return;

            if (currentActionIndex >= actions.Count)
            {
                isExecutingSequence = false;
                Continue();
                return;
            }

            IAction currentAction = actions[currentActionIndex];
            
            if (currentAction == null)
            {
                Debug.LogWarning($"[InvokeActionCommand] Null action found at index {currentActionIndex}. Skipping.");
                OnActionComplete();
                return;
            }

            // Inject contexts if needed
            if (currentAction is IMonoBehaviourConsumer mbConsumer)
            {
                mbConsumer.SetHost(this);
            }
            if (currentAction is IFlowchartConsumer fcConsumer)
            {
                fcConsumer.SetFlowchart(GetFlowchart());
            }
            if (currentAction is ICommandContextConsumer ccConsumer)
            {
                ccConsumer.SetCommandContext(this);
            }

            try
            {
                currentAction.Execute(OnActionComplete);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InvokeActionCommand] Exception during action execution: {e}");
                OnActionComplete();
            }
        }

        private void OnActionComplete()
        {
            if (!isExecutingSequence) return;
            
            currentActionIndex++;
            ExecuteNextAction();
        }
        
        /// <summary>
        /// Allows an IAction (e.g. flow control) to cancel the internal execution of this sequence
        /// so it can delegate jumping to the parent Block.
        /// </summary>
        public void CancelSequence()
        {
            isExecutingSequence = false;
        }

        public override string GetSummary()
        {
            if (actions == null || actions.Count == 0)
                return "Empty";
                
            if (actions.Count == 1)
                return actions[0] != null ? actions[0].GetType().Name : "None";
                
            return $"{actions.Count} Actions";
        }
    }
}
