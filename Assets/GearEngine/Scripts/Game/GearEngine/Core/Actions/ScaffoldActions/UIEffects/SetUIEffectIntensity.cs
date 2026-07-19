using System;
using Coffee.UIEffects;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("UI Effects", "Set Intensity", "Sets one of the UIEffect animated intensity channels.")]
    [AddComponentMenu("")]
    [Serializable]
    public class SetUIEffectIntensity : UIEffectActionBase
    {
        public enum IntensityChannel
        {
            Tone,
            Color,
            Sampling,
            Transition,
        }

        [SerializeField] protected IntensityChannel channel = IntensityChannel.Transition;

        [SerializeField] protected FloatData intensity = new FloatData(1f);

        public override void OnEnter()
        {
            if (TryResolveEffect(false, out UIEffect effect))
            {
                switch (channel)
                {
                    case IntensityChannel.Tone:
                        effect.toneIntensity = intensity.Value;
                        break;
                    case IntensityChannel.Color:
                        effect.colorIntensity = intensity.Value;
                        break;
                    case IntensityChannel.Sampling:
                        effect.samplingIntensity = intensity.Value;
                        break;
                    case IntensityChannel.Transition:
                        effect.transitionRate = intensity.Value;
                        break;
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            return $"{channel}: {intensity.Value:0.##} on {GetTargetDescription()}";
        }
    }
}
