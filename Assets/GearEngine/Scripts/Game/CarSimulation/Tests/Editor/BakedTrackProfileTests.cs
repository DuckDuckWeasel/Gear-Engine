using GearEngine.CarSimulation.Track;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class BakedTrackProfileTests
    {
        [Test]
        public void Bake_OpenSpline_HasExpectedLengthAndEvaluateEndpoints()
        {
            var spline = new Spline
            {
                Knots = new[]
                {
                    new BezierKnot(new Vector3(0f, 0f, 0f)),
                    new BezierKnot(new Vector3(10f, 0f, 0f)),
                },
                Closed = false,
            };

            BakedTrackProfile profile = TrackProfileBaker.Bake(spline, sampleSpacing: 0.5f);
            Assert.That(profile.TotalLength, Is.GreaterThan(9f));
            Assert.That(profile.Samples.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(profile.IsClosed, Is.False);

            TrackSample start = profile.Evaluate(0f);
            TrackSample end = profile.Evaluate(profile.TotalLength);
            Assert.That(start.Position.x, Is.EqualTo(0f).Within(0.05f));
            Assert.That(end.Position.x, Is.EqualTo(10f).Within(0.05f));
        }

        [Test]
        public void Bake_ThrowsWhenSplineTooShort()
        {
            var spline = new Spline();
            Assert.Throws<System.InvalidOperationException>(() => TrackProfileBaker.Bake(spline));
        }
    }
}
