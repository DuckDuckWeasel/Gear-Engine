using System;

namespace GearEngine.SplineEvaluate.Simulation
{
    /// <summary>
    /// Complete runtime state for one car being driven along a spline.
    /// Updated every tick by <see cref="SplineEvaluateDriver"/>.
    /// All values are deterministic and derived exclusively from the spline,
    /// the <see cref="Definitions.SplineDriverConfig"/>, and the
    /// <see cref="Definitions.DriverPersonality"/>.
    /// </summary>
    [Serializable]
    public struct SplineMotionState
    {
        /// <summary>Normalized spline parameter (0–1). Wraps on lap completion.</summary>
        public float T;

        /// <summary>Current scalar speed in meters per second.</summary>
        public float Speed;

        /// <summary>Target speed cap derived from curvature lookahead (m/s).</summary>
        public float TargetSpeed;

        /// <summary>Lateral offset from centerline in meters (positive = right, negative = left).</summary>
        public float LateralOffset;

        /// <summary>Raw (unsmoothed) target lateral offset before smoothing.</summary>
        public float RawLateralOffset;

        /// <summary>Visual-only yaw offset in degrees, simulating tire slip / oversteer.</summary>
        public float SlipAngle;

        /// <summary>Visual-only roll angle in degrees around the forward axis, simulating weight transfer.</summary>
        public float BodyRoll;

        /// <summary>Visual-only vertical offset in meters, simulating suspension movement.</summary>
        public float SuspensionOffset;

        /// <summary>Unsigned curvature magnitude at the current <see cref="T"/> position.</summary>
        public float Curvature;

        /// <summary>Signed curvature at the current <see cref="T"/> position (positive = right turn, negative = left turn).</summary>
        public float SignedCurvature;

        /// <summary>Maximum curvature magnitude found in the lookahead window (used for speed decisions).</summary>
        public float LookaheadMaxCurvature;

        /// <summary>Signed curvature of the maximum curvature point found in the lookahead window.</summary>
        public float SignedLookaheadMaxCurvature;

        /// <summary>Completed laps (incremented when <see cref="T"/> wraps past 1.0).</summary>
        public int CompletedLaps;

        /// <summary>True when the driver is actively decelerating toward <see cref="TargetSpeed"/>.</summary>
        public bool IsBraking;

        /// <summary>True when visual slip angle exceeds a noticeable threshold.</summary>
        public bool IsDrifting;

        /// <summary>True when the driver is accelerating toward <see cref="TargetSpeed"/>.</summary>
        public bool IsAccelerating;

        /// <summary>Previous frame's lateral offset, used to compute offset rate of change for slip angle.</summary>
        public float PreviousLateralOffset;

        /// <summary>Previous frame's T, used for lap-wrap detection.</summary>
        public float PreviousT;
    }
}
