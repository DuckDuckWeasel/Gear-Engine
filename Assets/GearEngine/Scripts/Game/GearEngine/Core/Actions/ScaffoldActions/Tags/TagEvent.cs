using System;
using GearEngine.Core.Actions;
using UnityEngine;
using GearEngine.GearEngine.Presentation.UI.Tags;

namespace Scaffold
{
    [CommandInfo("Tags",
                 "Tag Event",
                 "Sends an event to all Blackboards on GameObjects that possess a specific TagSO.")]
    [AddComponentMenu("")]
    [Serializable]
    public class TagEvent : ActionBase
    {
        [Tooltip("The Tag to broadcast the event to.")]
        [SerializeField] protected TagSO targetTag;

        [Tooltip("Name of the event to send.")]
        [SerializeField] protected StringData eventName = new StringData("");

        public override void OnEnter()
        {
            if (string.IsNullOrEmpty(eventName.Value) || targetTag == null)
            {
                Continue();
                return;
            }

            Context.EventBus.Publish(
                new TagBlackboardEvent(targetTag, eventName.Value));

            Continue();
        }

        public override string GetSummary()
        {
            if (targetTag == null)
            {
                return "Error: No Tag specified";
            }

            if (string.IsNullOrEmpty(eventName.Value))
            {
                return "Error: No event specified";
            }

            return $"Tag: {targetTag.name}, Event: {eventName.Value}";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }
    }

}
