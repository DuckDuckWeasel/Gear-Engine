namespace GearEngine.GearEngine.Presentation.World
{
    /// <summary>
    /// Determines how a world-space object is scaled to fill its configured
    /// frustum target box (frustumWidth * fillX, frustumHeight * fillY).
    /// Analogous to CSS object-fit values.
    /// </summary>
    public enum FrustumFillMode
    {
        /// <summary>Each axis scales independently to hit the exact fill percentages. Aspect ratio is not preserved.</summary>
        Stretch,

        /// <summary>Uniform scale. Fits entirely inside the target box; may leave empty space on two sides. (CSS contain)</summary>
        Fit,

        /// <summary>Uniform scale. Covers the entire target box; may exceed one axis. (CSS cover)</summary>
        Fill,

        /// <summary>Matches the target width exactly. Height scales proportionally to preserve aspect ratio.</summary>
        FillWidth,

        /// <summary>Matches the target height exactly. Width scales proportionally to preserve aspect ratio.</summary>
        FillHeight,
    }
}
