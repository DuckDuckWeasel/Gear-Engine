using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using Scaffold.MVVM;
using Scaffold.MVVM.Binding;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed partial class TrackViewModel : ViewModel
    {
        public TrackViewModel(TrackSimulation simulation)
        {
            this.simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        }

        public TrackDefinition Track => simulation.Track;
        public CarEntity Car => simulation.Car;
        public TrackSimulation Simulation => simulation;

        [NestedProperty] private TrackSimulation simulation;

        [ObservableProperty] private SimulationLifecycleState state;
        [ObservableProperty] private float currentSpeed;
        [ObservableProperty] private float progress01;
        [ObservableProperty] private int currentLap;
        [ObservableProperty] private bool isDrifting;

        protected override void Initialize()
        {
            base.Initialize();
            Bind(() => simulation.State, () => State);
            Bind(() => simulation.Race.CurrentSpeed, () => CurrentSpeed);
            Bind(() => simulation.Race.Progress01, () => Progress01);
            Bind(() => simulation.Race.CurrentLap, () => CurrentLap);
            Bind(() => simulation.Race.IsDrifting, () => IsDrifting);
        }

        public void Toggle(bool running)
        {
            simulation.Toggle(running);
        }

        public void Complete()
        {
            simulation.Complete();
        }

        internal void TearDown()
        {

        }
    }
}
