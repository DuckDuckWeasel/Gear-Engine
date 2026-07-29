using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class VariableReference
    {
        public VariableScope Scope
        {
            get => scope;
            set => scope = value;
        }

        [SerializeField] private VariableScope scope;

        public DefinitionId DefinitionId
        {
            get => definitionId;
            set => definitionId = value;
        }

        [SerializeField] private DefinitionId definitionId;

        public BlackboardRuntimeInstanceId SourceRuntimeInstanceId
        {
            get => sourceRuntimeInstanceId;
            set => sourceRuntimeInstanceId = value;
        }

        [SerializeField] private BlackboardRuntimeInstanceId sourceRuntimeInstanceId;
    }
}
