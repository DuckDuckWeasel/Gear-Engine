namespace Scaffold.VisualScripting
{
    public interface IPublicVariableRegistry
    {
        void Register(VariableAddress address, VariableCellBase cell);

        void Unregister(BlackboardRuntimeInstanceId runtimeInstanceId);

        bool TryGet(VariableAddress address, out VariableCellBase cell);
    }
}
