using GearEngine.CarSimulation.Definitions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GearEngine.CarSimulation.SplineSimulation
{
    /// <summary>
    /// Global tuning asset for the spline-evaluate car driver. Controls speed limits,
    /// acceleration/braking rates, curvature analysis, and visual effect intensities.
    /// <para>
    /// Physics-free simulation branch. There is no dependency on
    /// PrometeoCarController or Rigidbody.
    /// </para>
    /// </summary>
    [CreateAssetMenu(menuName = "GearEngine/Simulation/Spline Simulation Config", fileName = "SplineSimulationConfig")]
    public sealed class SplineDriverConfig : SimulationConfigBase
    {
        // ── Speed ──────────────────────────────────────────────────────

        [FoldoutGroup("Speed", expanded: true)]
        [Tooltip("Absolute maximum speed on straights (m/s).")]
        public float maxSpeed = 55f;

        [FoldoutGroup("Speed")]
        [Tooltip("Speed floor on the sharpest possible curve (m/s).")]
        public float minCurveSpeed = 12f;

        [FoldoutGroup("Speed")]
        [Tooltip("Speed gain per second when accelerating.")]
        public float accelerationRate = 18f;

        [FoldoutGroup("Speed")]
        [Tooltip("Speed loss per second when braking.")]
        public float brakeRate = 35f;

        // ── Curvature Analysis ─────────────────────────────────────────

        [FoldoutGroup("Curvature", expanded: true)]
        [Tooltip("How far ahead (meters) to sample the spline for curvature analysis.")]
        public float curvatureLookaheadMeters = 40f;

        [FoldoutGroup("Curvature")]
        [Tooltip("Number of evenly-spaced samples taken inside the lookahead window.")]
        [Range(3, 30)]
        public int curvatureSampleCount = 10;

        [FoldoutGroup("Curvature")]
        [Tooltip("Reference curvature that maps to difficulty = 1.0 (hardest). Track-agnostic normalizer.")]
        public float maxCurvatureReference = 0.15f;

        [FoldoutGroup("Curvature")]
        [Tooltip("Multiplier range for the Risk stat on effective lookahead distance. x = risk-0 mult, y = risk-10 mult.")]
        public Vector2 riskLookaheadMultiplier = new Vector2(1.2f, 0.6f);

        // ── Lateral Offset ─────────────────────────────────────────────

        [FoldoutGroup("Lateral", expanded: true)]
        [Tooltip("Maximum lateral offset from centerline (track half-width clamp) in meters.")]
        public float maxLateralOffset = 4f;

        [FoldoutGroup("Lateral")]
        [Tooltip("Smoothing rate for lateral offset changes (higher = snappier lane changes).")]
        public float lateralSmoothRate = 6f;

        // ── Visual Effects ─────────────────────────────────────────────

        [FoldoutGroup("Visuals", expanded: true)]
        [Tooltip("Multiplier from centripetal acceleration to body roll angle (degrees).")]
        public float bodyRollScale = 0.08f;

        [FoldoutGroup("Visuals")]
        [Tooltip("Max body roll angle in degrees.")]
        public float maxBodyRollDeg = 8f;

        [FoldoutGroup("Visuals")]
        [Tooltip("Multiplier from lateral offset rate-of-change to visual slip angle (degrees).")]
        public float slipAngleScale = 3f;

        [FoldoutGroup("Visuals")]
        [Tooltip("Max visual slip angle in degrees.")]
        public float maxSlipAngleDeg = 15f;

        [FoldoutGroup("Visuals")]
        [Tooltip("Smoothing rate for slip angle changes.")]
        public float slipAngleSmoothRate = 8f;

        [FoldoutGroup("Visuals")]
        [Tooltip("Vertical suspension bob frequency (Hz).")]
        public float suspensionBobFrequency = 1.5f;

        [FoldoutGroup("Visuals")]
        [Tooltip("Vertical suspension bob amplitude (meters).")]
        public float suspensionBobAmplitude = 0.02f;
    }
}
