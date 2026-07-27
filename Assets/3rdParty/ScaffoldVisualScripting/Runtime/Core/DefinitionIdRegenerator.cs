using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Scaffold.VisualScripting
{
    public sealed class DefinitionIdRegenerator
    {
        public DefinitionIdRegenerator()
        {
            visitCallback = Visit;
        }

        private readonly Action<object, ISet<object>> visitCallback;

        public void Regenerate(object root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            HashSet<object> visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            Visit(root, visited);
        }

        private void Visit(object value, ISet<object> visited)
        {
            if (ShouldSkip(value))
            {
                return;
            }

            Type type = value.GetType();
            if (!type.IsValueType && !visited.Add(value))
            {
                return;
            }

            if (value is DefinitionNode node)
            {
                node.RegenerateDefinitionId();
            }

            VisitMembers(value, type, visited);
        }

        private bool ShouldSkip(object value)
        {
            if (value == null || value is Object)
            {
                return true;
            }

            Type type = value.GetType();
            return type.IsPrimitive || type.IsEnum || type == typeof(string);
        }

        private void VisitMembers(object value, Type type, ISet<object> visited)
        {
            if (value is IDictionary dictionary)
            {
                VisitDictionary(dictionary, visited);
                return;
            }

            if (value is IEnumerable enumerable)
            {
                VisitEnumerable(enumerable, visited);
                return;
            }

            VisitFields(value, type, visited);
        }

        private void VisitDictionary(IDictionary dictionary, ISet<object> visited)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                visitCallback(entry.Key, visited);
                visitCallback(entry.Value, visited);
            }
        }

        private void VisitEnumerable(IEnumerable enumerable, ISet<object> visited)
        {
            foreach (object item in enumerable)
            {
                visitCallback(item, visited);
            }
        }

        private void VisitFields(object value, Type type, ISet<object> visited)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                VisitDeclaredFields(value, current, visited);
            }
        }

        private void VisitDeclaredFields(object value, Type type, ISet<object> visited)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            FieldInfo[] fields = type.GetFields(flags);
            foreach (FieldInfo field in fields)
            {
                if (!field.IsStatic && !typeof(Delegate).IsAssignableFrom(field.FieldType))
                {
                    visitCallback(field.GetValue(value), visited);
                }
            }
        }
    }
}
