using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Scaffold.VisualScripting.Unity
{
    public sealed class UnityPlayerPrefsBlackboardSaveService : IBlackboardSaveService
    {
        public UnityPlayerPrefsBlackboardSaveService(IBlackboardLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private readonly IBlackboardLogger logger;
        private readonly string keyPrefix = "Scaffold.Blackboard.";

        public Task SaveAsync(string slot, BlackboardSaveData data, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                SaveEnvelope envelope = CreateEnvelope(data ?? throw new ArgumentNullException(nameof(data)));
                PlayerPrefs.SetString(CreateKey(slot, data.RuntimeInstanceId), JsonUtility.ToJson(envelope));
                PlayerPrefs.Save();
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                logger.Error("Failed to save Blackboard data to PlayerPrefs.", exception);
                throw;
            }
        }

        public Task<BlackboardSaveData> LoadAsync(string slot, BlackboardRuntimeInstanceId runtimeInstanceId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                string key = CreateKey(slot, runtimeInstanceId);
                return Task.FromResult(PlayerPrefs.HasKey(key) ? ReadData(key, runtimeInstanceId) : null);
            }
            catch (Exception exception)
            {
                logger.Error("Failed to load Blackboard data from PlayerPrefs.", exception);
                throw;
            }
        }

        public Task DeleteAsync(string slot, BlackboardRuntimeInstanceId runtimeInstanceId, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                PlayerPrefs.DeleteKey(CreateKey(slot, runtimeInstanceId));
                PlayerPrefs.Save();
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                logger.Error("Failed to delete Blackboard data from PlayerPrefs.", exception);
                throw;
            }
        }

        private SaveEnvelope CreateEnvelope(BlackboardSaveData data)
        {
            SaveEnvelope envelope = new SaveEnvelope { RuntimeInstanceId = data.RuntimeInstanceId.Value };
            foreach (VariableSaveRecord variable in data.Variables)
            {
                envelope.Variables.Add(CreateRecord(variable));
            }

            return envelope;
        }

        private SaveRecord CreateRecord(VariableSaveRecord variable)
        {
            return new SaveRecord { DefinitionId = variable.DefinitionId.Value, TypeName = variable.TypeName, SerializedValue = variable.SerializedValue };
        }

        private BlackboardSaveData ReadData(string key, BlackboardRuntimeInstanceId expectedRuntimeId)
        {
            SaveEnvelope envelope = JsonUtility.FromJson<SaveEnvelope>(PlayerPrefs.GetString(key));
            ValidateEnvelope(envelope, expectedRuntimeId);
            List<VariableSaveRecord> variables = new List<VariableSaveRecord>();
            foreach (SaveRecord record in envelope.Variables)
            {
                DefinitionId definitionId = new DefinitionId(record.DefinitionId);
                variables.Add(new VariableSaveRecord(definitionId, record.TypeName, record.SerializedValue));
            }

            return new BlackboardSaveData(expectedRuntimeId, variables);
        }

        private void ValidateEnvelope(SaveEnvelope envelope, BlackboardRuntimeInstanceId expectedRuntimeId)
        {
            if (envelope == null || !string.Equals(envelope.RuntimeInstanceId, expectedRuntimeId.Value, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Stored Blackboard data does not target runtime '{expectedRuntimeId}'.");
            }
        }

        private string CreateKey(string slot, BlackboardRuntimeInstanceId runtimeInstanceId)
        {
            if (string.IsNullOrWhiteSpace(slot))
            {
                throw new ArgumentException("A save slot is required.", nameof(slot));
            }

            return $"{keyPrefix}{runtimeInstanceId.Value}.{slot}";
        }

        [Serializable]
        private sealed class SaveEnvelope
        {
            public string RuntimeInstanceId;
            public List<SaveRecord> Variables = new List<SaveRecord>();
        }

        [Serializable]
        private sealed class SaveRecord
        {
            public string DefinitionId;
            public string TypeName;
            public string SerializedValue;
        }
    }
}
