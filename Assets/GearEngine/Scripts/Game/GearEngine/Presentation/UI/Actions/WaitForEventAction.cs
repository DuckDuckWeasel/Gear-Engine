using System;
using System.Reflection;
using GearEngine.Core.Actions;
using UnityEngine;
using global::GearEngine.GearEngine.Presentation.UI.Input;

namespace GearEngine.GearEngine.Presentation.UI.Actions
{
    [Serializable]
    public class WaitForEventAction : ActionBase
    {
        [Tooltip("The type of event to wait for.")]
        [SubclassDropdown("AnalyticsEvent")]
        public string EventType;

        [NonSerialized, Scaffold.VisualScripting.BlackboardTransient]
        private bool eventFired;

        [NonSerialized, Scaffold.VisualScripting.BlackboardTransient]
        private Delegate cachedDelegate;

        [NonSerialized, Scaffold.VisualScripting.BlackboardTransient]
        private Type resolvedType;

        [NonSerialized, Scaffold.VisualScripting.BlackboardTransient]
        private Type eventsManagerType;

        [NonSerialized, Scaffold.VisualScripting.BlackboardTransient]
        private MethodInfo unsubscribeMethod;

        public override void OnEnter()
        {
            try
            {
                StartWaiting();
            }
            catch (Exception exception)
            {
                HandleSubscriptionError(exception);
            }
        }

        public override void OnStopExecuting()
        {
            Unsubscribe();
        }

        private void StartWaiting()
        {
            ResetExecutionState();
            if (string.IsNullOrEmpty(EventType))
            {
                Continue();
                return;
            }

            ResolveTypes();
            SubscribeOrFail();
        }

        private void HandleSubscriptionError(Exception exception)
        {
            Debug.LogError($"[WaitForEventAction] Failed to subscribe to '{EventType}': {exception.Message}\n{exception.StackTrace}");
            Unsubscribe();
            Fail();
        }

        private void ResetExecutionState()
        {
            Unsubscribe();
            eventFired = false;
            resolvedType = null;
            eventsManagerType = null;
        }

        private void ResolveTypes()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                resolvedType ??= assembly.GetType(EventType);
                eventsManagerType ??= assembly.GetType("OM.OM_EventsManager");
                if (resolvedType != null && eventsManagerType != null)
                {
                    return;
                }
            }
        }

        private void SubscribeOrFail()
        {
            if (!ValidateResolvedTypes())
            {
                return;
            }

            CreateCallbackDelegate();
            MethodInfo subscribeMethod = FindSubscriptionMethod("Subscribe");
            unsubscribeMethod = FindSubscriptionMethod("Unsubscribe");
            if (!ValidateSubscriptionMethods(subscribeMethod))
            {
                return;
            }

            subscribeMethod.Invoke(null, new object[] { cachedDelegate });
        }

        private bool ValidateResolvedTypes()
        {
            if (resolvedType != null && eventsManagerType != null)
            {
                return true;
            }

            Debug.LogError($"[WaitForEventAction] Event '{EventType}' or OM.OM_EventsManager was not found.");
            Fail();
            return false;
        }

        private bool ValidateSubscriptionMethods(MethodInfo subscribeMethod)
        {
            if (subscribeMethod != null && unsubscribeMethod != null)
            {
                return true;
            }

            Debug.LogError($"[WaitForEventAction] Symmetric subscription methods for '{EventType}' were not found.");
            Fail();
            return false;
        }

        private void CreateCallbackDelegate()
        {
            MethodInfo callback = GetType().GetMethod(nameof(OnEventFired), BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo closedCallback = callback?.MakeGenericMethod(resolvedType);
            if (closedCallback == null)
            {
                throw new InvalidOperationException($"Could not create the callback for '{EventType}'.");
            }

            Type actionType = typeof(Action<>).MakeGenericType(resolvedType);
            cachedDelegate = Delegate.CreateDelegate(actionType, this, closedCallback);
        }

        private MethodInfo FindSubscriptionMethod(string methodName)
        {
            Type actionType = typeof(Action<>).MakeGenericType(resolvedType);
            MethodInfo exactMethod = eventsManagerType.GetMethod(methodName, new[] { actionType });
            if (exactMethod != null)
            {
                return exactMethod;
            }

            return FindGenericSubscriptionMethod(methodName);
        }

        private MethodInfo FindGenericSubscriptionMethod(string methodName)
        {
            MethodInfo[] methods = eventsManagerType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                if (IsMatchingGenericMethod(method, methodName))
                {
                    return method.MakeGenericMethod(resolvedType);
                }
            }

            return null;
        }

        private bool IsMatchingGenericMethod(MethodInfo method, string methodName)
        {
            if (method.Name != methodName || !method.IsGenericMethodDefinition)
            {
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 1 && parameters[0].ParameterType.Name == "Action`1";
        }

        private void OnEventFired<TEvent>(TEvent eventValue)
        {
            if (eventFired)
            {
                return;
            }

            eventFired = true;
            Unsubscribe();
            Continue();
        }

        private void Unsubscribe()
        {
            if (unsubscribeMethod == null || cachedDelegate == null)
            {
                ClearSubscription();
                return;
            }

            TryUnsubscribe();
            ClearSubscription();
        }

        private void TryUnsubscribe()
        {
            try
            {
                unsubscribeMethod.Invoke(null, new object[] { cachedDelegate });
            }
            catch (Exception exception)
            {
                Debug.LogError($"[WaitForEventAction] Failed to unsubscribe from '{EventType}': {exception.Message}\n{exception.StackTrace}");
            }
        }

        private void ClearSubscription()
        {
            unsubscribeMethod = null;
            cachedDelegate = null;
        }
    }
}
