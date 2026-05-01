using System;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GearEngine.Campaign.Presentation
{
    public sealed partial class ItemPerkViewModel : ViewModel
    {
        private readonly Action<ItemPerkViewModel> onPick;

        public ItemPerkViewModel(IItem item, Action<ItemPerkViewModel> onPick, int amount = 1)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            this.onPick = onPick;
            Amount = amount;
        }

        public IItem Item { get; }
        public int Amount { get; }

        [ObservableProperty]
        private bool canPick = true;

        internal void Pick()
        {
            onPick?.Invoke(this);
        }
    }
}
