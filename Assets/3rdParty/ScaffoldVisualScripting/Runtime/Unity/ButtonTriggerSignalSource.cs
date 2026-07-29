using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scaffold.VisualScripting.Unity
{
    [Serializable]
    public sealed class ButtonTriggerSignalSource : ITriggerSignalSource
    {
        public Button Target
        {
            get => target;
            set => target = value;
        }

        [SerializeField] private Button target;

        public IDisposable Subscribe(Action<object> handler)
        {
            if (target == null)
            {
                throw new InvalidOperationException("A Button trigger source requires a target.");
            }

            UnityAction listener = () => handler?.Invoke(null);
            target.onClick.AddListener(listener);
            return new UnityListenerSubscription(() => RemoveListener(listener));
        }

        private void RemoveListener(UnityAction listener)
        {
            if (target != null)
            {
                target.onClick.RemoveListener(listener);
            }
        }
    }
}
