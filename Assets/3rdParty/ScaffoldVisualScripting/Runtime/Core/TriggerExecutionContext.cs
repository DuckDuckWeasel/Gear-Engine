using System;

namespace Scaffold.VisualScripting
{
    public sealed class TriggerExecutionContext
    {
        public TriggerExecutionContext(Blackboard blackboard, Block block)
        {
            Blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            Block = block ?? throw new ArgumentNullException(nameof(block));
        }

        public Blackboard Blackboard { get; }

        public Block Block { get; }

        public BlackboardRuntimeInstanceId RuntimeInstanceId =>
            Blackboard.RuntimeInstanceId;

        public IFrameScheduler Scheduler => Blackboard.Scheduler;

        public IBlackboardEventBus EventBus => Blackboard.EventBus;

        public IBlackboardLogger Logger => Blackboard.Logger;

        public BlackboardVariableSet Variables => Blackboard.Variables;

        public bool ExecuteBlock()
        {
            return Blackboard.ExecuteBlock(Block);
        }
    }
}
