using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardRegistry : IBlackboardRegistry
    {
        private readonly Dictionary<BlackboardRuntimeInstanceId, IBlackboardHandle> blackboards = new Dictionary<BlackboardRuntimeInstanceId, IBlackboardHandle>();

        public void Register(IBlackboardHandle blackboard)
        {
            if (blackboard == null)
            {
                throw new ArgumentNullException(nameof(blackboard));
            }

            if (blackboards.ContainsKey(blackboard.RuntimeInstanceId))
            {
                throw new InvalidOperationException($"Blackboard '{blackboard.RuntimeInstanceId}' is already registered.");
            }

            blackboards.Add(blackboard.RuntimeInstanceId, blackboard);
        }

        public void Unregister(BlackboardRuntimeInstanceId runtimeInstanceId)
        {
            blackboards.Remove(runtimeInstanceId);
        }

        public bool TryGet(BlackboardRuntimeInstanceId runtimeInstanceId, out IBlackboardHandle blackboard)
        {
            return blackboards.TryGetValue(runtimeInstanceId, out blackboard);
        }
    }
}
