using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class BlackboardMessageTriggerDefinition : TriggerDefinition
    {
        public string MessageName
        {
            get => messageName;
            set => messageName = value ?? string.Empty;
        }

        [SerializeField] private string messageName = string.Empty;

        public override ITriggerBinding CreateBinding(TriggerExecutionContext context)
        {
            return new Binding(context, messageName);
        }

        public override void Validate(string path, ICollection<BlackboardValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(messageName))
            {
                issues.Add(new BlackboardValidationIssue(path, "A Blackboard message trigger requires a message name."));
            }
        }

        private sealed class Binding : TriggerBindingBase
        {
            public Binding(TriggerExecutionContext context, string messageName) : base(context)
            {
                this.messageName = messageName;
            }

            private readonly string messageName;
            private IDisposable subscription;

            protected override void OnEnable()
            {
                subscription = Context.EventBus.Subscribe<BlackboardMessage>(OnMessage);
            }

            protected override void OnDisable()
            {
                subscription?.Dispose();
                subscription = null;
            }

            private void OnMessage(BlackboardMessage message)
            {
                if (message.IsFor(Context.RuntimeInstanceId) && string.Equals(message.Name, messageName, StringComparison.Ordinal))
                {
                    ExecuteBlock();
                }
            }
        }
    }
}
