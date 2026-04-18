using System;
using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// Dispatches to the appropriate bounds strategy and exposes the shared
    /// component-wise division helper used by all strategies.
    /// </summary>
    public static class FrustumFitBoundsResolver
    {
        /// <summary>
        /// Resolves the effective mesh size for <paramref name="target"/> using
        /// <paramref name="mode"/>. The result is in "units per unit of target
        /// lossyScale": world-space bounds divided component-wise by
        /// <c>target.lossyScale</c>, so it scales linearly when
        /// <c>targetTransform.localScale</c> changes.
        /// </summary>
        public static bool TryResolve(FrustumFitBoundsMode mode, Transform target, out Vector3 effectiveMeshSize)
        {
            return mode switch
            {
                FrustumFitBoundsMode.DirectRenderer      => DirectRendererBoundsStrategy.TryGetEffectiveMeshSize(target, out effectiveMeshSize),
                FrustumFitBoundsMode.CombineChildBounds  => CombineChildBoundsStrategy.TryGetEffectiveMeshSize(target, out effectiveMeshSize),
                FrustumFitBoundsMode.DirectCollider      => DirectColliderBoundsStrategy.TryGetEffectiveMeshSize(target, out effectiveMeshSize),
                FrustumFitBoundsMode.CombineChildColliders => CombineChildCollidersStrategy.TryGetEffectiveMeshSize(target, out effectiveMeshSize),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
            };
        }

        /// <summary>
        /// Component-wise <c>a / b</c>. Returns 0 for any component of <paramref name="b"/>
        /// that is (approximately) zero to avoid divide-by-zero.
        /// </summary>
        internal static Vector3 DivideComponents(Vector3 a, Vector3 b) =>
            new Vector3(
                Mathf.Approximately(b.x, 0f) ? 0f : a.x / b.x,
                Mathf.Approximately(b.y, 0f) ? 0f : a.y / b.y,
                Mathf.Approximately(b.z, 0f) ? 0f : a.z / b.z
            );
    }
}
