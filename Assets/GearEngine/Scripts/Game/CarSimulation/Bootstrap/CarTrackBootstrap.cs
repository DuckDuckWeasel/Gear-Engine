using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Bootstrap
{
    public sealed class CarTrackBootstrap : MonoBehaviour, IInitializable
    {
        [SerializeField] private TrackDefinition trackDefinition;
        [SerializeField] private List<CarDefinition> carDefinitions = new List<CarDefinition>();
        [FormerlySerializedAs("simulationConfig")]
        [SerializeField] private RaceSessionConfig sessionConfig = new RaceSessionConfig();

        [Inject] private INavigation navigation;

        public void Initialize()
        {
            try
            {
                ValidateSerializedReferences();
                navigation.Open(new CarTrackScreenViewModel(trackDefinition, carDefinitions, sessionConfig));
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

            if (carDefinitions == null || carDefinitions.Count == 0)
            {
                throw new InvalidOperationException("[CarTrackBootstrap] No CarDefinitions assigned.");
            }
        }
    }
}
