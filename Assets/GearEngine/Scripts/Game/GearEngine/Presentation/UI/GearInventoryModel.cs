using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class GearInventoryModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<GearConfigData> availableGears = new ObservableCollection<GearConfigData>();

        [ObservableProperty]
        private GearConfigData selectedGear;
    }
}
