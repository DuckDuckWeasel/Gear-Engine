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
        FailedOvershoot = 9
    }
}
