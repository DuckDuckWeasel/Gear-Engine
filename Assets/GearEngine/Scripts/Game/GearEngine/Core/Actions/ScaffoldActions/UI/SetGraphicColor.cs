using GearEngine.Core.Actions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold
{
    [CommandInfo("UI", "Set Graphic Color", "Instantly changes the color of a UI Graphic component (Image, RawImage, Text).")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetGraphicColor : ActionBase
    {
        [Tooltip("The UI Graphic component to modify")]
        [SerializeField] protected Graphic targetGraphic;
        
        [Tooltip("The new color")]
        [SerializeField] protected ColorData color = new ColorData(Color.white);

        public override void OnEnter()
        {
            if (targetGraphic != null)
            {
                targetGraphic.color = color.Value;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetGraphic == null) return "Error: No Target Graphic";
            return $"Set color on {targetGraphic.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
