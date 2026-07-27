using System;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Extensions;
using Scaffold.Events;
using Scaffold.Events.Contracts;
using Scaffold.Input;
using Scaffold.Input.Contracts;
using VContainer;

namespace GearEngine.Actions.Input
{
    /// <summary>
    /// Base class for actions that wait for specific UI input (clicks, hovers, drops, etc.).
    /// Encapsulates the IInputFilterService and IEventBus dependencies, and provides
    /// a robust fallback mechanism for when these dependencies are not injected
    /// (e.g. testing in isolated scenes without a LifetimeScope).
    /// </summary>
    [Serializable]
    public abstract class WaitForInputActionBase : ActionBase
    {
        [Inject] protected IInputFilterService _inputService;
        [Inject] protected IEventBus _eventBus;

        private bool tickInputManually;

        protected void InitializeInputService()
        {
            tickInputManually = false;
            if (_inputService != null && _eventBus != null)
            {
                return;
            }

            this.TryInject();
            if (_inputService != null && _eventBus != null)
            {
                return;
            }

            if (InputFilterService.TryGetGlobalContext(
                    out IInputFilterService globalInputService,
                    out IEventBus globalEventBus))
            {
                _inputService = globalInputService;
                _eventBus = globalEventBus;
                return;
            }

            _eventBus = new EventController();
            _inputService = new InputFilterService(_eventBus);
            tickInputManually = true;
        }

        protected void TickFallbackIfNeeded()
        {
            if (tickInputManually && _inputService is InputFilterService inputFilterService)
            {
                inputFilterService.Tick();
            }
        }
    }
}
