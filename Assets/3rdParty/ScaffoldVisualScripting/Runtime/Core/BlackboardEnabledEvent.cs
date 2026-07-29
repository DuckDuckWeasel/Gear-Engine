namespace Scaffold.VisualScripting
{
    public sealed class BlackboardEnabledEvent
    {
        public BlackboardEnabledEvent(BlackboardRuntimeInstanceId runtimeInstanceId)
        {
            RuntimeInstanceId = runtimeInstanceId;
        }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }
    }
}
