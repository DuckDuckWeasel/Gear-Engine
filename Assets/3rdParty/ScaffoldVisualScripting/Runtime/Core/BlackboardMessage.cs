using System;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardMessage
    {
        public BlackboardMessage(BlackboardRuntimeInstanceId sourceRuntimeInstanceId, string name, object payload = null)
        {
            SourceRuntimeInstanceId = sourceRuntimeInstanceId;
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("A message name is required.", nameof(name)) : name;
            Payload = payload;
        }

        public BlackboardRuntimeInstanceId SourceRuntimeInstanceId { get; }

        public string Name { get; }

        public object Payload { get; }
    }
}
