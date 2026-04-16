using UnityEngine;

public class DriftMarker : MonoBehaviour
{
    public SplineCarRunner runner;

    private bool isCurving;

    public Transform driftMarker;
    public Transform maxDriftMarker;
    public Transform currentDrift;

    [Tooltip("Exponential recovery smoothness (higher = snappier). Typical range ~4–12. Frame-rate stable.")]
    public float correctionSpeed = 8f;

    [Tooltip("Speed when the curve started; stored for reference / future tuning.")]
    public float entrySpeed;

    [Header("Current drift trail")]
    [Tooltip("If true, adds or configures a TrailRenderer on currentDrift to show its path during a curve.")]
    public bool enableCurrentDriftTrail = true;

    [Tooltip("How long trail segments stay visible (seconds).")]
    public float trailTime = 2f;

    [Tooltip("World-space width multiplier for the trail.")]
    public float trailWidth = 0.12f;

    [Tooltip("Smaller values = denser vertices on tight paths.")]
    public float trailMinVertexDistance = 0.02f;

    [Tooltip("Optional material (e.g. Unlit/Color). If null, Unity uses a default line material.")]
    public Material trailMaterial;

    [Tooltip("Trail color at the head (newest point). Alpha fades along the trail.")]
    public Color trailColor = new Color(0.2f, 0.85f, 1f, 1f);

    const float SnapSqrEpsilon = 1e-4f;

    Transform _trailOwner;
    TrailRenderer _currentDriftTrail;

    private void Update()
    {
        if (runner == null)
        {
            Debug.LogError("[DriftMarker] SplineCarRunner reference is not assigned.");
            enabled = false;
            return;
        }

        if (runner.isOnCurve != isCurving)
        {
            isCurving = runner.isOnCurve;
            if (isCurving)
            {
                SetDrift();
            }
            else
            {
                StopDrift();
            }
        }

        if (isCurving && runner.isOnCurve)
        {
            UpdateDrift();
        }
    }

    void LateUpdate()
    {
        if (runner == null || !enableCurrentDriftTrail || currentDrift == null)
        {
            return;
        }

        TrailRenderer trail = currentDrift.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            return;
        }

        trail.emitting = runner.isOnCurve;
    }

    private void SetDrift()
    {
        if (driftMarker == null || maxDriftMarker == null || currentDrift == null)
        {
            Debug.LogError("[DriftMarker] driftMarker, maxDriftMarker, and currentDrift must be assigned.");
            return;
        }

        Vector3 maxOffsetWorld = runner.transform.position + runner.velocityVector;
        driftMarker.position = maxOffsetWorld;
        maxDriftMarker.position = maxOffsetWorld;
        currentDrift.localPosition = Vector3.zero;
        entrySpeed = runner.speed;
        RestartCurrentDriftTrail();
    }

    private void UpdateDrift()
    {
        if (driftMarker == null || currentDrift == null)
        {
            return;
        }

        float t = 1f - Mathf.Exp(-correctionSpeed * Time.deltaTime);
        Vector3 carPos = runner.transform.position;

        driftMarker.position = Vector3.Lerp(driftMarker.position, carPos, t);
        if ((driftMarker.position - carPos).sqrMagnitude <= SnapSqrEpsilon)
        {
            driftMarker.position = carPos;
        }

        currentDrift.position = Vector3.Lerp(currentDrift.position, driftMarker.position, t);
        if ((currentDrift.position - driftMarker.position).sqrMagnitude <= SnapSqrEpsilon)
        {
            currentDrift.position = driftMarker.position;
        }
    }

    private void StopDrift()
    {
        if (driftMarker != null)
        {
            driftMarker.localPosition = Vector3.zero;
        }

        if (maxDriftMarker != null)
        {
            maxDriftMarker.localPosition = Vector3.zero;
        }

        if (currentDrift != null)
        {
            currentDrift.localPosition = Vector3.zero;
        }

        StopCurrentDriftTrail();
    }

    void RestartCurrentDriftTrail()
    {
        if (!enableCurrentDriftTrail || currentDrift == null)
        {
            return;
        }

        TrailRenderer trail = GetOrCreateCurrentDriftTrail();
        if (trail == null)
        {
            return;
        }

        trail.emitting = false;
        trail.Clear();
        trail.emitting = true;
    }

    void StopCurrentDriftTrail()
    {
        if (!enableCurrentDriftTrail || currentDrift == null)
        {
            return;
        }

        TrailRenderer trail = currentDrift.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            return;
        }

        trail.emitting = false;
        trail.Clear();
    }

    TrailRenderer GetOrCreateCurrentDriftTrail()
    {
        if (currentDrift == null)
        {
            return null;
        }

        if (_trailOwner != currentDrift)
        {
            _trailOwner = currentDrift;
            _currentDriftTrail = currentDrift.GetComponentInChildren<TrailRenderer>();
            if (_currentDriftTrail == null)
            {
                _currentDriftTrail = currentDrift.gameObject.AddComponent<TrailRenderer>();
            }
        }

        if (_currentDriftTrail == null)
        {
            Debug.LogError("[DriftMarker] Failed to resolve TrailRenderer on currentDrift.");
            return null;
        }

        ApplyTrailSettings(_currentDriftTrail);
        return _currentDriftTrail;
    }

    void ApplyTrailSettings(TrailRenderer trail)
    {
        trail.time = trailTime;
        trail.widthMultiplier = trailWidth;
        trail.minVertexDistance = trailMinVertexDistance;
        trail.autodestruct = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.textureMode = LineTextureMode.Stretch;
        trail.alignment = LineAlignment.View;

        var widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.35f));
        trail.widthCurve = widthCurve;

        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(trailColor, 0f),
                new GradientColorKey(trailColor, 1f),
            },
            new[]
            {
                new GradientAlphaKey(trailColor.a, 0f),
                new GradientAlphaKey(0f, 1f),
            });
        trail.colorGradient = gradient;

        if (trailMaterial != null)
        {
            trail.material = trailMaterial;
        }
    }
}
