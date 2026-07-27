using GearEngine.Core.Actions;
using GearEngine.Core.Actions.ScaffoldActions.Analytics;
using System;
using UnityEngine;
using Scaffold.Analytics;

namespace Scaffold
{
    [CommandInfo("Analytics", "Send Event", "Sends a dynamic analytics event to the configured Analytics Service (UGS).")]
    [Serializable]
    [AddComponentMenu("")]
    public class SendAnalyticsEvent : ActionBase
    {
        [Tooltip("The name of the analytics event to send.")]
        [SerializeField] protected StringData eventName = new StringData("");

        [Header("String Parameters")]
        [Tooltip("Keys for string parameters. Must match the length of String Values.")]
        [SerializeField] protected string[] stringKeys;
        [Tooltip("Values for string parameters.")]
        [SerializeField] protected StringData[] stringValues;

        [Header("Numeric Parameters")]
        [Tooltip("Keys for numeric parameters. Must match the length of Numeric Values.")]
        [SerializeField] protected string[] numericKeys;
        [Tooltip("Values for numeric parameters.")]
        [SerializeField] protected FloatData[] numericValues;

        public override void OnEnter()
        {
            if (string.IsNullOrEmpty(eventName.Value))
            {
                Debug.LogWarning("[SendAnalyticsEvent] Event name is empty, aborting.");
                Continue();
                return;
            }

            DynamicAnalyticsEvent evt = new DynamicAnalyticsEvent(eventName.Value);

            // Populate string parameters
            if (stringKeys != null && stringValues != null)
            {
                int count = Mathf.Min(stringKeys.Length, stringValues.Length);
                for (int i = 0; i < count; i++)
                {
                    if (!string.IsNullOrEmpty(stringKeys[i]))
                    {
                        evt.AddParameter(stringKeys[i], stringValues[i].Value);
                    }
                }
            }

            // Populate numeric parameters
            if (numericKeys != null && numericValues != null)
            {
                int count = Mathf.Min(numericKeys.Length, numericValues.Length);
                for (int i = 0; i < count; i++)
                {
                    if (!string.IsNullOrEmpty(numericKeys[i]))
                    {
                        evt.AddParameter(numericKeys[i], numericValues[i].Value);
                    }
                }
            }

            SendEvent(evt);

            Continue();
        }

        private void SendEvent(DynamicAnalyticsEvent evt)
        {
            Context.EventBus.Publish(evt);
        }

        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(eventName.Value))
            {
                return "Error: No Event Name";
            }

            return eventName.Value;
        }

        public override Color GetButtonColor()
        {
            return new Color32(173, 216, 230, 255);
        }
    }
}
