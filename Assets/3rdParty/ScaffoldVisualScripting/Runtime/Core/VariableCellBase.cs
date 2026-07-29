using System;

namespace Scaffold.VisualScripting
{
    public abstract class VariableCellBase
    {
        protected VariableCellBase(DefinitionId definitionId, string key, VariableScope scope)
        {
            DefinitionId = definitionId;
            Key = key ?? string.Empty;
            Scope = scope;
        }

        public DefinitionId DefinitionId { get; }

        public string Key { get; }

        public VariableScope Scope { get; }

        public abstract Type ValueType { get; }

        public abstract object UntypedValue { get; set; }

        public event Action<VariableCellBase> ValueChanged;

        public abstract void Reset();

        protected void RaiseValueChanged()
        {
            ValueChanged?.Invoke(this);
        }
    }
}
