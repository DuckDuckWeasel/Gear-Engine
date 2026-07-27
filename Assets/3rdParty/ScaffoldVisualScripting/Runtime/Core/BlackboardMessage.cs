using System;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardMessage
    {
        public BlackboardMessage(BlackboardRuntimeInstanceId sourceRuntimeInstanceId, string name, object payload = null, BlackboardRuntimeInstanceId targetRuntimeInstanceId = default)
        {
            SourceRuntimeInstanceId = sourceRuntimeInstanceId;
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("A message name is required.", nameof(name)) : name;
            Payload = payload;
            TargetRuntimeInstanceId = targetRuntimeInstanceId;
        }

        public BlackboardRuntimeInstanceId SourceRuntimeInstanceId { get; }

        public BlackboardRuntimeInstanceId TargetRuntimeInstanceId { get; }

        public string Name { get; }

        public object Payload { get; }

        public bool IsFor(BlackboardRuntimeInstanceId runtimeInstanceId)
        {
            return TargetRuntimeInstanceId.IsEmpty || TargetRuntimeInstanceId == runtimeInstanceId;
        }
    }
}
