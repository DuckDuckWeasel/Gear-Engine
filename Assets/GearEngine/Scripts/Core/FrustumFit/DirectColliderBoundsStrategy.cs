using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// Bounds strategy that finds the first <see cref="Collider"/> in the target's
    /// subtree — the target itself first, then its children in depth-first order.
    /// Parents are never searched. Fails if no Collider exists anywhere in the subtree.
    /// </summary>
    public static class DirectColliderBoundsStrategy
    {
        /// <summary>
        /// Tries to compute the effective mesh size for <paramref name="target"/>.
        /// The result is the collider's world-space bounds divided component-wise by
        /// <paramref name="target"/>'s lossyScale, giving the extent per unit of
        /// target scale. Returns <c>false</c> when no <see cref="Collider"/> is found
        /// in the target's subtree.
        /// </summary>
        public static bool TryGetEffectiveMeshSize(Transform target, out Vector3 meshSize)
        {
            if (target == null)
            {
                meshSize = default;
                return false;
            }

            Collider c = target.GetComponentInChildren<Collider>(true);
            if (c == null)
            {
                meshSize = default;
                return false;
            }

            meshSize = FrustumFitBoundsResolver.DivideComponents(c.bounds.size, target.lossyScale);
            return true;
        }
    }
}
