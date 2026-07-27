using System;
using Scaffold.Tutorial.Variables;
using UnityEngine;

namespace Scaffold.Tutorial.Requirements
{
    /// <summary>
    /// ScriptableObject that holds a concrete ITutorialRequirement.
    /// Uses [SerializeReference] (via TutorialScriptableObjectReference) so Unity can
    /// serialize any concrete class implementing the interface without Odin.
    /// </summary>
    [Serializable]
    public abstract class TutorialRequirementSO : ScriptableObject
    {
        [SerializeReference]
        protected ITutorialRequirement data;

        public virtual ITutorialRequirement Data
        {
            get => data;
            set => data = value;
        }
    }
}
