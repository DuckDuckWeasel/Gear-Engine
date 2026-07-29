using System;
using System.Collections.Generic;

namespace Scaffold
{
    /// <summary>
    /// Discovers all concrete compatibility variable types.
    /// </summary>
    public static class AllVariableTypes
    {
        public enum VariableAny
        {
            Any
        }

        private static Type[] s_allScaffoldVarTypes;

        public static Type[] AllScaffoldVarTypes
        {
            get
            {
                if (s_allScaffoldVarTypes == null)
                {
                    List<Type> types = new List<Type>();
                    Type baseType = typeof(Variable);
                    foreach (System.Reflection.Assembly assembly in
                        AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (assembly.IsDynamic)
                        {
                            continue;
                        }

                        foreach (Type type in assembly.GetExportedTypes())
                        {
                            if (type.IsSubclassOf(baseType) &&
                                !type.IsAbstract)
                            {
                                types.Add(type);
                            }
                        }
                    }

                    s_allScaffoldVarTypes = types.ToArray();
                }

                return s_allScaffoldVarTypes;
            }
        }
    }
}
