using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.CarSimulation
{
    public sealed class CarTrackBootstrap : MonoBehaviour, IInitializable
    {
        [SerializeField] private Track track;
        [SerializeField] private TrackDefinition trackDefinition;
        [SerializeField] private CarDefinition carDefinition;

        private ITrackSimulationService service;

        [Inject]
        public void Construct(ITrackSimulationService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
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
            if (track == null)
            {
                throw new InvalidOperationException("[CarTrackBootstrap] Track reference is missing.");
            }

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
            track.Bind(service.TrackViewModel);
            service.ToggleSimulation(true);
        }
    }
}
