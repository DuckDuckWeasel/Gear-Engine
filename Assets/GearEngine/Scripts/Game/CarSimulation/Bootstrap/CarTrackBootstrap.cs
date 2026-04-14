using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Bootstrap
{
    public sealed class CarTrackBootstrap : MonoBehaviour, IInitializable
    {
        [SerializeField] private TrackDefinition trackDefinition;
        [SerializeField] private CarDefinition carDefinition;
        [SerializeField] private CarVariableSet carVariables;

        [Inject] private TrackSimulationFactory factory;
        [Inject] private TrackSimulationRunner trackSimulationRunner;
        [Inject] private INavigation navigation;

        public void Initialize()
        {
            try
            {
                ValidateSerializedReferences();
                TrackSimulation simulation = factory.Create(carDefinition, trackDefinition, carVariables);
                trackSimulationRunner.SetSimulation(simulation);
                navigation.Open(new TrackViewModel(simulation));
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
    }
}
