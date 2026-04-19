using System;
using VContainer;

namespace GearEngine.LayeredScope.Internal
{
    internal sealed class LayerResolverProxy : ILayerResolver
    {
        private IObjectResolver top;

        internal void Bind(IObjectResolver newTop) => top = newTop;

        public IObjectResolver Top =>
            top ?? throw new InvalidOperationException(
                "[LayeredScope] LayerResolverProxy not bound. Did you create an ApplicationHost?");

        public bool TryResolve<T>(out T value) => Top.TryResolve(out value);
        public T Resolve<T>() => Top.Resolve<T>();
    }
}
