using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;

namespace GearEngine.Campaign.Services
{
    public partial class WalletModel : Model
    {
        [ObservableProperty]
        private int gold;
    }
}
