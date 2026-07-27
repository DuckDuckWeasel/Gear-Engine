using System;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public sealed class VariableSaveRecord
    {
        public VariableSaveRecord(DefinitionId definitionId, string typeName, string serializedValue)
        {
            DefinitionId = definitionId;
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            SerializedValue = serializedValue ?? string.Empty;
        }

        public DefinitionId DefinitionId { get; }

        public string TypeName { get; }

        public string SerializedValue { get; }
    }
}
