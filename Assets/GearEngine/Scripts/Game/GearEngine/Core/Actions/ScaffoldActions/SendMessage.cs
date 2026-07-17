using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Supported target types for messages.
    /// </summary>
    public enum MessageTarget
    {
        /// <summary>
        /// Send message to the Flowchart containing the SendMessage command.
        /// </summary>
        SameFlowchart,
        /// <summary>
        /// Broadcast message to all Flowcharts.
        /// </summary>
        AllFlowcharts
    }

    /// <summary>
    /// Sends a message to either the owner Flowchart or all Flowcharts in the scene. Blocks can listen for this message using a Message Received event handler.
    /// </summary>
    [CommandInfo("Flow", 
                 "Send Message", 
                 "Sends a message to either the owner Flowchart or all Flowcharts in the scene. Blocks can listen for this message using a Message Received event handler.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class SendMessage : ActionBase
    {
        [Tooltip("Target flowchart(s) to send the message to")]
        [SerializeField] protected MessageTarget messageTarget;

        [Tooltip("Name of the message to send")]
        [SerializeField] protected StringData message = new StringData("");

        #region Public members

        public override void OnEnter()
        {
            if (message.Value.Length == 0)
            {
                Continue();
                return;
            }

            MessageReceived[] receivers = null;
            if (messageTarget == MessageTarget.SameFlowchart)
            {
                receivers = host.GetComponents<MessageReceived>();
            }
            else
            {
                receivers = GameObject.FindObjectsOfType<MessageReceived>();
            }

            if (receivers != null)
            {
                for (int i = 0; i < receivers.Length; i++)
                {
                    var receiver = receivers[i];
                    receiver.OnSendScaffoldMessage(message.Value);
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (message.Value.Length == 0)
            {
                return "Error: No message specified";
            }
            
            return message.Value;
        }
        
        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return message.stringRef == variable || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("message")] public string messageOLD = "";

        protected virtual void OnEnable()
        {
            if (messageOLD != "")
            {
                message.Value = messageOLD;
                messageOLD = "";
            }
        }

        #endregion
    }
}