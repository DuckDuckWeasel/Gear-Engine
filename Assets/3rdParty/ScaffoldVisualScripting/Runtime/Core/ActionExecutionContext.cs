using System;

namespace Scaffold.VisualScripting
{
    public sealed class ActionExecutionContext
    {
        public ActionExecutionContext(Blackboard blackboard, Block block, ActionTrack track, ActionList actionList, IActionFlowController flowController, ExecutionId blockExecutionId, ExecutionId trackExecutionId, ExecutionId actionListExecutionId, ExecutionId actionExecutionId, int actionIndex = -1, int previousActionIndex = -1)
        {
            Blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            Block = block;
            Track = track;
            ActionList = actionList ?? throw new ArgumentNullException(nameof(actionList));
            FlowController = flowController ?? throw new ArgumentNullException(nameof(flowController));
            BlockExecutionId = blockExecutionId;
            TrackExecutionId = trackExecutionId;
            ActionListExecutionId = actionListExecutionId;
            ActionExecutionId = actionExecutionId;
            ActionIndex = actionIndex;
            PreviousActionIndex = previousActionIndex;
        }

        public Blackboard Blackboard { get; }

        public Block Block { get; }

        public ActionTrack Track { get; }

        public ActionList ActionList { get; }

        public IFrameScheduler Scheduler => Blackboard.Scheduler;

        public ITimeSource TimeSource => Blackboard.TimeSource;

        public IBlackboardEventBus EventBus => Blackboard.EventBus;

        public IBlackboardSaveService SaveService => Blackboard.SaveService;

        public IBlackboardLogger Logger => Blackboard.Logger;

        public IBlackboardRegistry Registry => Blackboard.Registry;

        public IActionFlowController FlowController { get; }

        public BlackboardRuntimeInstanceId RuntimeInstanceId => Blackboard.RuntimeInstanceId;

        public ExecutionId BlockExecutionId { get; }

        public ExecutionId TrackExecutionId { get; }

        public ExecutionId ActionListExecutionId { get; }

        public ExecutionId ActionExecutionId { get; }

        public int ActionIndex { get; }

        public int PreviousActionIndex { get; }
    }
}
