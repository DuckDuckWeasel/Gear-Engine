using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public sealed class BlackboardVariablePersistence
    {
        public BlackboardVariablePersistence(IVariableValueSerializer serializer, IBlackboardLogger logger)
        {
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly IVariableValueSerializer serializer;
        private readonly IBlackboardLogger logger;

        public BlackboardSaveData Capture(BlackboardVariableSet variables)
        {
            try
            {
                return CaptureValues(variables ?? throw new ArgumentNullException(nameof(variables)));
            }
            catch (Exception exception)
            {
                logger.Error("Failed to capture Blackboard variables.", exception);
                throw;
            }
        }

        public void Apply(BlackboardSaveData data, BlackboardVariableSet variables)
        {
            try
            {
                ApplyValues(data ?? throw new ArgumentNullException(nameof(data)), variables ?? throw new ArgumentNullException(nameof(variables)));
            }
            catch (Exception exception)
            {
                logger.Error("Failed to apply Blackboard variables.", exception);
                throw;
            }
        }

        private BlackboardSaveData CaptureValues(BlackboardVariableSet variables)
        {
            List<VariableSaveRecord> records = new List<VariableSaveRecord>();
            foreach (VariableCellBase cell in variables.Cells)
            {
                records.Add(CaptureCell(cell));
            }

            return new BlackboardSaveData(variables.RuntimeInstanceId, records);
        }

        private VariableSaveRecord CaptureCell(VariableCellBase cell)
        {
            string serializedValue = serializer.Serialize(cell.ValueType, cell.UntypedValue);
            string typeName = cell.ValueType.AssemblyQualifiedName;
            return new VariableSaveRecord(cell.DefinitionId, typeName, serializedValue);
        }

        private void ApplyValues(BlackboardSaveData data, BlackboardVariableSet variables)
        {
            if (data.RuntimeInstanceId != variables.RuntimeInstanceId)
            {
                throw new InvalidOperationException($"Save data targets Blackboard '{data.RuntimeInstanceId}', not '{variables.RuntimeInstanceId}'.");
            }

            foreach (VariableSaveRecord record in data.Variables)
            {
                ApplyRecord(record, variables);
            }
        }

        private void ApplyRecord(VariableSaveRecord record, BlackboardVariableSet variables)
        {
            if (!variables.TryGet(record.DefinitionId, out VariableCellBase cell))
            {
                throw new KeyNotFoundException($"Saved variable '{record.DefinitionId}' is not registered.");
            }

            ValidateRecordType(record, cell);
            cell.UntypedValue = serializer.Deserialize(cell.ValueType, record.SerializedValue);
        }

        private void ValidateRecordType(VariableSaveRecord record, VariableCellBase cell)
        {
            if (!string.Equals(record.TypeName, cell.ValueType.AssemblyQualifiedName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Saved variable '{record.DefinitionId}' has incompatible type '{record.TypeName}'.");
            }
        }
    }
}
