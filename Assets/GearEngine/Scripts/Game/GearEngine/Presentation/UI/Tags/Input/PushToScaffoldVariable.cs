using System;
using UnityEngine;
using Scaffold;
using GearEngine.Core.Actions;
using GearEngine.Core.Architecture.References;
using RuntimeVariableCellBase =
    Scaffold.VisualScripting.VariableCellBase;
using RuntimeGameObjectCell =
    Scaffold.VisualScripting.VariableCell<UnityEngine.GameObject>;

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

            GameObject targetGO = ResolveTarget(target);
            if (targetGO == null)
            {
                Debug.LogWarning($"[PushToScaffoldVariable] Could not resolve target for key {globalVariableKey}");
                return;
            }

            if (!GetBlackboard().Variables.TryGet(
                    globalVariableKey,
                    out RuntimeVariableCellBase cell))
            {
                Debug.LogError(
                    $"[PushToScaffoldVariable] Blackboard variable '{globalVariableKey}' was not found.");
                return;
            }

            if (cell is RuntimeGameObjectCell gameObjectCell)
            {
                gameObjectCell.Value = targetGO;
                return;
            }

            Debug.LogError(
                $"[PushToScaffoldVariable] Blackboard variable '{globalVariableKey}' is not a GameObject cell.");
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
