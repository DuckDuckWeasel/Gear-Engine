using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// The block will execute when the specified event is received from a Tag Event command.
    /// </summary>
    [EventHandlerInfo("Tags",
                      "Tag Event Received",
                      "The block will execute when the specified event is received from a Tag Event command.")]
    [AddComponentMenu("")]
    public class TagEventReceived : EventHandler 
    {
        [Tooltip("Tag event to listen for")]
        [SerializeField] protected string eventName = "";

        public void OnTagEventReceived(string incomingEvent)
        {
            if (this.eventName == incomingEvent)
            {
                ExecuteBlock();
            }
        }

        public override string GetSummary()
        {
            return eventName;
        }
    }
}
