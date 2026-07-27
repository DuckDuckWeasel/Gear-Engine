using System;

namespace Scaffold.VisualScripting
{
    public sealed partial class Blackboard : IBlackboardHandle
    {
        public Blackboard(BlackboardRuntimeInstanceId runtimeInstanceId, BlackboardVariableSet variables, IFrameScheduler scheduler, ITimeSource timeSource, IBlackboardEventBus eventBus, IBlackboardSaveService saveService, IBlackboardLogger logger)
        {
            RuntimeInstanceId = runtimeInstanceId;
            Variables = variables ?? throw new ArgumentNullException(nameof(variables));
            Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            TimeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            SaveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }

        public BlackboardVariableSet Variables { get; }

        public IFrameScheduler Scheduler { get; }

        public ITimeSource TimeSource { get; }

        public IBlackboardEventBus EventBus { get; }

        public IBlackboardSaveService SaveService { get; }

        public IBlackboardLogger Logger { get; }
    }
}
