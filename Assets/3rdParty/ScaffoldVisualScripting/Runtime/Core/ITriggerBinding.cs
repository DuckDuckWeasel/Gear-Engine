using System;

namespace Scaffold.VisualScripting
{
    public interface ITriggerBinding : IDisposable
    {
        bool IsEnabled { get; }

        void Enable();

        void Disable();

        void Tick();
    }
}
