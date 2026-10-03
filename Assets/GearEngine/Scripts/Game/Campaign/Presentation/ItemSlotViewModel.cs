using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GearEngine.Campaign.Presentation
{
    public sealed partial class ItemSlotViewModel : ViewModel
    {
        private readonly Action<ItemSlotViewModel> onPick;

        public ItemSlotViewModel(IItem item, Action<ItemSlotViewModel> onPick, int amount = 1)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            this.onPick = onPick;
            Amount = amount;
        }

        public IItem Item { get; }
        public int Amount { get; }
        public float IconScale => Item is GearItemData gear ? gear.UIIconScaleMultiplier : 1f;

        public bool IsOwned => Amount > 0;

        [ObservableProperty]
        private bool canPick = true;

        internal void Pick()
        {
            onPick?.Invoke(this);
        }
    }
}
