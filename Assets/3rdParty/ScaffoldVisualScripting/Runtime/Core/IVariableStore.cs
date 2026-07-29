using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public interface IVariableStore
    {
        IReadOnlyCollection<VariableCellBase> Cells { get; }

        void Add(VariableCellBase cell);

        bool TryGet(DefinitionId definitionId, out VariableCellBase cell);

        bool TryGet(string key, out VariableCellBase cell);
    }
}
