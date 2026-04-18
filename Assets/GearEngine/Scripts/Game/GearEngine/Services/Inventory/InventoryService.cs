using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Config;
using UnityEngine;
using GearEngine.GearEngine;

namespace GearEngine.GearEngine.Services.Inventory
{
    public sealed class InventoryService : IInventoryService
    {
        private readonly InventoryModel model;

        public InventoryService(GearInventoryLoadoutData loadout)
        {
            loadout ??= GearInventoryLoadoutData.Empty();
            model = new InventoryModel
            {
                MaxSlots = loadout.MaxSlots
            };

            IReadOnlyList<GearConfig> configs = loadout.StartingItems;
            foreach (GearConfig config in configs)
            {
                if (config != null)
                {
                    TryAdd(config.CreateRuntimeData());
                }
            }
        }

        public InventoryModel GetInventory() => model;

        public bool TryAdd(IItem item)
        {
            if (item == null)
            {
                return false;
            }

            if (model.Items.Count >= model.MaxSlots)
            {
                Debug.LogWarning($"[InventoryService] Inventory full ({model.Items.Count}/{model.MaxSlots}). Cannot add item '{item.Id}'.");
                return false;
            }

            model.Items.Add(item);
            return true;
        }

        public bool TryConsume(IItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            int index = FindItemIndex(item);
            if (index < 0)
            {
                Debug.LogError("[InventoryService] TryConsume: item not found in inventory.");
                return false;
            }

            model.Items.RemoveAt(index);
            return true;
        }

        private int FindItemIndex(IItem item)
        {
            for (int i = 0; i < model.Items.Count; i++)
            {
                if (ReferenceEquals(model.Items[i], item))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
