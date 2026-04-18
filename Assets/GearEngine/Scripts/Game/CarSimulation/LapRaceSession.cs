using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation
{
    public sealed class LapRaceSession
    {
        private const float defaultSpeedWhenUnset = 30f;

        public LapRaceSession(TrackDefinition trackDefinition, CarEntity car, RaceSessionConfig config)
        {
            this.trackDefinition = trackDefinition ?? throw new ArgumentNullException(nameof(trackDefinition));
            Car = car ?? throw new ArgumentNullException(nameof(car));
            RaceSessionConfig resolved = config ?? new RaceSessionConfig();
            variables = resolved.Variables;
            totalLaps = resolved.TotalLaps;
        }

        public TrackDefinition Track => trackDefinition;

        public CarEntity Car { get; }

        public SimulationLifecycleState Phase => phase;

        public bool IsSplineBound => trackLength > 1e-4f;

        internal float BoundTrackLength => trackLength;

        public float ProgressDistance => progressDistance;

        public float NormalizedProgress
        {
            get
            {
                if (!IsSplineBound || trackLength < 1e-4f)
                {
                    return 0f;
                }

                if (!isClosed)
                {
                    return Mathf.Clamp01(progressDistance / trackLength);
                }

                return (progressDistance % trackLength) / trackLength;
            }
        }

        public float CurrentSpeed => currentSpeed;

        public float RaceTime => raceTime;

        public int CurrentLap => currentLap;

        public IReadOnlyList<float> LapTimes => lapTimes;

        private readonly TrackDefinition trackDefinition;
        private readonly List<float> lapTimes = new List<float>();
        private readonly int totalLaps;
        private readonly CarVariableSet variables;
        private float trackLength;
        private bool isClosed;
        private bool clockRunning;
        private bool raceFinishNotified;
        private bool pendingSplineRestart;
        private float progressDistance;
        private float currentSpeed;
        private float raceTime;
        private int currentLap;
        private float previousLapStartTime;
        private SimulationLifecycleState phase = SimulationLifecycleState.Created;

        public event Action PresentationChanged;

        public event Action AfterTick;

        public bool ConsumePendingSplineRestart()
        {
            if (!pendingSplineRestart)
            {
                return false;
            }

            pendingSplineRestart = false;
            return true;
        }

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
        }

        public void Reset()
        {
            raceFinishNotified = false;
            progressDistance = 0f;
            currentSpeed = 0f;
            raceTime = 0f;
            currentLap = 0;
            lapTimes.Clear();
            previousLapStartTime = 0f;
            phase = SimulationLifecycleState.Created;
            pendingSplineRestart = true;
        }

        public void SetClockRunning(bool running)
        {
            clockRunning = running;
            if (running)
            {
                if (phase == SimulationLifecycleState.Created || phase == SimulationLifecycleState.Paused)
                {
                    phase = SimulationLifecycleState.Running;
                }

                return;
            }

            if (phase == SimulationLifecycleState.Running)
            {
                phase = SimulationLifecycleState.Paused;
            }
        }

        public void ForceFinish()
        {
            phase = SimulationLifecycleState.Completed;
            clockRunning = false;
            if (!raceFinishNotified)
            {
                raceFinishNotified = true;
                PresentationChanged?.Invoke();
            }
        }

        public void Tick(float dt)
        {
            if (!ShouldRunTick(dt))
            {
                return;
            }

            RunTickCore(dt);
            AfterTick?.Invoke();
        }

        private void RunTickCore(float dt)
        {
            currentSpeed = ReadSpeed();
            raceTime += dt;
            if (!isClosed)
            {
                TickOpenTrack(dt);
            }
            else
            {
                AdvanceClosedTrack(dt);
            }

            ApplyFinishClockStop();
            NotifyRaceFinishedOnce();
        }

        private bool ShouldRunTick(float dt)
        {
            return clockRunning && IsSplineBound && dt > 0f && phase == SimulationLifecycleState.Running;
        }

        private void TickOpenTrack(float dt)
        {
            progressDistance += currentSpeed * dt;
            if (progressDistance >= trackLength)
            {
                progressDistance = trackLength;
                currentSpeed = 0f;
                phase = SimulationLifecycleState.Completed;
                return;
            }
        }

        private void AdvanceClosedTrack(float dt)
        {
            progressDistance += currentSpeed * dt;
            int nextLap = Mathf.FloorToInt(progressDistance / trackLength);
            if (nextLap > currentLap)
            {
                float lapTime = raceTime - previousLapStartTime;
                lapTimes.Add(lapTime);
                previousLapStartTime = raceTime;
            }

            currentLap = nextLap;
            if (totalLaps >= 0 && currentLap >= totalLaps)
            {
                phase = SimulationLifecycleState.Completed;
            }
        }

        private void ApplyFinishClockStop()
        {
            if (phase == SimulationLifecycleState.Completed)
            {
                clockRunning = false;
            }
        }

        private void NotifyRaceFinishedOnce()
        {
            if (phase == SimulationLifecycleState.Completed && !raceFinishNotified)
            {
                raceFinishNotified = true;
                PresentationChanged?.Invoke();
            }
        }

        private float ReadSpeed()
        {
            if (variables == null || variables.Speed == null)
            {
                return defaultSpeedWhenUnset;
            }

            return Mathf.Max(0f, Car.GetValue<float>(variables.Speed));
        }
    }
}
