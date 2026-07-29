using System;

namespace Scaffold.VisualScripting
{
    internal sealed class ActionCompositeTask : ICompositeTask
    {
        public ActionCompositeTask(IAction action, Func<ActionExecutionContext> createContext)
        {
            this.action = action;
            this.createContext = createContext ?? throw new ArgumentNullException(nameof(createContext));
        }

        public bool IsEnabled => action != null && (!(action is IActionMetadata metadata) || metadata.Enabled);

        public float Utility => action is IActionMetadata metadata ? metadata.Utility : 0f;

        public float Weight => action is IActionMetadata metadata ? metadata.Weight : 0f;

        public bool HasWeightOverride => action is IActionMetadata metadata && metadata.HasWeightOverride;

        public bool BlockDuringExecution => action is IActionMetadata metadata && metadata.BlockDuringExecution;

        private readonly IAction action;
        private readonly Func<ActionExecutionContext> createContext;

        public void Execute(Action<ActionExecutionStatus> onComplete)
        {
            if (action == null)
            {
                onComplete.Invoke(ActionExecutionStatus.Failure);
                return;
            }

            ActionExecutionContext context = createContext.Invoke();
            action.Execute(context, onComplete);
        }

        public void Interrupt()
        {
            action?.Interrupt();
        }
    }
}
