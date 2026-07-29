namespace Scaffold.VisualScripting
{
    public readonly struct BlackboardBlockStartedEvent
    {
        public BlackboardBlockStartedEvent(BlackboardRuntimeInstanceId runtimeInstanceId, DefinitionId blockDefinitionId, string blockName, ExecutionId executionId)
        {
            RuntimeInstanceId = runtimeInstanceId;
            BlockDefinitionId = blockDefinitionId;
            BlockName = blockName ?? string.Empty;
            ExecutionId = executionId;
        }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }

        public DefinitionId BlockDefinitionId { get; }

        public string BlockName { get; }

        public ExecutionId ExecutionId { get; }
    }
}
