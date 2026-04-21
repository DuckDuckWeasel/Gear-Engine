using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.GearEngine.Services
{
    /// <summary>
    /// Player-owned gear (templates) persisted via LiveOps. Per-tick simulation state lives on <see cref="Nodes.IGridNode"/>, not here.
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>Catalog id for the motor (Core) gear from LiveOps <c>InventoryConfig</c>; empty when not using LiveOps inventory.</summary>
        string MotorCogGearId { get; }

        /// <summary>Grid cell from LiveOps <c>InventoryConfig</c> where the motor cog is placed when missing from loadout.</summary>
        Vector2Int MotorCogStartCell { get; }

        bool HasSavedInventory { get; }

        IReadOnlyList<OwnedGear> Owned { get; }

        event Action InventoryChanged;

        /// <summary>Mints a new InstanceId, adds the OwnedGear, schedules persistence in background. Returns the new ref.</summary>
        OwnedGear Add(GearConfig gear);

        /// <summary>Removes by reference equality, schedules persistence in background.</summary>
        bool Remove(OwnedGear gear);

        /// <summary>Clears all owned gear and persists an empty inventory in one request (LiveOps implementations).</summary>
        void Clear();
    }

    /// <summary>Runtime handle for one owned gear instance; <see cref="InstanceId"/> is client-minted (GUID).</summary>
    public sealed class OwnedGear
    {
        public string InstanceId { get; set; }

        public GearConfig Config { get; set; }
    }
}
