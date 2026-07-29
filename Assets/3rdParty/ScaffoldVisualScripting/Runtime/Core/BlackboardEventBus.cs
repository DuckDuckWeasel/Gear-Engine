using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardEventBus : IBlackboardEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> handlers = new Dictionary<Type, List<Delegate>>();

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Type eventType = typeof(TEvent);
            List<Delegate> eventHandlers = GetOrCreateHandlers(eventType);
            eventHandlers.Add(handler);
            return new Subscription(() => Remove(eventType, handler));
        }

        public void Publish<TEvent>(TEvent eventValue)
        {
            Type eventType = typeof(TEvent);
            if (!handlers.TryGetValue(eventType, out List<Delegate> eventHandlers))
            {
                return;
            }

            Delegate[] snapshot = eventHandlers.ToArray();
            foreach (Delegate handler in snapshot)
            {
                ((Action<TEvent>)handler).Invoke(eventValue);
            }
        }

        private List<Delegate> GetOrCreateHandlers(Type eventType)
        {
            if (handlers.TryGetValue(eventType, out List<Delegate> eventHandlers))
            {
                return eventHandlers;
            }

            eventHandlers = new List<Delegate>();
            handlers.Add(eventType, eventHandlers);
            return eventHandlers;
        }

        private void Remove(Type eventType, Delegate handler)
        {
            if (!handlers.TryGetValue(eventType, out List<Delegate> eventHandlers))
            {
                return;
            }

            eventHandlers.Remove(handler);
            if (eventHandlers.Count == 0)
            {
                handlers.Remove(eventType);
            }
        }

        private sealed class Subscription : IDisposable
        {
            public Subscription(Action dispose)
            {
                this.dispose = dispose;
            }

            private Action dispose;

            public void Dispose()
            {
                Action callback = dispose;
                dispose = null;
                callback?.Invoke();
            }
        }
    }
}
