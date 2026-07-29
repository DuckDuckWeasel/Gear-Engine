
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Scaffold.VisualScripting;

namespace Scaffold
{
    /// <summary>
    /// Abstract base class for variables.
    /// </summary>
    [System.Serializable]
    public abstract class Variable
    {
        [SerializeField] protected VariableScope scope;

        [SerializeField] protected string key = "";

        [SerializeField] private DefinitionId definitionId;

        public string Name => Key;

        #region Public members

        /// <summary>
        /// Visibility scope for the variable.
        /// </summary>
        public virtual VariableScope Scope { get { return scope; } set { scope = value; } }

        /// <summary>
        /// String identifier for the variable.
        /// </summary>
        public virtual string Key { get { return key; } set { key = value; } }

        /// <summary>
        /// Stable identifier of the managed Blackboard variable selected by
        /// this compatibility reference.
        /// </summary>
        public DefinitionId DefinitionId
        {
            get => definitionId;
            set => definitionId = value;
        }

        /// <summary>
        /// Value type accepted by this compatibility variable.
        /// </summary>
        public virtual System.Type ValueType => typeof(object);

        /// <summary>
        /// Connects this compatibility reference to a managed runtime cell.
        /// </summary>
        public virtual void Bind(VariableCellBase cell)
        {
            throw new System.InvalidOperationException(
                $"Variable type '{GetType().Name}' does not support managed Blackboard binding.");
        }

        /// <summary>
        /// Removes a previously established managed runtime binding.
        /// </summary>
        public virtual void Unbind()
        {
        }

        /// <summary>
        /// Connects every retained compatibility variable in a serialized
        /// object graph to its managed Blackboard runtime cell.
        /// </summary>
        public static void BindAll(
            object root,
            BlackboardVariableSet variables)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (variables == null)
            {
                throw new ArgumentNullException(nameof(variables));
            }

            HashSet<object> visited = new HashSet<object>(
                ReferenceComparer.Instance);
            Visit(root, variables, visited);
        }

        /// <summary>
        /// Callback to reset the variable if the Blackboard is reset.
        /// </summary>
        public abstract void OnReset();

        /// <summary>
        /// Used by SetVariable, child classes required to declare and implement operators.
        /// </summary>
        /// <param name="setOperator"></param>
        /// <param name="value"></param>
        public abstract void Apply(SetOperator setOperator, object value);

        /// <summary>
        /// Used by Ifs, While, and the like. Child classes required to declare and implement comparisons.
        /// </summary>
        /// <param name="compareOperator"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool Evaluate(CompareOperator compareOperator, object value);

        /// <summary>
        /// Does the underlying type provide support for +-*/
        /// </summary>
        public virtual bool IsArithmeticSupported(SetOperator setOperator) { return false; }

        /// <summary>
        /// Does the underlying type provide support for < <= > >=
        /// </summary>
        public virtual bool IsComparisonSupported() { return false; }

        /// <summary>
        /// Boxed or referenced value of type defined within inherited types.
        /// Not recommended for direct use, primarily intended for use in editor code.
        /// </summary>
        public abstract object GetValue();

        #endregion

        private static void Visit(
            object value,
            BlackboardVariableSet variables,
            ISet<object> visited)
        {
            if (value == null || IsTerminal(value))
            {
                return;
            }

            Type type = value.GetType();
            if (!type.IsValueType && !visited.Add(value))
            {
                return;
            }

            if (value is Variable variable)
            {
                BindVariable(variable, variables);
                return;
            }

            if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    Visit(item, variables, visited);
                }

                return;
            }

            VisitFields(value, type, variables, visited);
        }

        private static void BindVariable(
            Variable variable,
            BlackboardVariableSet variables)
        {
            variable.Unbind();
            if (!TryResolveCell(variable, variables, out VariableCellBase cell))
            {
                return;
            }

            try
            {
                variable.Bind(cell);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to bind compatibility variable '{variable.Key}': " +
                    $"{exception.Message}\n{exception.StackTrace}");
            }
        }

        private static bool TryResolveCell(
            Variable variable,
            BlackboardVariableSet variables,
            out VariableCellBase cell)
        {
            if (!variable.DefinitionId.IsEmpty &&
                variables.TryGet(variable.DefinitionId, out cell))
            {
                return true;
            }

            return variables.TryGet(variable.Key, out cell);
        }

        private static void VisitFields(
            object value,
            Type type,
            BlackboardVariableSet variables,
            ISet<object> visited)
        {
            for (Type current = type;
                 current != null;
                 current = current.BaseType)
            {
                BindingFlags flags =
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly;
                foreach (FieldInfo field in current.GetFields(flags))
                {
                    if (ShouldVisit(field))
                    {
                        Visit(
                            field.GetValue(value),
                            variables,
                            visited);
                    }
                }
            }
        }

        private static bool ShouldVisit(FieldInfo field)
        {
            if (field.IsStatic ||
                field.IsNotSerialized ||
                field.IsDefined(
                    typeof(BlackboardTransientAttribute),
                    true))
            {
                return false;
            }

            return field.IsPublic ||
                field.IsDefined(typeof(SerializeField), true) ||
                field.IsDefined(typeof(SerializeReference), true);
        }

        private static bool IsTerminal(object value)
        {
            Type type = value.GetType();
            return value is UnityEngine.Object ||
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid) ||
                typeof(Delegate).IsAssignableFrom(type);
        }

        private sealed class ReferenceComparer :
            IEqualityComparer<object>
        {
            public static ReferenceComparer Instance { get; } =
                new ReferenceComparer();

            public new bool Equals(object left, object right)
            {
                return ReferenceEquals(left, right);
            }

            public int GetHashCode(object value)
            {
                return System.Runtime.CompilerServices
                    .RuntimeHelpers.GetHashCode(value);
            }
        }
    }

}
