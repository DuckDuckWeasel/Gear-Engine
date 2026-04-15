using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public struct CurveBandDefinition
    {
        public float MinCurvature;
        public float MaxCurvature;
        public float Difficulty01;
    }

    [CreateAssetMenu(menuName = "Game/Car/Track Simulation Tuning", fileName = "TrackSimulationTuning")]
    public sealed class TrackSimulationTuning : ScriptableObject
    {
        public float LookAheadMinMetres => lookAheadMinMetres;

        public float LookAheadSpeedFactor => lookAheadSpeedFactor;

        public float AheadProbeStep => aheadProbeStep;

        public float HandlingNormalizationScale => handlingNormalizationScale;

        public float HandlingTurnRateDegPerSec => handlingTurnRateDegPerSec;

        public float RecoveryRateDegPerSec => recoveryRateDegPerSec;

        public float MaxHeadingErrorDeg => maxHeadingErrorDeg;

        public float SpeedPenaltyScale => speedPenaltyScale;

        public float SlipAngleScale => slipAngleScale;

        public float LateralOffsetScale => lateralOffsetScale;

        public float IsDriftingThreshold => isDriftingThreshold;

        public float IsOvershotThreshold => isOvershotThreshold;

        public CurveBandDefinition[] CurveBands => curveBands;

        [SerializeField] private float lookAheadMinMetres = 8f;

        [SerializeField] private float lookAheadSpeedFactor = 0.75f;

        [SerializeField] private float aheadProbeStep = 0.25f;

        [SerializeField] private float handlingNormalizationScale = 100f;

        [SerializeField] private float handlingTurnRateDegPerSec = 120f;

        [SerializeField] private float recoveryRateDegPerSec = 45f;

        [SerializeField] private float maxHeadingErrorDeg = 30f;

        [SerializeField] private float speedPenaltyScale = 0.15f;

        [SerializeField] private float slipAngleScale = 28f;

        [SerializeField] private float lateralOffsetScale = 0.45f;

        [SerializeField] private float isDriftingThreshold = 0.15f;

        [SerializeField] private float isOvershotThreshold = 0.6f;

        [SerializeField] private CurveBandDefinition[] curveBands = DefaultCurveBands();

        private static CurveBandDefinition[] DefaultCurveBands()
        {
            return new[]
            {
                new CurveBandDefinition { MinCurvature = 0f, MaxCurvature = 0.02f, Difficulty01 = 0f },
                new CurveBandDefinition { MinCurvature = 0.02f, MaxCurvature = 0.05f, Difficulty01 = 0.25f },
                new CurveBandDefinition { MinCurvature = 0.05f, MaxCurvature = 0.1f, Difficulty01 = 0.6f },
                new CurveBandDefinition { MinCurvature = 0.1f, MaxCurvature = 1e6f, Difficulty01 = 1f },
            };
        }
    }
}
