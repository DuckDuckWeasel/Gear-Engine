using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GearEngine.Core.Architecture.References;
using Scaffold.VisualScripting;
using UnityEngine;
using CoreActionExecutionStatus =
    Scaffold.VisualScripting.ActionExecutionStatus;

namespace GearEngine.Core.Actions
{
    [Serializable]
    [TriInspector.DrawWithTriInspector]
    public abstract class ActionBase :
        Scaffold.VisualScripting.ActionBase,
        Scaffold.IStringLocationIdentifier
    {
        public int ItemId => GetStableItemId(DefinitionId.Value);

        public virtual string ErrorMessage => string.Empty;

        public virtual int IndentLevel
        {
            get => indentLevel;
            set => indentLevel = Math.Max(value, 0);
        }

        [SerializeField] private int indentLevel;

        public int CommandIndex => IsExecutionActive ? Context.ActionIndex : -1;

        public int PreviousCommandIndex =>
            IsExecutionActive ? Context.PreviousActionIndex : -1;

        public IReadOnlyList<Scaffold.VisualScripting.IAction> CurrentActions =>
            IsExecutionActive
                ? Context.ActionList.Definition.Actions
                : Array.Empty<Scaffold.VisualScripting.IAction>();

        public bool IsExecuting => IsExecutionActive;

        public Scaffold.VisualScripting.Block ParentBlock =>
            IsExecutionActive ? Context.Block : null;

        public ActionTrack ParentTrack =>
            IsExecutionActive ? Context.Track : null;

        protected bool CanRunScheduledWork => IsExecutionActive;

        protected float CurrentDeltaTime => Context.TimeSource.DeltaTime;

        protected double CurrentElapsedSeconds => Context.TimeSource.ElapsedSeconds;

        protected List<Scaffold.Variable> referencedVariables =
            new List<Scaffold.Variable>();

        protected ITargetResolver TargetResolver =>
            IsExecutionActive
                ? new BlackboardTargetResolver(Context.Blackboard.Variables)
                : null;

        protected GameObject ResolveTarget(TargetReference targetReference)
        {
            return targetReference?.Resolve(TargetResolver);
        }

        protected IReadOnlyList<GameObject> ResolveTargets(
            TargetReference targetReference)
        {
            IReadOnlyList<GameObject> targets =
                targetReference?.ResolveAll(TargetResolver);
            return targets ?? Array.Empty<GameObject>();
        }

        protected bool IsTargetMatch(
            TargetReference targetReference,
            GameObject target)
        {
            return targetReference != null &&
                targetReference.IsMatch(target, TargetResolver);
        }

        [NonSerialized, BlackboardTransient]
        private List<IDisposable> scheduledWork;

        private sealed class BlackboardTargetResolver : ITargetResolver
        {
            public BlackboardTargetResolver(BlackboardVariableSet variables)
            {
                this.variables = variables ??
                    throw new ArgumentNullException(nameof(variables));
            }

            private readonly BlackboardVariableSet variables;

            public GameObject Resolve(string variableName)
            {
                if (!variables.TryGet(
                        variableName,
                        out VariableCellBase cell))
                {
                    return null;
                }

                return cell is VariableCell<GameObject> gameObjectCell
                    ? gameObjectCell.Value
                    : null;
            }
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
            CompleteAction(CoreActionExecutionStatus.Success);
        }

        public new virtual void Fail()
        {
            CompleteAction(CoreActionExecutionStatus.Failure);
        }

        public override void Interrupt()
        {
            CancelScheduledWork();
            if (!IsExecutionActive)
            {
                return;
            }

            OnStopExecuting();
            base.Interrupt();
        }

        public virtual void Continue(int nextCommandIndex)
        {
            JumpTo(nextCommandIndex);
        }

        public virtual void StopParentBlock()
        {
            StopBlock();
        }

        public Blackboard GetBlackboard()
        {
            return Context.Blackboard;
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
            return string.Empty;
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

        public virtual void OnCommandAdded(BlockDefinition parentBlock)
        {
        }

        public virtual void OnCommandRemoved(BlockDefinition parentBlock)
        {
        }

        public virtual bool HasReference(Scaffold.Variable variable)
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

        public virtual bool IsReorderableArray(string propertyName)
        {
            return false;
        }

        protected virtual void RefreshVariableCache()
        {
        }

        public virtual bool IsPropertyVisible(string propertyName)
        {
            return true;
        }

        public virtual void OnValidate()
        {
        }

        public virtual void GetConnectedBlocks(ref List<BlockDefinition> connectedBlocks)
        {
        }

        public virtual string GetLocationIdentifier()
        {
            return GetType().Name;
        }

        protected virtual void CompleteAction(CoreActionExecutionStatus status)
        {
            CancelScheduledWork();
            Complete(status);
        }

        protected void Invoke(string methodName, float delay)
        {
            TimeSpan duration = TimeSpan.FromSeconds(Math.Max(delay, 0f));
            IDisposable handle = Schedule(
                duration,
                () => InvokeScheduledMethod(methodName));
            GetScheduledWork().Add(handle);
        }

        protected IDisposable RunRoutine(IEnumerator routine, bool detached = false)
        {
            if (routine == null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            IDisposable handle = Context.Scheduler.ScheduleRoutine(routine);
            if (!detached)
            {
                GetScheduledWork().Add(handle);
            }

            return handle;
        }

        protected void RunTask(
            Func<System.Threading.Tasks.Task> operation,
            string operationName)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            try
            {
                System.Threading.Tasks.Task task = operation.Invoke();
                if (task == null)
                {
                    throw new InvalidOperationException(
                        $"{operationName} returned a null Task.");
                }

                RunRoutine(WaitForTask(task, operationName));
            }
            catch (Exception exception)
            {
                ReportTaskFailure(operationName, exception);
                Fail();
            }
        }

        private IEnumerator WaitForTask(
            System.Threading.Tasks.Task task,
            string operationName)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsCanceled)
            {
                InvalidOperationException exception =
                    new InvalidOperationException(
                        $"{operationName} was canceled.");
                ReportTaskFailure(operationName, exception);
                Fail();
                yield break;
            }

            if (task.IsFaulted)
            {
                Exception exception =
                    task.Exception?.GetBaseException() ??
                    new InvalidOperationException(
                        $"{operationName} failed without an exception.");
                ReportTaskFailure(operationName, exception);
                Fail();
                yield break;
            }

            Continue();
        }

        private void ReportTaskFailure(
            string operationName,
            Exception exception)
        {
            string message =
                $"[{GetType().Name}] {operationName} failed: {exception}";
            Context.Logger.Error(message, exception);
            Debug.LogError(message);
        }

        private static int GetStableItemId(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int index = 0; index < value.Length; index++)
                {
                    hash = hash * 31 + value[index];
                }

                return hash;
            }
        }

        private void InvokeScheduledMethod(string methodName)
        {
            const BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public;
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
    }
}
