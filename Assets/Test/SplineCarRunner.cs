using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Reflection;
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
    [Range(0f, 1f), PropertyTooltip("Scales Car Engine Max Speed & AI top prediction boundaries (e.g., Lada vs F1).")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statTopSpeed = 0.5f;

    [FoldoutGroup("Roguelike Base Stats")]
    [Range(0f, 1f), PropertyTooltip("Scales Car acceleration force multiplier.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statAcceleration = 0.5f;

    [FoldoutGroup("Roguelike Base Stats")]
    [Range(0f, 1f), PropertyTooltip("Scales physical brakes & AI curve confidence speeds.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statBrakingPower = 0.5f;

    [FoldoutGroup("Roguelike Base Stats")]
    [Range(0f, 1f), PropertyTooltip("Scales grip loss physics when drifting (1.0 = Drifts effortlessly, 0.0 = Sticks extremely well). INVERSELY PROPORTIONAL.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statDriftGrip = 0.5f;

    [FoldoutGroup("Roguelike Base Stats")]
    [Range(0f, 1f), PropertyTooltip("Scales the Arcade Steer Assist steering strength to overcome understeer.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statCorneringAssist = 0.5f;

    [FoldoutGroup("Roguelike Base Stats")]
    [Range(0f, 1f), PropertyTooltip("Scales the Simulation Speed (Nitro), dictating how much faster the car acts holistically.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statSimulationSpeed = 0f;

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
    [FoldoutGroup("Advanced Heuristics (Automated)"), LabelText(" Bounds"), OnValueChanged("ApplyProgressionStats")] public Vector2 boundsBaseWaypointDist = new Vector2(6f, 12f);

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

    // Internal states
    private bool isInitialized = false;
    private float _splineLength;
    private Rigidbody targetCarRb;
    private float currentTrackTargetDistance;
    private bool mustRecalculateWaypoints = true;

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
        float effectiveSimulationStat = Mathf.Clamp01(statSimulationSpeed + (statTopSpeed * 0.3f));
        currentSimulationMultiplier = Mathf.Lerp(1f, baseSimulationMultiplier, effectiveSimulationStat);

        // 2. Driving Aggressiveness & Braking Confidence (statBrakingPower)
        // Values calibrated for fixed 5m spatial chords parsing
        curveAngleThreshold = Mathf.Lerp(boundsCurveAngle.x, boundsCurveAngle.y, statBrakingPower);
        macroCurveAngleThreshold = Mathf.Lerp(boundsMacroCurveAngle.x, boundsMacroCurveAngle.y, statBrakingPower);
        handbrakeAngleThreshold = Mathf.Lerp(boundsHandbrakeAngle.x, boundsHandbrakeAngle.y, statBrakingPower);
        hairpinAngleThreshold = Mathf.Lerp(boundsHairpinAngle.x, boundsHairpinAngle.y, statBrakingPower);

        // 3. Cornering & Racing Line Mastery (statCorneringAssist)
        preCurveWideOffset = Mathf.Lerp(boundsPreCurveOffset.x, boundsPreCurveOffset.y, statCorneringAssist);
        postCurveWideOffset = Mathf.Lerp(boundsPostCurveOffset.x, boundsPostCurveOffset.y, statCorneringAssist);
        steerDeadzone = Mathf.Lerp(boundsSteerDeadzone.x, boundsSteerDeadzone.y, statCorneringAssist);
        
        // 4. Drift & Grip Mastery (statDriftGrip)
        driftAccelerationMultiplier = Mathf.Lerp(boundsDriftAccMult.x, boundsDriftAccMult.y, statDriftGrip);
        hairpinAccelerationBoost = Mathf.Lerp(boundsHairpinAccBoost.x, boundsHairpinAccBoost.y, statDriftGrip);
        driftSteerAssistMultiplier = Mathf.Lerp(boundsDriftSteerMult.x, boundsDriftSteerMult.y, statDriftGrip);
        hairpinSteerAssistBoost = Mathf.Lerp(boundsHairpinSteerBoost.x, boundsHairpinSteerBoost.y, statDriftGrip);
        
        // 5. Navigation / Spatial Prediction Profile (statTopSpeed)
        baseWaypointDistance = Mathf.Lerp(boundsBaseWaypointDist.x, boundsBaseWaypointDist.y, statTopSpeed);
        distanceSpeedMultiplier = Mathf.Lerp(boundsDistSpeedMult.x, boundsDistSpeedMult.y, statTopSpeed);
        waypointArrivalRangeBase = Mathf.Lerp(boundsArrivalRangeBase.x, boundsArrivalRangeBase.y, statTopSpeed);
        waypointArrivalSpeedMultiplier = Mathf.Lerp(boundsArrivalSpeedMult.x, boundsArrivalSpeedMult.y, statTopSpeed);

        // 1. Top Speed Sync (Engine Physics + AI Simulator Limit)
        // Set a dynamic minimum floor (based on minSpeedPercentage) so the car never breaks at 0% stats
        float minSpeedLimit = baseMaxSpeed * minSpeedPercentage;
        maxSimulationSpeed = Mathf.Lerp(minSpeedLimit, baseMaxSpeed, statTopSpeed) * currentSimulationMultiplier;
        if (Application.isPlaying) 
        {
            targetCar.maxSpeed = Mathf.RoundToInt(maxSimulationSpeed);
        }

        // 2. Acceleration Sync
        // Shifted the starting minimum from 1f to 3f so low-stat runs are generally faster and punchier.
        calculatedAcceleration = Mathf.RoundToInt(Mathf.Lerp(3f, baseAcceleration, statAcceleration) * currentSimulationMultiplier);
        if (Application.isPlaying) 
        {
            // Set base acceleration (dynamic drift damping is handled per-frame in Update)
            targetCar.accelerationMultiplier = calculatedAcceleration;
        }

        // 3. Braking Power Sync (Brake Physical Force)
        calculatedBrakeForce = Mathf.RoundToInt(Mathf.Lerp(100f, baseBrakeForce, statBrakingPower) * currentSimulationMultiplier);
        if (Application.isPlaying) 
        {
            targetCar.brakeForce = calculatedBrakeForce;
        }

        // 4. Cornering Assist & Safe AI Corner speeds (Confidence tied to chassis handling, not brakes!)
        safeCornerSpeed = Mathf.Lerp(boundsSafeCornerSpeed.x, boundsSafeCornerSpeed.y, statCorneringAssist) * currentSimulationMultiplier;
        arcadeSteerAssist = Mathf.Lerp(boundsArcadeSteerAssist.x, boundsArcadeSteerAssist.y, statCorneringAssist) * currentSimulationMultiplier;

        // 5. Drift Grip Sync (Prometeo: 10f is max slippery, 1f is super sticky)
        // INVERSELY PROPORTIONAL: High Roguelike stat = HIGHER slip capability (drifts better!)
        calculatedDriftGrip = Mathf.RoundToInt(Mathf.Lerp(1f, 10f, statDriftGrip));
        if (Application.isPlaying) 
        {
            targetCar.handbrakeDriftMultiplier = calculatedDriftGrip;
        }
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
        
        // --- Continuous AI Physic Overrides ---
        if (targetCar.isDrifting) {
            float boost = isHairpinPowerDrift ? hairpinAccelerationBoost : 1f;
            targetCar.accelerationMultiplier = Mathf.RoundToInt(calculatedAcceleration * driftAccelerationMultiplier * boost);
        } else {
            targetCar.accelerationMultiplier = calculatedAcceleration;
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
            if (currentSpeed > safeCornerSpeed * 1.5f) {
                aiReverse.buttonPressed = true;
            } 
            // Power Out: once speed is managed, smash throttle to exit ("gaining a lot of acceleration")
            else if (currentSpeed < maxSimulationSpeed) {
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
            if (currentSpeed < maxSimulationSpeed) {
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
                // This makes the vehicle follow the AI's steering line "fidedigno" without physics slipping off.
                Vector3 idealVel = targetCar.transform.forward * vel.magnitude;
                float gripEnforcement = Mathf.Lerp(0f, 6f, statCorneringAssist) * Time.deltaTime;
                targetCarRb.linearVelocity = Vector3.Lerp(vel, idealVel, gripEnforcement);
            }
            else
            {
                // Drift Grip: Interpolates the actual Rigidbody velocity directly towards the racing line Waypoint.
                // This restricts the drift slide from washing out too wide into the grass, acting as a spline magnet.
                Vector3 toWaypoint = (upcomingWaypoints[0] - targetCar.transform.position).normalized;
                Vector3 idealVel = toWaypoint * vel.magnitude;
                
                // Slightly weaker than standard grip to allow natural drifting, scaled by cornering assist!
                float driftEnforcement = Mathf.Lerp(0f, 3f, statCorneringAssist) * Time.deltaTime;
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
                float speedScaling = Mathf.Lerp(0.5f, 0.85f, statCorneringAssist);
                float dynamicSafeSpeed = Mathf.Max(safeCornerSpeed, currentSpeed * speedScaling);
                
                // Penalize dynamic speed if the car is NOT correct on the spline (forces it to brake to recover trajectory)
                float deviationPenalty = Mathf.Clamp01(currentDeviation / 10f); // Up to 30% penalty
                dynamicSafeSpeed *= (1f - (deviationPenalty * 0.3f));
                
                float reqSpeed = Mathf.Lerp(maxSimulationSpeed, dynamicSafeSpeed, curveSeverity);
                
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
            float macroReqSpeed = Mathf.Lerp(maxSimulationSpeed, safeCornerSpeed, macroSeverity);

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
            
            // Generate a natural, contiguous lane shift via Perlin Noise
            float noiseX = distAlongSpline * laneChangeVariationSpeed;
            float perlinOffset = (Mathf.PerlinNoise(noiseX, 0f) * 2f) - 1f; // Rescale 0-1 to -1 to +1
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
