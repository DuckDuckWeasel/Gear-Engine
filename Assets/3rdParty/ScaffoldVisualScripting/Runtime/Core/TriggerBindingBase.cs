using System;

namespace Scaffold.VisualScripting
{
    public abstract class TriggerBindingBase : ITriggerBinding
    {
        protected TriggerBindingBase(TriggerExecutionContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool IsEnabled { get; private set; }

        protected TriggerExecutionContext Context { get; }

        private bool disposed;

        public void Enable()
        {
            ThrowIfDisposed();
            if (IsEnabled)
            {
                return;
            }

            OnEnable();
            IsEnabled = true;
        }

        public void Tick()
        {
            ThrowIfDisposed();
            if (IsEnabled)
            {
                OnTick();
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Disable();
            disposed = true;
            OnDispose();
        }

        public void Disable()
        {
            if (disposed || !IsEnabled)
            {
                return;
            }

            IsEnabled = false;
            OnDisable();
        }

        protected bool ExecuteBlock()
        {
            return IsEnabled && Context.ExecuteBlock();
        }

        protected abstract void OnEnable();

        protected abstract void OnDisable();

        protected virtual void OnTick()
        {
        }

        protected virtual void OnDispose()
        {
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(TriggerBindingBase));
            }
        }
    }
}
