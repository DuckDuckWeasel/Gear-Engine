using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class BlockDefinition : DefinitionNode
    {
        public string Name
        {
            get => name;
            set => name = value ?? string.Empty;
        }

        [SerializeField] private string name = "Block";

        public TriggerDefinition Trigger
        {
            get => trigger;
            set => trigger = value;
        }

        [SerializeReference] private TriggerDefinition trigger;

        public List<ActionTrackDefinition> Tracks => tracks;

        [SerializeField]
        private List<ActionTrackDefinition> tracks =
            new List<ActionTrackDefinition>();
    }
}
