using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class BindableTriggerDefinition : TriggerDefinition
    {
        public ITriggerSignalSource Source
        {
            get => source;
            set => source = value;
        }

        [SerializeReference] private ITriggerSignalSource source;

        public VariableReference ValueTarget
        {
            get => valueTarget;
            set => valueTarget = value;
        }

        [SerializeField] private VariableReference valueTarget;

        public override ITriggerBinding CreateBinding(TriggerExecutionContext context)
        {
            return new Binding(context, source ?? throw new InvalidOperationException("A bindable trigger requires a signal source."), valueTarget);
        }

        public override void Validate(string path, ICollection<BlackboardValidationIssue> issues)
        {
            if (source == null)
            {
                issues.Add(new BlackboardValidationIssue(path, "A bindable trigger requires a signal source."));
            }
        }

        private sealed class Binding : TriggerBindingBase
        {
            public Binding(TriggerExecutionContext context, ITriggerSignalSource source, VariableReference valueTarget) : base(context)
            {
                this.source = source;
                this.valueTarget = valueTarget;
            }

            private readonly ITriggerSignalSource source;
            private readonly VariableReference valueTarget;
            private IDisposable subscription;

            protected override void OnEnable()
            {
                subscription = source.Subscribe(OnSignal);
                if (subscription == null)
                {
                    throw new InvalidOperationException("A trigger signal source returned a null subscription.");
                }
            }

            protected override void OnDisable()
            {
                subscription?.Dispose();
                subscription = null;
            }

            private void OnSignal(object value)
            {
                try
                {
                    StoreValue(value);
                    ExecuteBlock();
                }
                catch (Exception exception)
                {
                    Context.Logger.Error("Failed to process a Blackboard trigger signal.", exception);
                }
            }

            private void StoreValue(object value)
            {
                if (valueTarget != null)
                {
                    Context.Variables.Resolve(valueTarget).UntypedValue = value;
                }
            }
        }
    }
}
