using GearEngine.Core.Actions;
using System;
using UnityEngine;
using TMPro;

namespace Scaffold
{
    [CommandInfo("UI", "Set Text Color", "Instantly sets the color of a TextMeshPro UI element.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetTextColor : ActionBase
    {
        [Tooltip("The TMP Text component to modify")]
        [SerializeField] protected TextMeshProUGUI targetText;
        
        [Tooltip("The new color")]
        [SerializeField] protected ColorData color = new ColorData(Color.white);

        public override void OnEnter()
        {
            if (targetText != null)
            {
                targetText.color = color.Value;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetText == null) return "Error: No Target Text";
            return $"Set color on {targetText.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
