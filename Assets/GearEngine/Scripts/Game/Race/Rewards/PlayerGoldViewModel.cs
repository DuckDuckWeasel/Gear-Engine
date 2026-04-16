using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;

namespace GearEngine.Race.Rewards
{
    public sealed partial class PlayerGoldViewModel : ViewModel
    {
        [ObservableProperty]
        private int gold;
    }
}
