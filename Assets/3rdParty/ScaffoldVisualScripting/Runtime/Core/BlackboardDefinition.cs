using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class BlackboardDefinition : DefinitionNode
    {
        public string Name
        {
            get => name;
            set => name = value ?? string.Empty;
        }

        [SerializeField] private string name = "Blackboard";

        public List<BlockDefinition> Blocks => blocks;

        [SerializeField] private List<BlockDefinition> blocks = new List<BlockDefinition>();

        public List<VariableDefinitionBase> Variables => variables;

        [SerializeReference]
        private List<VariableDefinitionBase> variables =
            new List<VariableDefinitionBase>();
    }
}
