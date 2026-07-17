using System;
using Scaffold;
using UnityEngine;

namespace GearEngine.Core.Actions
{
    /// <summary>
    /// A base class to ease the migration of Scaffold Commands to IActions.
    /// It implements the necessary consumers and provides legacy methods like Continue() 
    /// and GetFlowchart() so that existing command logic can remain mostly untouched.
    /// </summary>
    [Serializable]
    public abstract class ActionBase : IAction, IMonoBehaviourConsumer, IFlowchartConsumer, ICommandContextConsumer, IStringLocationIdentifier
    {
        protected MonoBehaviour host;
        protected Flowchart flowchart;
        protected Command hostCommand;
        protected Action onCompleteCallback;

        // Legacy properties delegating to the host Command
        public virtual int ItemId 
        { 
            get { return hostCommand != null ? hostCommand.ItemId : -1; } 
            set { if (hostCommand != null) hostCommand.ItemId = value; } 
        }
        
        public virtual string ErrorMessage 
        { 
            get { return hostCommand != null ? hostCommand.ErrorMessage : ""; } 
        }
        
        public virtual int IndentLevel 
        { 
            get { return hostCommand != null ? hostCommand.IndentLevel : 0; } 
            set { if (hostCommand != null) hostCommand.IndentLevel = value; } 
        }
        
        public virtual int CommandIndex 
        { 
            get { return hostCommand != null ? hostCommand.CommandIndex : 0; } 
            set { if (hostCommand != null) hostCommand.CommandIndex = value; } 
        }
        
        public virtual bool IsExecuting 
        { 
            get { return hostCommand != null ? hostCommand.IsExecuting : false; } 
            set { if (hostCommand != null) hostCommand.IsExecuting = value; } 
        }
        
        public virtual Block ParentBlock 
        { 
            get { return hostCommand != null ? hostCommand.ParentBlock : null; } 
            set { if (hostCommand != null) hostCommand.ParentBlock = value; } 
        }

        public virtual void SetHost(MonoBehaviour host)
        {
            this.host = host;
        }

        public virtual void SetFlowchart(Flowchart flowchart)
        {
            this.flowchart = flowchart;
        }

        public virtual void SetCommandContext(Command hostCommand)
        {
            this.hostCommand = hostCommand;
        }

        public void Execute(Action onComplete)
        {
            this.onCompleteCallback = onComplete;
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
            onCompleteCallback?.Invoke();
        }

        /// <summary>
        /// Equivalent to Scaffold Command.Continue(int).
        /// Instructs the host InvokeActionCommand to cancel its internal sequence 
        /// and delegates the jump to the parent Block.
        /// </summary>
        public virtual void Continue(int nextCommandIndex)
        {
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
            if (hostCommand is global::GearEngine.GearEngine.Presentation.UI.Input.InvokeActionCommand invokeCmd)
            {
                invokeCmd.CancelSequence();
            }
            if (hostCommand != null && hostCommand.ParentBlock != null)
            {
                hostCommand.ParentBlock.Stop();
            }
        }

        public virtual Flowchart GetFlowchart()
        {
            return flowchart;
        }

        /// <summary>
        /// Legacy summary method. Can be used if we ever need a custom summary drawer.
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
        
        protected System.Collections.Generic.List<Scaffold.Variable> referencedVariables = new System.Collections.Generic.List<Scaffold.Variable>();
        
        protected void Invoke(string methodName, float delay)
        {
            if (flowchart != null)
            {
                flowchart.StartCoroutine(InvokeCoroutine(methodName, delay));
            }
        }
        
        private System.Collections.IEnumerator InvokeCoroutine(string methodName, float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            var method = GetType().GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (method != null)
            {
                method.Invoke(this, null);
            }
        }
        
        // Stubs to fix compilation errors in migrated classes
        public virtual bool IsReorderableArray(string propertyName) { return false; }
        protected virtual void RefreshVariableCache() {}
        public virtual bool IsPropertyVisible(string propertyName) { return true; }
        public virtual void OnValidate() {}
        public virtual void GetConnectedBlocks(ref System.Collections.Generic.List<Block> connectedBlocks) {}
        public virtual string GetLocationIdentifier() { return GetType().Name; }
    }
}
