using UnityEngine;

namespace Scaffold.Variables
{
    public abstract class VariableSO<T> : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The value of the variable. This will be shared among all references.")]
        private T value;

        public virtual T Value
        {
            get => value;
            set => this.value = value;
        }

        public static implicit operator T(VariableSO<T> variable)
        {
            return variable != null ? variable.Value : default(T);
        }
    }
}
