using System;
using UnityEngine;

namespace Scaffold
{
    [Serializable]
    public sealed class InvokeMethodParameter
    {
        [SerializeField]
        [Tooltip("The Obj value")]
        public ObjectValue ObjectValue;

        [SerializeField]
        [Tooltip("The Variable key")]
        public string VariableKey;
    }
}
