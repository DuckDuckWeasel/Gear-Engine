using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// Bounds strategy that encapsulates the world-space bounds of every
    /// <see cref="Collider"/> found in the target's full child hierarchy (including the
    /// target itself). Colliders whose world bounds are degenerate (zero volume) are
    /// skipped so that disabled or placeholder objects do not distort the result.
    /// </summary>
    public static class CombineChildCollidersStrategy
    {
        /// <summary>
        /// Tries to compute the effective mesh size for <paramref name="target"/> by
        /// encapsulating all valid child collider bounds. The result is the combined
        /// world-space bounds divided component-wise by <paramref name="target"/>'s
        /// lossyScale, giving the extent per unit of target scale. Returns <c>false</c>
        /// when no <see cref="Collider"/> with non-degenerate bounds is found.
        /// </summary>
        public static bool TryGetEffectiveMeshSize(Transform target, out Vector3 meshSize)
        {
            if (target == null)
            {
                meshSize = default;
                return false;
            }

            Collider[] colliders = target.GetComponentsInChildren<Collider>(true);

            Bounds combined = default;
            bool foundAny = false;

            for (int i = 0; i < colliders.Length; i++)
            {
                Bounds b = colliders[i].bounds;
                if (b.size.sqrMagnitude <= 0f)
                {
                    continue;
                }

                if (!foundAny)
                {
                    combined = b;
                    foundAny = true;
                }
                else
                {
                    combined.Encapsulate(b);
                }
            }

            if (!foundAny)
            {
                meshSize = default;
                return false;
            }

            meshSize = FrustumFitBoundsResolver.DivideComponents(combined.size, target.lossyScale);
            return true;
        }
    }
}
