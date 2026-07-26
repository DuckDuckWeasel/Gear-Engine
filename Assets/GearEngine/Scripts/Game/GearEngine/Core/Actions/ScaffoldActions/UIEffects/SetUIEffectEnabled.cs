using System;
using Coffee.UIEffects;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("UI Effects", "Set UI Effect Enabled", "Enables or disables a UIEffect on a UIEffect or target GameObject.")]
    [AddComponentMenu("")]
    [Serializable]
    public class SetUIEffectEnabled : UIEffectActionBase
    {
        [Tooltip("Whether the UIEffect should be enabled.")]
        [SerializeField] protected BooleanData isEnabled = new BooleanData(true);

        public override void OnEnter()
        {
            if (TryResolveEffect(false, out UIEffect effect))
            {
                effect.enabled = isEnabled.Value;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return $"{(isEnabled.Value ? "Enable" : "Disable")} {GetTargetDescription()}";
        }
    }
}
