namespace Scaffold.VisualScripting
{
    public sealed class BlackboardStartedEvent
    {
        public BlackboardStartedEvent(BlackboardRuntimeInstanceId runtimeInstanceId)
        {
            RuntimeInstanceId = runtimeInstanceId;
        }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }
    }
}
