using System;
using Coffee.UIEffects;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Applies a native or material-backed UI effect preset.
    /// </summary>
    [CommandInfo("UI Effects", "Apply UI Effect", "Applies a UIEffect preset or a self-animated UI material.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ApplyUIEffectPreset : UIEffectActionBase
    {
        [Tooltip("The UIEffect component to modify. Takes precedence over Target GameObject.")]
        [SerializeField] private UIEffect targetEffect;

        [Tooltip("A dynamic target. This enables use inside a For Each loop over GameObjects.")]
        [SerializeField] private GameObjectData targetGameObject;

        protected override UIEffect TargetEffect => targetEffect;

        protected override GameObjectData TargetGameObject =>
            targetGameObject;

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
                ApplyPreset(effect, effectPreset, append.Value);
                ScaffoldUIEffectRegistry.Track(effect);
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
                ApplyPreset(effect, preset, append.Value);
                ScaffoldUIEffectRegistry.Track(effect);
            }
        }

        public override bool HasReference(Variable variable)
        {
            return configuration.objectRef == variable || base.HasReference(variable);
        }
    }
}
