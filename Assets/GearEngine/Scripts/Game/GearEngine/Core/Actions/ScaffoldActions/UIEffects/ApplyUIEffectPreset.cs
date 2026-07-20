using System;
using Coffee.UIEffects;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Applies a native or material-backed UI effect preset.
    /// </summary>
    [CommandInfo("UI Effects", "Apply Effect", "Applies a UIEffect preset or a self-animated UI material.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ApplyUIEffectPreset : UIEffectActionBase
    {
        [Tooltip("The unified UI effect preset to apply directly or from an Object blackboard variable.")]
        [SerializeField] protected ObjectData configuration;

        [HideInInspector]
        [SerializeField] protected UIEffectPreset preset;

        [Tooltip("Append only the settings present in the preset.")]
        [SerializeField] protected BooleanData append = new BooleanData(false);

        [Tooltip("Add UIEffect to the target GameObject when it is missing.")]
        [SerializeField] protected BooleanData addIfMissing = new BooleanData(true);

        public override void OnEnter()
        {
            UIEffectPreset effectPreset = configuration.Value as UIEffectPreset;
            if (effectPreset != null && TryResolveEffect(addIfMissing.Value, out UIEffect effect))
            {
                effect.ExecutePreset(effectPreset, append.Value);
            }
            else
            {
                ApplyLegacyConfiguration();
            }

            Continue();
        }

        public override string GetSummary()
        {
            UIEffectPreset effectPreset = configuration.Value as UIEffectPreset;
            if (effectPreset != null)
            {
                return $"{effectPreset.name} on {GetTargetDescription()}";
            }

            return preset == null ? "Error: No UI effect configuration" : $"{preset.name} on {GetTargetDescription()}";
        }

        private void ApplyLegacyConfiguration()
        {
            if (preset != null && TryResolveEffect(addIfMissing.Value, out UIEffect effect))
            {
                effect.ExecutePreset(preset, append.Value);
            }
        }

        public override bool HasReference(Variable variable)
        {
            return configuration.objectRef == variable || base.HasReference(variable);
        }
    }
}
