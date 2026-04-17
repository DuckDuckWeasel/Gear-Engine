using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Services.Inventory
{
    public interface IInventoryService
    {
        InventoryModel Model { get; }
        int CurrentCount { get; }
        int MaxSlots { get; }

        void Initialize(int maxSlots, IReadOnlyList<GearConfig> inventoryGears);
        void LoadInventory(IEnumerable<IItem> items);
        void AddItem(IItem item);
        void ConsumeSpecificItem(IItem item);
    }
}
