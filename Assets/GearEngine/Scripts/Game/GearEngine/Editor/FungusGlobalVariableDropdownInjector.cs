using UnityEditor;
using UnityEngine;
using Fungus;
using System.Linq;
using System.Collections.Generic;
using GearEngine.Core.Architecture.Editor.References;

namespace GearEngine.GearEngine.Editor
{
    [InitializeOnLoad]
    public static class FungusGlobalVariableDropdownInjector
    {
        static FungusGlobalVariableDropdownInjector()
        {
            // Inject the method into the Core's TargetReferenceDrawer to keep the architecture clean
            TargetReferenceDrawer.GetGlobalVariableNames = () =>
            {
                var flowcharts = Resources.FindObjectsOfTypeAll<Flowchart>();
                var globalVars = new List<string>();

                foreach (var flowchart in flowcharts)
                {
                    foreach (var variable in flowchart.Variables)
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
