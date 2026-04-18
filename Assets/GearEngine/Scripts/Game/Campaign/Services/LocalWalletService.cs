using System;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    public sealed class LocalWalletService : IWalletService
    {
        private readonly WalletModel wallet = new WalletModel();

        public LocalWalletService(int initialGold = 0)
        {
            if (initialGold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialGold));
            }

            wallet.Gold = initialGold;
        }

        public WalletModel GetWallet() => wallet;

        public void AddGold(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            wallet.Gold += amount;
            Debug.Log($"[LocalWalletService] +{amount} gold → total: {wallet.Gold} (stub, not persisted).");
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0)
            {
                return false;
            }

            if (amount > wallet.Gold)
            {
                return false;
            }

            wallet.Gold -= amount;
            Debug.Log($"[LocalWalletService] -{amount} gold → remaining: {wallet.Gold} (stub, not persisted).");
            return true;
        }
    }
}
