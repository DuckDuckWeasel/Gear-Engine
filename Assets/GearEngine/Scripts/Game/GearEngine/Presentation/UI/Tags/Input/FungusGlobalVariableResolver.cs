using UnityEngine;
using Fungus;
using GearEngine.Core.Architecture.References;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Input
{
    /// <summary>
    /// Implements the ITargetResolver interface to read string-based target IDs 
    /// directly from Fungus Global Variables.
    /// </summary>
    public class FungusGlobalVariableResolver : ITargetResolver
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InjectResolver()
        {
            TargetReference.GlobalResolver = new FungusGlobalVariableResolver();
        }

        public GameObject Resolve(string globalVariableName)
        {
            if (FungusManager.Instance == null || FungusManager.Instance.GlobalVariables == null)
            {
                Debug.LogWarning("[FungusGlobalVariableResolver] FungusManager or GlobalVariables is missing.");
                return null;
            }

            Variable v = FungusManager.Instance.GlobalVariables.GetVariable(globalVariableName);
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
