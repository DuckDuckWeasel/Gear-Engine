using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class BlackboardSaveData
    {
        public BlackboardSaveData(BlackboardRuntimeInstanceId runtimeInstanceId, IReadOnlyList<VariableSaveRecord> variables)
        {
            RuntimeInstanceId = runtimeInstanceId;
            Variables = variables ?? throw new ArgumentNullException(nameof(variables));
        }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }

        public IReadOnlyList<VariableSaveRecord> Variables { get; }
    }
}
