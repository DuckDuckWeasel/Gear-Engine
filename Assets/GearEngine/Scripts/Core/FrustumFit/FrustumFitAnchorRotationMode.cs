namespace GearEngine.FrustumFit
{
    /// <summary>
    /// How <see cref="FrustumFitAnchor"/> sets rotation when applying or computing placement.
    /// </summary>
    public enum FrustumFitAnchorRotationMode
    {
        /// <summary>Do not change rotation; <see cref="FrustumFitAnchorPlacement.HasWorldRotation"/> is false when computing.</summary>
        PreserveTarget,

        /// <summary>World rotation matches the world camera transform (typical for screen-facing content).</summary>
        MatchCameraRotation,
    }
}
