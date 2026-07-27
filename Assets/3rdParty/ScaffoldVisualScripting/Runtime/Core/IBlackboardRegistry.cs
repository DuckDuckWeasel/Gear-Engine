namespace Scaffold.VisualScripting
{
    public interface IBlackboardRegistry
    {
        void Register(IBlackboardHandle blackboard);

        void Unregister(BlackboardRuntimeInstanceId runtimeInstanceId);

        bool TryGet(BlackboardRuntimeInstanceId runtimeInstanceId, out IBlackboardHandle blackboard);
    }
}
