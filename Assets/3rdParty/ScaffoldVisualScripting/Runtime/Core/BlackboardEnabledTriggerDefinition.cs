using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class BlackboardEnabledTriggerDefinition : TriggerDefinition
    {
        public int WaitForFrames
        {
            get => waitForFrames;
            set => waitForFrames = Math.Max(value, 0);
        }

        [SerializeField] private int waitForFrames = 1;

        public override ITriggerBinding CreateBinding(TriggerExecutionContext context)
        {
            return new Binding(context, waitForFrames);
        }

        private sealed class Binding : TriggerBindingBase
        {
            public Binding(TriggerExecutionContext context, int waitForFrames) : base(context)
            {
                this.waitForFrames = waitForFrames;
                deferredExecution = new FrameDeferredCallback(context.Scheduler);
            }

            private readonly int waitForFrames;
            private readonly FrameDeferredCallback deferredExecution;
            private IDisposable subscription;

            protected override void OnEnable()
            {
                subscription = Context.EventBus.Subscribe<BlackboardEnabledEvent>(OnBlackboardEnabled);
            }

            protected override void OnDisable()
            {
                subscription?.Dispose();
                subscription = null;
                deferredExecution.Cancel();
            }

            protected override void OnDispose()
            {
                deferredExecution.Dispose();
            }

            private void OnBlackboardEnabled(BlackboardEnabledEvent eventValue)
            {
                if (eventValue.RuntimeInstanceId != Context.RuntimeInstanceId)
                {
                    return;
                }

                deferredExecution.Schedule(waitForFrames, ExecuteDeferred);
            }

            private void ExecuteDeferred()
            {
                ExecuteBlock();
            }
        }
    }
}
