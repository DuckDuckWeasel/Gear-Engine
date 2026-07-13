using System;
using UnityEngine;

namespace Scaffold.Tutorial.Variables
{
    public enum VariableSource
    {
        Constant,
        Reference
    }

    /// <summary>
    /// Odin-free, serialization-safe variable reference using Unity's native [SerializeField]
    /// and [SerializeReference] for polymorphic interface support.
    /// 
    /// Replaces Scaffold.Core.Variables.VariableReference which depends on OdinSerializer.
    /// T1 = the value type (can be an interface — stored via [SerializeReference])
    /// T2 = the ScriptableObject reference type
    /// </summary>
    [Serializable]
    public abstract class TutorialVariableReference<T1, T2> where T2 : ScriptableObject
    {
        [SerializeField]
        private VariableSource source = VariableSource.Reference;

        // [SerializeReference] allows Unity to serialize any concrete class that implements T1,
        // including interfaces — this replaces [OdinSerialize] entirely.
        [SerializeReference]
        protected T1 data;

        [SerializeField]
        protected T2 reference;

        public T1 Value
        {
            get
            {
                if (source == VariableSource.Constant) return data;
                return reference != null ? GetReferenceValue(reference) : default;
            }
        }

        public VariableSource Source => source;

        /// <summary>
        /// Override to extract the value from the concrete reference SO.
        /// </summary>
        protected abstract T1 GetReferenceValue(T2 reference);

        public void SetData(T1 value)
        {
            source = VariableSource.Constant;
            data = value;
        }

        public void SetReference(T2 value)
        {
            source = VariableSource.Reference;
            reference = value;
        }

        public static implicit operator T1(TutorialVariableReference<T1, T2> variableReference)
        {
            return variableReference != null ? variableReference.Value : default;
        }
    }
}
