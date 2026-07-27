using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public abstract class DefinitionNode : IDefinitionNode
    {
        public DefinitionId DefinitionId =>
            string.IsNullOrWhiteSpace(definitionId)
                ? DefinitionId.Empty
                : new DefinitionId(definitionId);

        [SerializeField] private string definitionId = Guid.NewGuid().ToString("N");

        internal void RegenerateDefinitionId()
        {
            definitionId = Guid.NewGuid().ToString("N");
        }
    }
}
