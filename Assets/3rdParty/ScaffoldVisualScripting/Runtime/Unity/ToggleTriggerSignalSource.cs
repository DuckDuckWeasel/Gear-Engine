using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scaffold.VisualScripting.Unity
{
    [Serializable]
    public sealed class ToggleTriggerSignalSource : ITriggerSignalSource
    {
        public Toggle Target
        {
            get => target;
            set => target = value;
        }

        [SerializeField] private Toggle target;

        public IDisposable Subscribe(Action<object> handler)
        {
            if (target == null)
            {
                throw new InvalidOperationException("A Toggle trigger source requires a target.");
            }

            UnityAction<bool> listener = value => handler?.Invoke(value);
            target.onValueChanged.AddListener(listener);
            return new UnityListenerSubscription(() => RemoveListener(listener));
        }

        private void RemoveListener(UnityAction<bool> listener)
        {
            if (target != null)
            {
                target.onValueChanged.RemoveListener(listener);
            }
        }
    }
}
