using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("GameObject", "Set Component Enabled", "Enables or disables any Behaviour component (like Colliders, Scripts, Renderers).")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetComponentEnabled : ActionBase
    {
        [Tooltip("The Component to enable/disable")]
        [SerializeField] protected Behaviour targetComponent;
        
        [Tooltip("True to enable, false to disable")]
        [SerializeField] protected BooleanData state = new BooleanData(true);

        public override void OnEnter()
        {
            if (targetComponent != null)
            {
                targetComponent.enabled = state.Value;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetComponent == null) return "Error: No Component";
            return $"{(state.Value ? "Enable" : "Disable")} {targetComponent.GetType().Name}";
        }
        
        public override Color GetButtonColor() { return new Color32(216, 228, 240, 255); }
    }
}
