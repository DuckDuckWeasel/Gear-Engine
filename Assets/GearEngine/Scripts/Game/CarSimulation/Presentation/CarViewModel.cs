using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed partial class CarViewModel : ViewModel
    {
        public RaceState Session { get; }
        public CarEntity Car => Session.Car;
        public SplineCarRunnerService RunnerService { get; }

        [ObservableProperty] private float speed;
        [ObservableProperty] private float progress;
        [ObservableProperty] private bool isBraking;
        [ObservableProperty] private bool isDrifting;
        [ObservableProperty] private bool isAccelerating;
        [ObservableProperty] private float currentAcceleration;

        public CarViewModel(RaceState session, SplineCarRunnerService runnerService, bool attachRunnerOnBind = true)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            RunnerService = runnerService ?? throw new ArgumentNullException(nameof(runnerService));
            ShouldAttachRunnerOnBind = attachRunnerOnBind;
        }

        /// <summary>When false, <see cref="CarView"/> only places the car until <see cref="CarView.AttachRunner"/> runs.</summary>
        public bool ShouldAttachRunnerOnBind { get; }

        protected override void Initialize()
        {
            base.Initialize();
        }

        public void TickTelemetry()
        {
            if (RunnerService.GetTelemetry(Car, out CarTelemetryData data))
            {
                Speed = data.Speed;
                Progress = data.Progress;
                IsBraking = data.IsBraking;
                IsDrifting = data.IsDrifting;
                IsAccelerating = data.IsAccelerating;
                CurrentAcceleration = data.CurrentAcceleration;
            }
        }
    }
}
