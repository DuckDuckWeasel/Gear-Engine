using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
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

        [Inject] private TrackSimulationFactory factory;
        [Inject] private INavigation navigation;
        [Inject] private RaceManagerService raceManager;
        [Inject] private SplineCarRunnerService aiRunner;

        public void Initialize()
        {
            try
            {
                ValidateSerializedReferences();
                List<RaceState> sessions = CreateSessionsForCars();
                navigation.Open(new TrackListViewModel(sessions, factory, aiRunner, raceManager));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CarTrackBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private List<RaceState> CreateSessionsForCars()
        {
            var sessions = new List<RaceState>();
            foreach (CarDefinition carDef in carDefinitions)
            {
                if (carDef != null)
                {
                    AddSessionForCar(carDef, sessions);
                }
            }

            if (sessions.Count == 0)
            {
                throw new InvalidOperationException("[CarTrackBootstrap] No valid CarDefinitions assigned.");
            }

            return sessions;
        }

        private void AddSessionForCar(CarDefinition carDef, List<RaceState> sessions)
        {
            RaceState session = factory.Create(carDef, trackDefinition, sessionConfig);
            
            if (raceManager == null)
            {
                throw new InvalidOperationException("[CarTrackBootstrap] RaceManagerService is not injected.");
            }

            raceManager.RegisterRace(session);
            sessions.Add(session);
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
