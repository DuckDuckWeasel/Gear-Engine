using GearEngine.SplineEvaluate.Bootstrap;
using GearEngine.SplineEvaluate.Definitions;
using GearEngine.SplineEvaluate.Simulation;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.SplineEvaluate.Presentation
{
    /// <summary>
    /// Scene-view gizmo drawer for the spline-evaluate system.
    /// Shows: track centerline, lateral offset band, car position marker,
    /// velocity vector, curvature intensity, lookahead window, and braking zones.
    /// Works in both Edit and Play mode.
    /// </summary>
    [ExecuteAlways]
    public sealed class SplineEvaluateGizmos : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private SplineEvaluateBootstrap bootstrap;

        [Header("Track Visualization")]
        [SerializeField] private int trackSampleCount = 200;
        [SerializeField] private float trackWidth = 4f;
        [SerializeField] private Color centerlineColor = new Color(0.3f, 0.6f, 1f, 0.6f);
        [SerializeField] private Color trackEdgeColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        [Header("Curvature Heatmap")]
        [SerializeField] private bool showCurvatureHeatmap = true;
        [SerializeField] private float curvatureHeatmapHeight = 3f;

        [Header("Car Marker")]
        [SerializeField] private float carMarkerSize = 2f;
        [SerializeField] private Color carColor = Color.yellow;
        [SerializeField] private Color velocityColor = Color.green;
        [SerializeField] private Color brakeColor = Color.red;
        [SerializeField] private Color driftColor = new Color(1f, 0.5f, 0f);

        [Header("Lookahead")]
        [SerializeField] private bool showLookahead = true;
        [SerializeField] private Color lookaheadColor = new Color(1f, 1f, 0f, 0.25f);

        [Header("Lateral Path")]
        [SerializeField] private bool showLateralPath = true;
        [SerializeField] private Color lateralPathColor = new Color(0f, 1f, 0.5f, 0.5f);

        private void OnDrawGizmos()
        {
            if (splineContainer == null || splineContainer.Spline == null || splineContainer.Spline.Count < 2)
            {
                return;
            }

            Spline spline = splineContainer.Spline;
            Transform splineTr = splineContainer.transform;
            float splineLength = spline.GetLength();

            DrawTrack(spline, splineTr, splineLength);

            if (showCurvatureHeatmap)
            {
                DrawCurvatureHeatmap(spline, splineTr, splineLength);
            }

            // Play-mode only: car state
            if (Application.isPlaying && bootstrap != null && bootstrap.ActiveDriver != null)
            {
                SplineEvaluateDriver driver = bootstrap.ActiveDriver;
                SplineMotionState state = driver.State;

                DrawCarMarker(spline, splineTr, state);

                if (showLookahead)
                {
                    DrawLookahead(spline, splineTr, splineLength, state);
                }

                if (showLateralPath)
                {
                    DrawLateralOffsetPath(spline, splineTr, splineLength, state);
                }

                DrawStateLabels(spline, splineTr, state);
            }
        }

        // ================================================================
        // Track
        // ================================================================

        private void DrawTrack(Spline spline, Transform splineTr, float splineLength)
        {
            Vector3 prevCenter = Vector3.zero;
            Vector3 prevLeft = Vector3.zero;
            Vector3 prevRight = Vector3.zero;

            for (int i = 0; i <= trackSampleCount; i++)
            {
                float t = (float)i / trackSampleCount;

                Vector3 localPos = SplineUtility.EvaluatePosition(spline, t);
                Vector3 localTangent = SplineUtility.EvaluateTangent(spline, t);
                Vector3 localUp = SplineUtility.EvaluateUpVector(spline, t);

                Vector3 worldPos = splineTr.TransformPoint(localPos);
                Vector3 worldTangent = splineTr.TransformDirection(localTangent).normalized;
                Vector3 worldUp = splineTr.TransformDirection(localUp).normalized;
                Vector3 worldRight = Vector3.Cross(worldUp, worldTangent).normalized;

                Vector3 left = worldPos - worldRight * trackWidth;
                Vector3 right = worldPos + worldRight * trackWidth;

                if (i > 0)
                {
                    // Centerline
                    Gizmos.color = centerlineColor;
                    Gizmos.DrawLine(prevCenter, worldPos);

                    // Track edges
                    Gizmos.color = trackEdgeColor;
                    Gizmos.DrawLine(prevLeft, left);
                    Gizmos.DrawLine(prevRight, right);
                }

                prevCenter = worldPos;
                prevLeft = left;
                prevRight = right;

                // Tick marks every 10%
                if (i % (trackSampleCount / 10) == 0 && i > 0)
                {
                    Gizmos.color = new Color(1f, 1f, 1f, 0.4f);
                    Gizmos.DrawLine(left, right);
                }
            }

            // Start/finish line
            float startT = 0f;
            Vector3 startPos = splineTr.TransformPoint((Vector3)SplineUtility.EvaluatePosition(spline, startT));
            Vector3 startTangent = splineTr.TransformDirection((Vector3)SplineUtility.EvaluateTangent(spline, startT)).normalized;
            Vector3 startUp = splineTr.TransformDirection((Vector3)SplineUtility.EvaluateUpVector(spline, startT)).normalized;
            Vector3 startRight = Vector3.Cross(startUp, startTangent).normalized;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(startPos - startRight * trackWidth * 1.2f, startPos + startRight * trackWidth * 1.2f);
            Gizmos.DrawSphere(startPos + startUp * 0.5f, 0.5f);
        }

        // ================================================================
        // Curvature Heatmap
        // ================================================================

        private void DrawCurvatureHeatmap(Spline spline, Transform splineTr, float splineLength)
        {
            int samples = Mathf.Min(trackSampleCount, 100);
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;

                float curvature = SplineCurvatureHelper.SampleCurvatureAt(spline, splineLength, t, out _);
                float intensity = Mathf.Clamp01(curvature / 0.12f);

                Vector3 localPos = SplineUtility.EvaluatePosition(spline, t);
                Vector3 localUp = SplineUtility.EvaluateUpVector(spline, t);
                Vector3 worldPos = splineTr.TransformPoint(localPos);
                Vector3 worldUp = splineTr.TransformDirection(localUp).normalized;

                // Color: green (straight) → yellow → red (sharp)
                Color heatColor = Color.Lerp(
                    new Color(0.1f, 0.8f, 0.2f, 0.5f),
                    new Color(1f, 0.1f, 0.1f, 0.8f),
                    intensity);
                Gizmos.color = heatColor;

                // Vertical bar at the track height
                float barHeight = curvatureHeatmapHeight * intensity + 0.2f;
                Vector3 barBase = worldPos + worldUp * 0.1f;
                Vector3 barTop = barBase + worldUp * barHeight;
                Gizmos.DrawLine(barBase, barTop);
                Gizmos.DrawSphere(barTop, 0.15f);
            }
        }

        // ================================================================
        // Car Marker
        // ================================================================

        private void DrawCarMarker(Spline spline, Transform splineTr, SplineMotionState state)
        {
            float t = SplineCurvatureHelper.WrapT(state.T);

            // Spline center position
            Vector3 localPos = SplineUtility.EvaluatePosition(spline, t);
            Vector3 localTangent = SplineUtility.EvaluateTangent(spline, t);
            Vector3 localUp = SplineUtility.EvaluateUpVector(spline, t);

            Vector3 worldPos = splineTr.TransformPoint(localPos);
            Vector3 worldTangent = splineTr.TransformDirection(localTangent).normalized;
            Vector3 worldUp = splineTr.TransformDirection(localUp).normalized;
            Vector3 worldRight = Vector3.Cross(worldUp, worldTangent).normalized;

            // Actual car position (with lateral offset)
            Vector3 carPos = worldPos + worldRight * state.LateralOffset + worldUp * state.SuspensionOffset;

            // Car marker — color based on state
            Color markerColor = carColor;
            if (state.IsDrifting) markerColor = driftColor;
            else if (state.IsBraking) markerColor = brakeColor;

            Gizmos.color = markerColor;
            Gizmos.DrawWireSphere(carPos, carMarkerSize);
            Gizmos.DrawSphere(carPos, carMarkerSize * 0.3f);

            // Direction arrow (velocity)
            float speedNorm = Mathf.Clamp01(state.Speed / 55f);
            Gizmos.color = state.IsBraking ? brakeColor : velocityColor;
            Vector3 velocityEnd = carPos + worldTangent * (speedNorm * 10f + 2f);
            Gizmos.DrawLine(carPos, velocityEnd);
            // Arrowhead
            Vector3 arrowLeft = velocityEnd - worldTangent * 1f + worldRight * 0.5f;
            Vector3 arrowRight = velocityEnd - worldTangent * 1f - worldRight * 0.5f;
            Gizmos.DrawLine(velocityEnd, arrowLeft);
            Gizmos.DrawLine(velocityEnd, arrowRight);

            // Lateral offset indicator: line from centerline to car
            Gizmos.color = new Color(0f, 1f, 1f, 0.6f);
            Gizmos.DrawLine(worldPos, carPos);

            // Slip angle visualization — yaw offset line
            if (Mathf.Abs(state.SlipAngle) > 1f)
            {
                Gizmos.color = driftColor;
                Quaternion slipRot = Quaternion.AngleAxis(state.SlipAngle, worldUp);
                Vector3 slipDir = slipRot * worldTangent;
                Gizmos.DrawLine(carPos, carPos + slipDir * 5f);
            }

            // Body roll indicator — tilt line
            if (Mathf.Abs(state.BodyRoll) > 0.5f)
            {
                Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
                Quaternion rollRot = Quaternion.AngleAxis(state.BodyRoll, worldTangent);
                Vector3 rollUp = rollRot * worldUp;
                Gizmos.DrawLine(carPos - rollUp * carMarkerSize, carPos + rollUp * carMarkerSize);
            }
        }

        // ================================================================
        // Lookahead
        // ================================================================

        private void DrawLookahead(Spline spline, Transform splineTr, float splineLength, SplineMotionState state)
        {
            float lookaheadMeters = 40f; // from config default
            float lookaheadT = lookaheadMeters / splineLength;

            float startT = SplineCurvatureHelper.WrapT(state.T);
            float endT = SplineCurvatureHelper.WrapT(state.T + lookaheadT);

            int steps = 20;
            Vector3 prevPos = Vector3.zero;

            for (int i = 0; i <= steps; i++)
            {
                float interp = (float)i / steps;
                float t = SplineCurvatureHelper.WrapT(startT + lookaheadT * interp);

                Vector3 localPos = SplineUtility.EvaluatePosition(spline, t);
                Vector3 localUp = SplineUtility.EvaluateUpVector(spline, t);
                Vector3 worldPos = splineTr.TransformPoint(localPos);
                Vector3 worldUp = splineTr.TransformDirection(localUp).normalized;

                // Raise slightly above track
                worldPos += worldUp * 0.3f;

                Gizmos.color = Color.Lerp(lookaheadColor, new Color(1f, 0f, 0f, 0.5f), interp);

                if (i > 0)
                {
                    Gizmos.DrawLine(prevPos, worldPos);
                }

                prevPos = worldPos;
            }
        }

        // ================================================================
        // Lateral Offset Path (personality-driven racing line preview)
        // ================================================================

        private void DrawLateralOffsetPath(Spline spline, Transform splineTr, float splineLength, SplineMotionState state)
        {
            int samples = 80;
            Vector3 prevPos = Vector3.zero;

            for (int i = 0; i <= samples; i++)
            {
                float t = (float)i / samples;

                Vector3 localPos = SplineUtility.EvaluatePosition(spline, t);
                Vector3 localTangent = SplineUtility.EvaluateTangent(spline, t);
                Vector3 localUp = SplineUtility.EvaluateUpVector(spline, t);

                Vector3 worldPos = splineTr.TransformPoint(localPos);
                Vector3 worldTangent = splineTr.TransformDirection(localTangent).normalized;
                Vector3 worldUp = splineTr.TransformDirection(localUp).normalized;
                Vector3 worldRight = Vector3.Cross(worldUp, worldTangent).normalized;

                // Apply current lateral offset value (if near car, use actual; otherwise show raw)
                float offset = state.LateralOffset;
                Vector3 offsetPos = worldPos + worldRight * offset + worldUp * 0.15f;

                if (i > 0)
                {
                    Gizmos.color = lateralPathColor;
                    Gizmos.DrawLine(prevPos, offsetPos);
                }

                prevPos = offsetPos;
            }
        }

        // ================================================================
        // State Labels (drawn as Gizmo spheres with Handles text)
        // ================================================================

        private void DrawStateLabels(Spline spline, Transform splineTr, SplineMotionState state)
        {
            float t = SplineCurvatureHelper.WrapT(state.T);

            Vector3 localPos = SplineUtility.EvaluatePosition(spline, t);
            Vector3 localUp = SplineUtility.EvaluateUpVector(spline, t);
            Vector3 worldPos = splineTr.TransformPoint(localPos);
            Vector3 worldUp = splineTr.TransformDirection(localUp).normalized;

            // Speed indicator sphere — size proportional to speed
            float speedNorm = Mathf.Clamp01(state.Speed / 55f);
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(worldPos + worldUp * 4f, speedNorm * 2f + 0.3f);

            // Target speed sphere (smaller, ahead)
            float targetNorm = Mathf.Clamp01(state.TargetSpeed / 55f);
            Gizmos.color = state.IsBraking
                ? new Color(1f, 0.2f, 0.2f, 0.4f)
                : new Color(0.2f, 1f, 0.3f, 0.4f);
            Gizmos.DrawWireSphere(worldPos + worldUp * 6f, targetNorm * 1.5f + 0.2f);
        }
    }
}
