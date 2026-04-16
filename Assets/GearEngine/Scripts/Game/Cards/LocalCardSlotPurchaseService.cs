using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.Cards
{
    /// <summary>
    /// Local stub: spends gold via callback, rolls random card from catalog, assigns slot.
    /// Replace with Cloud Code / backend when ready.
    /// </summary>
    public sealed class LocalCardSlotPurchaseService
    {
        public LocalCardSlotPurchaseService(CardCatalogSO catalog, Func<long> getGold, Action<long> setGold)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.getGold = getGold ?? throw new ArgumentNullException(nameof(getGold));
            this.setGold = setGold ?? throw new ArgumentNullException(nameof(setGold));
        }

        private readonly CardCatalogSO catalog;
        private readonly Func<long> getGold;
        private readonly Action<long> setGold;

        public bool TryPurchaseSlot(PlayerCardInventoryState inventory, int slotIndex, System.Random rng, out string error)
        {
            error = null;
            if (inventory == null)
            {
                error = "Inventory is null.";
                return false;
            }

            if (slotIndex < 0 || slotIndex >= inventory.slots.Count)
            {
                error = "Invalid slot index.";
                return false;
            }

            CardSlotSnapshot slot = inventory.slots[slotIndex];
            if (slot.state != CardSlotState.Uncollected)
            {
                error = "Slot is not available for purchase.";
                return false;
            }

            long cost = CardCostCurve.GoldCostForSlot(slotIndex);
            long gold = getGold();
            if (gold < cost)
            {
                error = "Not enough gold.";
                return false;
            }

            IReadOnlyList<CardDefinition> pool = catalog.GetRollPool();
            if (pool == null || pool.Count == 0)
            {
                error = "Card catalog is empty.";
                return false;
            }

            CardDefinition pick = pool[rng.Next(0, pool.Count)];
            if (pick == null || string.IsNullOrEmpty(pick.Id))
            {
                error = "Rolled invalid card.";
                return false;
            }

            setGold(gold - cost);
            slot.state = CardSlotState.Collected;
            slot.cardId = pick.Id;
            inventory.slots[slotIndex] = slot;
            return true;
        }
    }
}
