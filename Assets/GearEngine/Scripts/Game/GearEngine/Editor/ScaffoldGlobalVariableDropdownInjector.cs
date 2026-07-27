using UnityEditor;
using UnityEngine;
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
                List<string> globalVars = new List<string>();

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
                        if (variable.Scope == VariableScope.InjectedGlobal)
                        {
                            globalVars.Add(variable.Key);
                        }
                    }
                }

                return globalVars.Distinct().OrderBy(x => x).ToArray();
            };
        }
    }
}
