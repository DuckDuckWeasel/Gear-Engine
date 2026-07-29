using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting
{
    public sealed class VariableCell<T> : VariableCellBase
    {
        public VariableCell(DefinitionId definitionId, string key, VariableScope scope, T initialValue) : base(definitionId, key, scope)
        {
            cloner = new SerializedGraphCloner();
            this.initialValue = cloner.CloneGraph(initialValue);
            value = CloneInitialValue();
        }

        public override Type ValueType => typeof(T);

        public T Value
        {
            get => value;
            set => SetValue(value);
        }

        public override object UntypedValue
        {
            get => value;
            set => SetUntypedValue(value);
        }

        private readonly SerializedGraphCloner cloner;
        private readonly T initialValue;
        private T value;

        public override void Reset()
        {
            T resetValue = CloneInitialValue();
            SetValue(resetValue);
        }

        private T CloneInitialValue()
        {
            return cloner.CloneGraph(initialValue);
        }

        private void SetUntypedValue(object newValue)
        {
            if (newValue == null && CanAcceptNull())
            {
                SetValue(default);
                return;
            }

            if (!(newValue is T typedValue))
            {
                throw new VariableTypeMismatchException(DefinitionId, typeof(T), newValue?.GetType());
            }

            SetValue(typedValue);
        }

        private bool CanAcceptNull()
        {
            Type valueType = typeof(T);
            return !valueType.IsValueType || Nullable.GetUnderlyingType(valueType) != null;
        }

        private void SetValue(T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(value, newValue))
            {
                return;
            }

            value = newValue;
            RaiseValueChanged();
        }
    }
}
