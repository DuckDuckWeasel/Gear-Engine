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
        [Inject] private IRaceSessionRunner raceSessionRunner;

        private readonly List<IRaceSessionRunner> runners = new List<IRaceSessionRunner>();

        public void Initialize()
        {
            try
            {
                ValidateSerializedReferences();
                List<LapRaceSession> sessions = CreateSessionsForCars();
                navigation.Open(new TrackListViewModel(sessions));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CarTrackBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void Update()
        {
            foreach (IRaceSessionRunner runner in runners)
            {
                runner.Tick();
            }
        }

        private List<LapRaceSession> CreateSessionsForCars()
        {
            var sessions = new List<LapRaceSession>();
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

        private void AddSessionForCar(CarDefinition carDef, List<LapRaceSession> sessions)
        {
            LapRaceSession session = factory.Create(carDef, trackDefinition, sessionConfig);
            IRaceSessionRunner runner = runners.Count == 0 ? raceSessionRunner : new RaceSessionRunner();
            if (runner == null)
            {
                throw new InvalidOperationException("[CarTrackBootstrap] IRaceSessionRunner is not injected.");
            }

            runner.SetSession(session);
            sessions.Add(session);
            runners.Add(runner);
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
