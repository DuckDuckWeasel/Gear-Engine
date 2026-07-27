using GearEngine.Core.Actions;
using System;
using UnityEngine;
using Scaffold.UI;

namespace Scaffold
{
    [CommandInfo("UI", "Update Progress Bar", "Updates a ScaffoldProgressBar with a new value.")]
    [Serializable]
    [AddComponentMenu("")]
    public class UpdateProgressBar : ActionBase
    {
        [Tooltip("The ScaffoldProgressBar to update")]
        [SerializeField] protected ScaffoldProgressBar targetProgressBar;
        
        [SerializeField] protected FloatData currentValue;
        [SerializeField] protected FloatData minValue = new FloatData(0f);
        [SerializeField] protected FloatData maxValue = new FloatData(100f);

        public override void OnEnter()
        {
            if (targetProgressBar != null)
            {
                targetProgressBar.UpdateBar(currentValue.Value, minValue.Value, maxValue.Value);
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetProgressBar == null) return "Error: No Target";
            return $"Set {targetProgressBar.name} to {currentValue.Value}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
