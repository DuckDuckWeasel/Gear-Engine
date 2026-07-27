using System;
using UnityEngine;
using GearEngine.Core.Actions;
using global::GearEngine.GearEngine.Presentation.UI.Input;
using System.Reflection;

namespace GearEngine.GearEngine.Presentation.UI.Actions
{
    [Serializable]
    public class CloseViewAction : ActionBase
    {
        [Tooltip("The View class to close.")]
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

            CloseResolvedViews(resolvedType);
            Continue();
        }

        private bool TryResolveViewType(out Type resolvedType)
        {
            resolvedType = ResolveViewType();
            if (resolvedType != null)
            {
                return true;
            }

            Debug.LogWarning($"[CloseViewAction] Type {ViewType} not found.");
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

        private void CloseResolvedViews(Type resolvedType)
        {
            UnityEngine.Object[] viewInstances = UnityEngine.Object.FindObjectsOfType(resolvedType, true); // true to include inactive, in case it's already inactive
            if (viewInstances != null && viewInstances.Length > 0)
            {
                CloseViews(resolvedType, viewInstances);
            }
        }

        private void CloseViews(Type resolvedType, UnityEngine.Object[] viewInstances)
        {
            foreach (UnityEngine.Object viewInstance in viewInstances)
            {
                CloseView(resolvedType, viewInstance as MonoBehaviour);
            }
        }

        private void CloseView(Type resolvedType, MonoBehaviour component)
        {
            if (component == null || !component.gameObject.activeInHierarchy)
            {
                return;
            }

            MethodInfo closeMethod = FindViewMethod(resolvedType, "Close");
            if (closeMethod != null)
            {
                closeMethod.Invoke(component, null);
                return;
            }

            HideView(resolvedType, component);
        }

        private void HideView(Type resolvedType, MonoBehaviour component)
        {
            MethodInfo hideMethod = FindViewMethod(resolvedType, "Hide");
            if (hideMethod != null)
            {
                hideMethod.Invoke(component, null);
                return;
            }

            component.gameObject.SetActive(false);
        }

        private MethodInfo FindViewMethod(Type resolvedType, string methodName)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            return resolvedType.GetMethod(methodName, flags, null, Type.EmptyTypes, null);
        }
    }
}
