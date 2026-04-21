using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Config;

namespace GearEngine.GearEngine.Services
{
    /// <summary>
    /// Player-owned gear (templates) persisted via LiveOps. Per-tick simulation state lives on <see cref="Nodes.IGridNode"/>, not here.
    /// </summary>
    public interface IInventoryService
    {
        bool HasSavedInventory { get; }

        IReadOnlyList<GearConfig> Owned { get; }

        event Action InventoryChanged;

        bool TryAdd(GearConfig gear);

        bool TryRemove(GearConfig gear);
    }
}
