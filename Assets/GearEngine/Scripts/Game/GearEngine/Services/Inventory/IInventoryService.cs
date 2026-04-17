using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Services.Inventory
{
    public interface IInventoryService
    {
        InventoryModel Model { get; }
        int CurrentCount { get; }
        int MaxSlots { get; }
        bool CanPerformActions { get; }

        void Initialize(int maxSlots, IReadOnlyList<GearConfig> inventoryGears, Func<bool> canPerformActionsDelegate = null);
        void LoadInventory(IEnumerable<IItem> items);
        void AddItem(IItem item);
        void ConsumeSpecificItem(IItem item);
        bool TryConsumeSelectedItem();
        void SelectItemLocal(IItem item);
    }
}
