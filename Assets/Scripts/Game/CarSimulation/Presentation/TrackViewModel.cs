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

        [NestedProperty] private TrackSimulation simulation;

        [ObservableProperty] private SimulationLifecycleState state;
        [ObservableProperty] private float currentSpeed;

        protected override void Initialize()
        {
            base.Initialize();
            BindChildViewModel(simulation);
            Bind(() => simulation.State, () => State);
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
