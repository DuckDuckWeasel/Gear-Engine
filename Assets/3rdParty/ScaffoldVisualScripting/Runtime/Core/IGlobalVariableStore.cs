namespace Scaffold.VisualScripting
{
    public interface IGlobalVariableStore
    {
        VariableCellBase GetOrAdd(VariableDefinitionBase definition);

        bool TryGet(DefinitionId definitionId, out VariableCellBase cell);
    }
}
