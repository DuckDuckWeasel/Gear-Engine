namespace Scaffold.VisualScripting
{
    public interface IBlackboardDefinitionVariableSource
    {
        bool TryGetBlackboardDefinition(DefinitionId variableId, out BlackboardDefinition definition);
    }
}
