using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Scaffold;
using Scaffold.VisualScripting;
using UnityEngine;
using Blackboard = Scaffold.Blackboard;
using Block = Scaffold.Block;
using CoreActionExecutionStatus =
    Scaffold.VisualScripting.ActionExecutionStatus;

namespace GearEngine.Core.Actions
{
    [Serializable]
    [TriInspector.DrawWithTriInspector]
    public abstract class ActionBase :
        Scaffold.VisualScripting.ActionBase,
        IAction,
        IActionWithStatus,
        IInterruptibleAction,
        IMonoBehaviourConsumer,
        IBlackboardConsumer,
        ICommandContextConsumer,
        IStringLocationIdentifier
    {
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

        protected bool CanRunScheduledWork =>
            IsExecutionActive ||
            blackboard != null ||
            host != null ||
            hostCommand != null;

        protected float CurrentDeltaTime =>
            IsExecutionActive ? Context.TimeSource.DeltaTime : Time.deltaTime;

        protected double CurrentElapsedSeconds =>
            IsExecutionActive ? Context.TimeSource.ElapsedSeconds : Time.timeAsDouble;

        [NonSerialized, BlackboardTransient]
        protected MonoBehaviour host;

        [NonSerialized, BlackboardTransient]
        protected Blackboard blackboard;

        [NonSerialized, BlackboardTransient]
        protected Command hostCommand;

        [NonSerialized, BlackboardTransient]
        protected Action onCompleteCallback;

        [NonSerialized, BlackboardTransient]
        protected Action<ActionExecutionStatus> onStatusCompleteCallback;

        protected List<Scaffold.Variable> referencedVariables =
            new List<Scaffold.Variable>();

        [NonSerialized, BlackboardTransient]
        private bool actionCompleted;

        [NonSerialized, BlackboardTransient]
        private List<IDisposable> scheduledWork;

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
            onCompleteCallback = onComplete;
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

        protected sealed override void OnExecute()
        {
            OnEnter();
        }

        public virtual void OnEnter()
        {
            Continue();
        }

        public virtual void Continue()
        {
            CompleteAction(ActionExecutionStatus.Success);
        }

        public new virtual void Fail()
        {
            CompleteAction(ActionExecutionStatus.Failure);
        }

        public override void Interrupt()
        {
            CancelScheduledWork();
            if (IsExecutionActive)
            {
                OnStopExecuting();
                base.Interrupt();
                return;
            }

            if (actionCompleted)
            {
                return;
            }

            actionCompleted = true;
            onCompleteCallback = null;
            onStatusCompleteCallback = null;
            OnStopExecuting();
        }

        public virtual void Continue(int nextCommandIndex)
        {
            if (IsExecutionActive)
            {
                JumpTo(nextCommandIndex);
                return;
            }

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

        public virtual void StopParentBlock()
        {
            if (IsExecutionActive)
            {
                StopBlock();
                return;
            }

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

        public virtual string GetSummary()
        {
            return "";
        }

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
            if (IsExecutionActive)
            {
                CompleteCoreAction(status);
                return;
            }

            CompleteLegacyAction(status);
        }

        private void CompleteCoreAction(ActionExecutionStatus status)
        {
            CancelScheduledWork();
            Complete(ToCoreStatus(status));
        }

        private void CompleteLegacyAction(ActionExecutionStatus status)
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
            CancelScheduledWork();
            actionCompleted = true;
            onCompleteCallback = null;
            onStatusCompleteCallback = null;
        }

        protected void Invoke(string methodName, float delay)
        {
            if (IsExecutionActive)
            {
                ScheduleCoreInvocation(methodName, delay);
                return;
            }

            RunRoutine(InvokeCoroutine(methodName, delay));
        }

        private IEnumerator InvokeCoroutine(string methodName, float delay)
        {
            yield return new WaitForSeconds(delay);
            InvokeScheduledMethod(methodName);
        }

        protected IDisposable RunRoutine(IEnumerator routine, bool detached = false)
        {
            if (routine == null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            IDisposable handle = IsExecutionActive ? Context.Scheduler.ScheduleRoutine(routine) : StartLegacyRoutine(routine);
            if (!detached)
            {
                GetScheduledWork().Add(handle);
            }

            return handle;
        }

        private void ScheduleCoreInvocation(string methodName, float delay)
        {
            TimeSpan duration = TimeSpan.FromSeconds(Math.Max(delay, 0f));
            IDisposable handle = Schedule(duration, () => InvokeScheduledMethod(methodName));
            GetScheduledWork().Add(handle);
        }

        private IDisposable StartLegacyRoutine(IEnumerator routine)
        {
            MonoBehaviour runner = GetLegacyRoutineRunner();
            if (runner == null)
            {
                throw new InvalidOperationException("No legacy coroutine runner is available.");
            }

            Coroutine coroutine = runner.StartCoroutine(routine);
            return new LegacyCoroutineHandle(runner, coroutine);
        }

        private MonoBehaviour GetLegacyRoutineRunner()
        {
            if (blackboard != null)
            {
                return blackboard;
            }

            if (host != null)
            {
                return host;
            }

            return hostCommand;
        }

        private void InvokeScheduledMethod(string methodName)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            MethodInfo method = GetType().GetMethod(methodName, flags);
            method?.Invoke(this, null);
        }

        private void CancelScheduledWork()
        {
            if (scheduledWork == null)
            {
                return;
            }

            foreach (IDisposable handle in scheduledWork)
            {
                handle.Dispose();
            }

            scheduledWork.Clear();
        }

        private List<IDisposable> GetScheduledWork()
        {
            if (scheduledWork == null)
            {
                scheduledWork = new List<IDisposable>();
            }

            return scheduledWork;
        }

        public virtual bool IsReorderableArray(string propertyName) { return false; }
        protected virtual void RefreshVariableCache() { }
        public virtual bool IsPropertyVisible(string propertyName) { return true; }
        public virtual void OnValidate() { }
        public virtual void GetConnectedBlocks(ref System.Collections.Generic.List<Block> connectedBlocks) { }
        public virtual string GetLocationIdentifier() { return GetType().Name; }

        private static CoreActionExecutionStatus ToCoreStatus(ActionExecutionStatus status)
        {
            return status == ActionExecutionStatus.Success ? CoreActionExecutionStatus.Success : CoreActionExecutionStatus.Failure;
        }
    }
}
