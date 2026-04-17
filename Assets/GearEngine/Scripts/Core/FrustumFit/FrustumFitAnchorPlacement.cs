using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// World position, local scale, and optional world rotation computed from a UI rect and frustum fit.
    /// Use with tweens by interpolating toward these values instead of calling <see cref="FrustumFitAnchor.Apply"/>.
    /// </summary>
    public readonly struct FrustumFitAnchorPlacement
    {
        public FrustumFitAnchorPlacement(Vector3 worldPosition, Vector3 localScale, bool hasWorldRotation, Quaternion worldRotation)
        {
            WorldPosition = worldPosition;
            LocalScale = localScale;
            HasWorldRotation = hasWorldRotation;
            WorldRotation = worldRotation;
        }

        public Vector3 WorldPosition { get; }

        /// <summary>Full <see cref="Transform.localScale"/> for the target (two axes from fit math; third axis preserved from the baseline passed into compute).</summary>
        public Vector3 LocalScale { get; }

        /// <summary>When true, apply <see cref="WorldRotation"/> to the target (world space).</summary>
        public bool HasWorldRotation { get; }

        public Quaternion WorldRotation { get; }
    }
}
