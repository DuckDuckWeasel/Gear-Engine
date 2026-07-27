using System;

namespace Scaffold.VisualScripting.Unity
{
    public sealed class UnityBlackboardRuntimeServicesFactory : IBlackboardRuntimeServicesFactory
    {
        public UnityBlackboardRuntimeServicesFactory(Func<IFrameScheduler> schedulerFactory, ITimeSource timeSource, IBlackboardEventBus eventBus, IBlackboardSaveService saveService, IBlackboardLogger logger, IVariableValueSerializer serializer, IBlackboardRegistry registry)
        {
            this.schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            this.timeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this.saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        private readonly Func<IFrameScheduler> schedulerFactory;
        private readonly ITimeSource timeSource;
        private readonly IBlackboardEventBus eventBus;
        private readonly IBlackboardSaveService saveService;
        private readonly IBlackboardLogger logger;
        private readonly IVariableValueSerializer serializer;
        private readonly IBlackboardRegistry registry;

        public BlackboardRuntimeServices Create()
        {
            IFrameScheduler scheduler = schedulerFactory();
            BlackboardVariablePersistence persistence = new BlackboardVariablePersistence(serializer, logger);
            TextSubstitutionService substitution = new TextSubstitutionService(logger);
            return new BlackboardRuntimeServices(scheduler, timeSource, eventBus, saveService, logger, persistence, substitution, registry);
        }
    }
}
