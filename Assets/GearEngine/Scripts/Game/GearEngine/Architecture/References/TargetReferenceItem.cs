using System;
using UnityEngine;

namespace GearEngine.Core.Architecture.References
{
    public enum TargetReferenceItemType
    {
        DirectReference,
        GlobalVariable
    }

    [Serializable]
    public class TargetReferenceItem
    {
        [Tooltip("The type of reference (direct object or global variable).")]
        public TargetReferenceItemType type = TargetReferenceItemType.DirectReference;

        [Tooltip("Direct reference to a GameObject in the scene.")]
        public GameObject directReference;

        [Tooltip("References a global variable registered in the system (e.g. Fungus).")]
        public string globalVariableName;

        public GameObject Resolve()
        {
            if (type == TargetReferenceItemType.DirectReference)
                return directReference;
            
            return (TargetReference.GlobalResolver != null && !string.IsNullOrEmpty(globalVariableName)) 
                ? TargetReference.GlobalResolver.Resolve(globalVariableName) 
                : null;
        }
    }
}
