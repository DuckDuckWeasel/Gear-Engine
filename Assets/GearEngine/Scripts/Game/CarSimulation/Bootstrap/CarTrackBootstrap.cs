using System;
using System.Collections.Generic;
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
        [SerializeField] private List<CarDefinition> carDefinitions = new List<CarDefinition>();
        [SerializeField] private TrackSimulationConfig simulationConfig = new TrackSimulationConfig();

        [Inject] private TrackSimulationFactory factory;
        [Inject] private INavigation navigation;

        private readonly List<TrackSimulationRunner> runners = new List<TrackSimulationRunner>();

        public void Initialize()
        {
            try
            {
                ValidateSerializedReferences();
                var simulations = new List<TrackSimulation>();
                foreach (CarDefinition carDef in carDefinitions)
                {
                    if (carDef == null)
                    {
                        continue;
                    }

                    TrackSimulation sim = factory.Create(carDef, trackDefinition, simulationConfig);
                    var runner = new TrackSimulationRunner(new UnityRaceRandom());
                    runner.SetSimulation(sim);
                    sim.Toggle(true);
                    simulations.Add(sim);
                    runners.Add(runner);
                }

                if (simulations.Count == 0)
                {
                    throw new InvalidOperationException("[CarTrackBootstrap] No valid CarDefinitions assigned.");
                }

                navigation.Open(new TrackListViewModel(trackDefinition, simulations));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CarTrackBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void Update()
        {
            foreach (TrackSimulationRunner runner in runners)
            {
                runner.Tick();
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
