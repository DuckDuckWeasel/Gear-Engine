using VContainer;

namespace GearEngine.LayeredScope
{
    public interface IScopeLayer
    {
        string Name => this.GetType().Name;

        void Install(IContainerBuilder builder);
    }
}
