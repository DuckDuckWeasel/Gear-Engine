using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;

namespace GearEngine.GearEngine.Services.Inventory
{
    public partial class InventoryModel : Model
    {
        [ObservableProperty]
        private ObservableCollection<IItem> items = new ObservableCollection<IItem>();

        [ObservableProperty]
        private int maxSlots;
    }
}
