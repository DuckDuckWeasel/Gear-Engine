using UnityEngine;
using Scaffold;
using GearEngine.Core.Architecture.References;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Input
{
    /// <summary>
    /// Implements the ITargetResolver interface to read string-based target IDs 
    /// directly from Scaffold Global Variables.
    /// </summary>
    public class ScaffoldGlobalVariableResolver : ITargetResolver
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InjectResolver()
        {
            TargetReference.GlobalResolver = new ScaffoldGlobalVariableResolver();
        }

        public GameObject Resolve(string globalVariableName)
        {
            if (ScaffoldManager.Instance == null || ScaffoldManager.Instance.GlobalVariables == null)
            {
                Debug.LogWarning("[ScaffoldGlobalVariableResolver] ScaffoldManager or GlobalVariables is missing.");
                return null;
            }

            Variable v = ScaffoldManager.Instance.GlobalVariables.GetVariable(globalVariableName);
            if (v != null)
            {
                GameObjectVariable goVar = v as GameObjectVariable;
                if (goVar != null)
                {
                    return goVar.Value;
                }
            }
            return null;
        }
    }
}
