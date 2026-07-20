using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold
{
    [CommandInfo("UI Effects", "Cycle Preset", "Applies the next UIEffect preset and describes the applied effect.")]
    [AddComponentMenu("")]
    [Serializable]
    public class CycleUIEffectPreset : UIEffectActionBase
    {
        [Tooltip("The UI Text updated after each preset is applied.")]
        [SerializeField] protected Text targetLabel;

        [Tooltip("The presets applied in sequence. Each preset replaces the previous effect.")]
        [SerializeField] protected List<UIEffectPreset> presets = new List<UIEffectPreset>();

        [Tooltip("One explanation for each preset, in the same order as Presets.")]
        [SerializeField] protected List<string> descriptions = new List<string>();

        [SerializeField] private int currentIndex = -1;

        public override void OnEnter()
        {
            if (presets == null || presets.Count == 0)
            {
                Continue();
                return;
            }

            currentIndex = (currentIndex + 1) % presets.Count;
            UIEffectPreset preset = presets[currentIndex];
            if (preset != null && TryResolveEffect(true, out UIEffect effect))
            {
                effect.ExecutePreset(preset, false);
                UpdateLabel(preset);
            }

            Continue();
        }

        public override string GetSummary()
        {
            return presets == null || presets.Count == 0
                ? "Error: No UIEffect presets"
                : $"Apply next preset on {GetTargetDescription()}";
        }

        private void UpdateLabel(UIEffectPreset preset)
        {
            if (targetLabel == null)
            {
                return;
            }

            string description = currentIndex < descriptions.Count && !string.IsNullOrWhiteSpace(descriptions[currentIndex])
                ? descriptions[currentIndex]
                : "Applied this UIEffect preset.";
            targetLabel.text = $"{preset.name}\n{description}\n\nClick for the next effect";
        }
    }
}
