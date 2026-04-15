using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Car/Track Simulation Tuning", fileName = "TrackSimulationTuning")]
    public sealed class TrackSimulationTuning : ScriptableObject
    {
        public float OvershootPenaltyScale => overshootPenaltyScale;

        public float ActiveCapCurvatureSpan => activeCapCurvatureSpan;

        public float LookAheadMinMetres => lookAheadMinMetres;

        public float LookAheadSpeedFactor => lookAheadSpeedFactor;

        public float AheadProbeStep => aheadProbeStep;

        public float CurvatureEpsilon => curvatureEpsilon;

        public float HandlingNormalizationScale => handlingNormalizationScale;

        public float CurvatureStressMultiplier => curvatureStressMultiplier;

        public float SpeedStressScale => speedStressScale;

        public float SpeedStressToLineErrorScale => speedStressToLineErrorScale;

        public float LineDifficultyFromCurvature => lineDifficultyFromCurvature;

        public float LineDifficultyFromSpeed => lineDifficultyFromSpeed;

        public float MaxAbsorbableDifficulty => maxAbsorbableDifficulty;

        public float LineErrorBuildRate => lineErrorBuildRate;

        public float LineErrorDecayRate => lineErrorDecayRate;

        public float SlipAngleScale => slipAngleScale;

        public float LateralOffsetScale => lateralOffsetScale;

        public float SlipBuildRate => slipBuildRate;

        public float OffsetBuildRate => offsetBuildRate;

        public float SlipRecoveryRate => slipRecoveryRate;

        public float OffsetRecoveryRate => offsetRecoveryRate;

        public float IsDriftingThreshold => isDriftingThreshold;

        public float IsOvershotThreshold => isOvershotThreshold;

        [SerializeField] private float overshootPenaltyScale = 0.15f;

        [SerializeField] private float activeCapCurvatureSpan = 0.06f;

        [SerializeField] private float lookAheadMinMetres = 8f;

        [SerializeField] private float lookAheadSpeedFactor = 0.75f;

        [SerializeField] private float aheadProbeStep = 0.25f;

        [SerializeField] private float curvatureEpsilon = 1e-5f;

        [SerializeField] private float handlingNormalizationScale = 100f;

        [SerializeField] private float curvatureStressMultiplier = 12f;

        [SerializeField] private float speedStressScale = 1f;

        [SerializeField] private float speedStressToLineErrorScale = 0.5f;

        [SerializeField] private float lineDifficultyFromCurvature = 1f;

        [SerializeField] private float lineDifficultyFromSpeed = 0.01f;

        [SerializeField] private float maxAbsorbableDifficulty = 1f;

        [SerializeField] private float lineErrorBuildRate = 1f;

        [SerializeField] private float lineErrorDecayRate = 0.8f;

        [SerializeField] private float slipAngleScale = 28f;

        [SerializeField] private float lateralOffsetScale = 0.45f;

        [SerializeField] private float slipBuildRate = 6f;

        [SerializeField] private float offsetBuildRate = 4f;

        [SerializeField] private float slipRecoveryRate = 12f;

        [SerializeField] private float offsetRecoveryRate = 8f;

        [SerializeField] private float isDriftingThreshold = 0.12f;

        [SerializeField] private float isOvershotThreshold = 0.5f;
    }
}
