using System;
using Scaffold;
using UnityEngine;

namespace GearEngine.Core.Actions
{
    /// <summary>
    /// A base class to ease the migration of Scaffold Commands to IActions.
    /// It implements the necessary consumers and provides legacy methods like Continue() 
    /// and GetBlackboard() so that existing command logic can remain mostly untouched.
    /// </summary>
    [Serializable]
    [TriInspector.DrawWithTriInspector]
    public abstract class ActionBase : IAction, IActionWithStatus, IInterruptibleAction, IMonoBehaviourConsumer, IBlackboardConsumer, ICommandContextConsumer, IStringLocationIdentifier
    {
        protected MonoBehaviour host;
        protected Blackboard blackboard;
        protected Command hostCommand;
        protected Action onCompleteCallback;
        protected Action<ActionExecutionStatus> onStatusCompleteCallback;

        private bool actionCompleted;

        // Legacy properties delegating to the host Command
        public virtual int ItemId
        {
            get { return hostCommand != null ? hostCommand.ItemId : -1; }
            set
            {
                if (hostCommand != null)
                {
                    hostCommand.ItemId = value;
                }
            }
        }

        public virtual string ErrorMessage
        {
            get { return hostCommand != null ? hostCommand.ErrorMessage : ""; }
        }

        public virtual int IndentLevel
        {
            get { return hostCommand != null ? hostCommand.IndentLevel : 0; }
            set
            {
                if (hostCommand != null)
                {
                    hostCommand.IndentLevel = value;
                }
            }
        }

        public virtual int CommandIndex
        {
            get { return hostCommand != null ? hostCommand.CommandIndex : 0; }
            set
            {
                if (hostCommand != null)
                {
                    hostCommand.CommandIndex = value;
                }
            }
        }

        public virtual bool IsExecuting
        {
            get { return hostCommand != null ? hostCommand.IsExecuting : false; }
            set
            {
                if (hostCommand != null)
                {
                    hostCommand.IsExecuting = value;
                }
            }
        }

        public virtual Block ParentBlock
        {
            get { return hostCommand != null ? hostCommand.ParentBlock : null; }
            set
            {
                if (hostCommand != null)
                {
                    hostCommand.ParentBlock = value;
                }
            }
        }

        /// <summary>
        /// The CommandTrack the host Command belongs to. Delegates to the host, same as ParentBlock,
        /// so flow-control actions (If/Else/End, loops, jumps) resolve against the track they actually
        /// run on rather than always the Block's first track.
        /// </summary>
        public virtual CommandTrack ParentTrack
        {
            get { return hostCommand != null ? hostCommand.ParentTrack : null; }
            set
            {
                if (hostCommand != null)
                {
                    hostCommand.ParentTrack = value;
                }
            }
        }

        public virtual void SetHost(MonoBehaviour host)
        {
            this.host = host;
        }

        public virtual void SetBlackboard(Blackboard blackboard)
        {
            this.blackboard = blackboard;
        }

        public virtual void SetCommandContext(Command hostCommand)
        {
            this.hostCommand = hostCommand;
        }

        public void Execute(Action onComplete)
        {
            this.onCompleteCallback = onComplete;
            onStatusCompleteCallback = null;
            actionCompleted = false;
            OnEnter();
        }

        public void ExecuteWithStatus(Action<ActionExecutionStatus> onComplete)
        {
            onCompleteCallback = null;
            onStatusCompleteCallback = onComplete;
            actionCompleted = false;
            OnEnter();
        }

        /// <summary>
        /// Legacy OnEnter from Scaffold. Override this to implement the action's logic.
        /// </summary>
        public virtual void OnEnter()
        {
            Continue();
        }

        /// <summary>
        /// Equivalent to Scaffold Command.Continue(). 
        /// Calls the onComplete callback to advance the InvokeActionCommand sequence.
        /// </summary>
        public virtual void Continue()
        {
            CompleteAction(ActionExecutionStatus.Success);
        }

        /// <summary>
        /// Completes this action with a failure result.
        /// </summary>
        public virtual void Fail()
        {
            CompleteAction(ActionExecutionStatus.Failure);
        }

        /// <summary>
        /// Stops this action without invoking its completion callback. The execution host owns
        /// the result assigned to an interrupted child.
        /// </summary>
        public virtual void Interrupt()
        {
            if (actionCompleted)
            {
                return;
            }

            actionCompleted = true;
            onCompleteCallback = null;
            onStatusCompleteCallback = null;
            OnStopExecuting();
        }

        /// <summary>
        /// Equivalent to Scaffold Command.Continue(int).
        /// Instructs the host InvokeActionCommand to cancel its internal sequence 
        /// and delegates the jump to the parent Block.
        /// </summary>
        public virtual void Continue(int nextCommandIndex)
        {
            CancelCompletionCallback();
            if (hostCommand is global::GearEngine.GearEngine.Presentation.UI.Input.InvokeActionCommand invokeCmd)
            {
                invokeCmd.CancelSequence();
            }
            if (hostCommand != null)
            {
                hostCommand.Continue(nextCommandIndex);
            }
        }

        /// <summary>
        /// Equivalent to Scaffold Command.StopParentBlock().
        /// </summary>
        public virtual void StopParentBlock()
        {
            CancelCompletionCallback();
            if (hostCommand is global::GearEngine.GearEngine.Presentation.UI.Input.InvokeActionCommand invokeCmd)
            {
                invokeCmd.CancelSequence();
            }
            if (hostCommand != null && hostCommand.ParentBlock != null)
            {
                hostCommand.ParentBlock.Stop();
            }
        }

        public virtual Blackboard GetBlackboard()
        {
            return blackboard;
        }

        /// <summary>
        /// Legacy summary method. Can be used if we ever need a custom summary drawer.
        /// </summary>
        public virtual bool IsComment()
        {
            return false;
        }

        public virtual bool IsLabel()
        {
            return false;
        }

        public virtual bool IsWeightEligible()
        {
            return true;
        }

        /// <summary>
        /// Legacy formatting methods
        /// </summary>
        public virtual string GetSummary()
        {
            return "";
        }

        /// <summary>
        /// Legacy formatting methods
        /// </summary>
        public virtual Color GetButtonColor()
        {
            return Color.white;
        }

        public virtual bool OpenBlock()
        {
            return false;
        }

        public virtual bool CloseBlock()
        {
            return false;
        }

        public virtual void OnCommandAdded(Block parentBlock)
        {
        }

        public virtual void OnCommandRemoved(Block parentBlock)
        {
        }

        public virtual bool HasReference(Variable variable)
        {
            return false;
        }

        public virtual void OnStopExecuting()
        {
        }

        public virtual void OnCommandListChanged()
        {
        }

        public virtual void OnReset()
        {
        }

        protected virtual void CompleteAction(ActionExecutionStatus status)
        {
            if (actionCompleted)
            {
                return;
            }

            actionCompleted = true;
            Action completion = onCompleteCallback;
            Action<ActionExecutionStatus> statusCompletion = onStatusCompleteCallback;
            onCompleteCallback = null;
            onStatusCompleteCallback = null;
            completion?.Invoke();
            statusCompletion?.Invoke(status);
        }

        private void CancelCompletionCallback()
        {
            actionCompleted = true;
            onCompleteCallback = null;
            onStatusCompleteCallback = null;
        }

        protected System.Collections.Generic.List<Scaffold.Variable> referencedVariables = new System.Collections.Generic.List<Scaffold.Variable>();

        protected void Invoke(string methodName, float delay)
        {
            if (blackboard != null)
            {
                blackboard.StartCoroutine(InvokeCoroutine(methodName, delay));
            }
        }

        private System.Collections.IEnumerator InvokeCoroutine(string methodName, float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            System.Reflection.MethodInfo method = GetType().GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (method != null)
            {
                method.Invoke(this, null);
            }
        }

        // Stubs to fix compilation errors in migrated classes
        public virtual bool IsReorderableArray(string propertyName) { return false; }
        protected virtual void RefreshVariableCache() { }
        public virtual bool IsPropertyVisible(string propertyName) { return true; }
        public virtual void OnValidate() { }
        public virtual void GetConnectedBlocks(ref System.Collections.Generic.List<Block> connectedBlocks) { }
        public virtual string GetLocationIdentifier() { return GetType().Name; }
    }
}
