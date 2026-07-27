using GearEngine.Core.Config.Events;
using Scaffold.Events.Contracts;
using Scaffold.Tutorial.Events;
using System;
using VContainer;
using VContainer.Unity;

namespace GearEngine.SceneFoundation.Presentation
{
    /// <summary>
    /// Listens for TutorialLoadingEvent (from the com.scaffold.tutorial package)
    /// and maps it to a GlobalLoadingEvent so the global loading screen reacts accordingly.
    /// </summary>
    public sealed class TutorialLoadingEventBinder : IInitializable, IDisposable
    {
        private readonly IEventBus eventBus;

        [Inject]
        public TutorialLoadingEventBinder(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public void Initialize()
        {
            eventBus.AddListener<TutorialLoadingEvent>(OnTutorialLoading);
        }

        public void Dispose()
        {
            eventBus?.RemoveListener<TutorialLoadingEvent>(OnTutorialLoading);
        }

        private void OnTutorialLoading(TutorialLoadingEvent evt)
        {
            eventBus.Raise(new GlobalLoadingEvent(evt.IsLoading, evt.Message));
        }
    }
}
