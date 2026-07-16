using System;
using UnityEngine;
using GearEngine.Core.Actions;
using global::GearEngine.GearEngine.Presentation.UI.Input;

namespace GearEngine.GearEngine.Presentation.UI.Actions
{
    [Serializable]
    public class OpenViewAction : IAction
    {
        [Tooltip("The View class to open.")]
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
                Debug.LogWarning($"[OpenViewAction] Type {viewType} not found.");
                return;
            }

            var viewInstances = UnityEngine.Object.FindObjectsOfType(resolvedType, true);
            if (viewInstances != null && viewInstances.Length > 0)
            {
                foreach (var viewInstance in viewInstances)
                {
                    var component = viewInstance as MonoBehaviour;
                    if (component != null)
                    {
                        var openMethod = resolvedType.GetMethod("Open", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
                        if (openMethod != null)
                        {
                            openMethod.Invoke(component, null);
                        }
                        else
                        {
                            var showMethod = resolvedType.GetMethod("Show", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
                            if (showMethod != null)
                            {
                                showMethod.Invoke(component, null);
                            }
                            else
                            {
                                component.gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[OpenViewAction] Could not find an instance of {viewType} in the scene.");
            }

            onComplete?.Invoke();
        }
    }
}
