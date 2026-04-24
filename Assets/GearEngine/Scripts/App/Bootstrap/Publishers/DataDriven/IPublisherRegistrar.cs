using VContainer;

namespace GearEngine.App.Bootstrap.Publishers.DataDriven
{
    // todo: Edit-time baked registration for one addressable SO publisher.
    public interface IPublisherRegistrar
    {
        void Register(IContainerBuilder builder);
    }
}
