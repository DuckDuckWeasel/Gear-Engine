using GearEngine.CarSimulation.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class SplineCurveSamplerTests
    {
        [Test]
        public void StraightOpenSpline_HasNearZeroCurveAmount()
        {
            var spline = new Spline
            {
                Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(50f, 0f, 0f)),
                },
                Closed = false,
            };

            var config = new SplineSamplerConfig
            {
                CurveLookAheadStep = 0.05f,
                MaxCurveAngle = 90f,
            };

            var sampler = new SplineCurveSampler(spline, config, isClosed: false);
            CurveSample mid = sampler.Sample(0.5f);

            Assert.That(mid.CurveAmount, Is.LessThan(0.05f));
        }

        [Test]
        public void ElbowTurn_HasNonZeroCurveAmountAndDirection()
        {
            var spline = new Spline
            {
                Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(20f, 0f, 0f)),
                    new BezierKnot(new Vector3(20f, 0f, 20f)),
                },
                Closed = false,
            };

            var config = new SplineSamplerConfig
            {
                CurveLookAheadStep = 0.04f,
                MaxCurveAngle = 90f,
            };

            var sampler = new SplineCurveSampler(spline, config, isClosed: false);
            CurveSample found = default;
            bool any = false;
            for (int i = 0; i <= 25; i++)
            {
                float t = i / 25f;
                CurveSample s = sampler.Sample(Mathf.Clamp01(t));
                if (s.CurveAmount > 0.08f && !Mathf.Approximately(s.CurveDirection, 0f))
                {
                    found = s;
                    any = true;
                    break;
                }
            }

            Assert.That(any, Is.True, "Expected a sample with meaningful bend and a signed turn direction.");
            Assert.That(Mathf.Abs(found.CurveDirection), Is.EqualTo(1f));
        }
    }
}
