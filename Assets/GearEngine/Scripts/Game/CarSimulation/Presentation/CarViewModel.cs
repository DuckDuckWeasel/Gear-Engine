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

        public RoguelikeCarStats Stats { get; private set; }

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
            Stats = ResolveStats(Session);
        }

        /// <summary>When false, <see cref="CarView"/> only places the car until <see cref="CarView.AttachRunner"/> runs.</summary>
        public bool ShouldAttachRunnerOnBind { get; }

        protected override void Initialize()
        {
            base.Initialize();
            Stats = ResolveStats(Session);
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

        private RoguelikeCarStats ResolveStats(RaceState state)
        {
            RoguelikeCarStats defs = RoguelikeCarStats.Default;
            CarEntity car = state.Car;
            CarVariableSet vars = state.Config?.Variables;
            if (vars == null) return defs;

            if (car.TryGetValue(vars.Speed, out float s)) defs.statTopSpeed += s;
            if (car.TryGetValue(vars.Acceleration, out float a)) defs.statAcceleration += a;
            if (car.TryGetValue(vars.Handling, out float h)) defs.statSteeringGrip += h;
            if (car.TryGetValue(vars.Stability, out float st)) defs.statRacingLine += st;
            if (car.TryGetValue(vars.Recovery, out float r)) defs.statDriverReflexes += r;
            if (car.TryGetValue(vars.DriftPenalty, out float d)) defs.statDriftControl -= d;

            return defs;
        }
    }
}
