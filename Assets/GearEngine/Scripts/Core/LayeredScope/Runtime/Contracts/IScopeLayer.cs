using VContainer;

namespace GearEngine.LayeredScope
{
    public interface IScopeLayer
    {
        string Name { get; }
        void Install(IContainerBuilder builder);
    }
}
