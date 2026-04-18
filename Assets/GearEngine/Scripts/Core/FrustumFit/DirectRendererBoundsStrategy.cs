using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// Bounds strategy that finds the first <see cref="Renderer"/> in the target's
    /// subtree — the target itself first, then its children in depth-first order.
    /// Parents are never searched. Fails if no Renderer exists anywhere in the subtree.
    /// </summary>
    public static class DirectRendererBoundsStrategy
    {
        /// <summary>
        /// Tries to compute the effective mesh size for <paramref name="target"/>.
        /// The result is the renderer's world-space bounds divided component-wise by
        /// <paramref name="target"/>'s lossyScale, giving the mesh extent per unit of
        /// target scale. Returns <c>false</c> when no <see cref="Renderer"/> is found
        /// in the target's subtree.
        /// </summary>
        public static bool TryGetEffectiveMeshSize(Transform target, out Vector3 meshSize)
        {
            if (target == null)
            {
                meshSize = default;
                return false;
            }

            Renderer r = target.GetComponentInChildren<Renderer>(true);
            if (r == null)
            {
                meshSize = default;
                return false;
            }

            meshSize = FrustumFitBoundsResolver.DivideComponents(r.bounds.size, target.lossyScale);
            return true;
        }
    }
}
