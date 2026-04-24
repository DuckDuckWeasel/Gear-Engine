using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring.Editor.Window
{
    /// <summary>
    /// Collects <see cref="ScriptableObject"/> references from a builder's serialized fields (any depth).
    /// </summary>
    public static class ConfigReferencedScriptableObjects
    {
        public static IReadOnlyList<ScriptableObject> Enumerate(ConfigBuilderSOBase builder)
        {
            var set = new HashSet<ScriptableObject>();
            if (builder == null)
            {
                return System.Array.Empty<ScriptableObject>();
            }

            var so = new SerializedObject(builder);
            SerializedProperty it = so.GetIterator();
            bool enterChildren = true;
            while (it.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (it.propertyType != SerializedPropertyType.ObjectReference)
                {
                    continue;
                }

                if (it.objectReferenceValue is ScriptableObject sob && sob != null && sob != builder)
                {
                    set.Add(sob);
                }
            }

            return new List<ScriptableObject>(set);
        }
    }
}
