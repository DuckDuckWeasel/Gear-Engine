using System;
using UnityEngine;

namespace GearEngine.Core.Architecture.References
{
    [Serializable]
    public class TargetReferenceItem
    {
        [Tooltip("The type of reference (direct object or global variable).")]
        public TargetReferenceItemType type = TargetReferenceItemType.DirectReference;

        [Tooltip("Direct reference to a GameObject in the scene.")]
        public GameObject directReference;

        [Tooltip("References a global variable registered in the system (e.g. Fungus).")]
        public string globalVariableName;

        public GameObject Resolve(ITargetResolver resolver = null)
        {
            if (type == TargetReferenceItemType.DirectReference)
            {
                return directReference;
            }

            return (resolver != null && !string.IsNullOrEmpty(globalVariableName))
                ? resolver.Resolve(globalVariableName)
                : null;
        }
    }
}
