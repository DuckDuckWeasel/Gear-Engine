namespace Scaffold.VisualScripting
{
    public sealed class BlackboardDisabledEvent
    {
        public BlackboardDisabledEvent(BlackboardRuntimeInstanceId runtimeInstanceId)
        {
            RuntimeInstanceId = runtimeInstanceId;
        }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }
    }
}
