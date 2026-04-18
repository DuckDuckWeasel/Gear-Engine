using System;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    public sealed class LocalWalletService : IWalletService
    {
        public LocalWalletService(int initialGold = 0)
        {
            if (initialGold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialGold));
            }

            currentGold = initialGold;
        }

        public int CurrentGold => currentGold;

        private int currentGold;

        public void AddGold(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            currentGold += amount;
            Debug.Log($"[LocalWalletService] +{amount} gold → total: {currentGold} (stub, not persisted).");
        }

        public void SpendGold(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (amount > currentGold)
            {
                throw new InvalidOperationException(
                    $"[LocalWalletService] Cannot spend {amount} gold — only {currentGold} available.");
            }

            currentGold -= amount;
            Debug.Log($"[LocalWalletService] -{amount} gold → remaining: {currentGold} (stub, not persisted).");
        }
    }
}
