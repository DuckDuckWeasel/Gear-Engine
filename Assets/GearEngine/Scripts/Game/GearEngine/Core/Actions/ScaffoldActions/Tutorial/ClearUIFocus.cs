using System;
using UnityEngine;
using GearEngine.Core.Actions;

namespace Scaffold
{
    [CommandInfo("Tutorial",
                 "Clear UI Focus",
                 "Clears the currently active UI focus overlay.")]
    [AddComponentMenu("")]
    [Serializable]
    public class ClearUIFocus : ActionBase
    {
        public override void OnEnter()
        {
            GearEngine.GearEngine.Presentation.UI.Tags.Highlight.TutorialFocusService focusService = GearEngine.GearEngine.Presentation.UI.Tags.Highlight.TutorialFocusService.Instance;

            if (focusService != null)
            {
                focusService.ClearFocus();
            }

            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(255, 204, 153, 255);
        }
    }
}
