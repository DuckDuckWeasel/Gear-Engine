using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class ActionTrackDefinition : DefinitionNode
    {
        public string Name
        {
            get => name;
            set => name = value ?? string.Empty;
        }

        [SerializeField] private string name = "Track";

        public ActionListDefinition ActionList
        {
            get => actionList;
            set => actionList = value;
        }

        [SerializeField] private ActionListDefinition actionList = new ActionListDefinition();
    }
}
