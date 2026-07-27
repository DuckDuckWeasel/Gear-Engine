using System;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardFactory
    {
        public BlackboardFactory(SerializedGraphCloner cloner, BlackboardDefinitionValidator validator, BlackboardRuntimeServices services, IPublicVariableRegistry publicVariables, IGlobalVariableStore globalVariables, IRandomSource randomSource)
        {
            this.cloner = cloner ?? throw new ArgumentNullException(nameof(cloner));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.publicVariables = publicVariables ?? throw new ArgumentNullException(nameof(publicVariables));
            this.globalVariables = globalVariables ?? throw new ArgumentNullException(nameof(globalVariables));
            this.randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
        }

        private readonly SerializedGraphCloner cloner;
        private readonly BlackboardDefinitionValidator validator;
        private readonly BlackboardRuntimeServices services;
        private readonly IPublicVariableRegistry publicVariables;
        private readonly IGlobalVariableStore globalVariables;
        private readonly IRandomSource randomSource;

        public Blackboard Create(BlackboardDefinition template)
        {
            try
            {
                return CreateValidated(template);
            }
            catch (Exception exception)
            {
                services.Logger.Error("Failed to create a Blackboard runtime.", exception);
                throw;
            }
        }

        private Blackboard CreateValidated(BlackboardDefinition template)
        {
            validator.ValidateOrThrow(template);
            BlackboardDefinitionClone clone = cloner.Clone(template);
            validator.ValidateOrThrow(clone.Definition);
            BlackboardVariableSet variables = CreateVariables(clone);
            return CreateRuntime(clone, variables);
        }

        private BlackboardVariableSet CreateVariables(BlackboardDefinitionClone clone)
        {
            return new BlackboardVariableSet(clone.RuntimeInstanceId, clone.Definition.Variables, publicVariables, globalVariables);
        }

        private Blackboard CreateRuntime(BlackboardDefinitionClone clone, BlackboardVariableSet variables)
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
