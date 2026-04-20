using Sirenix.OdinInspector;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(fileName = "SplineCarRunnerConfig", menuName = "GearEngine/Car Simulation/Spline Car Runner Config")]
    public class SplineCarRunnerConfigSO : ScriptableObject
    {
        [FoldoutGroup("Simulation Global Settings", expanded: true)]
        [InfoBox("Variables map definitions that this runner will attempt to read natively from Car Entities.")]
        public CarVariableSet variables;

        [FoldoutGroup("Simulation Hardware Constraints", expanded: true)]
        [InfoBox("Limits max scaling for the simulation limits.")]
        public float baseMaxSpeed = 160f;
        
        [FoldoutGroup("Simulation Hardware Constraints")]
        public float baseAcceleration = 22f;
        
        [FoldoutGroup("Simulation Hardware Constraints")]
        public float baseBrakeForce = 600f;
        
        [FoldoutGroup("Simulation Hardware Constraints")]
        [Range(0f, 1f), PropertyTooltip("Minimum speed floor as a percentage of Base Max Speed.")]
        public float minSpeedPercentage = 0.4f;

        [FoldoutGroup("Simulation Hardware Constraints")]
        [Range(1f, 1.5f), PropertyTooltip("Global artificial speed-up multiplier without drifting (Creates a sense of break-neck speed)")]
        public float baseSimulationMultiplier = 1.35f;


        [FoldoutGroup("Advanced Heuristics (Automated)", expanded: false)]
        [InfoBox("ALL variables here are calculated mathematically based on the Roguelike Base Stats. Tweak their 'Bounds' to alter the limits (X = 0% stat, Y = 100% stat).")]
        
        [LabelText("Drift Acceleration Mult")] public Vector2 boundsDriftAccMult = new Vector2(1.1f, 2.5f);
        [LabelText("Drift Steer Assist Mult")] public Vector2 boundsDriftSteerMult = new Vector2(1.0f, 2.0f);
        [LabelText("Hairpin Acc Boost")] public Vector2 boundsHairpinAccBoost = new Vector2(1.5f, 4f);
        [LabelText("Hairpin Steer Assist Boost")] public Vector2 boundsHairpinSteerBoost = new Vector2(1.2f, 3.0f);

        [LabelText("Waypoint Count")] public int waypointCount = 7;
        [LabelText("Base Waypoint Dist")] public Vector2 boundsBaseWaypointDist = new Vector2(4f, 8f);
        [LabelText("Dist Speed Mult")] public Vector2 boundsDistSpeedMult = new Vector2(0.1f, 0.2f);
        [LabelText("Waypoint Spacing Mult")] public float waypointSpacingMultiplier = 1.35f;

        [LabelText("Arrival Range Base")] public Vector2 boundsArrivalRangeBase = new Vector2(4f, 8f);
        [LabelText("Arrival Speed Mult")] public Vector2 boundsArrivalSpeedMult = new Vector2(0.1f, 0.2f);

        [LabelText("Max Lateral Offset")] public float waypointMaxLateralOffset = 0.7f;
        [LabelText("Lane Change Var Speed")] public float laneChangeVariationSpeed = 0.25f;
        [LabelText("Max Deviation Distance")] public float maxDeviationDistance = 7f;

        [LabelText("Pre Curve Wide Offset")] public Vector2 boundsPreCurveOffset = new Vector2(1f, 8f);
        [LabelText("Post Curve Wide Offset")] public Vector2 boundsPostCurveOffset = new Vector2(0.5f, 4f);
        [LabelText("Steer Deadzone")] public Vector2 boundsSteerDeadzone = new Vector2(0.1f, 0.02f);

        [LabelText("Curve Angle")] public Vector2 boundsCurveAngle = new Vector2(15f, 25f);
        [LabelText("Handbrake Angle")] public Vector2 boundsHandbrakeAngle = new Vector2(30f, 45f);
        [LabelText("Hairpin Angle")] public Vector2 boundsHairpinAngle = new Vector2(60f, 85f);
        [LabelText("Macro Curve Angle")] public Vector2 boundsMacroCurveAngle = new Vector2(35f, 50f);

        [LabelText("Safe Corner Bounds"), PropertyTooltip("Safe AI cornering speed target range (km/h)")] 
        public Vector2 boundsSafeCornerSpeed = new Vector2(60f, 130f);
        [LabelText("Steer Assist Bounds")] 
        public Vector2 boundsArcadeSteerAssist = new Vector2(0.2f, 3.5f);
    }
}
