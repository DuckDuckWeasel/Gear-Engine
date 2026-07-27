
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// The block will execute when the specified message is received from a Send Message command.
    /// </summary>
    [EventHandlerInfo("Scene",
                      "Message Received",
                      "The block will execute when the specified message is received from a Send Message command.")]
    [AddComponentMenu("")]
    public class MessageReceived : EventHandler
    {
        [Tooltip("Scaffold message to listen for")]
        [SerializeField] protected string message = "";

        #region Public members

        protected virtual void OnEnable()
        {
            Blackboard.MessageReceivers.Add(this);
        }

        protected virtual void OnDisable()
        {
            Blackboard.MessageReceivers.Remove(this);
        }

        /// <summary>
        /// Called from Blackboard when a message is sent.
        /// </summary>
        /// <param name="message">Message.</param>
        public void OnSendScaffoldMessage(string message)
        {
            if (this.message == message)
            {
                ExecuteBlock();
            }
        }

        public override string GetSummary()
        {
            return message;
        }

        #endregion
    }
}