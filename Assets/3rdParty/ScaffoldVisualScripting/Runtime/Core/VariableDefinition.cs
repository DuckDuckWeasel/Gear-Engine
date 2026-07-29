using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public abstract class VariableDefinition<T> : VariableDefinitionBase
    {
        public override string Key
        {
            get => key;
            set => key = value ?? string.Empty;
        }

        [SerializeField] private string key = string.Empty;

        public override VariableScope Scope
        {
            get => scope;
            set => scope = value;
        }

        [SerializeField] private VariableScope scope;

        public T InitialValue
        {
            get => initialValue;
            set => initialValue = value;
        }

        [SerializeField] private T initialValue;

        public override Type ValueType => typeof(T);

        internal override VariableCellBase CreateCell()
        {
            return new VariableCell<T>(DefinitionId, Key, Scope, initialValue);
        }
    }
}
