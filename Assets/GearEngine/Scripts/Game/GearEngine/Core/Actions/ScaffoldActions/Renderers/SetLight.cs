using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("Renderers", "Set Light", "Modifies a Unity Light component properties instantly.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetLight : ActionBase
    {
        [Tooltip("The light component to modify")]
        [SerializeField] protected Light targetLight;
        
        [SerializeField] protected bool changeColor = false;
        [SerializeField] protected ColorData color = new ColorData(Color.white);

        [SerializeField] protected bool changeIntensity = false;
        [SerializeField] protected FloatData intensity = new FloatData(1f);

        [SerializeField] protected bool changeRange = false;
        [SerializeField] protected FloatData range = new FloatData(10f);

        public override void OnEnter()
        {
            if (targetLight != null)
            {
                if (changeColor) targetLight.color = color.Value;
                if (changeIntensity) targetLight.intensity = intensity.Value;
                if (changeRange) targetLight.range = range.Value;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetLight == null) return "Error: No Light selected";
            return $"Set properties on {targetLight.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
