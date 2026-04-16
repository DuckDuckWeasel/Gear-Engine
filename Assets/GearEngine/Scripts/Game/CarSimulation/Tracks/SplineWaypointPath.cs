using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tracks
{
    public sealed class SplineWaypointPath
    {
        private readonly IReadOnlyList<Vector3> pointsLocal;
        private readonly bool isClosed;

        private SplineWaypointPath(IReadOnlyList<Vector3> pointsLocal, float totalLength, bool isClosed)
        {
            this.pointsLocal = pointsLocal;
            TotalLength = totalLength;
            this.isClosed = isClosed;
        }

        public int Count => pointsLocal.Count;

        public float TotalLength { get; }

        public bool IsClosed => isClosed;

        public Vector3 GetLocalPoint(int index)
        {
            return pointsLocal[ClampIndex(index)];
        }

        public Vector3 GetWorldPoint(int index, Transform splineTransform)
        {
            return splineTransform.TransformPoint(GetLocalPoint(index));
        }

        public static SplineWaypointPath Build(Spline spline, float spacingMetres)
        {
            if (spline == null)
            {
                throw new ArgumentNullException(nameof(spline));
            }

            if (spline.Count < 2)
            {
                throw new InvalidOperationException("Spline must contain at least two knots.");
            }

            float spacing = Mathf.Max(0.5f, spacingMetres);
            float length = spline.GetLength();
            if (length < 1e-4f)
            {
                throw new InvalidOperationException("Spline length is too small.");
            }

            bool closed = spline.Closed;
            var positions = new List<Vector3>();
            for (float d = 0f; d < length - 1e-4f; d += spacing)
            {
                float tNorm = d / length;
                SplineUtility.Evaluate(spline, tNorm, out float3 pos, out _, out _);
                positions.Add(pos);
            }

            if (!closed)
            {
                SplineUtility.Evaluate(spline, 1f, out float3 endPos, out _, out _);
                if (positions.Count == 0 || math.distancesq(positions[positions.Count - 1], endPos) > 1e-6f)
                {
                    positions.Add(endPos);
                }
            }
            else
            {
                if (positions.Count < 2)
                {
                    SplineUtility.Evaluate(spline, 0f, out float3 p0, out _, out _);
                    SplineUtility.Evaluate(spline, 0.5f, out float3 pm, out _, out _);
                    positions.Clear();
                    positions.Add(p0);
                    positions.Add(pm);
                }
            }

            float perimeter = 0f;
            for (int i = 1; i < positions.Count; i++)
            {
                perimeter += Vector3.Distance(positions[i - 1], positions[i]);
            }

            if (closed && positions.Count >= 2)
            {
                perimeter += Vector3.Distance(positions[positions.Count - 1], positions[0]);
            }

            float total = perimeter > 1e-4f ? perimeter : length;
            return new SplineWaypointPath(positions, total, closed);
        }

        private int ClampIndex(int index)
        {
            if (pointsLocal.Count == 0)
            {
                return 0;
            }

            if (!isClosed)
            {
                return Mathf.Clamp(index, 0, pointsLocal.Count - 1);
            }

            int n = pointsLocal.Count;
            int m = index % n;
            return m < 0 ? m + n : m;
        }

        public Vector3 EvaluateLookaheadWorld(int startWaypointIndex, Transform splineTransform, float lookaheadMetres)
        {
            if (pointsLocal.Count == 0)
            {
                return splineTransform.position;
            }

            float want = Mathf.Max(0f, lookaheadMetres);
            int idx = ClampIndex(startWaypointIndex);
            Vector3 worldA = splineTransform.TransformPoint(pointsLocal[idx]);
            if (want < 1e-4f)
            {
                return worldA;
            }

            float remaining = want;
            int safety = Mathf.Max(pointsLocal.Count * 2, 8);
            while (remaining > 1e-4f && safety-- > 0)
            {
                int nextIdx = NextWaypointIndex(idx);
                Vector3 worldB = splineTransform.TransformPoint(pointsLocal[nextIdx]);
                float seg = HorizontalDistance(worldA, worldB);
                if (seg < 1e-4f)
                {
                    idx = nextIdx;
                    worldA = worldB;
                    continue;
                }

                if (remaining <= seg)
                {
                    float t = remaining / seg;
                    return Vector3.Lerp(worldA, worldB, t);
                }

                remaining -= seg;
                idx = nextIdx;
                worldA = worldB;
            }

            return worldA;
        }

        public int NextWaypointIndex(int index)
        {
            if (!isClosed)
            {
                return Mathf.Min(index + 1, pointsLocal.Count - 1);
            }

            int n = pointsLocal.Count;
            if (n <= 1)
            {
                return 0;
            }

            return (index + 1) % n;
        }

        public float HorizontalDistanceToWaypoint(Vector3 worldPosition, int waypointIndex, Transform splineTransform)
        {
            Vector3 w = GetWorldPoint(waypointIndex, splineTransform);
            Vector3 a = worldPosition;
            Vector3 b = w;
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
