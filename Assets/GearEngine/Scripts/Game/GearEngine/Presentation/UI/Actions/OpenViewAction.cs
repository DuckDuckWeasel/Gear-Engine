using System;
using UnityEngine;
using GearEngine.Core.Actions;
using global::GearEngine.GearEngine.Presentation.UI.Input;
using System.Reflection;

namespace GearEngine.GearEngine.Presentation.UI.Actions
{
    [Serializable]
    public class OpenViewAction : ActionBase
    {
        [Tooltip("The View class to open.")]
        [SubclassDropdown("View")]
        public string ViewType;

        public override void OnEnter()
        {
            if (string.IsNullOrEmpty(ViewType))
            {
                Continue();
                return;
            }

            if (!TryResolveViewType(out Type resolvedType))
            {
                return;
            }

            OpenResolvedViews(resolvedType);
            Continue();
        }

        private bool TryResolveViewType(out Type resolvedType)
        {
            resolvedType = ResolveViewType();
            if (resolvedType != null)
            {
                return true;
            }

            Debug.LogWarning($"[OpenViewAction] Type {ViewType} not found.");
            Fail();
            return false;
        }

        private Type ResolveViewType()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type resolvedType = assembly.GetType(ViewType);
                if (resolvedType != null)
                {
                    return resolvedType;
                }
            }

            return null;
        }

        private void OpenResolvedViews(Type resolvedType)
        {
            UnityEngine.Object[] viewInstances = UnityEngine.Object.FindObjectsOfType(resolvedType, true);
            if (viewInstances == null || viewInstances.Length == 0)
            {
                Debug.LogWarning($"[OpenViewAction] Could not find an instance of {ViewType} in the scene.");
                return;
            }

            OpenViews(resolvedType, viewInstances);
        }

        private void OpenViews(Type resolvedType, UnityEngine.Object[] viewInstances)
        {
            foreach (UnityEngine.Object viewInstance in viewInstances)
            {
                OpenView(resolvedType, viewInstance as MonoBehaviour);
            }
        }

        private void OpenView(Type resolvedType, MonoBehaviour component)
        {
            if (component == null)
            {
                return;
            }

            MethodInfo openMethod = FindViewMethod(resolvedType, "Open");
            if (openMethod != null)
            {
                openMethod.Invoke(component, null);
                return;
            }

            ShowView(resolvedType, component);
        }

        private void ShowView(Type resolvedType, MonoBehaviour component)
        {
            MethodInfo showMethod = FindViewMethod(resolvedType, "Show");
            if (showMethod != null)
            {
                showMethod.Invoke(component, null);
                return;
            }

            component.gameObject.SetActive(true);
        }

        private MethodInfo FindViewMethod(Type resolvedType, string methodName)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            return resolvedType.GetMethod(methodName, flags, null, Type.EmptyTypes, null);
        }
    }
}
