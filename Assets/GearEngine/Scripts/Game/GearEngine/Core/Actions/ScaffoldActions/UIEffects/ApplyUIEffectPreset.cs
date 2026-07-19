using System;
using Coffee.UIEffects;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("UI Effects", "Apply Preset", "Applies a UIEffect preset to a UIEffect or target GameObject.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ApplyUIEffectPreset : UIEffectActionBase
    {
        [Tooltip("The preset to apply.")]
        [SerializeField] protected UIEffectPreset preset;

        [Tooltip("Append only the settings present in the preset.")]
        [SerializeField] protected BooleanData append = new BooleanData(false);

        [Tooltip("Add UIEffect to the target GameObject when it is missing.")]
        [SerializeField] protected BooleanData addIfMissing = new BooleanData(true);

        public override void OnEnter()
        {
            if (preset != null && TryResolveEffect(addIfMissing.Value, out UIEffect effect))
            {
                effect.LoadPreset(preset, append.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            return preset == null ? "Error: No UIEffect preset" : $"{preset.name} on {GetTargetDescription()}";
        }
    }
}
