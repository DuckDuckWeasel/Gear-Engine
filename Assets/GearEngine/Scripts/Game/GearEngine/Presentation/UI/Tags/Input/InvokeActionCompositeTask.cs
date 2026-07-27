using System;
using GearEngine.Core.Actions;
using Scaffold;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    public sealed class InvokeActionCompositeTask : ICompositeTask
    {
        private readonly IAction action;
        private readonly Func<bool> isEnabled;
        private readonly Func<float> getUtility;
        private readonly Func<float> getWeight;
        private readonly Func<bool> isBlockedDuringExecution;
        private readonly Action<IAction> prepareAction;

        public InvokeActionCompositeTask(
            IAction action,
            Func<bool> isEnabled,
            Func<float> getUtility,
            Func<float> getWeight,
            Func<bool> isBlockedDuringExecution,
            Action<IAction> prepareAction)
        {
            this.action = action;
            this.isEnabled = isEnabled ?? throw new ArgumentNullException(nameof(isEnabled));
            this.getUtility = getUtility ?? throw new ArgumentNullException(nameof(getUtility));
            this.getWeight = getWeight ?? throw new ArgumentNullException(nameof(getWeight));
            this.isBlockedDuringExecution = isBlockedDuringExecution ??
                throw new ArgumentNullException(nameof(isBlockedDuringExecution));
            this.prepareAction = prepareAction ?? throw new ArgumentNullException(nameof(prepareAction));
        }

        public bool IsEnabled => action != null && isEnabled();

        public float Utility => getUtility();

        public float Weight => getWeight();

        public bool BlockDuringExecution => isBlockedDuringExecution();

        public void Execute(Action<CompositeExecutionStatus> onComplete)
        {
            prepareAction(action);
            if (action is IActionWithStatus actionWithStatus)
            {
                actionWithStatus.ExecuteWithStatus(status => onComplete(ConvertStatus(status)));
                return;
            }

            action.Execute(() => onComplete(CompositeExecutionStatus.Success));
        }

        public void Interrupt()
        {
            if (action is IInterruptibleAction interruptibleAction)
            {
                interruptibleAction.Interrupt();
            }
        }

        private static CompositeExecutionStatus ConvertStatus(ActionExecutionStatus status)
        {
            return status == ActionExecutionStatus.Success
                ? CompositeExecutionStatus.Success
                : CompositeExecutionStatus.Failure;
        }
    }
}
