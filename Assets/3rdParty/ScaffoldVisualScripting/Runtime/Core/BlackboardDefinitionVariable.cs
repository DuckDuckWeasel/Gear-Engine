using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class BlackboardDefinitionVariable : VariableDefinition
    {
        public BlackboardDefinitionVariable()
        {
        }

        public BlackboardDefinitionVariable(BlackboardDefinition value)
        {
            this.value = value;
        }

        public BlackboardDefinition Value
        {
            get => value;
            set => this.value = value;
        }

        [SerializeReference] private BlackboardDefinition value;
    }
}
