using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public sealed class VariableStore : IVariableStore
    {
        public IReadOnlyCollection<VariableCellBase> Cells => cellsById.Values;

        private readonly Dictionary<DefinitionId, VariableCellBase> cellsById = new Dictionary<DefinitionId, VariableCellBase>();
        private readonly Dictionary<string, VariableCellBase> cellsByKey = new Dictionary<string, VariableCellBase>(StringComparer.Ordinal);

        public void Add(VariableCellBase cell)
        {
            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            if (cellsById.ContainsKey(cell.DefinitionId))
            {
                throw new InvalidOperationException($"Variable ID '{cell.DefinitionId}' is already registered.");
            }

            AddKey(cell);
            cellsById.Add(cell.DefinitionId, cell);
        }

        public bool TryGet(DefinitionId definitionId, out VariableCellBase cell)
        {
            return cellsById.TryGetValue(definitionId, out cell);
        }

        public bool TryGet(string key, out VariableCellBase cell)
        {
            return cellsByKey.TryGetValue(key ?? string.Empty, out cell);
        }

        private void AddKey(VariableCellBase cell)
        {
            if (string.IsNullOrWhiteSpace(cell.Key))
            {
                return;
            }

            if (cellsByKey.ContainsKey(cell.Key))
            {
                throw new InvalidOperationException($"Variable key '{cell.Key}' is already registered.");
            }

            cellsByKey.Add(cell.Key, cell);
        }
    }
}
