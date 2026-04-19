using System;
using VContainer.Unity;

namespace GearEngine.LayeredScope.Internal
{
    internal sealed class LayerEntry
    {
        public IScopeLayer Layer { get; }
        public LifetimeScope Scope { get; }
        public IAsyncInitializable[] OwnedInitializables { get; }
        public IAsyncDisposable[] OwnedDisposables { get; }

        public LayerEntry(
            IScopeLayer layer,
            LifetimeScope scope,
            IAsyncInitializable[] inits,
            IAsyncDisposable[] disposables)
        {
            Layer = layer;
            Scope = scope;
            OwnedInitializables = inits;
            OwnedDisposables = disposables;
        }

        public static LayerEntry Root(LifetimeScope root) =>
            new(null, root, Array.Empty<IAsyncInitializable>(), Array.Empty<IAsyncDisposable>());
    }
}
