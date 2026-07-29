using System;
using Coffee.UIEffects;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("UI Effects", "Clear UI Effect", "Resets a UIEffect to its default preset.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ClearUIEffect : UIEffectActionBase
    {
        [Tooltip("The UIEffect component to modify. Takes precedence over Target GameObject.")]
        [SerializeField] private UIEffect targetEffect;

        [Tooltip("A dynamic target. This enables use inside a For Each loop over GameObjects.")]
        [SerializeField] private GameObjectData targetGameObject;

        protected override UIEffect TargetEffect => targetEffect;

        protected override GameObjectData TargetGameObject =>
            targetGameObject;

        public override void OnEnter()
        {
            if (TryResolveEffect(false, out UIEffect effect))
            {
                effect.Clear();
            }

            Continue();
        }

        public override string GetSummary()
        {
            return $"Clear {GetTargetDescription()}";
        }
    }
}
