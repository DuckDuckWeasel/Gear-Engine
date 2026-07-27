using System;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public abstract class VariableDefinitionBase : DefinitionNode
    {
        public abstract string Key { get; set; }

        public abstract VariableScope Scope { get; set; }

        public abstract Type ValueType { get; }

        internal abstract VariableCellBase CreateCell();
    }
}
