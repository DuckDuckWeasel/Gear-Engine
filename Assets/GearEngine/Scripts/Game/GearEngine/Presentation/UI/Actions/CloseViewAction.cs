using System;
using UnityEngine;
using GearEngine.Core.Actions;
using global::GearEngine.GearEngine.Presentation.UI.Input;

namespace GearEngine.GearEngine.Presentation.UI.Actions
{
    [Serializable]
    public class CloseViewAction : IAction
    {
        [Tooltip("The View class to close.")]
        [SubclassDropdown("View")]
        public string viewType;

        public void Execute(System.Action onComplete)
        {
            if (string.IsNullOrEmpty(viewType))
            {
                onComplete?.Invoke();
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
                Debug.LogWarning($"[CloseViewAction] Type {viewType} not found.");
                return;
            }

            var viewInstances = UnityEngine.Object.FindObjectsOfType(resolvedType, true); // true to include inactive, in case it's already inactive
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

            onComplete?.Invoke();
        }
    }
}
