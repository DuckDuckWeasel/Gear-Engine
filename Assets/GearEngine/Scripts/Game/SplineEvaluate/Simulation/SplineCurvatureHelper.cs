using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.SplineEvaluate.Simulation
{
    /// <summary>
    /// Static utility for sampling spline curvature. Curvature is computed via
    /// finite differences of normalized tangent vectors over small arc-length steps.
    /// <para>
    /// Curvature ≈ |T'(s+ds) - T'(s)| / ds, where T' is the unit tangent and s is
    /// arc-length parameterization. Higher curvature = sharper turn.
    /// </para>
    /// </summary>
    public static class SplineCurvatureHelper
    {
        /// <summary>
        /// Samples the maximum unsigned curvature within a forward window starting at
        /// <paramref name="fromT"/> and covering <paramref name="lookaheadMeters"/>.
        /// </summary>
        /// <param name="spline">The spline to sample.</param>
        /// <param name="splineLength">Cached total arc-length of the spline.</param>
        /// <param name="fromT">Starting normalized parameter (0–1).</param>
        /// <param name="lookaheadMeters">Forward distance to scan (meters).</param>
        /// <param name="sampleCount">Number of evenly-spaced probe points.</param>
        /// <returns>Maximum unsigned curvature found in the window.</returns>
        public static float SampleMaxCurvature(
            Spline spline,
            float splineLength,
            float fromT,
            float lookaheadMeters,
            int sampleCount,
            out float signedMaxCurvature)
        {
            signedMaxCurvature = 0f;
            if (spline == null || spline.Count < 2 || sampleCount < 2 || splineLength <= 0f)
            {
                return 0f;
            }

            float stepMeters = lookaheadMeters / (sampleCount - 1);
            float stepT = stepMeters / splineLength;

            float maxCurvature = 0f;
            Vector3 prevTangent = ((Vector3)SplineUtility.EvaluateTangent(spline, WrapT(fromT))).normalized;

            for (int i = 1; i < sampleCount; i++)
            {
                float t = WrapT(fromT + stepT * i);
                Vector3 tangent = ((Vector3)SplineUtility.EvaluateTangent(spline, t)).normalized;

                float curvature = (tangent - prevTangent).magnitude / stepMeters;
                if (curvature > maxCurvature)
                {
                    maxCurvature = curvature;
                    // Compute sign
                    Vector3 up = ((Vector3)SplineUtility.EvaluateUpVector(spline, t)).normalized;
                    Vector3 right = Vector3.Cross(up, tangent).normalized;
                    float sign = Mathf.Sign(Vector3.Dot((tangent - prevTangent) / stepMeters, right));
                    signedMaxCurvature = curvature * sign;
                }

                prevTangent = tangent;
            }

            return maxCurvature;
        }

        /// <summary>
        /// Computes the unsigned curvature at a single point on the spline using a
        /// small finite-difference step.
        /// </summary>
        public static float SampleCurvatureAt(Spline spline, float splineLength, float t, out float signedCurvature, float epsilonMeters = 0.5f)
        {
            signedCurvature = 0f;
            if (spline == null || spline.Count < 2 || splineLength <= 0f)
            {
                return 0f;
            }

            float epsilonT = epsilonMeters / splineLength;
            Vector3 tangentA = ((Vector3)SplineUtility.EvaluateTangent(spline, WrapT(t - epsilonT * 0.5f))).normalized;
            Vector3 tangentB = ((Vector3)SplineUtility.EvaluateTangent(spline, WrapT(t + epsilonT * 0.5f))).normalized;
            
            float curvature = (tangentB - tangentA).magnitude / epsilonMeters;

            Vector3 tangent = ((Vector3)SplineUtility.EvaluateTangent(spline, WrapT(t))).normalized;
            Vector3 up = ((Vector3)SplineUtility.EvaluateUpVector(spline, WrapT(t))).normalized;
            Vector3 right = Vector3.Cross(up, tangent).normalized;
            
            float sign = Mathf.Sign(Vector3.Dot((tangentB - tangentA) / epsilonMeters, right));
            signedCurvature = curvature * sign;

            return curvature;
        }

        /// <summary>Wraps t into [0, 1) for closed-loop splines.</summary>
        public static float WrapT(float t)
        {
            t %= 1f;
            if (t < 0f) t += 1f;
            return t;
        }
    }
}
