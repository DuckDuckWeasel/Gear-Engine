using GearEngine.Core.Actions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scaffold
{
    [CommandInfo("UI", "Set Raycast Target", "Enables or disables RaycastTarget on a UI Graphic.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetRaycastTarget : ActionBase
    {
        [Tooltip("The UI Graphic component")]
        [SerializeField] protected Graphic targetGraphic;
        
        [Tooltip("Should it block raycasts?")]
        [SerializeField] protected BooleanData raycastTarget = new BooleanData(true);

        public override void OnEnter()
        {
            if (targetGraphic != null)
            {
                targetGraphic.raycastTarget = raycastTarget.Value;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetGraphic == null) return "Error: No Graphic";
            return $"Raycast: {raycastTarget.Value}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
