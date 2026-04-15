using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using VContainer;
using GearEngine.GearEngine.Presentation;

namespace GearEngine.Race
{
    public sealed class RaceViewModel : ViewModel
    {
        public RaceViewModel(RaceStartData startData)
        {
            this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
        }

        public TrackViewModel Track { get; private set; }
        public GearEngineViewModel GearEngine { get; private set; }

        public bool IsRaceRunning => false;

        private readonly RaceStartData startData;

        [Inject] private TrackSimulationFactory trackFactory;
        [Inject] private ITrackSimulationRunner trackSimulationRunner;
        
        protected override void Initialize()
        {
            base.Initialize();
            ValidateStartData();

            TrackSimulation simulation = trackFactory.Create(startData.CarDefinition, startData.TrackDefinition, startData.SimulationConfig);
            SetupTrack(simulation);
            SetupGears(startData.GearEngineData, simulation);
        }

        public void ToggleRace()
        {
            if (IsRaceRunning)
            {
                //stop engine
                Track.Toggle(false);
            }
            else
            {
                //start engine
                Track.Toggle(true);
            }
        }

        private void ValidateStartData()
        {
            if (startData.TrackDefinition == null)
            {
                throw new InvalidOperationException("[RaceViewModel] RaceStartData.TrackDefinition is missing.");
            }

            if (startData.CarDefinition == null)
            {
                throw new InvalidOperationException("[RaceViewModel] RaceStartData.CarDefinition is missing.");
            }
        }

        private void SetupTrack(TrackSimulation simulation)
        {
            trackSimulationRunner.SetSimulation(simulation);
            Track = new TrackViewModel(simulation);
            BindChildViewModel(Track);
        }

        private void SetupGears(GearEngineStartData gearData, TrackSimulation simulation)
        {
            GearEngine = new GearEngineViewModel(gearData, simulation);
            BindChildViewModel(GearEngine);
        }
    }
}
