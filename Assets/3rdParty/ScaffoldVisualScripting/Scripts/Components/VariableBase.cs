using UnityEngine;
using Scaffold.VisualScripting;

namespace Scaffold
{
    /// <summary>
    /// Generic plain-C# compatibility base for legacy variable value types.
    /// </summary>
    public abstract class VariableBase<T> : Variable
    {
        [SerializeField] protected T value;

        [System.NonSerialized] private T runtimeValue;
        [System.NonSerialized] private bool runtimeValueInitialized;
        [System.NonSerialized] private VariableCellBase boundCell;

        public override System.Type ValueType => typeof(T);

        public virtual T Value
        {
            get
            {
                if (boundCell != null)
                {
                    return GetBoundValue();
                }

                EnsureRuntimeValue();
                return runtimeValue;
            }
            set
            {
                if (boundCell != null)
                {
                    boundCell.UntypedValue = value;
                    return;
                }

                EnsureRuntimeValue();
                runtimeValue = value;
            }
        }

        public override object GetValue()
        {
            return Value;
        }

        public override void OnReset()
        {
            if (boundCell != null)
            {
                return;
            }

            runtimeValue = value;
            runtimeValueInitialized = true;
        }

        public override void Bind(VariableCellBase cell)
        {
            if (cell == null)
            {
                throw new System.ArgumentNullException(nameof(cell));
            }

            if (!CanBind(cell.ValueType))
            {
                throw new VariableTypeMismatchException(
                    cell.DefinitionId,
                    typeof(T),
                    cell.ValueType);
            }

            boundCell = cell;
        }

        public override void Unbind()
        {
            boundCell = null;
        }

        public override string ToString()
        {
            return Value != null ? Value.ToString() : "Null";
        }

        public override void Apply(SetOperator op, object incomingValue)
        {
            if (incomingValue is T || incomingValue == null)
            {
                Apply(op, (T)incomingValue);
                return;
            }

            if (incomingValue is VariableBase<T> variable)
            {
                Apply(op, variable.Value);
                return;
            }

            Debug.LogError(
                $"Cannot apply a value of type {incomingValue.GetType().Name} to {typeof(T).Name}.");
        }

        public virtual void Apply(SetOperator setOperator, T incomingValue)
        {
            if (setOperator == SetOperator.Assign)
            {
                Value = incomingValue;
                return;
            }

            Debug.LogError($"The {setOperator} set operator is not valid.");
        }

        public override bool Evaluate(
            CompareOperator op,
            object incomingValue)
        {
            if (incomingValue is T || incomingValue == null)
            {
                return Evaluate(op, (T)incomingValue);
            }

            if (incomingValue is VariableBase<T> variable)
            {
                return Evaluate(op, variable.Value);
            }

            Debug.LogError(
                $"Cannot compare a value of type {incomingValue.GetType().Name} with {typeof(T).Name}.");
            return false;
        }

        public virtual bool Evaluate(
            CompareOperator compareOperator,
            T incomingValue)
        {
            switch (compareOperator)
            {
                case CompareOperator.Equals:
                    return Equals(Value, incomingValue);
                case CompareOperator.NotEquals:
                    return !Equals(Value, incomingValue);
                default:
                    Debug.LogError(
                        $"The {compareOperator} comparison operator is not valid.");
                    return false;
            }
        }

        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return setOperator == SetOperator.Assign ||
                base.IsArithmeticSupported(setOperator);
        }

        private void EnsureRuntimeValue()
        {
            if (runtimeValueInitialized)
            {
                return;
            }

            runtimeValue = value;
            runtimeValueInitialized = true;
        }

        private static bool CanBind(System.Type cellValueType)
        {
            return cellValueType == typeof(T) ||
                cellValueType.IsAssignableFrom(typeof(T));
        }

        private T GetBoundValue()
        {
            object boundValue = boundCell.UntypedValue;
            if (boundValue == null)
            {
                return default;
            }

            if (boundValue is T typedValue)
            {
                return typedValue;
            }

            throw new VariableTypeMismatchException(
                boundCell.DefinitionId,
                typeof(T),
                boundValue.GetType());
        }
    }
}
