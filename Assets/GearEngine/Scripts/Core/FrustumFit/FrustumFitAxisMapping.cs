using System;
using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// Maps <see cref="FrustumFitAxes"/> to mesh/parent vectors and applies 2D scale pairs to <see cref="Transform.localScale"/>.
    /// </summary>
    public static class FrustumFitAxisMapping
    {
        public static Vector2 ExtractAxesPair(Vector3 v, FrustumFitAxes axes)
        {
            return axes switch
            {
                FrustumFitAxes.XY => new Vector2(v.x, v.y),
                FrustumFitAxes.XZ => new Vector2(v.x, v.z),
                FrustumFitAxes.YZ => new Vector2(v.y, v.z),
                _ => throw new ArgumentOutOfRangeException(nameof(axes), axes, null),
            };
        }

        /// <summary>
        /// Merges a 2D scale pair into a full local scale vector, leaving the third axis unchanged from <paramref name="baselineLocalScale"/>.
        /// </summary>
        public static Vector3 MergeLocalScaleAxes(Vector3 baselineLocalScale, Vector2 scale2D, FrustumFitAxes axes)
        {
            Vector3 ls = baselineLocalScale;
            if (!TryWriteAxisPairToLocalScale(ref ls, scale2D, axes))
            {
                throw new ArgumentOutOfRangeException(nameof(axes), axes, null);
            }

            return ls;
        }

        public static void WriteLocalScaleAxes(Transform transform, Vector2 scale2D, FrustumFitAxes axes)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            transform.localScale = MergeLocalScaleAxes(transform.localScale, scale2D, axes);
        }

        private static bool TryWriteAxisPairToLocalScale(ref Vector3 ls, Vector2 scale2D, FrustumFitAxes axes)
        {
            if (axes == FrustumFitAxes.XY)
            {
                ls.x = scale2D.x;
                ls.y = scale2D.y;
                return true;
            }

            if (axes == FrustumFitAxes.XZ)
            {
                ls.x = scale2D.x;
                ls.z = scale2D.y;
                return true;
            }

            if (axes == FrustumFitAxes.YZ)
            {
                ls.y = scale2D.x;
                ls.z = scale2D.y;
                return true;
            }

            return false;
        }
    }
}
