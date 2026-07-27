using System;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardDefinitionClone
    {
        public BlackboardDefinitionClone(BlackboardDefinition definition, BlackboardRuntimeInstanceId runtimeInstanceId)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            RuntimeInstanceId = runtimeInstanceId;
        }

        public BlackboardDefinition Definition { get; }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }
    }
}
