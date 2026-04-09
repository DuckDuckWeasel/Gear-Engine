using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Game.GearEngine.Presentation
{
    public partial class GearInventoryModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<GearConfigData> availableGears = new ObservableCollection<GearConfigData>();

        [ObservableProperty]
        private GearConfigData selectedGear;
    }
}
