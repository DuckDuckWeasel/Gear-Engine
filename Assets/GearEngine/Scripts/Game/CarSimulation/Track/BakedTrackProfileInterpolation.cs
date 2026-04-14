using UnityEngine;

namespace GearEngine.CarSimulation.Track
{
    internal static class BakedTrackProfileInterpolation
    {
        internal static TrackSample BuildInterpolatedSample(TrackSample a, TrackSample b, float d)
        {
            if (b.Distance <= a.Distance)
            {
                return a;
            }

            float span = b.Distance - a.Distance;
            if (span <= Mathf.Epsilon)
            {
                return b;
            }

            float t = Mathf.Clamp01((d - a.Distance) / span);
            return BuildBlended(a, b, t);
        }

        private static TrackSample BuildBlended(TrackSample a, TrackSample b, float t)
        {
            Vector3 pos = Vector3.LerpUnclamped(a.Position, b.Position, t);
            Vector3 fwd = Vector3.SlerpUnclamped(a.Forward, b.Forward, t).normalized;
            Vector3 up = Vector3.SlerpUnclamped(a.Up, b.Up, t).normalized;
            float curv = Mathf.LerpUnclamped(a.Curvature, b.Curvature, t);
            float signed = Mathf.LerpUnclamped(a.SignedCurvature, b.SignedCurvature, t);
            float dist = Mathf.LerpUnclamped(a.Distance, b.Distance, t);
            float norm = Mathf.LerpUnclamped(a.NormalizedT, b.NormalizedT, t);
            return new TrackSample(dist, norm, pos, fwd, up, curv, signed);
        }
    }
}
