using System;

namespace Scaffold.VisualScripting
{
    public interface IAction : IDefinitionNode
    {
        void Execute(ActionExecutionContext context, Action<ActionExecutionStatus> onComplete);

        void Interrupt();
    }
}
