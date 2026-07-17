using GearEngine.Core.Actions;
using System;
using UnityEngine;
using Scaffold.UI;

namespace Scaffold
{
    [CommandInfo("UI", "Trigger Fader", "Controls a ScaffoldFader (Fade In/Out, Directional, Round).")]
    [Serializable]
    [AddComponentMenu("")]
    public class TriggerFader : ActionBase
    {
        public enum FaderAction { FadeIn, FadeOut }

        [Tooltip("The Fader to trigger")]
        [SerializeField] protected ScaffoldFader targetFader;
        
        [Tooltip("Action to perform")]
        [SerializeField] protected FaderAction action = FaderAction.FadeIn;

        [Tooltip("Duration of the fade")]
        [SerializeField] protected FloatData duration = new FloatData(1f);
        
        [Tooltip("For round/directional faders, the scale of the mask at FadeOut (hidden). E.g. (0,0) for round.")]
        [SerializeField] protected Vector2Data targetMaskScale = new Vector2Data(Vector2.zero);

        public override void OnEnter()
        {
            if (targetFader != null)
            {
                float targetAlpha = (action == FaderAction.FadeIn) ? 0f : 1f; // FadeIn means screen becomes visible (alpha 0)
                Vector2 scale = (action == FaderAction.FadeIn) ? Vector2.one * 50f : targetMaskScale.Value;
                
                targetFader.FadeTo(targetAlpha, duration.Value, scale);
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetFader == null) return "Error: No Fader";
            return $"{action} {targetFader.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
