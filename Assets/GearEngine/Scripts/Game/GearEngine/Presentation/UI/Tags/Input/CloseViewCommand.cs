using Fungus;
using System;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    [CommandInfo("UI", "Close View", "Closes a View by finding it in the scene and calling Close() or deactivating its GameObject.")]
    [AddComponentMenu("")]
    public class CloseViewCommand : Command
    {
        [Tooltip("The View class to close.")]
        [SubclassDropdown("View")]
        public string viewType;

        public override void OnEnter()
        {
            if (string.IsNullOrEmpty(viewType))
            {
                Continue();
                return;
            }

            Type resolvedType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                resolvedType = asm.GetType(viewType);
                if (resolvedType != null) break;
            }

            if (resolvedType == null)
            {
                Debug.LogWarning($"[CloseViewCommand] Type {viewType} not found.");
                Continue();
                return;
            }

            var viewInstances = FindObjectsOfType(resolvedType, true); // true to include inactive, in case it's already inactive
            if (viewInstances != null && viewInstances.Length > 0)
            {
                foreach (var viewInstance in viewInstances)
                {
                    var component = viewInstance as MonoBehaviour;
                    if (component != null && component.gameObject.activeInHierarchy)
                    {
                        var closeMethod = resolvedType.GetMethod("Close", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
                        if (closeMethod != null)
                        {
                            closeMethod.Invoke(component, null);
                        }
                        else
                        {
                            // Try Hide()
                            var hideMethod = resolvedType.GetMethod("Hide", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
                            if (hideMethod != null)
                            {
                                hideMethod.Invoke(component, null);
                            }
                            else
                            {
                                component.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(viewType)) return "None";
            var parts = viewType.Split('.');
            return parts[parts.Length - 1];
        }
    }
}
