using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace GearEngine.CarSimulation.Tracks
{
    [InfoBox("Generates props along a spline-based track.\n\nAdd rules to the list. Each rule type determines WHERE it applies (Curves, Straights, All, or the Inside/Outside fill areas).")]
    public class SplinePropGenerator : MonoBehaviour
    {
        // ──────────────────────────────────────────────
    //  Enums
    // ──────────────────────────────────────────────

    public enum PlacementSide
    {
        Left,
        Right,
        Both,
        Inside,
        Outside
    }

    // ──────────────────────────────────────────────
    //  Rule Base Class
    // ──────────────────────────────────────────────

    [System.Serializable]
    public abstract class PropRule
    {
        [Tooltip("Uncheck to disable this rule from generating.")]
        public bool enabled = true;

        [Title("Prefabs")]
        [Tooltip("Possible prefabs to spawn. A random one is chosen per placement.")]
        public List<GameObject> prefabs;

        [Title("Density")]
        [Range(0f, 1f)]
        [Tooltip("Probability of spawning in a given spline segment.")]
        public float chancePerSegment = 0.8f;

        [Tooltip("Minimum props per segment (if selected).")]
        public int minAmount = 3;

        [Tooltip("Maximum props per segment (if selected).")]
        public int maxAmount = 10;

        [Title("Jitter")]
        [Tooltip("Random offset.\nX = lateral, Y = forward/backward along the spline.")]
        public Vector2 randomOffsetRange = Vector2.zero;

        [Tooltip("If true, applies a random Y rotation (0-360) to the spawned prop.")]
        public bool randomYRotation = false;

        [Title("Overlap")]
        [Tooltip("Minimum distance between any two props. 0 = disabled.")]
        public float minSeparation = 1f;
    }

    // ──────────────────────────────────────────────
    //  Rule Types (Polymorphic)
    // ──────────────────────────────────────────────

    [System.Serializable]
    public class All : PropRule
    {
        [Title("Placement Settings")]
        public PlacementSide side = PlacementSide.Both;

        [ShowIf("@this.side == PlacementSide.Left || this.side == PlacementSide.Both")]
        [Tooltip("Distance from spline center to the left edge.")]
        public float leftDistance = 4.5f;

        [ShowIf("@this.side == PlacementSide.Right || this.side == PlacementSide.Both")]
        [Tooltip("Distance from spline center to the right edge.")]
        public float rightDistance = 4.5f;

        [ShowIf("side", PlacementSide.Inside)]
        [Tooltip("Maximum inward distance from the road toward the center.")]
        public float insideMaxDistance = 20f;

        [ShowIf("side", PlacementSide.Outside)]
        [Tooltip("Maximum outward distance from the road away from the center.")]
        public float outsideMaxDistance = 20f;

        [ShowIf("@this.side == PlacementSide.Inside || this.side == PlacementSide.Outside")]
        [Tooltip("Minimum buffer from the road surface to keep props off the track.")]
        public float minSplineDistance = 1.5f;
    }

    [System.Serializable]
    public class Curves : PropRule
    {
        [Title("Curve Settings")]
        [Tooltip("Angle threshold (degrees) to classify this as a curve.")]
        [Range(1f, 45f)]
        public float curvatureThreshold = 5f;

        [Title("Placement Settings")]
        public PlacementSide side = PlacementSide.Both;

        [ShowIf("@this.side == PlacementSide.Left || this.side == PlacementSide.Both")]
        [Tooltip("Distance from spline center to the left edge.")]
        public float leftDistance = 4.5f;

        [ShowIf("@this.side == PlacementSide.Right || this.side == PlacementSide.Both")]
        [Tooltip("Distance from spline center to the right edge.")]
        public float rightDistance = 4.5f;

        [ShowIf("side", PlacementSide.Inside)]
        [Tooltip("Maximum inward distance from the road toward the center.")]
        public float insideMaxDistance = 20f;

        [ShowIf("side", PlacementSide.Outside)]
        [Tooltip("Maximum outward distance from the road away from the center.")]
        public float outsideMaxDistance = 20f;

        [ShowIf("@this.side == PlacementSide.Inside || this.side == PlacementSide.Outside")]
        [Tooltip("Minimum buffer from the road surface to keep props off the track.")]
        public float minSplineDistance = 1.5f;
    }

    [System.Serializable]
    public class Straights : PropRule
    {
        [Title("Straight Settings")]
        [Tooltip("Angle threshold (degrees). Below this is classified as straight.")]
        [Range(1f, 45f)]
        public float curvatureThreshold = 5f;

        [Title("Placement Settings")]
        public PlacementSide side = PlacementSide.Both;

        [ShowIf("@this.side == PlacementSide.Left || this.side == PlacementSide.Both")]
        [Tooltip("Distance from spline center to the left edge.")]
        public float leftDistance = 4.5f;

        [ShowIf("@this.side == PlacementSide.Right || this.side == PlacementSide.Both")]
        [Tooltip("Distance from spline center to the right edge.")]
        public float rightDistance = 4.5f;

        [ShowIf("side", PlacementSide.Inside)]
        [Tooltip("Maximum inward distance from the road toward the center.")]
        public float insideMaxDistance = 20f;

        [ShowIf("side", PlacementSide.Outside)]
        [Tooltip("Maximum outward distance from the road away from the center.")]
        public float outsideMaxDistance = 20f;

        [ShowIf("@this.side == PlacementSide.Inside || this.side == PlacementSide.Outside")]
        [Tooltip("Minimum buffer from the road surface to keep props off the track.")]
        public float minSplineDistance = 1.5f;
    }

    [System.Serializable]
    public class Inside : PropRule
    {
        [Title("Inside Fill Settings")]
        [Tooltip("Maximum distance from the road inward toward this object's position.")]
        public float maxDistance = 20f;

        [Tooltip("Minimum buffer from the road surface to keep props off the track.")]
        public float minSplineDistance = 1.5f;
    }

    [System.Serializable]
    public class Outside : PropRule
    {
        [Title("Outside Fill Settings")]
        [Tooltip("Maximum distance from the road outward (away from this object's position).")]
        public float maxDistance = 20f;

        [Tooltip("Minimum buffer from the road surface to keep props off the track.")]
        public float minSplineDistance = 1.5f;
    }

    // ──────────────────────────────────────────────
    //  Inspector
    // ──────────────────────────────────────────────

    [Required]
    public SplineContainer track;

    [SerializeReference]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<PropRule> propRules = new List<PropRule>();

    [Tooltip("Automatically generate props on Start.")]
    public bool generateOnStart = false;

    [FoldoutGroup("Generated Objects"), ReadOnly]
    public List<GameObject> generatedProps = new List<GameObject>();

    // ──────────────────────────────────────────────
    //  Internal — Placement descriptor
    // ──────────────────────────────────────────────

    private struct Placement
    {
        public bool isInside;
        public bool isOutside;
        public float lateralMultiplier; // -1 left, +1 right
        public float distance;          // edge distance or inside/outside max distance
        public float minSplineDistance; // only for inside/outside
    }

    private static void CollectPlacements(PropRule rule, List<Placement> output)
    {
        output.Clear();

        if (rule is All all)
        {
            if (all.side == PlacementSide.Left || all.side == PlacementSide.Both)
                output.Add(new Placement { lateralMultiplier = -1f, distance = all.leftDistance });
            if (all.side == PlacementSide.Right || all.side == PlacementSide.Both)
                output.Add(new Placement { lateralMultiplier = 1f, distance = all.rightDistance });
            if (all.side == PlacementSide.Inside)
                output.Add(new Placement { isInside = true, distance = all.insideMaxDistance, minSplineDistance = all.minSplineDistance });
            if (all.side == PlacementSide.Outside)
                output.Add(new Placement { isOutside = true, distance = all.outsideMaxDistance, minSplineDistance = all.minSplineDistance });
        }
        else if (rule is Curves curves)
        {
            if (curves.side == PlacementSide.Left || curves.side == PlacementSide.Both)
                output.Add(new Placement { lateralMultiplier = -1f, distance = curves.leftDistance });
            if (curves.side == PlacementSide.Right || curves.side == PlacementSide.Both)
                output.Add(new Placement { lateralMultiplier = 1f, distance = curves.rightDistance });
            if (curves.side == PlacementSide.Inside)
                output.Add(new Placement { isInside = true, distance = curves.insideMaxDistance, minSplineDistance = curves.minSplineDistance });
            if (curves.side == PlacementSide.Outside)
                output.Add(new Placement { isOutside = true, distance = curves.outsideMaxDistance, minSplineDistance = curves.minSplineDistance });
        }
        else if (rule is Straights straights)
        {
            if (straights.side == PlacementSide.Left || straights.side == PlacementSide.Both)
                output.Add(new Placement { lateralMultiplier = -1f, distance = straights.leftDistance });
            if (straights.side == PlacementSide.Right || straights.side == PlacementSide.Both)
                output.Add(new Placement { lateralMultiplier = 1f, distance = straights.rightDistance });
            if (straights.side == PlacementSide.Inside)
                output.Add(new Placement { isInside = true, distance = straights.insideMaxDistance, minSplineDistance = straights.minSplineDistance });
            if (straights.side == PlacementSide.Outside)
                output.Add(new Placement { isOutside = true, distance = straights.outsideMaxDistance, minSplineDistance = straights.minSplineDistance });
        }
        else if (rule is Inside insideFill)
        {
            output.Add(new Placement { isInside = true, distance = insideFill.maxDistance, minSplineDistance = insideFill.minSplineDistance });
        }
        else if (rule is Outside outsideFill)
        {
            output.Add(new Placement { isOutside = true, distance = outsideFill.maxDistance, minSplineDistance = outsideFill.minSplineDistance });
        }
    }

    // ──────────────────────────────────────────────
    //  Lifecycle
    // ──────────────────────────────────────────────

    private void Start()
    {
        // When driven by a TrackViewComponent, generation is triggered by
        // BroadcastMessage("Generate") at the right time (after FrustumFit).
        // Skip automatic Start() generation in that case.
        if (generateOnStart && GetComponentInParent<GearEngine.CarSimulation.Tracks.TrackViewComponent>() == null)
        {
            Generate();
        }
    }

    // ──────────────────────────────────────────────
    //  Generation
    // ──────────────────────────────────────────────

    [Button("Generate / Reset Props", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    public void Generate()
    {
        ClearProps();
        if (track == null || propRules == null || propRules.Count == 0) return;

        var spline = track.Spline;
        if (spline == null) return;

        Transform container = new GameObject("Generated Props").transform;
        container.SetParent(this.transform);
        container.localPosition = Vector3.zero;

        // The center reference for Inside/Outside placement is this object's position
        Vector3 center = transform.position;

        // Global overlap tracking
        List<Vector3> placedPositions = new List<Vector3>();
        List<Placement> placements = new List<Placement>();

        int curveCount = spline.GetCurveCount();

        for (int curveIndex = 0; curveIndex < curveCount; curveIndex++)
        {
            foreach (var rule in propRules)
            {
                if (rule == null) continue;
                if (!rule.enabled) continue;
                if (rule.prefabs == null || rule.prefabs.Count == 0) continue;
                if (UnityEngine.Random.value > rule.chancePerSegment) continue;

                CollectPlacements(rule, placements);
                if (placements.Count == 0) continue;

                // Zone filtering
                bool isCurvesRule = rule is Curves;
                bool isStraightsRule = rule is Straights;
                float threshold = 0f;
                if (isCurvesRule) threshold = ((Curves)rule).curvatureThreshold;
                if (isStraightsRule) threshold = ((Straights)rule).curvatureThreshold;

                int amount = UnityEngine.Random.Range(rule.minAmount, rule.maxAmount + 1);

                for (int i = 0; i < amount; i++)
                {
                    float localT = (i + 0.5f) / amount;
                    float globalT = (curveIndex + localT) / curveCount;

                    // ── Curve / Straight filter ──
                    if (isCurvesRule || isStraightsRule)
                    {
                        bool isCurve = GetCurvatureAngle(spline, globalT, curveCount) >= threshold;
                        if (isCurvesRule && !isCurve) continue;
                        if (isStraightsRule && isCurve) continue;
                    }

                    // ── Sample spline ──
                    Vector3 splinePos = track.transform.TransformPoint(SplineUtility.EvaluatePosition(spline, globalT));
                    Vector3 tangent   = track.transform.TransformDirection(SplineUtility.EvaluateTangent(spline, globalT)).normalized;
                    Vector3 up        = track.transform.TransformDirection(SplineUtility.EvaluateUpVector(spline, globalT)).normalized;
                    Vector3 right     = Vector3.Cross(up, tangent).normalized;

                    // ── Place on each active placement ──
                    foreach (var placement in placements)
                    {
                        Vector3 finalPos;

                        if (placement.isInside)
                        {
                            if (!TryComputeInsidePosition(splinePos, center, tangent, right, placement, rule.randomOffsetRange, out finalPos))
                                continue;
                        }
                        else if (placement.isOutside)
                        {
                            if (!TryComputeOutsidePosition(splinePos, center, tangent, right, placement, rule.randomOffsetRange, out finalPos))
                                continue;
                        }
                        else
                        {
                            finalPos = ComputeEdgePosition(splinePos, tangent, right, placement, rule.randomOffsetRange);
                        }

                        if (rule.minSeparation > 0f && IsOverlapping(finalPos, placedPositions, rule.minSeparation))
                            continue;

                        GameObject prefab = rule.prefabs[UnityEngine.Random.Range(0, rule.prefabs.Count)];
                        if (prefab == null) continue;

                        Quaternion rotation = prefab.transform.rotation;
                        if (rule.randomYRotation)
                        {
                            rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up) * rotation;
                        }

                        GameObject instance = Instantiate(prefab, finalPos, rotation);
                        instance.transform.SetParent(container);
                        generatedProps.Add(instance);
                        placedPositions.Add(finalPos);
                    }
                }
            }
        }
    }

    // ──────────────────────────────────────────────
    //  Position Computation
    // ──────────────────────────────────────────────

    /// <summary>
    /// Computes a position on the left or right edge of the track.
    /// </summary>
    private Vector3 ComputeEdgePosition(
        Vector3 splinePos, Vector3 tangent, Vector3 right,
        Placement placement, Vector2 jitter)
    {
        float dist = placement.distance * track.transform.lossyScale.x;
        Vector3 pos = splinePos + right * dist * placement.lateralMultiplier;

        if (jitter.x > 0f)
            pos += right * UnityEngine.Random.Range(-jitter.x, jitter.x) * track.transform.lossyScale.x;

        if (jitter.y > 0f)
            pos += tangent * UnityEngine.Random.Range(-jitter.y, jitter.y) * track.transform.lossyScale.z;

        return pos;
    }

    /// <summary>
    /// Computes a position inside the enclosed track area.
    /// Projects perpendicularly from the spline towards the inner area.
    /// </summary>
    private bool TryComputeInsidePosition(
        Vector3 splinePos, Vector3 center, Vector3 tangent, Vector3 right,
        Placement placement, Vector2 jitter, out Vector3 result)
    {
        result = Vector3.zero;

        Vector3 toCenter = center - splinePos;
        toCenter.y = 0f;
        float distToCenter = toCenter.magnitude;
        if (distToCenter < 0.01f) return false;

        float dot = Vector3.Dot(toCenter, right);
        Vector3 inward = dot >= 0 ? right : -right;

        float scale = track.transform.lossyScale.x;
        float minDist = placement.minSplineDistance * scale;
        float maxDist = Mathf.Min(placement.distance * scale, distToCenter);

        if (maxDist <= minDist) return false;

        float dist = UnityEngine.Random.Range(minDist, maxDist);
        result = splinePos + inward * dist;

        // Lateral jitter perpendicular to inward direction
        if (jitter.x > 0f)
        {
            Vector3 lateralDir = Vector3.Cross(Vector3.up, inward).normalized;
            result += lateralDir * UnityEngine.Random.Range(-jitter.x, jitter.x) * scale;
        }

        // Tangent jitter
        if (jitter.y > 0f)
            result += tangent * UnityEngine.Random.Range(-jitter.y, jitter.y) * track.transform.lossyScale.z;

        return true; // Perpendicular projection respects boundaries inherently better
    }

    /// <summary>
    /// Computes a position outside the enclosed track area.
    /// Projects perpendicularly from the spline outwards.
    /// </summary>
    private bool TryComputeOutsidePosition(
        Vector3 splinePos, Vector3 center, Vector3 tangent, Vector3 right,
        Placement placement, Vector2 jitter, out Vector3 result)
    {
        result = Vector3.zero;

        Vector3 toCenter = center - splinePos;
        toCenter.y = 0f;

        // Outward is the opposite of inward
        float dot = Vector3.Dot(toCenter, right);
        Vector3 outward = dot >= 0 ? -right : right;

        float scale = track.transform.lossyScale.x;
        float minDist = placement.minSplineDistance * scale;
        float maxDist = placement.distance * scale;

        if (maxDist <= minDist) return false;

        float dist = UnityEngine.Random.Range(minDist, maxDist);
        result = splinePos + outward * dist;

        // Lateral jitter perpendicular to outward direction
        if (jitter.x > 0f)
        {
            Vector3 lateralDir = Vector3.Cross(Vector3.up, outward).normalized;
            result += lateralDir * UnityEngine.Random.Range(-jitter.x, jitter.x) * scale;
        }

        // Tangent jitter
        if (jitter.y > 0f)
            result += tangent * UnityEngine.Random.Range(-jitter.y, jitter.y) * track.transform.lossyScale.z;

        return true;
    }

    // ──────────────────────────────────────────────
    //  Utilities
    // ──────────────────────────────────────────────

    /// <summary>
    /// Measures curvature (degrees) at a spline point by comparing
    /// tangent directions slightly before and after it.
    /// </summary>
    private float GetCurvatureAngle(Spline spline, float globalT, int curveCount)
    {
        float delta = 1f / (curveCount * 20f);

        float t0, t1;
        if (spline.Closed)
        {
            t0 = Mathf.Repeat(globalT - delta, 1f);
            t1 = Mathf.Repeat(globalT + delta, 1f);
        }
        else
        {
            t0 = Mathf.Clamp01(globalT - delta);
            t1 = Mathf.Clamp01(globalT + delta);
        }

        Vector3 tan0 = track.transform.TransformDirection(SplineUtility.EvaluateTangent(spline, t0)).normalized;
        Vector3 tan1 = track.transform.TransformDirection(SplineUtility.EvaluateTangent(spline, t1)).normalized;

        return Vector3.Angle(tan0, tan1);
    }

    /// <summary>
    /// Returns true if pos is within minSeparation of any existing placement.
    /// </summary>
    private static bool IsOverlapping(Vector3 pos, List<Vector3> existing, float minSeparation)
    {
        float sqr = minSeparation * minSeparation;
        foreach (var p in existing)
        {
            if ((pos - p).sqrMagnitude < sqr) return true;
        }
        return false;
    }

    // ──────────────────────────────────────────────
    //  Cleanup
    // ──────────────────────────────────────────────

    [Button("Clear Props")]
    public void ClearProps()
    {
        foreach (var obj in generatedProps)
        {
            if (obj != null)
            {
                if (Application.isPlaying) Destroy(obj);
                else DestroyImmediate(obj);
            }
        }
        generatedProps.Clear();

        Transform container = transform.Find("Generated Props");
        if (container != null)
        {
            container.name = "DELETED_PROPS";
            if (Application.isPlaying) Destroy(container.gameObject);
            else DestroyImmediate(container.gameObject);
        }
    }
}
}
