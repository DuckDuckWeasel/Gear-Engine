using VContainer;

namespace GearEngine.LayeredScope
{
    public interface ILayerResolver
    {
        IObjectResolver Top { get; }
        bool TryResolve<T>(out T value);
        T Resolve<T>();
    }
}
