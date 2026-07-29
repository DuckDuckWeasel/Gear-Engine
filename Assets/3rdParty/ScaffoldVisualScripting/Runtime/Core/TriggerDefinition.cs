using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public abstract class TriggerDefinition : DefinitionNode
    {
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        [SerializeField] private bool enabled = true;

        public abstract ITriggerBinding CreateBinding(TriggerExecutionContext context);

        public virtual void Validate(string path, ICollection<BlackboardValidationIssue> issues)
        {
        }
    }
}
