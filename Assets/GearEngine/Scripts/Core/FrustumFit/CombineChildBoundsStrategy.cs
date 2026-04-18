using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// Bounds strategy that encapsulates the world-space bounds of every
    /// <see cref="Renderer"/> found in the target's full child hierarchy (including the
    /// target itself). Renderers whose world bounds are degenerate (zero volume) are
    /// skipped so that disabled or placeholder objects do not distort the result.
    /// </summary>
    public static class CombineChildBoundsStrategy
    {
        /// <summary>
        /// Tries to compute the effective mesh size for <paramref name="target"/> by
        /// encapsulating all valid child renderer bounds. The result is the combined
        /// world-space bounds divided component-wise by <paramref name="target"/>'s
        /// lossyScale, giving the mesh extent per unit of target scale. Returns
        /// <c>false</c> when no <see cref="Renderer"/> with non-degenerate bounds is found.
        /// </summary>
        public static bool TryGetEffectiveMeshSize(Transform target, out Vector3 meshSize)
        {
            if (target == null)
            {
                meshSize = default;
                return false;
            }

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);

            Bounds combined = default;
            bool foundAny = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                Bounds b = renderers[i].bounds;
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
