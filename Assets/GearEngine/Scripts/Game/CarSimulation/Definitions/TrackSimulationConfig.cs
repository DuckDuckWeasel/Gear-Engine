using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public sealed class TrackSimulationConfig
    {
        public CarVariableSet Variables => variables;

        public SimpleTrackDriverTuning Driver => driver;

        [SerializeField]
        private CarVariableSet variables;

        [SerializeField]
        private SimpleTrackDriverTuning driver = new SimpleTrackDriverTuning();
    }

    [Serializable]
    public sealed class SimpleTrackDriverTuning
    {
        public float WaypointSpacingMetres => waypointSpacingMetres;

        public float WaypointCaptureRadius => waypointCaptureRadius;

        public float LookaheadMetres => lookaheadMetres;

        public float BaseMaxYawRateDegreesPerSecond => baseMaxYawRateDegreesPerSecond;

        public float Acceleration => acceleration;

        public float Braking => braking;

        public float CornerSlowdownYawDemandScale => cornerSlowdownYawDemandScale;

        public float PerfectLineErrorDegrees => perfectLineErrorDegrees;

        public float DriftErrorMinDegrees => driftErrorMinDegrees;

        public float DriftErrorMaxDegrees => driftErrorMaxDegrees;

        public float DriftSpeedPenalty => driftSpeedPenalty;

        public float SlipVisualLerpSpeed => slipVisualLerpSpeed;

        [SerializeField]
        private float waypointSpacingMetres = 4f;

        [SerializeField]
        private float waypointCaptureRadius = 2.5f;

        [SerializeField]
        private float lookaheadMetres = 6f;

        [SerializeField]
        private float baseMaxYawRateDegreesPerSecond = 90f;

        [SerializeField]
        private float acceleration = 12f;

        [SerializeField]
        private float braking = 18f;

        [SerializeField]
        private float cornerSlowdownYawDemandScale = 0.35f;

        [SerializeField]
        private float perfectLineErrorDegrees = 8f;

        [SerializeField]
        private float driftErrorMinDegrees = 12f;

        [SerializeField]
        private float driftErrorMaxDegrees = 35f;

        [SerializeField]
        private float driftSpeedPenalty = 0.06f;

        [SerializeField]
        private float slipVisualLerpSpeed = 4f;
    }
}
