using System;
using UnityEngine;

namespace Scaffold.VisualScripting.Unity
{
    [Serializable]
    public sealed class UnityKeyTriggerCondition : ITriggerCondition
    {
        public KeyCode Key
        {
            get => key;
            set => key = value;
        }

        [SerializeField] private KeyCode key;

        public UnityKeyTriggerMode Mode
        {
            get => mode;
            set => mode = value;
        }

        [SerializeField] private UnityKeyTriggerMode mode;

        public bool Evaluate(TriggerExecutionContext context)
        {
            switch (mode)
            {
                case UnityKeyTriggerMode.KeyDown:
                    return Input.GetKeyDown(key);
                case UnityKeyTriggerMode.KeyUp:
                    return Input.GetKeyUp(key);
                case UnityKeyTriggerMode.KeyRepeat:
                    return Input.GetKey(key);
                default:
                    throw new InvalidOperationException($"Unsupported key trigger mode '{mode}'.");
            }
        }
    }
}
