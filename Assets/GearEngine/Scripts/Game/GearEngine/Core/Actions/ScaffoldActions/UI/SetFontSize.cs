using GearEngine.Core.Actions;
using System;
using UnityEngine;
using TMPro;

namespace Scaffold
{
    [CommandInfo("UI", "Set Font Size", "Instantly sets the font size of a TextMeshPro UI element.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetFontSize : ActionBase
    {
        [Tooltip("The TMP Text component to modify")]
        [SerializeField] protected TextMeshProUGUI targetText;
        
        [Tooltip("The new font size")]
        [SerializeField] protected FloatData fontSize = new FloatData(36f);

        public override void OnEnter()
        {
            if (targetText != null)
            {
                targetText.fontSize = fontSize.Value;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetText == null) return "Error: No Target Text";
            return $"Size = {fontSize.Value}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
