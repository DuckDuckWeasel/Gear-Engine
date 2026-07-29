using System;

namespace Scaffold.VisualScripting
{
    public interface IBlackboardEventBus
    {
        IDisposable Subscribe<TEvent>(Action<TEvent> handler);

        void Publish<TEvent>(TEvent eventValue);
    }
}
