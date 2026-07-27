using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class PollingTriggerDefinition : TriggerDefinition
    {
        public ITriggerCondition Condition
        {
            get => condition;
            set => condition = value;
        }

        [SerializeReference] private ITriggerCondition condition;

        public PollingTriggerFireMode FireMode
        {
            get => fireMode;
            set => fireMode = value;
        }

        [SerializeField] private PollingTriggerFireMode fireMode = PollingTriggerFireMode.WhileTrue;

        public override ITriggerBinding CreateBinding(TriggerExecutionContext context)
        {
            return new Binding(context, condition ?? throw new InvalidOperationException("A polling trigger requires a condition."), fireMode);
        }

        public override void Validate(string path, ICollection<BlackboardValidationIssue> issues)
        {
            if (condition == null)
            {
                issues.Add(new BlackboardValidationIssue(path, "A polling trigger requires a condition."));
            }
        }

        private sealed class Binding : TriggerBindingBase
        {
            public Binding(TriggerExecutionContext context, ITriggerCondition condition, PollingTriggerFireMode fireMode) : base(context)
            {
                this.condition = condition;
                this.fireMode = fireMode;
            }

            private readonly ITriggerCondition condition;
            private readonly PollingTriggerFireMode fireMode;
            private bool wasTrue;

            protected override void OnEnable()
            {
                wasTrue = false;
            }

            protected override void OnDisable()
            {
                wasTrue = false;
            }

            protected override void OnTick()
            {
                bool isTrue = EvaluateCondition();
                if (ShouldFire(isTrue))
                {
                    ExecuteBlock();
                }

                wasTrue = isTrue;
            }

            private bool EvaluateCondition()
            {
                try
                {
                    return condition.Evaluate(Context);
                }
                catch (Exception exception)
                {
                    Context.Logger.Error("Failed to evaluate a Blackboard polling trigger.", exception);
                    return false;
                }
            }

            private bool ShouldFire(bool isTrue)
            {
                return isTrue && (fireMode == PollingTriggerFireMode.WhileTrue || !wasTrue);
            }
        }
    }
}
