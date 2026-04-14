using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tracks
{
    public static class TrackProfileBaker
    {
        public const float DefaultSampleSpacing = 0.25f;

        public static BakedTrackProfile Bake(Spline spline, float sampleSpacing = DefaultSampleSpacing)
        {
            ValidateSpline(spline, sampleSpacing);
            float length = spline.GetLength();
            bool closed = spline.Closed;
            List<float> distances = BuildDistanceStops(length, closed, sampleSpacing);
            List<TrackSample> raw = SampleAlongDistances(spline, length, distances);
            ApplyCurvaturePass(raw, closed, length);
            AppendClosedLoopDuplicateIfNeeded(raw, closed, length);
            return new BakedTrackProfile(length, raw, closed);
        }

        private static void ValidateSpline(Spline spline, float sampleSpacing)
        {
            if (spline == null)
            {
                throw new ArgumentNullException(nameof(spline));
            }

            if (spline.Count < 2)
            {
                throw new InvalidOperationException("Spline must contain at least two knots.");
            }

            EnsurePositiveSpacing(sampleSpacing);
            EnsureSplineHasLength(spline);
        }

        private static void EnsurePositiveSpacing(float sampleSpacing)
        {
            if (sampleSpacing <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleSpacing), "Sample spacing must be positive.");
            }
        }

        private static void EnsureSplineHasLength(Spline spline)
        {
            if (spline.GetLength() < 1e-4f)
            {
                throw new InvalidOperationException("Spline length is too small to bake.");
            }
        }

        private static List<float> BuildDistanceStops(float length, bool closed, float sampleSpacing)
        {
            var distances = new List<float>();
            for (float d = 0f; d < length - 1e-4f; d += sampleSpacing)
            {
                distances.Add(d);
            }

            if (!closed)
            {
                AppendOpenEndpointIfNeeded(distances, length);
            }
            else
            {
                EnsureClosedLoopHasStart(distances);
            }

            EnsureAtLeastTwoStops(distances, sampleSpacing, length);
            return distances;
        }

        private static void AppendOpenEndpointIfNeeded(List<float> distances, float length)
        {
            float last = distances.Count > 0 ? distances[distances.Count - 1] : -1f;
            if (Mathf.Abs(last - length) > 1e-3f)
            {
                distances.Add(length);
            }
        }

        private static void EnsureClosedLoopHasStart(List<float> distances)
        {
            if (distances.Count == 0)
            {
                distances.Add(0f);
            }
        }

        private static void EnsureAtLeastTwoStops(List<float> distances, float sampleSpacing, float length)
        {
            if (distances.Count < 2)
            {
                distances.Add(Mathf.Min(sampleSpacing, length * 0.5f));
            }
        }

        private static List<TrackSample> SampleAlongDistances(Spline spline, float length, List<float> distances)
        {
            var raw = new List<TrackSample>(distances.Count);
            for (int i = 0; i < distances.Count; i++)
            {
                float d = distances[i];
                float tNorm = d / length;
                SplineUtility.Evaluate(spline, tNorm, out float3 pos, out float3 tan, out float3 upVec);
                Vector3 p = pos;
                Vector3 forward = math.lengthsq(tan) > 1e-8f ? ((Vector3)tan).normalized : Vector3.forward;
                Vector3 up = math.lengthsq(upVec) > 1e-8f ? ((Vector3)upVec).normalized : Vector3.up;
                raw.Add(new TrackSample(d, tNorm, p, forward, up, 0f, 0f));
            }

            return raw;
        }

        private static void AppendClosedLoopDuplicateIfNeeded(List<TrackSample> raw, bool closed, float length)
        {
            if (closed && raw.Count >= 2)
            {
                TrackSample s0 = raw[0];
                raw.Add(new TrackSample(length, 1f, s0.Position, s0.Forward, s0.Up, s0.Curvature, s0.SignedCurvature));
            }
        }

        private static void ApplyCurvaturePass(IList<TrackSample> list, bool closed, float totalLength)
        {
            int n = list.Count;
            for (int i = 0; i < n; i++)
            {
                ApplyCurvatureAtIndex(list, i, n, closed, totalLength);
            }
        }

        private static void ApplyCurvatureAtIndex(IList<TrackSample> list, int i, int n, bool closed, float totalLength)
        {
            if (!TryResolveNextIndex(i, n, closed, out int next))
            {
                CopyCurvatureFromPrevious(list, i);
                return;
            }

            TrackSample a = list[i];
            TrackSample b = list[next];
            float ds = ComputeSegmentDs(a, b, closed, next, totalLength);
            float signedK = ComputeSignedCurvature(a, b, ds);
            float unsignedK = Mathf.Abs(signedK);
            list[i] = new TrackSample(a.Distance, a.NormalizedT, a.Position, a.Forward, a.Up, unsignedK, signedK);
        }

        private static bool TryResolveNextIndex(int i, int n, bool closed, out int next)
        {
            next = i + 1;
            if (next < n)
            {
                return true;
            }

            if (closed)
            {
                next = 0;
                return true;
            }

            next = 0;
            return false;
        }

        private static void CopyCurvatureFromPrevious(IList<TrackSample> list, int i)
        {
            if (i <= 0)
            {
                return;
            }

            TrackSample prevS = list[i - 1];
            TrackSample cur = list[i];
            list[i] = new TrackSample(cur.Distance, cur.NormalizedT, cur.Position, cur.Forward, cur.Up, prevS.Curvature, prevS.SignedCurvature);
        }

        private static float ComputeSegmentDs(TrackSample a, TrackSample b, bool closed, int next, float totalLength)
        {
            float ds = Mathf.Abs(b.Distance - a.Distance);
            if (closed && next == 0)
            {
                ds = totalLength - a.Distance;
            }

            return Mathf.Max(ds, 1e-4f);
        }

        private static float ComputeSignedCurvature(TrackSample a, TrackSample b, float ds)
        {
            Vector3 f0 = a.Forward;
            Vector3 f1 = b.Forward;
            Vector3 up = (a.Up + b.Up).normalized;
            if (up.sqrMagnitude < 1e-8f)
            {
                up = Vector3.up;
            }

            float signedAngle = Vector3.SignedAngle(f0, f1, up) * Mathf.Deg2Rad;
            return signedAngle / ds;
        }
    }
}
