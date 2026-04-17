using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Reflection;
using System.Collections.Generic;
using Sirenix.OdinInspector;


[InfoBox("AI Spline Runner\nControls the logic for a vehicle following a Spline in a Roguelike environment.\nIt overrides input on the Prometeo Car Controller based on upcoming curve angles, distances, and scaled Roguelike physics stats.")]
public class SplineCarRunner : MonoBehaviour
{
    [FoldoutGroup("Scene Configuration", Expanded = true)]
    [Required, PropertyTooltip("The spline acting as the pathing track for the AI.")]
    public SplineContainer track;

    [FoldoutGroup("Scene Configuration")]
    [Required, PropertyTooltip("The Prometeo Car Controller that will receive AI inputs from this script.")]
    public PrometeoCarController targetCar;

    [FoldoutGroup("Engine Base Limits", Expanded = true)]
    [InfoBox("These define the absolute maximum capabilities of the vehicle when all Roguelike stats are at 100%.")]
    [Range(20, 190), PropertyTooltip("Maximum capacity of Top Speed.")]
    [OnValueChanged("ApplyProgressionStats")]
    public int baseMaxSpeed = 190;

    [FoldoutGroup("Engine Base Limits")]
    [Range(0f, 1f), PropertyTooltip("Minimum speed floor as a percentage of Base Max Speed (e.g. 0.4 means 40%). Prevents the car from becoming completely immobile at 0% stats.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float minSpeedPercentage = 0.4f;

    [FoldoutGroup("Engine Base Limits")]
    [Range(1, 10), PropertyTooltip("Maximum capacity of Acceleration Multiplier.")]
    [OnValueChanged("ApplyProgressionStats")]
    public int baseAcceleration = 10;

    [FoldoutGroup("Engine Base Limits")]
    [Range(100, 600), PropertyTooltip("Maximum capacity of Brake Force.")]
    [OnValueChanged("ApplyProgressionStats")]
    public int baseBrakeForce = 350;

    [FoldoutGroup("Engine Base Limits")]
    [Range(1, 10), PropertyTooltip("Maximum slippery capability (highest value).")]
    [OnValueChanged("ApplyProgressionStats")]
    public int baseDriftGrip = 5;

    [FoldoutGroup("Engine Base Limits")]
    [Range(1f, 10f), PropertyTooltip("Maximum capacity of Simulation Speed (Nitro). Multiplies overall physics execution speeds.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float baseSimulationMultiplier = 2f;

    [FoldoutGroup("Roguelike Base Stats", Expanded = true)]
    [InfoBox("Normalized stats (0 to 1) representing the current power level of the vehicle in the run.")]
    
    [Header("Car Mechanical Capabilities")]
    [Range(0f, 1f), PropertyTooltip("Scales Car Engine Max Speed limit.")]
    [OnValueChanged("ApplyProgressionStats")] public float statTopSpeed = 0.5f;

    [Range(0f, 1f), PropertyTooltip("Scales Engine raw acceleration torque.")]
    [OnValueChanged("ApplyProgressionStats")] public float statAcceleration = 0.5f;

    [Range(0f, 1f), PropertyTooltip("Scales Physical Brake stopping power.")]
    [OnValueChanged("ApplyProgressionStats")] public float statBrakingSystem = 0.5f;

    [Range(0f, 1f), PropertyTooltip("Scales physical drift grip logic (1.0 = Drifts beautifully with wide slippery slides).")]
    [OnValueChanged("ApplyProgressionStats")] public float statDriftControl = 0.5f;

    [Range(0f, 1f), PropertyTooltip("Scales Nitrous Oxide explosion power when exiting drifts and hairpins!")]
    [OnValueChanged("ApplyProgressionStats")] public float statNitrousBoost = 0.5f;

    [Header("AI Driver Skill (Ghost)")]
    [Range(0f, 1f), PropertyTooltip("How faithfully the AI sticks to the track limits and nullifies physics understeer.")]
    [OnValueChanged("ApplyProgressionStats")] public float statSteeringGrip = 0.5f;

    [Range(0f, 1f), PropertyTooltip("AI's ability to take Out-In-Out racing lines instead of rigidly sniffing the middle of the road.")]
    [OnValueChanged("ApplyProgressionStats")] public float statRacingLine = 0.5f;

    [Range(0f, 1f), PropertyTooltip("AI's courage to brake late and read predictive road chords fast.")]
    [OnValueChanged("ApplyProgressionStats")] public float statDriverReflexes = 0.5f;

    [FoldoutGroup("Advanced Heuristics (Automated)", expanded: false)]
    [InfoBox("ALL variables here are calculated mathematically based on the Roguelike Base Stats. Tweak their 'Bounds' to alter the limits (X = 0% stat, Y = 100% stat).")]
    
    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float driftAccelerationMultiplier;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsDriftAccMult = new Vector2(1.1f, 2.5f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float driftSteerAssistMultiplier;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsDriftSteerMult = new Vector2(1.0f, 2.0f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float hairpinAccelerationBoost;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsHairpinAccBoost = new Vector2(1.5f, 4f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float hairpinSteerAssistBoost;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsHairpinSteerBoost = new Vector2(1.2f, 3.0f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public int waypointCount = 7;
    
    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float baseWaypointDistance;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsBaseWaypointDist = new Vector2(4f, 8f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float distanceSpeedMultiplier;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsDistSpeedMult = new Vector2(0.1f, 0.2f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float waypointSpacingMultiplier = 1.35f;

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float waypointArrivalRangeBase;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsArrivalRangeBase = new Vector2(4f, 8f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float waypointArrivalSpeedMultiplier;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsArrivalSpeedMult = new Vector2(0.1f, 0.2f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float waypointMaxLateralOffset = 0.7f;
    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float laneChangeVariationSpeed = 0.25f;
    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float maxDeviationDistance = 7f;

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float preCurveWideOffset;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsPreCurveOffset = new Vector2(1f, 8f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float postCurveWideOffset;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsPostCurveOffset = new Vector2(0.5f, 4f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float steerDeadzone;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsSteerDeadzone = new Vector2(0.1f, 0.02f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float curveAngleThreshold;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsCurveAngle = new Vector2(15f, 25f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float handbrakeAngleThreshold;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsHandbrakeAngle = new Vector2(30f, 45f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float hairpinAngleThreshold;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsHairpinAngle = new Vector2(60f, 85f);

    [FoldoutGroup("Advanced Heuristics (Automated)")] [ReadOnly] public float macroCurveAngleThreshold;
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsMacroCurveAngle = new Vector2(35f, 50f);

    [FoldoutGroup("Calculated Derived Constraints", expanded: false)]
    [ReadOnly, ShowInInspector] public float maxSimulationSpeed;
    
    [FoldoutGroup("Calculated Derived Constraints")]
    [ReadOnly, ShowInInspector] public float safeCornerSpeed; 
    [FoldoutGroup("Calculated Derived Constraints"), LabelText(" Safe Corner Bounds"), OnValueChanged("ApplyProgressionStats"), PropertyTooltip("Safe AI cornering speed target range (km/h)")] 
    public Vector2 boundsSafeCornerSpeed = new Vector2(60f, 130f);

    [FoldoutGroup("Calculated Derived Constraints")]
    [ReadOnly, ShowInInspector] public float arcadeSteerAssist;
    [FoldoutGroup("Calculated Derived Constraints"), LabelText(" Steer Assist Bounds"), OnValueChanged("ApplyProgressionStats")] 
    public Vector2 boundsArcadeSteerAssist = new Vector2(0.2f, 3.5f);

    [FoldoutGroup("Calculated Derived Constraints")]
    [ReadOnly, ShowInInspector] public int calculatedAcceleration;
    [FoldoutGroup("Calculated Derived Constraints")]
    [ReadOnly, ShowInInspector] public int calculatedBrakeForce;
    [FoldoutGroup("Calculated Derived Constraints")]
    [ReadOnly, ShowInInspector] public int calculatedDriftGrip;
    [FoldoutGroup("Calculated Derived Constraints")]
    [ReadOnly, ShowInInspector] public float currentSimulationMultiplier;

    [FoldoutGroup("Runtime Status", expanded: false)]
    [ReadOnly, ShowInInspector] public float currentSpeed;
    [FoldoutGroup("Runtime Status")]
    [ReadOnly, ShowInInspector] public Vector3[] upcomingWaypoints;
    [FoldoutGroup("Runtime Status")]
    [ReadOnly, ShowInInspector] public bool isBrakingForCurve;
    [FoldoutGroup("Runtime Status")]
    [ReadOnly, ShowInInspector] public bool requiresHandbrake;
    [FoldoutGroup("Runtime Status")]
    [ReadOnly, ShowInInspector] public bool isHairpinPowerDrift;
    [FoldoutGroup("Runtime Status")]
    [ReadOnly, ShowInInspector] public float currentDeviation;

    // External Area Modifiers
    private List<CarAreaModifier> activeAreaModifiers = new List<CarAreaModifier>();

    public void AddAreaModifier(CarAreaModifier modifier)
    {
        if (!activeAreaModifiers.Contains(modifier)) activeAreaModifiers.Add(modifier);
    }

    public void RemoveAreaModifier(CarAreaModifier modifier)
    {
        activeAreaModifiers.Remove(modifier);
    }

    // Internal states
    private bool isInitialized = false;
    private float _splineLength;
    private Rigidbody targetCarRb;
    private float currentTrackTargetDistance;
    private bool mustRecalculateWaypoints = true;
    private float activeTempSpeedMult = 1f;

    // Dummy Touch Inputs for bypassing Prometeo's keyboard logic
    private PrometeoTouchInput aiThrottle;
    private PrometeoTouchInput aiReverse;
    private PrometeoTouchInput aiLeft;
    private PrometeoTouchInput aiRight;
    private PrometeoTouchInput aiBrake;

    void Awake()
    {
        if (targetCar != null)
        {
            isInitialized = true;
        }
    }

    void Start()
    {
        upcomingWaypoints = new Vector3[waypointCount];

        if (track == null || track.Spline.Count == 0 || targetCar == null)
        {
            enabled = false;
            return;
        }

        targetCarRb = targetCar.GetComponent<Rigidbody>();

        _splineLength = track.Spline.GetLength();
        SetupPrometeoTouchOverride();
        ApplyProgressionStats();
    }

    void OnValidate()
    {
        ApplyProgressionStats();
    }

    public void ApplyProgressionStats()
    {
        if (targetCar == null) return;
        
        // Prevent OnValidate from wrecking physics properties during Editor-To-PlayMode transition delays
        if (Application.isPlaying && !isInitialized) return;

        // --- HEURISTICS TRANSLATION ---
        
        // 1. Tie top speed stat slightly to simulation speed heuristic so "Speed feels faster" holistically
        float effectiveSimulationStat = Mathf.Clamp01(statNitrousBoost + (statTopSpeed * 0.3f));
        currentSimulationMultiplier = Mathf.Lerp(1f, baseSimulationMultiplier, effectiveSimulationStat);

        // --- AI Driver Skill Translation ---

        // Reflexes & Courage (statDriverReflexes)
        curveAngleThreshold = Mathf.Lerp(boundsCurveAngle.x, boundsCurveAngle.y, statDriverReflexes);
        macroCurveAngleThreshold = Mathf.Lerp(boundsMacroCurveAngle.x, boundsMacroCurveAngle.y, statDriverReflexes);
        handbrakeAngleThreshold = Mathf.Lerp(boundsHandbrakeAngle.x, boundsHandbrakeAngle.y, statDriverReflexes);
        hairpinAngleThreshold = Mathf.Lerp(boundsHairpinAngle.x, boundsHairpinAngle.y, statDriverReflexes);
        
        baseWaypointDistance = Mathf.Lerp(boundsBaseWaypointDist.x, boundsBaseWaypointDist.y, statDriverReflexes);
        distanceSpeedMultiplier = Mathf.Lerp(boundsDistSpeedMult.x, boundsDistSpeedMult.y, statDriverReflexes);
        waypointArrivalRangeBase = Mathf.Lerp(boundsArrivalRangeBase.x, boundsArrivalRangeBase.y, statDriverReflexes);
        waypointArrivalSpeedMultiplier = Mathf.Lerp(boundsArrivalSpeedMult.x, boundsArrivalSpeedMult.y, statDriverReflexes);

        // Track Knowledge (statRacingLine)
        preCurveWideOffset = Mathf.Lerp(boundsPreCurveOffset.x, boundsPreCurveOffset.y, statRacingLine);
        postCurveWideOffset = Mathf.Lerp(boundsPostCurveOffset.x, boundsPostCurveOffset.y, statRacingLine);

        // Steering Grip (statSteeringGrip)
        steerDeadzone = Mathf.Lerp(boundsSteerDeadzone.x, boundsSteerDeadzone.y, statSteeringGrip);
        safeCornerSpeed = Mathf.Lerp(boundsSafeCornerSpeed.x, boundsSafeCornerSpeed.y, statSteeringGrip) * currentSimulationMultiplier;
        arcadeSteerAssist = Mathf.Lerp(boundsArcadeSteerAssist.x, boundsArcadeSteerAssist.y, statSteeringGrip) * currentSimulationMultiplier;

        // --- Car Mechanics Translation ---
        
        // Nitrous/Burst (statNitrousBoost)
        driftAccelerationMultiplier = Mathf.Lerp(boundsDriftAccMult.x, boundsDriftAccMult.y, statNitrousBoost);
        hairpinAccelerationBoost = Mathf.Lerp(boundsHairpinAccBoost.x, boundsHairpinAccBoost.y, statNitrousBoost);
        
        // Drift Tuning (statDriftControl)
        driftSteerAssistMultiplier = Mathf.Lerp(boundsDriftSteerMult.x, boundsDriftSteerMult.y, statDriftControl);
        hairpinSteerAssistBoost = Mathf.Lerp(boundsHairpinSteerBoost.x, boundsHairpinSteerBoost.y, statDriftControl);
        calculatedDriftGrip = Mathf.RoundToInt(Mathf.Lerp(1f, 10f, statDriftControl));
        if (Application.isPlaying) { targetCar.handbrakeDriftMultiplier = calculatedDriftGrip; }

        // Engine Top Speed
        float minSpeedLimit = baseMaxSpeed * minSpeedPercentage;
        maxSimulationSpeed = Mathf.Lerp(minSpeedLimit, baseMaxSpeed, statTopSpeed) * currentSimulationMultiplier;
        if (Application.isPlaying) { targetCar.maxSpeed = Mathf.RoundToInt(maxSimulationSpeed); }

        // Engine Acceleration
        calculatedAcceleration = Mathf.RoundToInt(Mathf.Lerp(3f, baseAcceleration, statAcceleration) * currentSimulationMultiplier);
        if (Application.isPlaying) { targetCar.accelerationMultiplier = calculatedAcceleration; }

        // Braking System
        calculatedBrakeForce = Mathf.RoundToInt(Mathf.Lerp(100f, baseBrakeForce, statBrakingSystem) * currentSimulationMultiplier);
        if (Application.isPlaying) { targetCar.brakeForce = calculatedBrakeForce; }
    }

    private void SetupPrometeoTouchOverride()
    {
        // Create dummy objects to host the touch scripts so we don't throw NREs
        aiThrottle = CreateDummyInput("AI_Throttle");
        aiReverse  = CreateDummyInput("AI_Reverse");
        aiLeft     = CreateDummyInput("AI_Left");
        aiRight    = CreateDummyInput("AI_Right");
        aiBrake    = CreateDummyInput("AI_Brake");

        // Force Prometeo into reading Touch inputs instead of keyboard via Reflection
        targetCar.useTouchControls = true;

        var type = typeof(PrometeoCarController);
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

        type.GetField("touchControlsSetup", flags)?.SetValue(targetCar, true);
        type.GetField("throttlePTI", flags)?.SetValue(targetCar, aiThrottle);
        type.GetField("reversePTI", flags)?.SetValue(targetCar, aiReverse);
        type.GetField("turnLeftPTI", flags)?.SetValue(targetCar, aiLeft);
        type.GetField("turnRightPTI", flags)?.SetValue(targetCar, aiRight);
        type.GetField("handbrakePTI", flags)?.SetValue(targetCar, aiBrake);
    }

    private PrometeoTouchInput CreateDummyInput(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        go.AddComponent<RectTransform>(); 
        return go.AddComponent<PrometeoTouchInput>();
    }

    void Update()
    {
        if (targetCar == null || track == null) return;
        
        activeTempSpeedMult = 1f;
        float tempAccMult = 1f;
        float tempGripMult = 1f;

        activeAreaModifiers.RemoveAll(m => m == null);
        for (int i = 0; i < activeAreaModifiers.Count; i++)
        {
            activeTempSpeedMult *= activeAreaModifiers[i].speedMultiplier;
            tempAccMult *= activeAreaModifiers[i].accelerationMultiplier;
            tempGripMult *= activeAreaModifiers[i].splineGripMultiplier;
        }

        float effectiveMaxSimulationSpeed = maxSimulationSpeed * activeTempSpeedMult;
        targetCar.maxSpeed = Mathf.RoundToInt(effectiveMaxSimulationSpeed);

        // --- Continuous AI Physic Overrides ---
        if (targetCar.isDrifting) {
            float boost = isHairpinPowerDrift ? hairpinAccelerationBoost : 1f;
            targetCar.accelerationMultiplier = Mathf.RoundToInt(calculatedAcceleration * driftAccelerationMultiplier * boost * tempAccMult);
        } else {
            targetCar.accelerationMultiplier = Mathf.RoundToInt(calculatedAcceleration * tempAccMult);
        }

        // Ghost follow for gizmos and components
        transform.position = targetCar.transform.position;
        transform.rotation = targetCar.transform.rotation;

        // Use true chassis velocity (smoothed) instead of wheel RPM to avoid burnout/drift flickering
        float trueSpeed = targetCarRb != null ? targetCarRb.linearVelocity.magnitude * 3.6f : Mathf.Abs(targetCar.carSpeed);
        currentSpeed = Mathf.Lerp(currentSpeed, trueSpeed, Time.deltaTime * 10f);

        // 1. Dynamic Arrival System for Hybrid Waypoints (Breadcrumb Consumption)
        float dynamicArrivalRange = waypointArrivalRangeBase + (currentSpeed * waypointArrivalSpeedMultiplier);
        
        // Guard against missing array data during boot
        if (upcomingWaypoints == null || upcomingWaypoints.Length == 0) mustRecalculateWaypoints = true;

        // Pre-calculate Spline Deviation for the current frame
        SplineUtility.GetNearestPoint(track.Spline, track.transform.InverseTransformPoint(targetCar.transform.position), out _, out float tPosition);
        Vector3 geometricIdeal = track.transform.TransformPoint(track.Spline.EvaluatePosition(tPosition));
        currentDeviation = Vector3.Distance(targetCar.transform.position, geometricIdeal);

        if (!mustRecalculateWaypoints)
        {
            // Check Panic Recalculation (Deviation limit)
            if (currentDeviation > maxDeviationDistance)
            {
                mustRecalculateWaypoints = true;
            }
            // Check Arrival & Trajectory Consumption (True Predictive Window)
            else 
            {
                Vector3 toWp = upcomingWaypoints[0] - targetCar.transform.position;
                float dot = Vector3.Dot(targetCar.transform.forward, toWp.normalized);
                
                // If car is inside the radius OR the waypoint is completely behind the car (dot < 0), slide the window!
                if (dot < -0.1f || Vector3.Distance(targetCar.transform.position, upcomingWaypoints[0]) <= dynamicArrivalRange)
                {
                    mustRecalculateWaypoints = true;
                }
            }
        }

        if (mustRecalculateWaypoints)
        {
            GenerateTrajectory();
            mustRecalculateWaypoints = false;
        }
        // 2. Predictive Curve Braking (Based strictly on the projected 3-point snake)
        float targetSafeSpeed;
        isBrakingForCurve = CheckForApproachingCurveVector(out targetSafeSpeed, out requiresHandbrake, out isHairpinPowerDrift);

        // Reset inputs each frame
        aiBrake.buttonPressed = false;
        aiThrottle.buttonPressed = false;
        aiLeft.buttonPressed = false;
        aiRight.buttonPressed = false;
        aiReverse.buttonPressed = false;

        // 3. Command Prometeo Controller
        if (isHairpinPowerDrift)
        {
            // Correction Drift: Tap handbrake to initiate slide
            aiBrake.buttonPressed = true;
            
            // Rapid Deceleration: if coming in way too hot, brake hard ("zerar o que precisar zerar")
            if (currentSpeed > safeCornerSpeed * activeTempSpeedMult * 1.5f) {
                aiReverse.buttonPressed = true;
            } 
            // Power Out: once speed is managed, smash throttle to exit ("gaining a lot of acceleration")
            else if (currentSpeed < effectiveMaxSimulationSpeed) {
                aiThrottle.buttonPressed = true;
            }
        }
        else if (isBrakingForCurve && currentSpeed > targetSafeSpeed)
        {
            if (requiresHandbrake) {
                aiBrake.buttonPressed = true; // Drifting Handbrake
            } else {
                aiReverse.buttonPressed = true; // Normal Brakes
            }
        }
        else
        {
            // Cap the max speed logic by limiting throttle if we're going too fast
            if (currentSpeed < effectiveMaxSimulationSpeed) {
                aiThrottle.buttonPressed = true;
            }
        }

        // 4. Steering Calculation
        // Look deeper into the curve when aligning wheels, especially aggressive when drifting!
        float steerDepth = targetCar.isDrifting ? 1f : 0.3f;
        Vector3 steeringTargetWorld = Vector3.Lerp(upcomingWaypoints[0], upcomingWaypoints[1], steerDepth);
        Vector3 localWaypoint = targetCar.transform.InverseTransformPoint(steeringTargetWorld);
        float angleToWaypoint = Mathf.Atan2(localWaypoint.x, localWaypoint.z) * Mathf.Rad2Deg;
        
        // Target steer normalized -1 to 1 based on Prometeo's limit
        float steerInput = Mathf.Clamp(angleToWaypoint / targetCar.maxSteeringAngle, -1f, 1f);
        
        if (steerInput < -steerDeadzone) {
            aiLeft.buttonPressed = true;
        } else if (steerInput > steerDeadzone) {
            aiRight.buttonPressed = true;
        }

        // 5. Arcade Physics Steer Assist
        // Forcibly rotate the chassis towards the target curve proportionately to speed to overcome physics understeer.
        if (arcadeSteerAssist > 0f && currentSpeed > 5f)
        {
            float assistStrength = targetCar.isDrifting ? arcadeSteerAssist * driftSteerAssistMultiplier : arcadeSteerAssist;
            if (isHairpinPowerDrift) assistStrength *= hairpinSteerAssistBoost;
            
            // Deviation booster: if the car is very off the correct spline, strengthen the assist to rescue the trajectory!
            float deviationMultiplier = 1f + (currentDeviation / 5f);
            assistStrength *= deviationMultiplier;

            float extraRotation = steerInput * assistStrength * currentSpeed * Time.deltaTime;
            targetCar.transform.Rotate(0, extraRotation, 0, Space.Self);
        }

        // 6. Spline Enforcement (Faithful Grip Tracker)
        if (targetCarRb != null && currentSpeed > 10f)
        {
            Vector3 vel = targetCarRb.linearVelocity;

            if (!targetCar.isDrifting)
            {
                // Normal Grip: Interpolates the actual Rigidbody velocity to perfectly match the car's forward direction.
                // To make it even MORE faithful to the track (fidedigno), we subtly blend the forward direction with the desired track waypoint.
                Vector3 toWaypoint = (upcomingWaypoints[0] - targetCar.transform.position).normalized;
                Vector3 faithfulForward = Vector3.Lerp(targetCar.transform.forward, toWaypoint, 0.15f).normalized;
                Vector3 idealVel = faithfulForward * vel.magnitude;
                
                float gripEnforcement = Mathf.Lerp(0f, 8f, statSteeringGrip) * tempGripMult * Time.deltaTime; // Rounded magnetic pull based on feedback
                targetCarRb.linearVelocity = Vector3.Lerp(vel, idealVel, gripEnforcement);
            }
            else
            {
                // Drift Grip: Interpolates the actual Rigidbody velocity directly towards the racing line Waypoint.
                // This restricts the drift slide from washing out too wide into the grass, acting as a spline magnet.
                Vector3 toWaypoint = (upcomingWaypoints[0] - targetCar.transform.position).normalized;
                Vector3 idealVel = toWaypoint * vel.magnitude;
                
                // Slightly weaker than standard grip to allow natural drifting, scaled by cornering assist!
                float driftEnforcement = Mathf.Lerp(0f, 3f, statSteeringGrip) * tempGripMult * Time.deltaTime;
                targetCarRb.linearVelocity = Vector3.Lerp(vel, idealVel, driftEnforcement);
            }
        }
    }

    bool CheckForApproachingCurveVector(out float targetSpeed, out bool needsHandbrake, out bool isHairpin)
    {
        targetSpeed = maxSimulationSpeed;
        bool needsBrake = false;
        needsHandbrake = false;
        isHairpin = false;

        // Measure pure geometric vectors ahead, ignoring Perlin Noise offsets to prevent phantom braking
        float tApprox;
        SplineUtility.GetNearestPoint(track.Spline, track.transform.InverseTransformPoint(targetCar.transform.position), out _, out tApprox);
        float baseDistance = tApprox * _splineLength;

        // FIXED GEOMETRIC TRAJECTORY PARSING (Dynamic Lookahead, Fixed Chord Length)
        // We evaluate purely mathematically with 5m fixed chords preventing phantom angle distortions.
        // We only cast as many chords as the car's current speed demands (1.25 seconds of reaction time).
        // This stops "braking too early" at low speeds, and prevents "braking too late" at high speeds.
        float lookaheadDistance = Mathf.Clamp((currentSpeed / 3.6f) * 1.25f, 15f, 60f); // meters
        float fixedSpacing = 5f; 
        int sampleCount = Mathf.CeilToInt(lookaheadDistance / fixedSpacing);
        if (sampleCount < 3) sampleCount = 3; 
        
        Vector3[] pureW = new Vector3[sampleCount];
        float accDist = 4f; // Start sampling slightly ahead to prevent reading behind the car's current pivot

        for (int i = 0; i < sampleCount; i++)
        {
            pureW[i] = GetPureSplinePoint(baseDistance + accDist);
            accDist += fixedSpacing;
        }

        // We analyze the highest bend between purely contiguous geometric chords 
        float maxAngle = 0f;
        for (int i = 0; i < sampleCount - 2; i++)
        {
            Vector3 dir = (pureW[i + 1] - pureW[i]).normalized;
            Vector3 nextDir = (pureW[i + 2] - pureW[i + 1]).normalized;
            
            float angle = Vector3.Angle(dir, nextDir);
            if (angle > maxAngle) maxAngle = angle;
        }

        // --- Macro Curve Check (Early Prediction) ---
        float macroAngle = 0f;
        Vector3 wFirst = pureW[0];
        Vector3 wMid = pureW[sampleCount / 2];
        Vector3 wLast = pureW[sampleCount - 1];

        Vector3 dir1 = (wMid - wFirst).normalized;
        Vector3 dir2 = (wLast - wMid).normalized;
        macroAngle = Vector3.Angle(dir1, dir2);

        if (maxAngle > curveAngleThreshold)
        {
            if (maxAngle >= hairpinAngleThreshold)
            {
                isHairpin = true;
                needsHandbrake = true;
            }
            else if (maxAngle >= handbrakeAngleThreshold)
            {
                needsHandbrake = true;
            }

            if (!isHairpin)
            {
                // Smoothly bridge the target speed based on the severity of the curve.
                float curveSeverity = Mathf.InverseLerp(curveAngleThreshold, handbrakeAngleThreshold, maxAngle);
                
                // Make Safe Corner Speed relative to Current Speed to preserve momentum naturally
                float speedScaling = Mathf.Lerp(0.5f, 0.85f, statSteeringGrip);
                float dynamicSafeSpeed = Mathf.Max(safeCornerSpeed * activeTempSpeedMult, currentSpeed * speedScaling);
                
                // Penalize dynamic speed if the car is NOT correct on the spline (forces it to brake to recover trajectory)
                float deviationPenalty = Mathf.Clamp01(currentDeviation / 10f); // Up to 30% penalty
                dynamicSafeSpeed *= (1f - (deviationPenalty * 0.3f));
                
                float effectiveMaxSpeed = maxSimulationSpeed * activeTempSpeedMult;
                float reqSpeed = Mathf.Lerp(effectiveMaxSpeed, dynamicSafeSpeed, curveSeverity);
                
                if (reqSpeed < targetSpeed)
                {
                    targetSpeed = reqSpeed;
                    needsBrake = true;
                }
            }
        }
        
        // Apply Macro Curve early braking (no handbrake)
        if (macroAngle > macroCurveAngleThreshold)
        {
            // Calculate a gentle slow-down requirement based on how severe the macro curve is.
            float macroSeverity = Mathf.InverseLerp(macroCurveAngleThreshold, macroCurveAngleThreshold * 2f, macroAngle);
            float effectiveMaxSpeed = maxSimulationSpeed * activeTempSpeedMult;
            float macroReqSpeed = Mathf.Lerp(effectiveMaxSpeed, safeCornerSpeed * activeTempSpeedMult, macroSeverity);

            if (macroReqSpeed < targetSpeed)
            {
                targetSpeed = macroReqSpeed;
                needsBrake = true;
            }
        }
        
        return needsBrake;
    }

    void GenerateTrajectory()
    {
        if (upcomingWaypoints == null || upcomingWaypoints.Length != waypointCount)
        {
            upcomingWaypoints = new Vector3[waypointCount];
        }

        // The exact nearest T on spline is used to figure out the Car's current raw progression
        Transform subjectTransform = targetCar != null ? targetCar.transform : transform;
        SplineUtility.GetNearestPoint(track.Spline, track.transform.InverseTransformPoint(subjectTransform.position), out _, out float tApprox);
        
        float baseDistance = (tApprox * _splineLength);
        
        // Push the entire snake frame forward linearly based on speed to compensate for physics response time
        // CAP the distance push strictly for the STEERING trajectory so the car doesn't try to steer towards points 50m away and cut into the grass
        float speedPush = Mathf.Clamp(currentSpeed * distanceSpeedMultiplier, 0f, 15f);
        
        // PURE PURSUIT PREDICTION SAFEGUARD: The inspector allows the user to make arrival ranges huge.
        // We MUST guarantee that Waypoint[0] is born just OUTSIDE the arrival range, or we get a 60FPS infinite looping calculation freeze.
        float rawArrivalRange = waypointArrivalRangeBase + (currentSpeed * waypointArrivalSpeedMultiplier);
        float safeStartingDistance = Mathf.Max(baseWaypointDistance + speedPush, rawArrivalRange + 1f);

        // Pre-calculate Anchor Perlin Noises (First, Middle, Last) to avoid individual waypoint zigzag on straights
        float accTemp = safeStartingDistance;
        float curTemp = baseWaypointDistance;
        
        float startDist = baseDistance + accTemp;
        for (int i = 0; i < waypointCount / 2; i++) { accTemp += curTemp; curTemp *= waypointSpacingMultiplier; }
        float midDist = baseDistance + accTemp;
        for (int i = waypointCount / 2; i < waypointCount - 1; i++) { accTemp += curTemp; curTemp *= waypointSpacingMultiplier; }
        float endDist = baseDistance + accTemp;

        float p0 = (Mathf.PerlinNoise(startDist * laneChangeVariationSpeed, 0f) * 2f) - 1f;
        float p1 = (Mathf.PerlinNoise(midDist * laneChangeVariationSpeed, 0f) * 2f) - 1f;
        float p2 = (Mathf.PerlinNoise(endDist * laneChangeVariationSpeed, 0f) * 2f) - 1f;
        
        int midIndex = waypointCount / 2;
        int endIndex = waypointCount - 1;

        float accDist = safeStartingDistance;
        float curSpacing = baseWaypointDistance;

        for (int i = 0; i < waypointCount; i++)
        {
            float distAlongSpline = baseDistance + accDist;
            
            // Exponential distance projection for the next iterations
            accDist += curSpacing;
            curSpacing *= waypointSpacingMultiplier;
            distAlongSpline %= _splineLength;
            
            float tWaypoint = distAlongSpline / _splineLength;
            
            Vector3 splLocalPos = SplineUtility.EvaluatePosition(track.Spline, tWaypoint);
            Vector3 splLocalTang = SplineUtility.EvaluateTangent(track.Spline, tWaypoint);
            Vector3 splLocalUp = SplineUtility.EvaluateUpVector(track.Spline, tWaypoint);
            
            Vector3 splinePos = track.transform.TransformPoint(splLocalPos);
            Vector3 splineTangent = track.transform.TransformDirection(splLocalTang);
            Vector3 splineUp = track.transform.TransformDirection(splLocalUp);
            Vector3 right = Vector3.Cross(splineUp, splineTangent).normalized;
            
            // Evaluate smooth perlin offset linearly interpolated by anchoring points
            float perlinOffset = 0f;
            if (i <= midIndex)
            {
                float t = midIndex > 0 ? (float)i / midIndex : 0f;
                perlinOffset = Mathf.Lerp(p0, p1, t);
            }
            else
            {
                float t = (endIndex - midIndex) > 0 ? (float)(i - midIndex) / (endIndex - midIndex) : 0f;
                perlinOffset = Mathf.Lerp(p1, p2, t);
            }
            
            float absoluteOffset = perlinOffset * waypointMaxLateralOffset;
            
            // --- Pre-Curve Wide Offset (OUTSIDE APPROACH) ---
            if (preCurveWideOffset > 0f)
            {
                float lookahead = baseWaypointDistance * 2f + (currentSpeed * distanceSpeedMultiplier * 1.5f);
                float futureT = (distAlongSpline + lookahead) / _splineLength;
                futureT %= 1f;
                Vector3 futureTang = track.transform.TransformDirection(SplineUtility.EvaluateTangent(track.Spline, futureT)).normalized;
                
                // If track bends right, futureTang projects positively onto 'right'. We negate it to move left (outside).
                float curveBendInfluence = -Vector3.Dot(right, futureTang);
                absoluteOffset += (curveBendInfluence * preCurveWideOffset);
            }

            // --- Post-Curve Close Offset (INSIDE APEX) ---
            if (postCurveWideOffset > 0f)
            {
                float shortLookahead = 5f; 
                float nearFutureT = (distAlongSpline + shortLookahead) / _splineLength;
                nearFutureT %= 1f;
                Vector3 nearFutureTang = track.transform.TransformDirection(SplineUtility.EvaluateTangent(track.Spline, nearFutureT)).normalized;
                
                // If track bends right, nearFutureTang projects positively onto 'right'. We keep it positive to move right (inside).
                float immediateBendInfluence = Vector3.Dot(right, nearFutureTang);
                absoluteOffset += (immediateBendInfluence * postCurveWideOffset);
            }
            
            upcomingWaypoints[i] = splinePos + (right * absoluteOffset);
        }
    }

    Vector3 GetPureSplinePoint(float distance)
    {
        distance %= _splineLength;
        float t = distance / _splineLength;
        Vector3 locPos = SplineUtility.EvaluatePosition(track.Spline, t);
        return track.transform.TransformPoint(locPos);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || upcomingWaypoints == null || upcomingWaypoints.Length < waypointCount) return;

        // Draw track projection snake
        Gizmos.color = Color.magenta;
        
        for (int i = 0; i < upcomingWaypoints.Length; i++) {
            Gizmos.DrawWireSphere(upcomingWaypoints[i], 1f);
            
            if (i == 0) {
                if (targetCar) Gizmos.DrawLine(targetCar.transform.position, upcomingWaypoints[i]);
            } else {
                Gizmos.DrawLine(upcomingWaypoints[i - 1], upcomingWaypoints[i]);
            }
        }

        // Draw braking ray state
        Gizmos.color = requiresHandbrake ? Color.red : (isBrakingForCurve ? new Color(1f, 0.5f, 0f) : Color.green);
        if (targetCar) {
            Vector3 dir = targetCar.transform.forward * currentSpeed;
            Gizmos.DrawRay(targetCar.transform.position, dir * 0.2f);
        }
    }
}
