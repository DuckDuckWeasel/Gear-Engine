using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public abstract class ActionDefinition : DefinitionNode, IAction, IActionMetadata
    {
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        [SerializeField] private bool enabled = true;

        public float Utility
        {
            get => utility;
            set => utility = value;
        }

        [SerializeField] private float utility;

        public float Weight
        {
            get => weight;
            set => weight = Mathf.Clamp(value, 0f, 100f);
        }

        [SerializeField] private float weight;

        public bool HasWeightOverride
        {
            get => hasWeightOverride;
            set => hasWeightOverride = value;
        }

        [SerializeField] private bool hasWeightOverride;

        public bool BlockDuringExecution
        {
            get => blockDuringExecution;
            set => blockDuringExecution = value;
        }

        [SerializeField] private bool blockDuringExecution;

        public abstract void Execute(ActionExecutionContext context, Action<ActionExecutionStatus> onComplete);

        public virtual void Interrupt()
        {
        }
    }
}
