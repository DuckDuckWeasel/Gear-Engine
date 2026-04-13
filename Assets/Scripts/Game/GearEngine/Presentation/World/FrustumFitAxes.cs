namespace Scaffold.GearEngine.Presentation.World
{
    /// <summary>
    /// Selects which two local-space axes of the target object map to
    /// screen horizontal (primary) and screen vertical (secondary).
    ///
    /// Use XY for Unity Quads and sprites (default, mesh lies on XY plane).
    /// Use XZ for Unity Plane primitives rotated 90° on X to face the camera
    ///         (mesh lies on XZ plane; local Z maps to screen vertical after rotation).
    /// Use YZ for meshes whose screen-facing axes are local Y and Z.
    /// </summary>
    public enum FrustumFitAxes
    {
        /// <summary>Local X = screen horizontal, local Y = screen vertical. Default for Quad/sprite meshes.</summary>
        XY,

        /// <summary>Local X = screen horizontal, local Z = screen vertical. Use for Unity Plane primitives facing the camera.</summary>
        XZ,

        /// <summary>Local Y = screen horizontal, local Z = screen vertical.</summary>
        YZ,
    }
}
