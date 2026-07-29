using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using GearEngine.Core.Architecture.Editor.References;
using Scaffold.VisualScripting;
using Scaffold.VisualScripting.Authoring;
using Scaffold.VisualScripting.Unity;

namespace GearEngine.GearEngine.Editor
{
    [InitializeOnLoad]
    public static class ScaffoldGlobalVariableDropdownInjector
    {
        static ScaffoldGlobalVariableDropdownInjector()
        {
            // Inject the method into the Core's TargetReferenceDrawer to keep the architecture clean
            TargetReferenceDrawer.GetGlobalVariableNames = () =>
            {
                BlackboardBehaviour[] blackboards =
                    Resources.FindObjectsOfTypeAll<BlackboardBehaviour>();
                List<VariableDefinitionBase> definitions =
                    new List<VariableDefinitionBase>();

                foreach (BlackboardBehaviour blackboard in blackboards)
                {
                    BlackboardDefinition definition;
                    try
                    {
                        definition = blackboard.IsRuntimeAvailable
                            ? blackboard.Runtime.Definition
                            : blackboard.DefinitionReference.ResolveDefinition();
                    }
                    catch (BlackboardDefinitionResolutionException)
                    {
                        continue;
                    }

                    foreach (VariableDefinitionBase variable in definition.Variables)
                    {
                        definitions.Add(variable);
                    }
                }

                return GetCompatibleGlobalVariableNames(definitions);
            };
        }

        public static string[] GetCompatibleGlobalVariableNames(
            IEnumerable<VariableDefinitionBase> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            return definitions
                .Where(IsGameObjectGlobal)
                .Select(variable => variable.Key)
                .Distinct()
                .OrderBy(key => key)
                .ToArray();
        }

        private static bool IsGameObjectGlobal(
            VariableDefinitionBase variable)
        {
            return variable != null &&
                variable.Scope == VariableScope.InjectedGlobal &&
                variable is UnityObjectVariableDefinition objectVariable &&
                objectVariable.InitialValue is GameObject;
        }
    }
}
