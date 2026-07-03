using System;

namespace GearEngine.CarSimulation.SplineSimulation
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

        /// <summary>Vertical offset in meters, used specifically for the arcade drift jump.</summary>
        public float JumpOffset;

        /// <summary>Maximum scale intensity for the faux perspective jump pop (e.g. 0.1 to 0.4).</summary>
        public float JumpScaleIntensity;

        /// <summary>Timer used to anticipate the drift and delay traction loss.</summary>
        public float DriftAnticipationTimer;

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

        /// <summary>The currently active trajectory strategy for a curve sequence.</summary>
        public CurveMode ActiveCurveMode;

        /// <summary>Sign (+1 or -1) of the current curve sequence. Determines inside/outside directions.</summary>
        public float CurrentCurveSign;

        /// <summary>True when the driver is actively navigating a curve sequence (entry, apex, or exit).</summary>
        public bool IsInCurveSequence;

        /// <summary>True if the driver decided to drift during the current curve sequence.</summary>
        public bool WillDriftCurrentCurve;

        /// <summary>True if the driver has already executed a wobble during the current curve sequence.</summary>
        public bool HasWobbledThisCurve;

        /// <summary>The Unity Time.time when the wobble was triggered.</summary>
        public float WobbleTriggerTime;

        /// <summary>True if the driver already received their drift boost at the end of this curve.</summary>
        public bool HasBoostedThisCurve;

        /// <summary>True if the driver already received their failure penalty for this curve.</summary>
        public bool HasFailedThisCurve;

        /// <summary>True if the driver has already executed the drift entry jump for this curve.</summary>
        public bool HasJumpedThisCurve;
    }
}
