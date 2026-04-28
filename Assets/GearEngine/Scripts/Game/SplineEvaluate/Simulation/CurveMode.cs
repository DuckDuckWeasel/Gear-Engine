namespace GearEngine.SplineEvaluate.Simulation
{
    /// <summary>
    /// Trajectory strategies when navigating a curve.
    /// </summary>
    public enum CurveMode
    {
        // ── PERFECT MODES (Successful execution of CorneringSkill) ──
        PerfectOutInOut = 0,
        PerfectLateApex = 1,
        PerfectEarlyApex = 2,
        PerfectCenter = 3,
        PerfectHugInside = 4,

        // ── FAILED MODES (Failed execution of CorneringSkill) ──
        FailedInOutIn = 5,
        FailedHugOutside = 6,
        FailedWobble = 7,
        FailedBalk = 8,
        FailedOvershoot = 9,

        // ── SIMPLE MODES (Used when NOT drifting to avoid weird snappy visuals) ──
        SimpleCenter = 10,
        SimpleInside = 11,
        SimpleOutside = 12,
        SimpleFailedOutside = 13,
        SimpleFailedInside = 14
    }

    public static class CurveModeExtensions
    {
        public static bool IsFailedMode(this CurveMode mode)
        {
            int m = (int)mode;
            return (m >= 5 && m <= 9) || m == 13 || m == 14;
        }
    }
}
