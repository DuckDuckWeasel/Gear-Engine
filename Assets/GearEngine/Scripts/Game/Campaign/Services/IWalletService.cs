namespace GearEngine.Campaign.Services
{
    public interface IWalletService
    {
        int CurrentGold { get; }
        void AddGold(int amount);
        void SpendGold(int amount);
    }
}
