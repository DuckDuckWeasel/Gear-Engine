using UnityEngine;
using Fungus;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Input
{
    /// <summary>
    /// Attaching this to any GameObject automatically registers it to a Fungus Global Variable
    /// using a Domain/Role naming convention (e.g., "Ships/MainCruise"), allowing 
    /// global access without relying on the specific GameObject name or hierarchy.
    /// </summary>
    public class PushToFungusVariable : MonoBehaviour
    {
        [Tooltip("The global String ID/Key for this object. E.g. 'Player', 'Combat/TargetEnemy'")]
        public string globalVariableKey;

        private void OnEnable()
        {
            RegisterToGlobal();
        }

        private void RegisterToGlobal()
        {
            if (string.IsNullOrEmpty(globalVariableKey)) return;

            if (FungusManager.Instance != null && FungusManager.Instance.GlobalVariables != null)
            {
                var globalVars = FungusManager.Instance.GlobalVariables;
                
                // Get or add the variable in the global flowchart
                var goVar = globalVars.GetOrAddVariable<GameObject>(
                    globalVariableKey, 
                    this.gameObject, 
                    typeof(GameObjectVariable)
                );
                
                if (goVar != null)
                {
                    goVar.Value = this.gameObject;
                }
            }
            else
            {
                Debug.LogWarning("[PushToFungusVariable] Could not find FungusManager.GlobalVariables to register: " + globalVariableKey);
            }
        }
    }
}
