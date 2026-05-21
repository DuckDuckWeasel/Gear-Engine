using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ItemPopupViewModel : ViewModel
    {
        private readonly IReadOnlyList<ItemSlotViewModel> itemsList;
        private readonly Func<string, Task<bool>> onAction;
        private readonly bool requireMultipleForAction;
        private int currentIndex;

        public string ActionName { get; }

        public ItemPopupViewModel(
            IReadOnlyList<ItemSlotViewModel> itemsList, 
            int initialIndex, 
            Func<string, Task<bool>> onAction, 
            string actionName = "Burn", 
            bool requireMultipleForAction = true)
        {
            this.itemsList = itemsList ?? throw new ArgumentNullException(nameof(itemsList));
            this.onAction = onAction;
            this.ActionName = actionName;
            this.requireMultipleForAction = requireMultipleForAction;
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

        public bool CanExecuteAction
        {
            get
            {
                if (onAction == null || itemsList.Count == 0 || CurrentItem == null || CurrentItem.Item == null) return false;
                if (requireMultipleForAction) return CurrentItem.Amount > 1;
                return true;
            }
        }

        public async void ExecuteAction()
        {
            Debug.Log($"[ItemPopupViewModel] ExecuteAction called. CanExecuteAction: {CanExecuteAction}, CurrentItemId: {CurrentItem?.Item?.Id}");
            if (!CanExecuteAction) return;
            
            string id = CurrentItem.Item.Id;
            bool success = false;
            if (onAction != null)
            {
                Debug.Log($"[ItemPopupViewModel] Invoking onAction for id: {id}");
                success = await onAction.Invoke(id);
                Debug.Log($"[ItemPopupViewModel] onAction returned success: {success}");
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
            OnPropertyChanged(nameof(CanExecuteAction));
            OnPropertyChanged(nameof(HasMultipleItems));
        }
    }
}
