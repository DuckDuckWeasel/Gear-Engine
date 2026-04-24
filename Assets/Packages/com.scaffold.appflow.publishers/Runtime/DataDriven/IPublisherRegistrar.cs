using VContainer;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    /// <summary>Edit-time baked registration for one Addressables-backed ScriptableObject publisher.</summary>
    public interface IPublisherRegistrar
    {
        void Register(IContainerBuilder builder);
    }
}
