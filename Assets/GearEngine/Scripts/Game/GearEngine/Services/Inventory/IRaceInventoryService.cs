using System;

namespace GearEngine.GearEngine.Services.Inventory
{
    public interface IRaceInventoryService
    {
        InventoryModel GetInventory();

        bool TryAdd(IItem item);

        bool TryConsume(IItem item);

        event Action ItemsChanged;
    }
}
