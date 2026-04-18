using System;
using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// sample: Placement from a UI rect and frustum fit; tween toward these values or call <see cref="FrustumFitAnchor.Apply"/>.
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

        /// <summary>sample: Full <see cref="Transform.localScale"/> for the target (two axes from fit; third from baseline).</summary>
        public Vector3 LocalScale { get; }

        /// <summary>sample: When true, apply <see cref="WorldRotation"/> in world space.</summary>
        public bool HasWorldRotation { get; }

        public Quaternion WorldRotation { get; }

        public void ApplyTo(Transform targetTransform)
        {
            if (targetTransform == null)
            {
                throw new ArgumentNullException(nameof(targetTransform));
            }

            targetTransform.position = WorldPosition;
            targetTransform.localScale = LocalScale;
            if (HasWorldRotation)
            {
                targetTransform.rotation = WorldRotation;
            }
        }
    }
}
