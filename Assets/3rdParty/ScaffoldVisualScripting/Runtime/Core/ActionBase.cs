using System;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public abstract class ActionBase : ActionDefinition
    {
        protected ActionExecutionContext Context => context ?? throw new InvalidOperationException("The action is not executing.");

        protected bool IsExecutionActive => !completed && completion != null;

        [BlackboardTransient] private ActionExecutionContext context;
        [BlackboardTransient] private Action<ActionExecutionStatus> completion;
        [BlackboardTransient] private bool completed;

        public sealed override void Execute(ActionExecutionContext executionContext, Action<ActionExecutionStatus> onComplete)
        {
            context = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
            completion = onComplete ?? throw new ArgumentNullException(nameof(onComplete));
            completed = false;
            OnExecute();
        }

        public override void Interrupt()
        {
            if (!IsExecutionActive)
            {
                return;
            }

            OnInterrupted();
            Complete(ActionExecutionStatus.Interrupted);
        }

        protected abstract void OnExecute();

        protected virtual void OnInterrupted()
        {
        }

        protected void JumpTo(int actionIndex)
        {
            Context.FlowController.JumpTo(actionIndex);
            Succeed();
        }

        protected void Succeed()
        {
            Complete(ActionExecutionStatus.Success);
        }

        protected void Fail()
        {
            Complete(ActionExecutionStatus.Failure);
        }

        protected void StopBlock()
        {
            Context.FlowController.StopBlock();
            Complete(ActionExecutionStatus.Interrupted);
        }

        protected IDisposable Schedule(TimeSpan delay, Action callback)
        {
            return Context.Scheduler.Schedule(delay, callback);
        }

        protected IDisposable ScheduleNextFrame(Action callback)
        {
            return Context.Scheduler.ScheduleNextFrame(callback);
        }

        protected void Complete(ActionExecutionStatus status)
        {
            if (!IsExecutionActive)
            {
                return;
            }

            completed = true;
            Action<ActionExecutionStatus> callback = completion;
            completion = null;
            context = null;
            callback.Invoke(status);
        }
    }
}
