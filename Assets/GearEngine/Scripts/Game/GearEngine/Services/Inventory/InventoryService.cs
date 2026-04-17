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
        
        public bool CanPerformActions => canPerformActionsDelegate == null || canPerformActionsDelegate.Invoke();

        private Func<bool> canPerformActionsDelegate;

        public void Initialize(int maxSlots, IReadOnlyList<GearConfig> inventoryGears, Func<bool> canPerformActionsDelegate = null)
        {
            MaxSlots = maxSlots;
            this.canPerformActionsDelegate = canPerformActionsDelegate;

            var runtimeGears = new System.Collections.Generic.List<IItem>();
            foreach (var config in inventoryGears)
            {
                if (config != null) runtimeGears.Add(config.CreateRuntimeData());
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
                if (item == null) continue;
                AddItem(item);
            }
        }

        public void AddItem(IItem item)
        {
            if (item == null) return;

            if (Model.AvailableItems.Count >= MaxSlots)
            {
                Debug.LogWarning($"[InventoryService] Inventory full ({Model.AvailableItems.Count}/{MaxSlots}). Cannot add item '{item.Id}'.");
                return;
            }

            Model.AvailableItems.Add(item);
        }

        public bool TryConsumeSelectedItem()
        {
            if (Model.SelectedItem == null)
            {
                return false;
            }

            bool success = Model.AvailableItems.Remove(Model.SelectedItem);
            if (success)
            {
                Model.SelectedItem = null;
            }

            return success;
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

        public void SelectItemLocal(IItem item)
        {
            if (Model.AvailableItems.Contains(item))
            {
                Model.SelectedItem = item;
                Debug.Log($"<color=#aaaaff>[InventoryService]</color> Player selected item: {item.Id}");
            }
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
