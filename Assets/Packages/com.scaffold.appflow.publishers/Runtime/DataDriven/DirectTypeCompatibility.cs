using System;
using System.Collections.Generic;
namespace Scaffold.AppFlow.Publishers.DataDriven
{
    internal static class DirectTypeCompatibility
    {
        /// <summary>Least common ancestor in the C# class hierarchy, or <c>null</c> if either is null.</summary>
        internal static Type LeastCommonAncestor(Type a, Type b)
        {
            if (a == null)
            {
                return b;
            }

            if (b == null)
            {
                return a;
            }

            var set = new HashSet<Type>();
            for (Type t = a; t != null; t = t.BaseType)
            {
                set.Add(t);
            }

            for (Type t = b; t != null; t = t.BaseType)
            {
                if (set.Contains(t))
                {
                    return t;
                }
            }

            return typeof(object);
        }
    }
}
