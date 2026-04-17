using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.Cards
{
    /// <summary>sample: Local stub — spends gold via callback, rolls random card from catalog, assigns slot. Replace with Cloud Code / backend when ready.</summary>
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
            if (inventory == null)
            {
                error = "Inventory is null.";
                return false;
            }

            error = null;
            return TryPurchaseAfterInventoryValidated(inventory, slotIndex, rng, out error);
        }

        private bool TryPurchaseAfterInventoryValidated(PlayerCardInventoryState inventory, int slotIndex, System.Random rng, out string error)
        {
            if (!TryGetPurchasableSlot(inventory, slotIndex, out error, out CardSlotSnapshot slot))
            {
                return false;
            }

            if (!TryEvaluateGoldAgainstSlotCost(slotIndex, out long gold, out long cost, out error))
            {
                return false;
            }

            return TryFinalizeRandomCardAndCommit(inventory, slotIndex, slot, rng, gold, cost, out error);
        }

        private bool TryEvaluateGoldAgainstSlotCost(int slotIndex, out long gold, out long cost, out string error)
        {
            cost = CardCostCurve.GoldCostForSlot(slotIndex);
            gold = getGold();
            if (gold < cost)
            {
                error = "Not enough gold.";
                return false;
            }

            error = null;
            return true;
        }

        private bool TryFinalizeRandomCardAndCommit(PlayerCardInventoryState inventory, int slotIndex, CardSlotSnapshot slot, System.Random rng, long gold, long cost, out string error)
        {
            if (!TryPickRandomCard(rng, out CardDefinition pick, out error))
            {
                return false;
            }

            CommitPurchase(inventory, slotIndex, slot, gold, cost, pick);
            return true;
        }

        private bool TryGetPurchasableSlot(PlayerCardInventoryState inventory, int slotIndex, out string error, out CardSlotSnapshot slot)
        {
            if (slotIndex < 0 || slotIndex >= inventory.Slots.Count)
            {
                error = "Invalid slot index.";
                slot = default;
                return false;
            }

            slot = inventory.Slots[slotIndex];
            if (slot.State != CardSlotState.Uncollected)
            {
                error = "Slot is not available for purchase.";
                return false;
            }

            error = null;
            return true;
        }

        private bool TryPickRandomCard(System.Random rng, out CardDefinition pick, out string error)
        {
            IReadOnlyList<CardDefinition> pool = catalog.GetRollPool();
            if (pool == null || pool.Count == 0)
            {
                pick = null;
                error = "Card catalog is empty.";
                return false;
            }

            pick = pool[rng.Next(0, pool.Count)];
            if (pick == null || string.IsNullOrEmpty(pick.Id))
            {
                error = "Rolled invalid card.";
                return false;
            }

            error = null;
            return true;
        }

        private void CommitPurchase(PlayerCardInventoryState inventory, int slotIndex, CardSlotSnapshot slot, long goldBefore, long cost, CardDefinition pick)
        {
            setGold(goldBefore - cost);
            slot.State = CardSlotState.Collected;
            slot.CardId = pick.Id;
            inventory.Slots[slotIndex] = slot;
        }
    }
}
