using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation
{
    public sealed class LapRaceSession
    {
        public LapRaceSession(TrackDefinition trackDefinition, CarEntity car, RaceSessionConfig config)
        {
            this.trackDefinition = trackDefinition ?? throw new ArgumentNullException(nameof(trackDefinition));
            Car = car ?? throw new ArgumentNullException(nameof(car));
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Variables = config.Variables;
            lapConfig = config.Lap ?? new LapSimulationConfig();
            samplerConfig = config.Sampler ?? new SplineSamplerConfig();
            CarVisualConfig visualConfig = config.Visual ?? new CarVisualConfig();
            lapSimulation = new LapSimulation(raceState, lapConfig);
            visualPlayback = new CarVisualPlayback(visualState, visualConfig, lapConfig);
        }

        public TrackDefinition Track => trackDefinition;

        public CarEntity Car { get; }

        public CarVariableSet Variables { get; }

        public RaceState RaceState => raceState;

        public CarVisualState VisualState => visualState;

        public LapSimulationConfig LapConfig => lapConfig;

        public CurveSample LastCurveSample { get; private set; }

        public bool ClockRunning => clockRunning;

        public bool VisualPlaybackEnabled
        {
            get => visualPlaybackEnabled;
            set => visualPlaybackEnabled = value;
        }

        public bool IsSplineBound => sampler != null && trackLength > 1e-4f;

        private readonly TrackDefinition trackDefinition;
        private readonly LapSimulation lapSimulation;
        private readonly CarVisualPlayback visualPlayback;
        private readonly RaceState raceState = new RaceState();
        private readonly CarVisualState visualState = new CarVisualState();
        private readonly LapSimulationConfig lapConfig;
        private readonly SplineSamplerConfig samplerConfig;
        private SplineCurveSampler sampler;
        private float trackLength;
        private bool isClosed;
        private bool clockRunning;
        private bool visualPlaybackEnabled = true;
        private bool raceFinishNotified;

        public event Action PresentationChanged;

        public void BindSpline(SplineContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            Spline spline = container.Spline;
            if (spline.Count < 2)
            {
                Debug.LogError("[LapRaceSession] Spline must contain at least two knots.");
                return;
            }

            isClosed = spline.Closed;
            trackLength = spline.GetLength();
            sampler = new SplineCurveSampler(spline, samplerConfig, isClosed);
            PrimeSample();
        }

        public void Reset()
        {
            raceFinishNotified = false;
            raceState.Reset();
            visualState.Reset();
            PrimeSample();
        }

        public void PrimeSample()
        {
            if (sampler == null || trackLength < 1e-4f)
            {
                return;
            }

            float t = Mathf.Clamp01(raceState.NormalizedProgress);
            LastCurveSample = sampler.Sample(t);
        }

        public void SetClockRunning(bool running)
        {
            clockRunning = running;
            if (running && raceState.Lifecycle == RaceLifecycle.Idle)
            {
                raceState.Lifecycle = RaceLifecycle.Running;
            }
        }

        public void ForceFinish()
        {
            raceState.Lifecycle = RaceLifecycle.Finished;
            clockRunning = false;
            if (!raceFinishNotified)
            {
                raceFinishNotified = true;
                PresentationChanged?.Invoke();
            }
        }

        public bool IsCarPlaybackAllowed()
        {
            return clockRunning && raceState.Lifecycle == RaceLifecycle.Running && IsSplineBound;
        }

        public void Tick(float dt)
        {
            if (!ShouldRunTick(dt))
            {
                return;
            }

            RunLapAndVisual(dt);
            ApplyFinishClockStop();
            NotifyRaceFinishedOnce();
        }

        private bool ShouldRunTick(float dt)
        {
            return clockRunning && IsSplineBound && dt > 0f && raceState.Lifecycle == RaceLifecycle.Running;
        }

        private void RunLapAndVisual(float dt)
        {
            LastCurveSample = sampler.Sample(raceState.NormalizedProgress);
            lapSimulation.Tick(dt, Car, Variables, LastCurveSample, trackLength, isClosed);
            if (visualPlaybackEnabled)
            {
                visualPlayback.Tick(dt, Car, Variables, LastCurveSample);
            }
            else
            {
                visualState.ClearCosmetic();
            }
        }

        private void ApplyFinishClockStop()
        {
            if (raceState.Lifecycle == RaceLifecycle.Finished)
            {
                clockRunning = false;
            }
        }

        private void NotifyRaceFinishedOnce()
        {
            if (raceState.Lifecycle == RaceLifecycle.Finished && !raceFinishNotified)
            {
                raceFinishNotified = true;
                PresentationChanged?.Invoke();
            }
        }
    }
}
