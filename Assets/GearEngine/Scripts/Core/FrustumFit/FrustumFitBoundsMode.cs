namespace GearEngine.FrustumFit
{
    /// <summary>
    /// Determines how <see cref="FrustumFitAnchor"/> discovers the bounds it uses to
    /// compute the scale that makes the target fill the configured UI region.
    /// </summary>
    public enum FrustumFitBoundsMode
    {
        /// <summary>
        /// Finds the first <see cref="UnityEngine.Renderer"/> in the target's subtree
        /// (target itself first, then children in depth-first order). Parents are never
        /// searched. Fails if no Renderer exists anywhere in the subtree.
        /// </summary>
        DirectRenderer,

        /// <summary>
        /// Encapsulates the world-space bounds of every
        /// <see cref="UnityEngine.Renderer"/> found in the target's full child
        /// hierarchy (including the target itself). Use this when the target is a
        /// logical root whose visual extent is defined by many child renderers
        /// (e.g. a grid board made of individual tile sprites).
        /// </summary>
        CombineChildBounds,

        /// <summary>
        /// Finds the first <see cref="UnityEngine.Collider"/> in the target's subtree
        /// (target itself first, then children in depth-first order). Parents are never
        /// searched. Useful for objects whose logical extent is defined by a physics
        /// collider rather than a visual renderer (e.g. a board root with a BoxCollider).
        /// </summary>
        DirectCollider,

        /// <summary>
        /// Encapsulates the world-space bounds of every
        /// <see cref="UnityEngine.Collider"/> found in the target's full child
        /// hierarchy (including the target itself). Use this when the target has a
        /// compound collider setup that collectively defines its spatial extent.
        /// </summary>
        CombineChildColliders,
    }
}
