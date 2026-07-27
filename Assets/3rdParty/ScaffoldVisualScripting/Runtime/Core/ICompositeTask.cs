using System;

namespace Scaffold.VisualScripting
{
    internal interface ICompositeTask
    {
        bool IsEnabled { get; }

        float Utility { get; }

        float Weight { get; }

        bool HasWeightOverride { get; }

        bool BlockDuringExecution { get; }

        void Execute(Action<ActionExecutionStatus> onComplete);

        void Interrupt();
    }
}
