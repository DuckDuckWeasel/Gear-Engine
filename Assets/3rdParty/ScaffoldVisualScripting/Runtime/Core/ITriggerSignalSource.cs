using System;

namespace Scaffold.VisualScripting
{
    public interface ITriggerSignalSource
    {
        IDisposable Subscribe(Action<object> handler);
    }
}
