namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>
    /// Holds the shared <see cref="IDragService"/> for all <see cref="Draggable"/> instances.
    /// Any scene or flow that can show draggable UI must call <see cref="Register"/> with that scene's
    /// <see cref="GearEngine.GearEngine.IDragService"/> from the root view's bind/bootstrap before the user drags
    /// (e.g. <see cref="GearEngine.GearEngine.Presentation.GearEngineCoreViewComponent"/>; campaign flows should register similarly).
    /// </summary>
    public static class DragServiceRegistry
    {
        public static IDragService Instance { get; private set; }

        public static void Register(IDragService service)
        {
            Instance = service;
        }
    }
}
