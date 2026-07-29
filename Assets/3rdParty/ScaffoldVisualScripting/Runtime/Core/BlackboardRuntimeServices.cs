using System;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardRuntimeServices : IDisposable
    {
        public BlackboardRuntimeServices(IFrameScheduler scheduler, ITimeSource timeSource, IBlackboardEventBus eventBus, IBlackboardSaveService saveService, IBlackboardLogger logger, BlackboardVariablePersistence variablePersistence, ITextSubstitutionService textSubstitution, IBlackboardRegistry registry)
        {
            Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            TimeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            SaveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            VariablePersistence = variablePersistence ?? throw new ArgumentNullException(nameof(variablePersistence));
            TextSubstitution = textSubstitution ?? throw new ArgumentNullException(nameof(textSubstitution));
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        private BlackboardRuntimeServices(IFrameScheduler scheduler, ITimeSource timeSource, IBlackboardEventBus eventBus, IBlackboardSaveService saveService, IBlackboardLogger logger)
        {
            Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            TimeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            SaveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IFrameScheduler Scheduler { get; }

        public ITimeSource TimeSource { get; }

        public IBlackboardEventBus EventBus { get; }

        public IBlackboardSaveService SaveService { get; }

        public IBlackboardLogger Logger { get; }

        public BlackboardVariablePersistence VariablePersistence { get; }

        public ITextSubstitutionService TextSubstitution { get; }

        public IBlackboardRegistry Registry { get; }

        public void Dispose()
        {
            if (Scheduler is IDisposable disposableScheduler)
            {
                disposableScheduler.Dispose();
            }
        }

        internal static BlackboardRuntimeServices CreateExecutionOnly(IFrameScheduler scheduler, ITimeSource timeSource, IBlackboardEventBus eventBus, IBlackboardSaveService saveService, IBlackboardLogger logger)
        {
            return new BlackboardRuntimeServices(scheduler, timeSource, eventBus, saveService, logger);
        }
    }
}
