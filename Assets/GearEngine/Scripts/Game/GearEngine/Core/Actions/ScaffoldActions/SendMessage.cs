using System;
using GearEngine.Core.Actions;

using UnityEngine;
namespace Scaffold
{
    /// <summary>
    /// Sends a message to either the owner Blackboard or all Blackboards in the scene. Blocks can listen for this message using a Message Received event handler.
    /// </summary>
    [CommandInfo("Flow",
                 "Send Message",
                 "Sends a message to either the owner Blackboard or all Blackboards in the scene. Blocks can listen for this message using a Message Received event handler.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class SendMessage : ActionBase
    {
        [Tooltip("Target blackboard(s) to send the message to")]
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

            if (messageTarget == MessageTarget.SameBlackboard)
            {
                GetBlackboard().SendMessage(message.Value);
            }
            else
            {
                GetBlackboard().BroadcastMessage(message.Value);
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

    }
}
