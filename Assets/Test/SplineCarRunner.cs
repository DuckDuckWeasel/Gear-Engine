using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Reflection;
using Sirenix.OdinInspector;

public class SplineCarRunner : MonoBehaviour
{
    [TitleGroup("Scene References")]
    [Required, Tooltip("The spline acting as the pathing track for the AI.")]
    public SplineContainer track;

    [TitleGroup("Scene References")]
    [Required, Tooltip("The Prometeo Car Controller that will receive AI inputs from this script.")]
    public PrometeoCarController targetCar;

    [FoldoutGroup("AI Navigation Tweaks")]
    [Range(2, 20), PropertyTooltip("How many predictive waypoints the AI will cast. More points trace further into the future.")]
    public int waypointCount = 7;

    [FoldoutGroup("AI Navigation Tweaks")]
    [Range(2f, 30f), PropertyTooltip("Base distance (in meters) the AI casts its initial first point along the Spline at zero speed.")]
    public float baseWaypointDistance = 8f;

    [FoldoutGroup("AI Navigation Tweaks")]
    [Range(0f, 1f), PropertyTooltip("How much extra distance is added per unit of speed. Expands the AI vision dynamically when going fast.")]
    public float distanceSpeedMultiplier = 0.3f;

    [FoldoutGroup("AI Navigation Tweaks")]
    [Range(.1f, 3f), PropertyTooltip("How much the distance multiplies for each successive waypoint (exponential lookahead reach).")]
    public float waypointSpacingMultiplier = 1.35f;

    [FoldoutGroup("AI Navigation Tweaks")]
    [Range(1f, 20f), PropertyTooltip("Base Distance to trigger a recalculation when approaching the current steering waypoint.")]
    public float waypointArrivalRangeBase = 6f;

    [FoldoutGroup("AI Navigation Tweaks")]
    [Range(0f, 1f), PropertyTooltip("Increments the arrival range dynamically based on speed (e.g. at high speeds, trigger recalculation earlier to avoid overshoot).")]
    public float waypointArrivalSpeedMultiplier = 0.2f;

    [FoldoutGroup("AI Navigation Tweaks")]
    [Range(0f, 10f), PropertyTooltip("Maximum lateral offset (left/right) the AI can drift. Offset is computed smoothly using Perlin noise.")]
    public float waypointMaxLateralOffset = 0.7f;  

    [FoldoutGroup("AI Navigation Tweaks")]
    [Range(0.01f, 2f), PropertyTooltip("How quickly the AI randomly smoothly changes lanes over distance (Perlin frequency).")]
    public float laneChangeVariationSpeed = 0.25f;

    [FoldoutGroup("AI Navigation Tweaks")]
    [Range(0f, 1f), PropertyTooltip("Deadzone for steering input (0 to 1) to avoid micro-jittering on straights.")]
    public float steerDeadzone = 0.05f;

    [TitleGroup("Roguelike Base Stats (0 to 1)")]
    [Range(0f, 1f), PropertyTooltip("Scales Car Engine Max Speed & AI top prediction boundaries (e.g., Lada vs F1).")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statTopSpeed = 0.5f;

    [TitleGroup("Roguelike Base Stats (0 to 1)")]
    [Range(0f, 1f), PropertyTooltip("Scales Car acceleration force multiplier.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statAcceleration = 0.5f;

    [TitleGroup("Roguelike Base Stats (0 to 1)")]
    [Range(0f, 1f), PropertyTooltip("Scales physical brakes & AI curve confidence speeds.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statBrakingPower = 0.5f;

    [TitleGroup("Roguelike Base Stats (0 to 1)")]
    [Range(0f, 1f), PropertyTooltip("Scales grip loss physics when drifting (1.0 = Drifts effortlessly, 0.0 = Gruda no asfalto pesado). INVERSELY PROPORTIONAL.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statDriftGrip = 0.5f;

    [TitleGroup("Roguelike Base Stats (0 to 1)")]
    [Range(0f, 1f), PropertyTooltip("Scales the Arcade Steer Assist steering strength to overcome understeer.")]
    [OnValueChanged("ApplyProgressionStats")]
    public float statCorneringAssist = 0.5f;

    [TitleGroup("AI Drifting Tweaks")]
    [PropertyTooltip("Multiplies acceleration when the car is drifting to prevent understeer pushing into walls.")]
    [Range(0.1f, 1f)]
    public float driftAccelerationMultiplier = 0.85f;
    
    [TitleGroup("AI Drifting Tweaks")]
    [PropertyTooltip("Multiplies the chassis rotational assist strength specifically when drifting to help align to the waypoint.")]
    [Range(1f, 3f)]
    public float driftSteerAssistMultiplier = 1.25f;

    [TitleGroup("Calculated Derived Constraints (ReadOnly)")]
    [ReadOnly, ShowInInspector] public float maxSimulationSpeed;
    [TitleGroup("Calculated Derived Constraints (ReadOnly)")]
    [ReadOnly, ShowInInspector] public float safeCornerSpeed; 
    [TitleGroup("Calculated Derived Constraints (ReadOnly)")]
    [ReadOnly, ShowInInspector] public float arcadeSteerAssist;

    [TitleGroup("Calculated Derived Engine Physics (ReadOnly)")]
    [ReadOnly, ShowInInspector] public int calculatedAcceleration;
    [TitleGroup("Calculated Derived Engine Physics (ReadOnly)")]
    [ReadOnly, ShowInInspector] public int calculatedBrakeForce;
    [TitleGroup("Calculated Derived Engine Physics (ReadOnly)")]
    [ReadOnly, ShowInInspector] public int calculatedDriftGrip;

    // --- Core Boundaries for AI AI-only mapping ---
    [HideInInspector] public Vector2 boundsAI_SafeCornerSpeed = new Vector2(8f, 35f);
    [HideInInspector] public Vector2 boundsArcadeSteerAssist = new Vector2(0.2f, 3.5f);

    // --- Engine Base Limits (Source of Truth) ---
    [TitleGroup("Engine Base Limits (Max Capabilities)")]
    [Range(20, 190), PropertyTooltip("Maximum capacity of Top Speed.")]
    [OnValueChanged("ApplyProgressionStats")]
    public int baseMaxSpeed = 190;

    [TitleGroup("Engine Base Limits (Max Capabilities)")]
    [Range(1, 10), PropertyTooltip("Maximum capacity of Acceleration Multiplier.")]
    [OnValueChanged("ApplyProgressionStats")]
    public int baseAcceleration = 10;

    [TitleGroup("Engine Base Limits (Max Capabilities)")]
    [Range(100, 600), PropertyTooltip("Maximum capacity of Brake Force.")]
    [OnValueChanged("ApplyProgressionStats")]
    public int baseBrakeForce = 350;

    [TitleGroup("Engine Base Limits (Max Capabilities)")]
    [Range(1, 10), PropertyTooltip("Maximum slippery capability (highest value).")]
    [OnValueChanged("ApplyProgressionStats")]
    public int baseDriftGrip = 5;

    [FoldoutGroup("Predictive Braking & Speed Restrictions")]
    [Range(1f, 30f), PropertyTooltip("The bend angle threshold that the AI considers 'sharp' enough to require normal braking.")]
    public float curveAngleThreshold = 4f;

    [FoldoutGroup("Predictive Braking & Speed Restrictions")]
    [Range(5f, 60f), PropertyTooltip("If the curve bend is higher than this, the AI will pull the Handbrake to drift instead of standard braking.")]
    public float handbrakeAngleThreshold = 14f;

    [TitleGroup("Runtime Status (Read-Only)")]
    [ReadOnly, ShowInInspector]
    public float currentSpeed;

    [TitleGroup("Runtime Status (Read-Only)")]
    [ReadOnly, ShowInInspector]
    public Vector3[] upcomingWaypoints;

    [TitleGroup("Runtime Status (Read-Only)")]
    [ReadOnly, ShowInInspector]
    public bool isBrakingForCurve;
    
    [TitleGroup("Runtime Status (Read-Only)")]
    [ReadOnly, ShowInInspector]
    public bool requiresHandbrake;

    [TitleGroup("Breadcrumb Routing Limits")]
    [Range(2f, 20f), PropertyTooltip("Maximum distance the car can drift off the pure track line before the AI panics and recalculates the whole route.")]
    public float maxDeviationDistance = 7f;

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

        // 1. Top Speed Sync (Engine Physics + AI Simulator Limit)
        // Set a minimum floor of 20km/h so the car never breaks at 0% stats
        maxSimulationSpeed = Mathf.Lerp(20f, baseMaxSpeed, statTopSpeed);
        if (Application.isPlaying) 
        {
            targetCar.maxSpeed = Mathf.RoundToInt(maxSimulationSpeed);
        }

        // 2. Acceleration Sync
        calculatedAcceleration = Mathf.RoundToInt(Mathf.Lerp(1f, baseAcceleration, statAcceleration));
        if (Application.isPlaying) 
        {
            // Set base acceleration (dynamic drift damping is handled per-frame in Update)
            targetCar.accelerationMultiplier = calculatedAcceleration;
        }

        // 3. Braking Power Sync (Brake Physical Force)
        calculatedBrakeForce = Mathf.RoundToInt(Mathf.Lerp(100f, baseBrakeForce, statBrakingPower));
        if (Application.isPlaying) 
        {
            targetCar.brakeForce = calculatedBrakeForce;
        }

        // 4. Cornering Assist & Safe AI Corner speeds (Confidence tied to chassis handling, not brakes!)
        safeCornerSpeed = Mathf.Lerp(boundsAI_SafeCornerSpeed.x, boundsAI_SafeCornerSpeed.y, statCorneringAssist);
        arcadeSteerAssist = Mathf.Lerp(boundsArcadeSteerAssist.x, boundsArcadeSteerAssist.y, statCorneringAssist);

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
            targetCar.accelerationMultiplier = Mathf.RoundToInt(calculatedAcceleration * driftAccelerationMultiplier);
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

        if (!mustRecalculateWaypoints)
        {
            // Check Panic Recalculation (Deviation)
            float tApprox;
            SplineUtility.GetNearestPoint(track.Spline, track.transform.InverseTransformPoint(targetCar.transform.position), out _, out tApprox);
            Vector3 geometricIdeal = track.transform.TransformPoint(track.Spline.EvaluatePosition(tApprox));
            if (Vector3.Distance(targetCar.transform.position, geometricIdeal) > maxDeviationDistance)
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
        isBrakingForCurve = CheckForApproachingCurveVector(out targetSafeSpeed, out requiresHandbrake);

        // Reset inputs each frame
        aiBrake.buttonPressed = false;
        aiThrottle.buttonPressed = false;
        aiLeft.buttonPressed = false;
        aiRight.buttonPressed = false;
        aiReverse.buttonPressed = false;

        // 3. Command Prometeo Controller
        if (isBrakingForCurve && currentSpeed > targetSafeSpeed)
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
            float extraRotation = steerInput * assistStrength * currentSpeed * Time.deltaTime;
            targetCar.transform.Rotate(0, extraRotation, 0, Space.Self);
        }
    }

    bool CheckForApproachingCurveVector(out float targetSpeed, out bool needsHandbrake)
    {
        targetSpeed = maxSimulationSpeed;
        bool needsBrake = false;
        needsHandbrake = false;

        // Measure pure geometric vectors ahead, ignoring Perlin Noise offsets to prevent phantom braking
        float tApprox;
        SplineUtility.GetNearestPoint(track.Spline, track.transform.InverseTransformPoint(targetCar.transform.position), out _, out tApprox);
        float baseDistance = tApprox * _splineLength;

        Vector3[] pureW = new Vector3[waypointCount];
        float accDist = (currentSpeed * distanceSpeedMultiplier);
        float curSpacing = baseWaypointDistance;

        for (int i = 0; i < waypointCount; i++)
        {
            accDist += curSpacing;
            pureW[i] = GetPureSplinePoint(baseDistance + accDist);
            curSpacing *= waypointSpacingMultiplier;
        }

        // We analyze the highest bend in the next N segments
        float maxAngle = 0f;
        for (int i = 0; i < waypointCount - 1; i++)
        {
            Vector3 fromPos = i == 0 ? targetCar.transform.position : pureW[i - 1];
            Vector3 dir = (pureW[i] - fromPos).normalized;
            Vector3 nextDir = (pureW[i + 1] - pureW[i]).normalized;
            
            float angle = Vector3.Angle(dir, nextDir);
            if (angle > maxAngle) maxAngle = angle;
        }

        if (maxAngle > curveAngleThreshold)
        {
            if (maxAngle >= handbrakeAngleThreshold)
            {
                needsHandbrake = true;
            }

            // Smoothly bridge the target speed between max straight speed and safe curve speed
            // based on the severity of the curve.
            // A gentle curve will require less braking, a sharp curve drops the requirement exactly down to safeCornerSpeed.
            float curveSeverity = Mathf.InverseLerp(curveAngleThreshold, handbrakeAngleThreshold, maxAngle);
            float reqSpeed = Mathf.Lerp(maxSimulationSpeed, safeCornerSpeed, curveSeverity);
            
            if (reqSpeed < targetSpeed)
            {
                targetSpeed = reqSpeed;
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
