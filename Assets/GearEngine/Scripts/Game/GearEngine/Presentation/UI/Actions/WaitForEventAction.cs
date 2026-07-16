using System;
using UnityEngine;
using GearEngine.Core.Actions;
using global::GearEngine.GearEngine.Presentation.UI.Input;

namespace GearEngine.GearEngine.Presentation.UI.Actions
{
    [Serializable]
    public class WaitForEventAction : IAction
    {
        [Tooltip("The type of event to wait for.")]
        [SubclassDropdown("AnalyticsEvent")]
        public string eventType;

        private bool eventFired = false;
        private Delegate cachedDelegate;
        private Type resolvedType;
        private Type omEventsManagerType;
        private System.Action onCompleteCallback;

        public void Execute(System.Action onComplete)
        {
            this.onCompleteCallback = onComplete;
            eventFired = false;

            if (string.IsNullOrEmpty(eventType))
            {
                onCompleteCallback?.Invoke();
                return;
            }

            resolvedType = null;
            omEventsManagerType = null;
            
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (resolvedType == null)
                    resolvedType = asm.GetType(eventType);
                
                if (omEventsManagerType == null)
                    omEventsManagerType = asm.GetType("OM.OM_EventsManager");

                if (resolvedType != null && omEventsManagerType != null) 
                    break;
            }

            if (resolvedType == null)
            {
                Debug.LogWarning($"[WaitForEventAction] Type {eventType} not found.");
                onCompleteCallback?.Invoke();
                return;
            }

            if (omEventsManagerType == null)
            {
                Debug.LogWarning($"[WaitForEventAction] OM.OM_EventsManager not found.");
                onCompleteCallback?.Invoke();
                return;
            }

            var methodInfo = GetType().GetMethod(nameof(OnEventFiredGeneric), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                      .MakeGenericMethod(resolvedType);
            
            var actionType = typeof(Action<>).MakeGenericType(resolvedType);
            cachedDelegate = Delegate.CreateDelegate(actionType, this, methodInfo);

            bool subscribed = false;
            var subscribeMethod = omEventsManagerType.GetMethod("Subscribe", new Type[] { actionType });
            if (subscribeMethod != null)
            {
                subscribeMethod.Invoke(null, new object[] { cachedDelegate });
                subscribed = true;
            }
            else
            {
                var methods = omEventsManagerType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                foreach (var m in methods)
                {
                    if (m.Name == "Subscribe" && m.IsGenericMethodDefinition)
                    {
                        var parameters = m.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType.Name == "Action`1")
                        {
                            m.MakeGenericMethod(resolvedType).Invoke(null, new object[] { cachedDelegate });
                            subscribed = true;
                            break;
                        }
                    }
                }
            }

            if (!subscribed)
            {
                Debug.LogError($"[WaitForEventAction] Failed to find Subscribe for type {eventType}");
                onCompleteCallback?.Invoke();
            }
        }

        private void OnEventFiredGeneric<T>(T evt)
        {
            if (eventFired) return;
            eventFired = true;
            
            if (omEventsManagerType != null)
            {
                var methods = omEventsManagerType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                foreach (var m in methods)
                {
                    if (m.Name == "Unsubscribe" && m.IsGenericMethodDefinition)
                    {
                        var parameters = m.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType.Name == "Action`1")
                        {
                            m.MakeGenericMethod(resolvedType).Invoke(null, new object[] { cachedDelegate });
                            break;
                        }
                    }
                }
            }

            onCompleteCallback?.Invoke();
        }
    }
}
