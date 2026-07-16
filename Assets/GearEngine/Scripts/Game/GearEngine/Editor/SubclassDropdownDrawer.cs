using UnityEditor;
using UnityEngine;
using System.Linq;
using GearEngine.GearEngine.Presentation.UI.Input;
using System;
using System.Collections.Generic;

namespace GearEngine.GearEngine.Editor
{
    [CustomPropertyDrawer(typeof(SubclassDropdownAttribute))]
    public class SubclassDropdownDrawer : PropertyDrawer
    {
        private static Dictionary<string, string[]> _displayOptionsCache = new Dictionary<string, string[]>();
        private static Dictionary<string, List<string>> _typeNamesCache = new Dictionary<string, List<string>>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use SubclassDropdown with string.");
                return;
            }

            SubclassDropdownAttribute attr = attribute as SubclassDropdownAttribute;
            
            if (!_displayOptionsCache.ContainsKey(attr.BaseTypeName))
            {
                BuildCache(attr.BaseTypeName);
            }

            string[] displayOptions = _displayOptionsCache[attr.BaseTypeName];
            List<string> typeNames = _typeNamesCache[attr.BaseTypeName];

            if (displayOptions.Length == 0)
            {
                EditorGUI.LabelField(position, label.text, "No subclasses found");
                return;
            }

            int currentIndex = Mathf.Max(0, typeNames.IndexOf(property.stringValue));
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, displayOptions);

            if (newIndex >= 0 && newIndex < typeNames.Count)
            {
                property.stringValue = typeNames[newIndex];
            }
        }

        private static void BuildCache(string baseTypeName)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName.StartsWith("UnityEditor") && !a.FullName.StartsWith("UnityEngine"))
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass)
                .ToList();

            List<Type> matchingTypes = new List<Type>();

            if (baseTypeName == "View")
            {
                matchingTypes = allTypes.Where(t => InheritsFromViewGeneric(t) || InheritsFrom(t, "View")).ToList();
            }
            else
            {
                matchingTypes = allTypes.Where(t => InheritsFrom(t, baseTypeName)).ToList();
                
                if (matchingTypes.Count == 0 && baseTypeName.EndsWith("Event"))
                {
                    matchingTypes = allTypes.Where(t => t.Name.EndsWith("Event")).ToList();
                }
            }

            matchingTypes = matchingTypes.OrderBy(t => t.Name).ToList();

            _typeNamesCache[baseTypeName] = matchingTypes.Select(t => t.FullName).ToList();
            _displayOptionsCache[baseTypeName] = matchingTypes.Select(t => t.Name).ToArray();
        }

        private static bool InheritsFromViewGeneric(Type t)
        {
            Type current = t.BaseType;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.Name.StartsWith("View`"))
                    return true;
                current = current.BaseType;
            }
            return false;
        }

        private static bool InheritsFrom(Type t, string baseName)
        {
            if (t.GetInterfaces().Any(i => i.Name == baseName || i.FullName == baseName))
                return true;

            Type current = t.BaseType;
            while (current != null && current != typeof(object))
            {
                if (current.Name == baseName || current.FullName == baseName)
                    return true;
                current = current.BaseType;
            }
            return false;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
