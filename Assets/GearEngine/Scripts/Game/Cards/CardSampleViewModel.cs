using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.Cards
{
    public sealed partial class CardSampleViewModel : ViewModel
    {
        public CardSampleViewModel(CardCatalogSO catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            purchaseService = new LocalCardSlotPurchaseService(catalog, () => Gold, v => Gold = v);
            SeedSlots();
        }

        public IReadOnlyList<CardSlotSnapshot> Slots => inventory.Slots;
        
        private readonly PlayerCardInventoryState inventory = new PlayerCardInventoryState();
        private readonly LocalCardSlotPurchaseService purchaseService;
        private readonly System.Random rng = new System.Random();

        [ObservableProperty]
        private long gold = 1000;

        [ObservableProperty]
        private int inventoryRevision;

        public void TryPurchaseSlot(int slotIndex)
        {
            try
            {
                if (!purchaseService.TryPurchaseSlot(inventory, slotIndex, rng, out string error))
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError($"[CardSampleViewModel] Purchase failed: {error}");
                    }

                    return;
                }

                InventoryRevision++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardSampleViewModel] TryPurchaseSlot failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void SeedSlots()
        {
            inventory.Slots.Clear();
            for (var i = 0; i < 3; i++)
            {
                inventory.Slots.Add(new CardSlotSnapshot
                {
                    SlotIndex = i,
                    State = CardSlotState.Uncollected,
                    CardId = null,
                });
            }

            InventoryRevision++;
        }
    }
}
