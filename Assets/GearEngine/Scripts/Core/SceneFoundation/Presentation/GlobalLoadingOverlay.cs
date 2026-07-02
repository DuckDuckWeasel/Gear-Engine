using GearEngine.Core.Config.Events;
using Scaffold.Events.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.SceneFoundation.Presentation
{
    /// <summary>
    /// Listens to GlobalLoadingEvent and activates/deactivates a blocking UI overlay.
    /// This should be placed on a Canvas with a high SortOrder.
    /// </summary>
    public sealed class GlobalLoadingOverlay : MonoBehaviour, IInitializable
    {
        [SerializeField]
        [Tooltip("The GameObject to activate when loading starts (e.g. a semi-transparent panel with a spinner).")]
        private GameObject loadingVisualsRoot;

        private readonly System.Collections.Generic.HashSet<IEventBus> registeredBuses = new System.Collections.Generic.HashSet<IEventBus>();
        private int activeRequests = 0;



        [Inject]
        public void Construct(IEventBus eventBus)
        {
            if (eventBus != null && registeredBuses.Add(eventBus))
            {
                eventBus.AddListener<GlobalLoadingEvent>(OnGlobalLoadingEvent);
            }
        }

        private void OnEnable()
        {
            if (loadingVisualsRoot != null)
            {
                loadingVisualsRoot.SetActive(activeRequests > 0);
            }
        }

        private void OnDisable()
        {
            // We do not remove listeners on disable because this is a global DontDestroyOnLoad object
            // and we want it to keep its state and subscriptions alive across scenes.
        }

        private void OnDestroy()
        {
            foreach (var bus in registeredBuses)
            {
                bus?.RemoveListener<GlobalLoadingEvent>(OnGlobalLoadingEvent);
            }
            registeredBuses.Clear();
        }

        public void Initialize()
        {
            // Empty method just to satisfy IInitializable so VContainer forces instantiation of this component on startup
        }

        private void OnGlobalLoadingEvent(GlobalLoadingEvent evt)
        {
            if (evt.IsLoading)
            {
                activeRequests++;
            }
            else
            {
                activeRequests = Mathf.Max(0, activeRequests - 1);
            }

            if (loadingVisualsRoot != null)
            {
                loadingVisualsRoot.SetActive(activeRequests > 0);
            }
        }
    }
}
