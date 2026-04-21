using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Config;

namespace GearEngine.GearEngine.Services
{
    /// <summary>
    /// No-op inventory for scopes without LiveOps (e.g. isolated race or gear test scenes). Register before <see cref="Bootstrap.GearMechanicsInstaller"/>.
    /// </summary>
    public sealed class EmptyInventoryService : IInventoryService
    {
        public bool HasSavedInventory => false;

        public IReadOnlyList<GearConfig> Owned => Array.Empty<GearConfig>();

        public event Action InventoryChanged;

        public bool TryAdd(GearConfig gear) => gear != null;

        public bool TryRemove(GearConfig gear) => false;

        public void Clear()
        {
        }
    }
}
