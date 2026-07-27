namespace Scaffold.VisualScripting
{
    public readonly struct BlackboardBlockCompletedEvent
    {
        public BlackboardBlockCompletedEvent(BlackboardRuntimeInstanceId runtimeInstanceId, DefinitionId blockDefinitionId, string blockName, ExecutionId executionId, ActionExecutionStatus status)
        {
            RuntimeInstanceId = runtimeInstanceId;
            BlockDefinitionId = blockDefinitionId;
            BlockName = blockName ?? string.Empty;
            ExecutionId = executionId;
            Status = status;
        }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }

        public DefinitionId BlockDefinitionId { get; }

        public string BlockName { get; }

        public ExecutionId ExecutionId { get; }

        public ActionExecutionStatus Status { get; }
    }
}
