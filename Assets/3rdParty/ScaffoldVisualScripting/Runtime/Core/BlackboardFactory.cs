using System;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardFactory
    {
        public BlackboardFactory(SerializedGraphCloner cloner, BlackboardDefinitionValidator validator, IBlackboardRuntimeServicesFactory servicesFactory, IPublicVariableRegistry publicVariables, IGlobalVariableStore globalVariables, IRandomSource randomSource, IBlackboardLogger logger)
        {
            this.cloner = cloner ?? throw new ArgumentNullException(nameof(cloner));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.servicesFactory = servicesFactory ?? throw new ArgumentNullException(nameof(servicesFactory));
            this.publicVariables = publicVariables ?? throw new ArgumentNullException(nameof(publicVariables));
            this.globalVariables = globalVariables ?? throw new ArgumentNullException(nameof(globalVariables));
            this.randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly SerializedGraphCloner cloner;
        private readonly BlackboardDefinitionValidator validator;
        private readonly IBlackboardRuntimeServicesFactory servicesFactory;
        private readonly IPublicVariableRegistry publicVariables;
        private readonly IGlobalVariableStore globalVariables;
        private readonly IRandomSource randomSource;
        private readonly IBlackboardLogger logger;

        public Blackboard Create(BlackboardDefinition template)
        {
            BlackboardRuntimeServices services = null;
            try
            {
                services = servicesFactory.Create();
                return CreateValidated(template, services);
            }
            catch (Exception exception)
            {
                services?.Dispose();
                logger.Error("Failed to create a Blackboard runtime.", exception);
                throw;
            }
        }

        private Blackboard CreateValidated(BlackboardDefinition template, BlackboardRuntimeServices services)
        {
            validator.ValidateOrThrow(template);
            BlackboardDefinitionClone clone = cloner.Clone(template);
            validator.ValidateOrThrow(clone.Definition);
            BlackboardVariableSet variables = CreateVariables(clone);
            return CreateRuntime(clone, variables, services);
        }

        private BlackboardVariableSet CreateVariables(BlackboardDefinitionClone clone)
        {
            return new BlackboardVariableSet(clone.RuntimeInstanceId, clone.Definition.Variables, publicVariables, globalVariables);
        }

        private Blackboard CreateRuntime(BlackboardDefinitionClone clone, BlackboardVariableSet variables, BlackboardRuntimeServices services)
        {
            try
            {
                return new Blackboard(clone, variables, services, randomSource, true);
            }
            catch
            {
                variables.Dispose();
                throw;
            }
        }
    }
}
