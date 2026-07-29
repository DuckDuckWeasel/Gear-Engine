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
        [Tooltip("The UIEffect component to modify. Takes precedence over Target GameObject.")]
        [SerializeField] private UIEffect targetEffect;

        [Tooltip("A dynamic target. This enables use inside a For Each loop over GameObjects.")]
        [SerializeField] private GameObjectData targetGameObject;

        protected override UIEffect TargetEffect => targetEffect;

        protected override GameObjectData TargetGameObject =>
            targetGameObject;

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
