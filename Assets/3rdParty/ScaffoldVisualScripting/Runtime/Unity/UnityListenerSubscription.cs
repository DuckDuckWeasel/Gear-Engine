using System;

namespace Scaffold.VisualScripting.Unity
{
    internal sealed class UnityListenerSubscription : IDisposable
    {
        public UnityListenerSubscription(Action detach)
        {
            this.detach = detach ?? throw new ArgumentNullException(nameof(detach));
        }

        private Action detach;

        public void Dispose()
        {
            Action callback = detach;
            detach = null;
            callback?.Invoke();
        }
    }
}
