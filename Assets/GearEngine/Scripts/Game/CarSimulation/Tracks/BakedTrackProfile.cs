using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.CarSimulation.Tracks
{
    public sealed class BakedTrackProfile
    {
        public BakedTrackProfile(float totalLength, IReadOnlyList<TrackSample> samples, bool isClosed)
        {
            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }

            if (samples.Count < 2)
            {
                throw new ArgumentException("At least two samples are required.", nameof(samples));
            }

            TotalLength = totalLength;
            this.samples = samples;
            this.isClosed = isClosed;
        }

        public float TotalLength { get; }
        public IReadOnlyList<TrackSample> Samples => samples;

        private readonly IReadOnlyList<TrackSample> samples;

        public bool IsClosed => isClosed;

        private readonly bool isClosed;

        public int FindSampleIndexNear(float distance)
        {
            float d = NormalizeDistance(distance);
            return FindBracketIndex(d);
        }

        public TrackSample LookAhead(float fromDistance, float aheadMetres)
        {
            return Evaluate(fromDistance + aheadMetres);
        }

        public TrackSample Evaluate(float distance)
        {
            if (samples.Count == 0)
            {
                return default;
            }

            float d = NormalizeDistance(distance);
            GetInterpolationSpan(d, out TrackSample a, out TrackSample b);
            return BakedTrackProfileInterpolation.BuildInterpolatedSample(a, b, d);
        }

        private float NormalizeDistance(float distance)
        {
            if (TotalLength <= Mathf.Epsilon)
            {
                return 0f;
            }

            if (isClosed)
            {
                float m = distance % TotalLength;
                return m < 0f ? m + TotalLength : m;
            }

            return Mathf.Clamp(distance, 0f, TotalLength);
        }

        private void GetInterpolationSpan(float distanceAlong, out TrackSample a, out TrackSample b)
        {
            int right = FindBracketIndex(distanceAlong);
            int left = right <= 0 ? (isClosed ? samples.Count - 1 : 0) : right - 1;
            a = samples[left];
            b = samples[right];
        }

        private int FindBracketIndex(float distanceAlong)
        {
            int lo = 0;
            int hi = samples.Count - 1;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (samples[mid].Distance <= distanceAlong)
                {
                    lo = mid;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return Mathf.Min(lo + 1, samples.Count - 1);
        }
    }
}
