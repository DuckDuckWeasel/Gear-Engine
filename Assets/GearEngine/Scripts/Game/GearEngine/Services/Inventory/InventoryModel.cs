using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GearEngine.GearEngine.Services.Inventory
{
    public sealed partial class InventoryModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<IItem> availableItems = new ObservableCollection<IItem>();

        [ObservableProperty]
        private IItem selectedItem;
    }
}
