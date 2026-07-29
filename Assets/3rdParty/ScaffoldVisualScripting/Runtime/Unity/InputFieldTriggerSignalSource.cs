using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scaffold.VisualScripting.Unity
{
    [Serializable]
    public sealed class InputFieldTriggerSignalSource : ITriggerSignalSource
    {
        public InputField Target
        {
            get => target;
            set => target = value;
        }

        [SerializeField] private InputField target;

        public IDisposable Subscribe(Action<object> handler)
        {
            if (target == null)
            {
                throw new InvalidOperationException("An InputField trigger source requires a target.");
            }

            UnityAction<string> listener = value => handler?.Invoke(value);
            target.onEndEdit.AddListener(listener);
            return new UnityListenerSubscription(() => RemoveListener(listener));
        }

        private void RemoveListener(UnityAction<string> listener)
        {
            if (target != null)
            {
                target.onEndEdit.RemoveListener(listener);
            }
        }
    }
}
