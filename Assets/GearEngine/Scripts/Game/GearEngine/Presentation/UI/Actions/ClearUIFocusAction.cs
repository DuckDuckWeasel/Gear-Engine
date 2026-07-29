using System;
using GearEngine.Core.Actions;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    [Serializable]
    public class ClearUIFocusAction : ActionBase
    {
        public override void OnEnter()
        {
            TutorialFocusService focusService = TutorialFocusService.Instance;

            if (focusService != null)
            {
                focusService.ClearFocus();
            }

            Continue();
        }
    }
}
