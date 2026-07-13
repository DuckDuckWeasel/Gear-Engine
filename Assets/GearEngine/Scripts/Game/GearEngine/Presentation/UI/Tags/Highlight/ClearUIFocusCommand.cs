using Fungus;
using UnityEngine;
using VContainer;
using Command = Fungus.Command;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    [CommandInfo("Tutorial", "Clear UI Focus", "Clears any active UI focus overlay and indicator.")]
    public class ClearUIFocusCommand : Command
    {
        [Inject] 
        private TutorialFocusService _focusService;

        public override void OnEnter()
        {
            if (_focusService != null)
            {
                _focusService.ClearFocus();
            }
            
            Continue();
        }

        public override string GetSummary()
        {
            return "Clears the active Focus Overlay";
        }
    }
}
