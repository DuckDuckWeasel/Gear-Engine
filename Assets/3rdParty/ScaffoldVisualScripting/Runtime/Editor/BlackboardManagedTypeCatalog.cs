using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Scaffold.VisualScripting.Editor
{
    public static class BlackboardManagedTypeCatalog
    {
        public static IReadOnlyList<Type> GetActionTypes(string search = null)
        {
            return GetCreatableTypes<IAction>(search)
                .Where(HasActionMenuMetadata)
                .ToArray();
        }

        public static IReadOnlyList<Type> GetTriggerTypes(string search = null)
        {
            return GetCreatableTypes<TriggerDefinition>(search);
        }

        public static IReadOnlyList<Type> GetVariableTypes(string search = null)
        {
            return GetCreatableTypes<VariableDefinitionBase>(search);
        }

        private static IReadOnlyList<Type> GetCreatableTypes<T>(string search)
        {
            return TypeCache.GetTypesDerivedFrom<T>()
                .Where(IsCreatable)
                .Where(type => MatchesSearch(type, search))
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToArray();
        }

        private static bool IsCreatable(Type type)
        {
            return type != null &&
                type.IsClass &&
                type.IsVisible &&
                !type.IsAbstract &&
                !type.IsGenericTypeDefinition &&
                type.GetConstructor(Type.EmptyTypes) != null;
        }

        private static bool MatchesSearch(Type type, string search)
        {
            return string.IsNullOrWhiteSpace(search) || type.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool HasActionMenuMetadata(Type type)
        {
            return Attribute.IsDefined(type, typeof(global::Scaffold.CommandInfoAttribute), false);
        }
    }
}
