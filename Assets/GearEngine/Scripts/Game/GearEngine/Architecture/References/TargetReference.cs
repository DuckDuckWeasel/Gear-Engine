using System;
using UnityEngine;
using GearEngine.GearEngine.Presentation.UI.Tags;

namespace GearEngine.Core.Architecture.References
{
    /// <summary>
    /// A robust way to reference a GameObject target from anywhere (Scene, Prefab, ScriptableObject).
    /// Allows the designer to choose the best strategy to find the target.
    /// </summary>
    [Serializable]
    public class TargetReference
    {
        [Tooltip("How should this system find its target?")]
        public TargetResolutionStrategy strategy = TargetResolutionStrategy.DirectReference;

        [Tooltip("Direct reference to a GameObject in the scene.")]
        public GameObject directReference;

        [Tooltip("Finds or validates a target by its tags.")]
        public TagFilter tagFilter = new TagFilter();

        [Tooltip("References a global variable registered in the system (e.g. Fungus).")]
        public string globalVariableName;

        [Tooltip("A list of multiple explicit targets.")]
        public System.Collections.Generic.List<TargetReferenceItem> references = new System.Collections.Generic.List<TargetReferenceItem>();

        /// <summary>
        /// Attempts to get the exact target object right now.
        /// Useful if you need the object instance (e.g. to move it).
        /// Note: Tags strategy cannot "find" an object directly unless you pass candidates. 
        /// Use IsMatch() for Tag validation.
        /// </summary>
        public GameObject Resolve(ITargetResolver resolver = null)
        {
            switch (strategy)
            {
                case TargetResolutionStrategy.DirectReference:
                    return directReference;
                case TargetResolutionStrategy.GlobalVariable:
                    return (resolver != null && !string.IsNullOrEmpty(globalVariableName))
                        ? resolver.Resolve(globalVariableName)
                        : null;
                case TargetResolutionStrategy.MultipleReferences:
                    if (references != null && references.Count > 0)
                    {
                        foreach (TargetReferenceItem item in references)
                        {
                            GameObject go = item.Resolve(resolver);
                            if (go != null)
                            {
                                return go; // Returns the first valid one
                            }
                        }
                    }
                    return null;
                case TargetResolutionStrategy.Tags:
                    // Tags are better suited for IsMatch(target).
                    // If forced to Resolve, it would mean FindGameObjectsWithTag, which is heavy.
                    Debug.LogWarning("TargetReference.Resolve() called with Tags strategy. Tags are meant for validation (IsMatch). Returning null.");
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Attempts to get ALL valid targets if the strategy supports multiple (e.g. MultipleReferences).
        /// </summary>
        public System.Collections.Generic.List<GameObject> ResolveAll(
            ITargetResolver resolver = null)
        {
            System.Collections.Generic.List<GameObject> list = new System.Collections.Generic.List<GameObject>();

            if (strategy == TargetResolutionStrategy.DirectReference)
            {
                if (directReference != null)
                {
                    list.Add(directReference);
                }
            }
            else if (strategy == TargetResolutionStrategy.GlobalVariable)
            {
                GameObject go =
                    resolver != null && !string.IsNullOrEmpty(globalVariableName)
                        ? resolver.Resolve(globalVariableName)
                        : null;
                if (go != null)
                {
                    list.Add(go);
                }
            }
            else if (strategy == TargetResolutionStrategy.MultipleReferences)
            {
                if (references != null)
                {
                    foreach (TargetReferenceItem item in references)
                    {
                        GameObject go = item.Resolve(resolver);
                        if (go != null && !list.Contains(go))
                        {
                            list.Add(go);
                        }
                    }
                }
            }
            else if (strategy == TargetResolutionStrategy.Tags)
            {
                Debug.LogWarning("TargetReference.ResolveAll() cannot resolve Tags effectively without scanning the scene. Use IsMatch() for tags.");
            }

            return list;
        }

        /// <summary>
        /// Returns a short string summary of the target configuration for UI/Editor displays.
        /// </summary>
        public string GetSummary()
        {
            switch (strategy)
            {
                case TargetResolutionStrategy.DirectReference:
                    return directReference != null ? directReference.name : "None";
                case TargetResolutionStrategy.GlobalVariable:
                    return !string.IsNullOrEmpty(globalVariableName) ? globalVariableName : "None";
                case TargetResolutionStrategy.MultipleReferences:
                    return $"[{references?.Count ?? 0} Targets]";
                case TargetResolutionStrategy.Tags:
                    if (tagFilter.soTags != null && tagFilter.soTags.Count > 0)
                    {
                        return tagFilter.soTags[0] != null ? tagFilter.soTags[0].name + (tagFilter.soTags.Count > 1 ? "..." : "") : "None";
                    }

                    return "None";
                default:
                    return "None";
            }
        }

        /// <summary>
        /// Validates if a given target GameObject matches the criteria defined in this reference.
        /// </summary>
        public bool IsMatch(
            GameObject target,
            ITargetResolver resolver = null)
        {
            if (target == null)
            {
                return false;
            }

            switch (strategy)
            {
                case TargetResolutionStrategy.DirectReference:
                    return directReference != null && (target == directReference || target.transform.IsChildOf(directReference.transform));
                case TargetResolutionStrategy.GlobalVariable:
                    if (resolver == null || string.IsNullOrEmpty(globalVariableName))
                    {
                        return false;
                    }

                    GameObject globalObj = resolver.Resolve(globalVariableName);
                    return globalObj != null && (target == globalObj || target.transform.IsChildOf(globalObj.transform));
                case TargetResolutionStrategy.MultipleReferences:
                    if (references != null)
                    {
                        foreach (TargetReferenceItem item in references)
                        {
                            if (item.Resolve(resolver) == target)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                case TargetResolutionStrategy.Tags:
                    return tagFilter.IsMatch(target);
                default:
                    return false;
            }
        }
    }
}
