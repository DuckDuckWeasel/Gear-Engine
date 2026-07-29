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

        public ActionListExecutionMethod ExecutionMethod
        {
            get => executionMethod;
            set => executionMethod = value;
        }

        [SerializeField] private ActionListExecutionMethod executionMethod;

        public ActionListAwaitMode AwaitMode
        {
            get => awaitMode;
            set => awaitMode = value;
        }

        [SerializeField] private ActionListAwaitMode awaitMode;

        public ActionListOrderMode OrderMode
        {
            get => orderMode;
            set => orderMode = value;
        }

        [SerializeField] private ActionListOrderMode orderMode;

        public bool AvoidRepeatingLastAction
        {
            get => avoidRepeatingLastAction;
            set => avoidRepeatingLastAction = value;
        }

        [SerializeField] private bool avoidRepeatingLastAction;

        public List<ActionTrackDefinition> Tracks => tracks;

        [SerializeField]
        private List<ActionTrackDefinition> tracks =
            new List<ActionTrackDefinition>();
    }
}
