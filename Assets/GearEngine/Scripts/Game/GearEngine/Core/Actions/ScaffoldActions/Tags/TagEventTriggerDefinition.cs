using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI.Tags;
using Scaffold.VisualScripting;
using UnityEngine;

namespace Scaffold
{
    [Serializable]
    public sealed class TagEventTriggerDefinition : TriggerDefinition
    {
        [SerializeField] private TagSO tag;
        [SerializeField] private string eventName = string.Empty;

        public override ITriggerBinding CreateBinding(
            TriggerExecutionContext context)
        {
            return new Binding(context, tag, eventName);
        }

        public override void Validate(
            string path,
            ICollection<BlackboardValidationIssue> issues)
        {
            if (tag == null)
            {
                issues.Add(
                    new BlackboardValidationIssue(
                        path,
                        "A tag event trigger requires a TagSO."));
            }

            if (string.IsNullOrWhiteSpace(eventName))
            {
                issues.Add(
                    new BlackboardValidationIssue(
                        path,
                        "A tag event trigger requires an event name."));
            }
        }

        private sealed class Binding : TriggerBindingBase
        {
            public Binding(
                TriggerExecutionContext context,
                TagSO tag,
                string eventName)
                : base(context)
            {
                this.tag = tag;
                this.eventName = eventName;
            }

            private readonly TagSO tag;
            private readonly string eventName;
            private IDisposable subscription;

            protected override void OnEnable()
            {
                subscription =
                    Context.EventBus.Subscribe<TagBlackboardEvent>(
                        HandleEvent);
            }

            protected override void OnDisable()
            {
                subscription?.Dispose();
                subscription = null;
            }

            private void HandleEvent(TagBlackboardEvent eventValue)
            {
                if (eventValue.Tag == tag &&
                    string.Equals(
                        eventValue.EventName,
                        eventName,
                        StringComparison.Ordinal))
                {
                    ExecuteBlock();
                }
            }
        }
    }
}
