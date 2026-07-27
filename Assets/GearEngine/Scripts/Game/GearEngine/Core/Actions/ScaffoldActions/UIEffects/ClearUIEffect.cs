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
