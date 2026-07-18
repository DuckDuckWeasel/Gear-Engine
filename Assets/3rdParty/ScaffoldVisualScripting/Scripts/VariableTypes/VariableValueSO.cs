using UnityEngine;

namespace Scaffold
{
    public abstract class VariableValueSO<T> : ScriptableObject
    {
        public virtual T Value
        {
            get { return value; }
            set { this.value = value; }
        }

        [SerializeField]
        protected T value;
    }
}
