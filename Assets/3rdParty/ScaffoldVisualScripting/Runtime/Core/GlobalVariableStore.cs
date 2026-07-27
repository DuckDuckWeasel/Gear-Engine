using System;

namespace Scaffold.VisualScripting
{
    public sealed class GlobalVariableStore : IGlobalVariableStore
    {
        private readonly VariableStore store = new VariableStore();

        public VariableCellBase GetOrAdd(VariableDefinitionBase definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (store.TryGet(definition.DefinitionId, out VariableCellBase existing))
            {
                ValidateExistingType(definition, existing);
                return existing;
            }

            VariableCellBase cell = definition.CreateCell();
            store.Add(cell);
            return cell;
        }

        public bool TryGet(DefinitionId definitionId, out VariableCellBase cell)
        {
            return store.TryGet(definitionId, out cell);
        }

        private void ValidateExistingType(VariableDefinitionBase definition, VariableCellBase existing)
        {
            if (definition.ValueType != existing.ValueType)
            {
                throw new VariableTypeMismatchException(definition.DefinitionId, definition.ValueType, existing.ValueType);
            }
        }
    }
}
