namespace GearEngine.Campaign.Services
{
    public interface IWalletService
    {
        WalletModel GetWallet();

        void AddGold(int amount);

        bool TrySpendGold(int amount);
    }
}
