using System;
using UnityEngine;
using Scaffold;
using GearEngine.Core.Actions;
using GearEngine.Core.Architecture.References;

namespace GearEngine.GearEngine.Presentation.UI.Actions
{
    [CommandInfo("Variable", "Push To Scaffold Variable", "Registers a target GameObject to a Scaffold global variable.")]
    [AddComponentMenu("")]
    [Serializable]
    public class PushToScaffoldVariable : ActionBase
    {
        [Tooltip("The global String ID/Key for this object. E.g. 'Player', 'Combat/TargetEnemy'")]
        public string globalVariableKey;

        [Tooltip("The target GameObject to register to the global variable")]
        public TargetReference target = new TargetReference();

        public override void OnEnter()
        {
            RegisterToGlobal();
            Continue();
        }

        private void RegisterToGlobal()
        {
            if (string.IsNullOrEmpty(globalVariableKey))
            {
                return;
            }

            GameObject targetGO = target.Resolve();
            if (targetGO == null)
            {
                Debug.LogWarning($"[PushToScaffoldVariable] Could not resolve target for key {globalVariableKey}");
                return;
            }

            if (ScaffoldManager.Instance != null && ScaffoldManager.Instance.GlobalVariables != null)
            {
                GlobalVariables globalVars = ScaffoldManager.Instance.GlobalVariables;

                // Get or add the variable in the global blackboard
                VariableBase<GameObject> goVar = globalVars.GetOrAddVariable<GameObject>(
                    globalVariableKey,
                    targetGO,
                    typeof(GameObjectVariable)
                );

                if (goVar != null)
                {
                    goVar.Value = targetGO;
                }
            }
            else
            {
                Debug.LogWarning("[PushToScaffoldVariable] Could not find ScaffoldManager.GlobalVariables to register: " + globalVariableKey);
            }
        }

        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(globalVariableKey) || target == null)
            {
                return "Error: Missing setup";
            }

            return $"{target.GetSummary()} -> {globalVariableKey}";
        }
    }
}
