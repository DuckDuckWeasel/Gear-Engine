using System;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Presentation.UI.Tags.Highlight;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo("UI Effects", "Clear All UI Effects", "Clears UI effects applied by Scaffold actions, including the active tutorial UI focus.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ClearAllUIEffects : ActionBase
    {
        public override void OnEnter()
        {
            ScaffoldUIEffectRegistry.ClearAll();

            if (TutorialFocusService.TryGetInstance(out TutorialFocusService focusService))
            {
                focusService.ClearFocus();
            }

            Continue();
        }

        public override string GetSummary()
        {
            return "Clear All Applied UI Effects and Focus";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }
    }
}
