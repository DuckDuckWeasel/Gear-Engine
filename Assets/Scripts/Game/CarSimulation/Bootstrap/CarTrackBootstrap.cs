using System;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.CarSimulation
{
    public sealed class CarTrackBootstrap : MonoBehaviour, IInitializable
    {
        [SerializeField] private TrackDefinition trackDefinition;
        [SerializeField] private CarDefinition carDefinition;

        private ITrackSimulationService service;
        private INavigation navigation;

        [Inject]
        public void Construct(ITrackSimulationService service, INavigation navigation)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        public void Initialize()
        {
            try
            {
                ValidateSerializedReferences();
                RunStartupSequence();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CarTrackBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void ValidateSerializedReferences()
        {
            if (trackDefinition == null)
            {
                throw new InvalidOperationException("[CarTrackBootstrap] TrackDefinition is missing.");
            }

            if (carDefinition == null)
            {
                throw new InvalidOperationException("[CarTrackBootstrap] CarDefinition is missing.");
            }
        }

        private void RunStartupSequence()
        {
            service.CreateSimulation(carDefinition, trackDefinition);
            navigation.Open(service.TrackViewModel);
            service.ToggleSimulation(true);
        }
    }
}
