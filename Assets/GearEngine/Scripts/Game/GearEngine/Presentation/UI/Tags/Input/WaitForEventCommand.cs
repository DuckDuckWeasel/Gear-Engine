using Fungus;
using System;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    [CommandInfo("Events", "Wait For Event", "Waits for a specific event to be published.")]
    [AddComponentMenu("")]
    public class WaitForEventCommand : Command
    {
        [Tooltip("The type of event to wait for.")]
        [SubclassDropdown("AnalyticsEvent")]
        public string eventType;

        private bool eventFired = false;
        private Delegate cachedDelegate;
        private Type resolvedType;
        private Type omEventsManagerType;

        public override void OnEnter()
        {
            eventFired = false;
            if (string.IsNullOrEmpty(eventType))
            {
                Continue();
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
                Debug.LogWarning($"[WaitForEvent] Type {eventType} not found.");
                Continue();
                return;
            }

            if (omEventsManagerType == null)
            {
                Debug.LogWarning($"[WaitForEvent] OM.OM_EventsManager not found.");
                Continue();
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
                Debug.LogError($"[WaitForEvent] Failed to find Subscribe for type {eventType}");
                Continue();
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

            Continue();
        }

        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(eventType)) return "None";
            var parts = eventType.Split('.');
            return parts[parts.Length - 1];
        }
    }
}
