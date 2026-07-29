using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class ActionListDefinition : DefinitionNode
    {
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

        public List<IAction> Actions => actions;

        [SerializeReference] private List<IAction> actions = new List<IAction>();
    }
}
