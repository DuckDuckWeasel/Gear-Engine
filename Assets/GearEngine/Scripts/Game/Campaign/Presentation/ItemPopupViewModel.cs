using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ItemPopupViewModel : ViewModel
    {
        private readonly IReadOnlyList<ItemSlotViewModel> itemsList;
        private readonly Func<string, Task<bool>> onBurn;
        private int currentIndex;

        public ItemPopupViewModel(IReadOnlyList<ItemSlotViewModel> itemsList, int initialIndex, Func<string, Task<bool>> onBurn)
        {
            this.itemsList = itemsList ?? throw new ArgumentNullException(nameof(itemsList));
            this.onBurn = onBurn;
            this.currentIndex = initialIndex;

            if (this.itemsList.Count > 0)
            {
                CurrentItem = this.itemsList[currentIndex];
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            if (CurrentItem != null)
            {
                CurrentItem = new ItemSlotViewModel(this.itemsList[currentIndex].Item, _ => { }, this.itemsList[currentIndex].Amount);
                BindChildViewModel(CurrentItem);
            }
        }

        public ItemSlotViewModel CurrentItem { get; private set; }

        public bool HasMultipleItems => itemsList != null && itemsList.Count > 1;

        public bool CanBurn
        {
            get
            {
                if (itemsList.Count == 0 || CurrentItem == null || CurrentItem.Item == null) return false;
                return CurrentItem.Amount > 1;
            }
        }

        public async void Burn()
        {
            if (!CanBurn) return;
            
            string id = CurrentItem.Item.Id;
            bool success = false;
            if (onBurn != null)
            {
                success = await onBurn.Invoke(id);
            }
            
            if (success)
            {
                RefreshCurrentItem(id);
            }
        }
        
        private void RefreshCurrentItem(string expectedId)
        {
            int newIndex = -1;
            for (int i = 0; i < itemsList.Count; i++)
            {
                if (itemsList[i].Item.Id == expectedId)
                {
                    newIndex = i;
                    break;
                }
            }
            
            if (newIndex >= 0)
            {
                currentIndex = newIndex;
                UpdateCurrentItem();
            }
            else
            {
                Close();
            }
        }

        public void Next()
        {
            if (itemsList.Count == 0) return;

            currentIndex = (currentIndex + 1) % itemsList.Count;
            UpdateCurrentItem();
        }

        public void Previous()
        {
            if (itemsList.Count == 0) return;

            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = itemsList.Count - 1;
            }
            UpdateCurrentItem();
        }

        public void Close()
        {
            navigation.Return();
        }

        private void UpdateCurrentItem()
        {
            CurrentItem = new ItemSlotViewModel(itemsList[currentIndex].Item, _ => { }, itemsList[currentIndex].Amount);
            BindChildViewModel(CurrentItem);
            OnPropertyChanged(nameof(CurrentItem));
            OnPropertyChanged(nameof(CanBurn));
            OnPropertyChanged(nameof(HasMultipleItems));
        }
    }
}
