using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Scaffold.VisualScripting
{
    public sealed class SerializedGraphCloner
    {
        public SerializedGraphCloner()
        {
            memberwiseCloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            cloneValueCallback = CloneValue;
        }

        private readonly MethodInfo memberwiseCloneMethod;
        private readonly Func<object, IDictionary<object, object>, object> cloneValueCallback;

        public BlackboardDefinitionClone Clone(BlackboardDefinition source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            BlackboardDefinition definition = CloneGraph(source);
            BlackboardRuntimeInstanceId instanceId = BlackboardRuntimeInstanceId.New();
            return new BlackboardDefinitionClone(definition, instanceId);
        }

        public T CloneGraph<T>(T source)
        {
            if (source == null)
            {
                return default;
            }

            Dictionary<object, object> visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            return (T)CloneValue(source, visited);
        }

        private object CloneValue(object source, IDictionary<object, object> visited)
        {
            if (source == null)
            {
                return null;
            }

            Type type = source.GetType();
            if (IsPreservedValue(source, type))
            {
                return source;
            }

            if (!type.IsValueType && visited.TryGetValue(source, out object existing))
            {
                return existing;
            }

            return CloneNewValue(source, type, visited);
        }

        private object CloneNewValue(object source, Type type, IDictionary<object, object> visited)
        {
            if (source is Delegate)
            {
                return null;
            }

            if (source is Array array)
            {
                return CloneArray(array, visited);
            }

            if (source is IDictionary dictionary)
            {
                return CloneDictionary(dictionary, type, visited);
            }

            return source is IList list ? CloneList(list, type, visited) : CloneManagedObject(source, type, visited);
        }

        private object CloneManagedObject(object source, Type type, IDictionary<object, object> visited)
        {
            object clone = memberwiseCloneMethod.Invoke(source, null);
            if (!type.IsValueType)
            {
                visited.Add(source, clone);
            }

            CloneFields(source, clone, type, visited);
            return clone;
        }

        private Array CloneArray(Array source, IDictionary<object, object> visited)
        {
            Type elementType = source.GetType().GetElementType();
            int[] lengths = CreateArrayLengths(source);
            int[] lowerBounds = CreateArrayLowerBounds(source);
            Array clone = Array.CreateInstance(elementType, lengths, lowerBounds);
            visited.Add(source, clone);
            int[] indices = new int[source.Rank];
            CloneArrayDimension(source, clone, visited, indices, 0);
            return clone;
        }

        private int[] CreateArrayLengths(Array source)
        {
            int[] lengths = new int[source.Rank];
            for (int dimension = 0; dimension < source.Rank; dimension++)
            {
                lengths[dimension] = source.GetLength(dimension);
            }

            return lengths;
        }

        private int[] CreateArrayLowerBounds(Array source)
        {
            int[] lowerBounds = new int[source.Rank];
            for (int dimension = 0; dimension < source.Rank; dimension++)
            {
                lowerBounds[dimension] = source.GetLowerBound(dimension);
            }

            return lowerBounds;
        }

        private void CloneArrayDimension(Array source, Array clone, IDictionary<object, object> visited, int[] indices, int dimension)
        {
            int lower = source.GetLowerBound(dimension);
            int upper = source.GetUpperBound(dimension);
            for (int index = lower; index <= upper; index++)
            {
                indices[dimension] = index;
                if (dimension + 1 < source.Rank)
                {
                    CloneArrayDimension(source, clone, visited, indices, dimension + 1);
                    continue;
                }

                object sourceValue = source.GetValue(indices);
                object clonedValue = cloneValueCallback(sourceValue, visited);
                clone.SetValue(clonedValue, indices);
            }
        }

        private object CloneDictionary(IDictionary source, Type type, IDictionary<object, object> visited)
        {
            IDictionary clone = CreateCollection<IDictionary>(type);
            visited.Add(source, clone);
            foreach (DictionaryEntry entry in source)
            {
                object clonedKey = cloneValueCallback(entry.Key, visited);
                object clonedValue = cloneValueCallback(entry.Value, visited);
                clone.Add(clonedKey, clonedValue);
            }

            return clone;
        }

        private object CloneList(IList source, Type type, IDictionary<object, object> visited)
        {
            IList clone = CreateCollection<IList>(type);
            visited.Add(source, clone);
            foreach (object item in source)
            {
                object clonedItem = cloneValueCallback(item, visited);
                clone.Add(clonedItem);
            }

            return clone;
        }

        private TCollection CreateCollection<TCollection>(Type type) where TCollection : class
        {
            try
            {
                object instance = Activator.CreateInstance(type, true);
                return instance as TCollection ?? throw new InvalidOperationException($"Collection type '{type.FullName}' could not be created.");
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Collection type '{type.FullName}' requires a parameterless constructor.", exception);
            }
        }

        private void CloneFields(object source, object clone, Type type, IDictionary<object, object> visited)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                CloneDeclaredFields(source, clone, current, visited);
            }
        }

        private void CloneDeclaredFields(object source, object clone, Type type, IDictionary<object, object> visited)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            FieldInfo[] fields = type.GetFields(flags);
            foreach (FieldInfo field in fields)
            {
                CloneField(source, clone, field, visited);
            }
        }

        private void CloneField(object source, object clone, FieldInfo field, IDictionary<object, object> visited)
        {
            if (field.IsStatic)
            {
                return;
            }

            object value = ShouldReset(field) ? GetDefaultValue(field.FieldType) : cloneValueCallback(field.GetValue(source), visited);
            field.SetValue(clone, value);
        }

        private bool ShouldReset(FieldInfo field)
        {
            return field.IsNotSerialized || typeof(Delegate).IsAssignableFrom(field.FieldType) || field.IsDefined(typeof(BlackboardTransientAttribute), true);
        }

        private object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private bool IsPreservedValue(object source, Type type)
        {
            return source is Object || IsImmutable(type);
        }

        private bool IsImmutable(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan) || type == typeof(Guid) || type == typeof(Type);
        }
    }
}
