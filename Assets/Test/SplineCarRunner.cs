using GearEngine.CarSimulation.Definitions;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

// Prototype: enable the Gizmos toggle in the Game view toolbar to see velocity and curve gizmos while playing.
public class SplineCarRunner : MonoBehaviour
{
    [Header("Setup")]
    public SplineContainer track;

    [Tooltip("When set, spline knots are copied from this asset onto the SplineContainer at startup (same as TrackDefinition-driven tracks).")]
    public TrackDefinition trackDefinition;

    [Header("Settings")]
    public float speed = 10f;

    [Header("Curve Detection")]
    [Range(1f, 30f)]
    public float curveAngleThreshold = 8f;

    [Header("Debug (read-only)")]
    public Vector3 velocityVector;
    public float curveAngle;
    public bool isOnCurve;

    float _t;
    float _splineLength;

    void Awake()
    {
        ApplyTrackDefinitionIfPresent();
    }

    void Start()
    {
        if (track == null)
        {
            Debug.LogError("[SplineCarRunner] SplineContainer track is not assigned.");
            enabled = false;
            return;
        }

        if (track.Spline.Count == 0)
        {
            Debug.LogError("[SplineCarRunner] Spline has no knots; assign a TrackDefinition or author the spline on the container.");
            enabled = false;
            return;
        }

        _splineLength = track.Spline.GetLength();
        _t = 0f;
    }

    void Update()
    {
        _t = (_t + speed * Time.deltaTime / _splineLength) % 1f;

        var localPos = SplineUtility.EvaluatePosition(track.Spline, _t);
        var tangent = SplineUtility.EvaluateTangent(track.Spline, _t);
        var up = SplineUtility.EvaluateUpVector(track.Spline, _t);

        transform.position = track.transform.TransformPoint(localPos);
        transform.rotation = Quaternion.LookRotation(math.normalize(tangent), up);

        velocityVector = transform.forward * speed;

        float tPrev = Mathf.Repeat(_t - 0.01f, 1f);
        float tNext = Mathf.Repeat(_t + 0.01f, 1f);
        var dirPrev = (Vector3)math.normalize(SplineUtility.EvaluateTangent(track.Spline, tPrev));
        var dirNext = (Vector3)math.normalize(SplineUtility.EvaluateTangent(track.Spline, tNext));
        curveAngle = Vector3.Angle(dirPrev, dirNext);
        isOnCurve = curveAngle > curveAngleThreshold;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.green;
        Vector3 dir = velocityVector.normalized;
        Gizmos.DrawRay(transform.position, dir * 4f);
        Gizmos.DrawWireSphere(transform.position + dir * 4f, 0.4f);

        Gizmos.color = isOnCurve ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.7f);
    }

    void ApplyTrackDefinitionIfPresent()
    {
        if (trackDefinition == null)
        {
            return;
        }

        if (track == null)
        {
            Debug.LogError("[SplineCarRunner] TrackDefinition is set but SplineContainer track is missing.");
            return;
        }

        if (trackDefinition.Spline.Count == 0)
        {
            Debug.LogError($"[SplineCarRunner] TrackDefinition '{trackDefinition.name}' has no spline knots.");
            return;
        }

        Spline target = track.Spline;
        target.Knots = trackDefinition.Spline.Knots;
        target.Closed = trackDefinition.Spline.Closed;

        var extrude = track.GetComponentInChildren<SplineExtrude>(true);
        if (extrude != null)
        {
            extrude.Rebuild();
        }
    }
}
