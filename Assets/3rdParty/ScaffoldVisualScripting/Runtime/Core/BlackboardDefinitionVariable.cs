using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class BlackboardDefinitionVariable : VariableDefinitionBase
    {
        public BlackboardDefinitionVariable()
        {
        }

        public BlackboardDefinitionVariable(BlackboardDefinition value)
        {
            this.value = value;
        }

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

        public BlackboardDefinition Value
        {
            get => value;
            set => this.value = value;
        }

        [SerializeReference] private BlackboardDefinition value;

        public override Type ValueType => typeof(BlackboardDefinition);

        internal override VariableCellBase CreateCell()
        {
            return new VariableCell<BlackboardDefinition>(DefinitionId, Key, Scope, value);
        }
    }
}
