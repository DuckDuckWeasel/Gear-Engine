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
        [SerializeField] protected bool changeFogState = true;
        [SerializeField] protected BooleanData fogEnabled = new BooleanData(true);
        
        [SerializeField] protected bool changeFogColor = false;
        [SerializeField] protected ColorData fogColor = new ColorData(new Color(0.5f, 0.5f, 0.5f, 1f));

        [SerializeField] protected bool changeFogDensity = false;
        [SerializeField] protected FloatData fogDensity = new FloatData(0.01f);

        public override void OnEnter()
        {
            if (changeFogState) RenderSettings.fog = fogEnabled.Value;
            if (changeFogColor) RenderSettings.fogColor = fogColor.Value;
            if (changeFogDensity) RenderSettings.fogDensity = fogDensity.Value;
            
            Continue();
        }

        public override string GetSummary()
        {
            return $"Fog Enabled: {fogEnabled.Value}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
