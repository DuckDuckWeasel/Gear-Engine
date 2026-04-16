using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Simulation
{
    public sealed class SplineCurveSampler
    {
        public SplineCurveSampler(Spline spline, SplineSamplerConfig config, bool isClosed)
        {
            this.spline = spline;
            this.config = config ?? throw new System.ArgumentNullException(nameof(config));
            this.isClosed = isClosed;
        }

        private readonly Spline spline;
        private readonly SplineSamplerConfig config;
        private readonly bool isClosed;

        public CurveSample Sample(float normalizedProgress)
        {
            float t = Mathf.Clamp01(normalizedProgress);
            float step = config.CurveLookAheadStep;
            float tNext = isClosed ? Mathf.Repeat(t + step, 1f) : Mathf.Clamp01(t + step);
            SplineUtility.Evaluate(spline, t, out float3 pos, out float3 tan, out float3 upVec);
            SplineUtility.Evaluate(spline, tNext, out _, out float3 tanNext, out _);
            return BuildSample(pos, tan, upVec, tanNext);
        }

        private CurveSample BuildSample(float3 pos, float3 tan, float3 upVec, float3 tanNext)
        {
            Vector3 tangent = ((Vector3)tan).normalized;
            Vector3 up = ((Vector3)upVec).normalized;
            Vector3 nextTangent = ((Vector3)tanNext).normalized;
            float signedAngleDeg = Vector3.SignedAngle(tangent, nextTangent, up);
            if (float.IsNaN(signedAngleDeg) || float.IsInfinity(signedAngleDeg))
            {
                signedAngleDeg = 0f;
            }

            float angleDeg = Mathf.Abs(signedAngleDeg);
            float curveAmount = Mathf.Clamp01(angleDeg / Mathf.Max(1e-6f, config.MaxCurveAngle));
            float curveDirection = Mathf.Approximately(angleDeg, 0f) ? 0f : Mathf.Sign(signedAngleDeg);

            return new CurveSample(curveAmount, curveDirection, pos, tangent, up);
        }
    }
}
