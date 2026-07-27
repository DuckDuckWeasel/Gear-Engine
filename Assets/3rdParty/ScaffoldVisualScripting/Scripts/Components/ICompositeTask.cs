using System;

namespace Scaffold
{
    public interface ICompositeTask
    {
        bool IsEnabled { get; }

        float Utility { get; }

        float Weight { get; }

        bool BlockDuringExecution { get; }

        void Execute(Action<CompositeExecutionStatus> onComplete);

        void Interrupt();
    }
}
