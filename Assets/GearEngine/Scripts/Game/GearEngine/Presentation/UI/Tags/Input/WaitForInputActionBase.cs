using System;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Extensions;
using Scaffold.Events;
using Scaffold.Events.Contracts;
using Scaffold.Input;
using Scaffold.Input.Contracts;

namespace GearEngine.Actions.Input
{
    /// <summary>
    /// Base class for actions that wait for specific UI input (clicks, hovers, drops, etc.).
    /// Encapsulates the IInputFilterService and IEventBus dependencies, and provides
    /// a local polling fallback for script-created Blackboards.
    /// </summary>
    [Serializable]
    public abstract class WaitForInputActionBase : ActionBase
    {
        protected IInputFilterService inputService;
        protected IEventBus eventBus;

        private bool tickInputManually;

        protected void InitializeInputService()
        {
            tickInputManually = false;
            if (inputService != null && eventBus != null)
            {
                return;
            }

            eventBus = new EventController();
            inputService = new InputFilterService(eventBus);
            tickInputManually = true;
        }

        protected void TickFallbackIfNeeded()
        {
            if (tickInputManually && inputService is InputFilterService inputFilterService)
            {
                inputFilterService.Tick();
            }
        }
    }
}
