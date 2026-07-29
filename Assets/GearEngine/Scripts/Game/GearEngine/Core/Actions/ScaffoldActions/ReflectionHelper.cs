using System;
using System.Collections.Generic;
using System.Linq;

namespace Scaffold
{
    public static class ReflectionHelper
    {
        private static Dictionary<string, Type> Types { get; } = new Dictionary<string, Type>();

        public static Type GetType(string assemblyQualifiedTypeName)
        {
            if (Types.TryGetValue(assemblyQualifiedTypeName, out Type cachedType) && cachedType != null)
            {
                return cachedType;
            }

            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
            Type resolvedType = types.FirstOrDefault(type => type.AssemblyQualifiedName == assemblyQualifiedTypeName);
            Types[assemblyQualifiedTypeName] = resolvedType;
            return resolvedType;
        }
    }
}
