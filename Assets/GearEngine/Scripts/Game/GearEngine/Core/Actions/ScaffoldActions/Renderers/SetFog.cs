using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Renderers", "Set Fog", "Modifies global fog settings instantly.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetFog : ActionBase
    {
        [Tooltip("The Change fog state")]
        [SerializeField] protected bool changeFogState = true;
        [Tooltip("The Fog enabled")]
        [SerializeField] protected BooleanData fogEnabled = new BooleanData(true);

        [Tooltip("The Change fog color")]
        [SerializeField] protected bool changeFogColor = false;
        [Tooltip("The Fog color")]
        [SerializeField] protected ColorData fogColor = new ColorData(new Color(0.5f, 0.5f, 0.5f, 1f));

        [Tooltip("The Change fog density")]
        [SerializeField] protected bool changeFogDensity = false;
        [Tooltip("The Fog density")]
        [SerializeField] protected FloatData fogDensity = new FloatData(0.01f);

        public override void OnEnter()
        {
            if (changeFogState)
            {
                RenderSettings.fog = fogEnabled.Value;
            }

            if (changeFogColor)
            {
                RenderSettings.fogColor = fogColor.Value;
            }

            if (changeFogDensity)
            {
                RenderSettings.fogDensity = fogDensity.Value;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return $"Fog Enabled: {fogEnabled.Value}";
        }

        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
