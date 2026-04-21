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

        public IReadOnlyList<OwnedGear> Owned => Array.Empty<OwnedGear>();

        public event Action InventoryChanged;

        public OwnedGear Add(GearConfig gear)
        {
            if (gear == null)
            {
                return null;
            }

            return new OwnedGear { InstanceId = Guid.NewGuid().ToString("N"), Config = gear };
        }

        public bool Remove(OwnedGear gear) => false;

        public void Clear()
        {
        }
    }
}
