using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        public InventoryModel Model { get; } = new InventoryModel();

        public int CurrentCount => Model.AvailableItems.Count;
        public int MaxSlots { get; private set; } = int.MaxValue;

        public void Initialize(int maxSlots, IReadOnlyList<GearConfig> inventoryGears)
        {
            MaxSlots = maxSlots;

            inventoryGears ??= Array.Empty<GearConfig>();

            var runtimeGears = new List<IItem>();
            foreach (GearConfig config in inventoryGears)
            {
                if (config != null)
                {
                    runtimeGears.Add(config.CreateRuntimeData());
                }
            }

            LoadInventory(runtimeGears);
        }

        public void LoadInventory(IEnumerable<IItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (IItem item in items)
            {
                if (item == null)
                {
                    continue;
                }

                AddItem(item);
            }
        }

        public void AddItem(IItem item)
        {
            if (item == null)
            {
                return;
            }

            if (Model.AvailableItems.Count >= MaxSlots)
            {
                Debug.LogWarning($"[InventoryService] Inventory full ({Model.AvailableItems.Count}/{MaxSlots}). Cannot add item '{item.Id}'.");
                return;
            }

            Model.AvailableItems.Add(item);
        }

        public void ConsumeSpecificItem(IItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            int index = FindItemIndex(item);
            if (index < 0)
            {
                Debug.LogError("[InventoryService] ConsumeSpecificItem: item not found in inventory.");
                return;
            }

            RemoveItemAt(index);
        }

        private int FindItemIndex(IItem item)
        {
            for (int i = 0; i < Model.AvailableItems.Count; i++)
            {
                if (ReferenceEquals(Model.AvailableItems[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        private void RemoveItemAt(int index)
        {
            IItem removed = Model.AvailableItems[index];
            Model.AvailableItems.RemoveAt(index);

            if (ReferenceEquals(Model.SelectedItem, removed))
            {
                Model.SelectedItem = null;
            }
        }
    }
}
