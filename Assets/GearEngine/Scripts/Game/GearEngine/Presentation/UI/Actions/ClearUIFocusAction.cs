using System;
using GearEngine.Core.Actions;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    [Serializable]
    public class ClearUIFocusAction : IAction
    {
        public void Execute(System.Action onComplete)
        {
            var focusService = TutorialFocusService.Instance;

            if (focusService != null)
            {
                focusService.ClearFocus();
            }
            
            onComplete?.Invoke();
        }
    }
}
