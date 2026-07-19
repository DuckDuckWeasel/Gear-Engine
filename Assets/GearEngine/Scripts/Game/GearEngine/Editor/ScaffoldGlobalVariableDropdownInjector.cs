using UnityEditor;
using UnityEngine;
using Scaffold;
using System.Linq;
using System.Collections.Generic;
using GearEngine.Core.Architecture.Editor.References;

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
                var blackboards = Resources.FindObjectsOfTypeAll<Blackboard>();
                var globalVars = new List<string>();

                foreach (var blackboard in blackboards)
                {
                    foreach (var variable in blackboard.Variables)
                    {
                        if (variable.Scope == VariableScope.Global)
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
