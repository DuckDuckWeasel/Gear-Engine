using GearEngine.Core.Actions;
using System;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("UI", "Set Canvas Group Raycast", "Enables or disables Blocks Raycasts on a CanvasGroup.")]
    [Serializable]
    [AddComponentMenu("")]
    public class SetCanvasGroupRaycast : ActionBase
    {
        [Tooltip("The CanvasGroup to modify")]
        [SerializeField] protected CanvasGroup targetCanvasGroup;
        
        [Tooltip("Should it block raycasts?")]
        [SerializeField] protected BooleanData blocksRaycasts = new BooleanData(true);
        
        [Tooltip("Optionally change interactable state as well")]
        [SerializeField] protected bool changeInteractable = false;
        
        [SerializeField] protected BooleanData interactable = new BooleanData(true);

        public override void OnEnter()
        {
            if (targetCanvasGroup != null)
            {
                targetCanvasGroup.blocksRaycasts = blocksRaycasts.Value;
                if (changeInteractable)
                {
                    targetCanvasGroup.interactable = interactable.Value;
                }
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (targetCanvasGroup == null) return "Error: No CanvasGroup";
            return $"Raycasts: {blocksRaycasts.Value} on {targetCanvasGroup.name}";
        }
        
        public override Color GetButtonColor() { return new Color32(235, 191, 217, 255); }
    }
}
