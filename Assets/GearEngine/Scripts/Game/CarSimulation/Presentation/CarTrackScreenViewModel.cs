using GearEngine.CarSimulation;
using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using Scaffold.MVVM;
using UnityEngine;
using VContainer;

namespace GearEngine.CarSimulation.Presentation
{
    /// <summary>
    /// Root screen for the spline track test scene: owns preview <see cref="RaceState"/> instances,
    /// defers runner attachment until the user starts a race, and exposes child VMs to the view.
    /// </summary>
    public sealed class CarTrackScreenViewModel : ViewModel
    {
        private readonly TrackDefinition trackDefinition;
        private readonly IReadOnlyList<CarDefinition> carDefinitions;
        private readonly RaceSessionConfig sessionConfig;

        private bool racesRegisteredWithManager;

        public CarTrackScreenViewModel(
            TrackDefinition trackDefinition,
            IReadOnlyList<CarDefinition> carDefinitions,
            RaceSessionConfig sessionConfig)
        {
            this.trackDefinition = trackDefinition ?? throw new ArgumentNullException(nameof(trackDefinition));
            this.carDefinitions = carDefinitions ?? throw new ArgumentNullException(nameof(carDefinitions));
            this.sessionConfig = sessionConfig ?? new RaceSessionConfig();
        }

        [Inject] private TrackSimulationFactory factory;
        [Inject] private RaceManagerService raceManager;
        [Inject] private SplineCarRunnerService aiRunner;

        /// <summary>Fired once before the first race start so views can call <see cref="CarView.AttachRunner"/> on spawned cars.</summary>
        public event Action AttachRunnersRequested;

        public IReadOnlyList<RaceState> Sessions { get; private set; }

        public TrackViewModel Track { get; private set; }

        public IReadOnlyList<CarViewModel> Cars { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();
            BuildPreviewState();
        }

        protected override void OnClosed()
        {
            try
            {
                if (racesRegisteredWithManager && Sessions != null && raceManager != null)
                {
                    foreach (RaceState session in Sessions)
                    {
                        raceManager.UnregisterRace(session);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CarTrackScreenViewModel] OnClosed failed: {ex.Message}\n{ex.StackTrace}");
            }

            base.OnClosed();
        }

        private void BuildPreviewState()
        {
            var sessions = new List<RaceState>();
            var carVms = new List<CarViewModel>();

            foreach (CarDefinition carDef in carDefinitions)
            {
                if (carDef == null)
                {
                    continue;
                }

                RaceState session = factory.Create(carDef, trackDefinition, sessionConfig);
                sessions.Add(session);
                var carVm = new CarViewModel(session, aiRunner, attachRunnerOnBind: false);
                BindChildViewModel(carVm);
                carVms.Add(carVm);
            }

            Sessions = sessions;
            Cars = carVms;

            if (sessions.Count == 0)
            {
                return;
            }

            Track = new TrackViewModel(sessions[0], raceManager, aiRunner, factory);
            BindChildViewModel(Track);
        }

        public void ToggleRace()
        {
            if (Sessions == null || Sessions.Count == 0 || Track == null)
            {
                return;
            }

            RaceState primary = Sessions[0];

            if (primary.Phase == SimulationLifecycleState.Running)
            {
                Track.Toggle(false);
                return;
            }

            if (primary.Phase == SimulationLifecycleState.Completed)
            {
                primary.Reset();
            }

            if (!racesRegisteredWithManager)
            {
                foreach (RaceState session in Sessions)
                {
                    raceManager.RegisterRace(session);
                }

                racesRegisteredWithManager = true;
                AttachRunnersRequested?.Invoke();
            }

            Track.Toggle(true);
        }
    }
}
