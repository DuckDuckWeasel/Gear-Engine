using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    public static class BlackboardEditorDisplay
    {
        public static string GetName(Type type)
        {
            object attribute = FindInfoAttribute(type);
            string name = GetStringProperty(attribute, "CommandName");
            name = string.IsNullOrWhiteSpace(name) ? GetStringProperty(attribute, "EventHandlerName") : name;
            return string.IsNullOrWhiteSpace(name) ? GetFallbackName(type) : name;
        }

        public static string GetCategory(Type type)
        {
            return GetStringProperty(FindInfoAttribute(type), "Category");
        }

        public static string GetHelp(Type type)
        {
            return GetStringProperty(FindInfoAttribute(type), "HelpText");
        }

        public static string GetMenuPath(Type type)
        {
            string name = GetName(type);
            string category = GetCategory(type);
            return string.IsNullOrWhiteSpace(category) ? name : $"{category}/{name}";
        }

        public static string GetSummary(IAction action)
        {
            if (action == null)
            {
                return "Missing Action";
            }

            return InvokeSummary(action);
        }

        public static Color GetTint(IAction action)
        {
            object value = InvokeNoArgument(action, "GetButtonColor");
            return value is Color tint ? tint : Color.white;
        }

        public static bool IsPropertyVisible(IAction action, string propertyName)
        {
            object value = InvokeOneArgument(action, "IsPropertyVisible", propertyName);
            return !(value is bool visible) || visible;
        }

        private static object FindInfoAttribute(Type type)
        {
            if (type == null)
            {
                return null;
            }

            object[] attributes = type.GetCustomAttributes(true);
            for (int index = 0; index < attributes.Length; index++)
            {
                string name = attributes[index].GetType().Name;
                if (name == "CommandInfoAttribute" || name == "EventHandlerInfoAttribute")
                {
                    return attributes[index];
                }
            }

            return null;
        }

        private static string GetFallbackName(Type type)
        {
            string name = type?.Name ?? "Missing";
            string[] suffixes =
            {
                "TriggerDefinition",
                "VariableDefinition",
                "ActionDefinition",
                "Definition",
                "Trigger",
                "Action",
            };
            for (int index = 0; index < suffixes.Length; index++)
            {
                string suffix = suffixes[index];
                if (name.EndsWith(suffix, StringComparison.Ordinal) && name.Length > suffix.Length)
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                    break;
                }
            }

            return ObjectNames.NicifyVariableName(name);
        }

        private static string GetStringProperty(object target, string propertyName)
        {
            PropertyInfo property = target?.GetType().GetProperty(propertyName);
            return property?.GetValue(target) as string ?? string.Empty;
        }

        private static string InvokeSummary(IAction action)
        {
            try
            {
                object value = InvokeNoArgument(action, "GetSummary");
                string summary = value as string;
                return string.IsNullOrWhiteSpace(summary) ? GetName(action.GetType()) : summary.Replace('\n', ' ').Replace('\r', ' ');
            }
            catch (Exception exception)
            {
                Debug.LogError($"[BlackboardEditor] Failed to read the action summary: {exception.Message}");
                return GetName(action.GetType());
            }
        }

        private static object InvokeNoArgument(object target, string methodName)
        {
            MethodInfo method = target?.GetType().GetMethod(methodName, Type.EmptyTypes);
            return method?.Invoke(target, null);
        }

        private static object InvokeOneArgument(object target, string methodName, object argument)
        {
            MethodInfo method = target?.GetType().GetMethod(methodName, new[] { argument?.GetType() ?? typeof(string) });
            return method?.Invoke(target, new[] { argument });
        }
    }
}
