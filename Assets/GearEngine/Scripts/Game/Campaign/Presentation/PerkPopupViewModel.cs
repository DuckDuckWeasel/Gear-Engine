using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;

namespace GearEngine.Campaign.Presentation
{
    public sealed class PerkPopupViewModel : ViewModel
    {
        private readonly IReadOnlyList<ItemPerkViewModel> perks;
        private readonly Func<string, Task<bool>> onBurn;
        private int currentIndex;

        public PerkPopupViewModel(IReadOnlyList<ItemPerkViewModel> perks, int initialIndex, Func<string, Task<bool>> onBurn)
        {
            this.perks = perks ?? throw new ArgumentNullException(nameof(perks));
            this.onBurn = onBurn;
            this.currentIndex = initialIndex;

            if (this.perks.Count > 0)
            {
                CurrentPerk = this.perks[currentIndex];
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            // In case we want to re-bind or keep the reference
            // Usually, if we pass existing ViewModels, they are already bound by TalentPerksViewModel.
            // But if we want to ensure lifecycle, we can bind it here if not already bound.
            // For now, no need to BindChildViewModel since it's already a child of TalentPerksViewModel.
            // However, to satisfy View nested bindings, we might need it.
            if (CurrentPerk != null)
            {
                // We won't call BindChildViewModel here if it causes duplicate bind issues, 
                // but let's assume it's safe or we create a new one to be safe.
                CurrentPerk = new ItemPerkViewModel(this.perks[currentIndex].Item, _ => { }, this.perks[currentIndex].Amount);
                BindChildViewModel(CurrentPerk);
            }
        }

        public ItemPerkViewModel CurrentPerk { get; private set; }

        public bool HasMultiplePerks => perks != null && perks.Count > 1;

        public bool CanBurn
        {
            get
            {
                if (perks.Count == 0 || CurrentPerk == null || CurrentPerk.Item == null) return false;
                return CurrentPerk.Amount > 1;
            }
        }

        public async void Burn()
        {
            if (!CanBurn) return;
            
            string id = CurrentPerk.Item.Id;
            bool success = false;
            if (onBurn != null)
            {
                success = await onBurn.Invoke(id);
            }
            
            if (success)
            {
                RefreshCurrentPerk(id);
            }
        }
        
        private void RefreshCurrentPerk(string expectedId)
        {
            int newIndex = -1;
            for (int i = 0; i < perks.Count; i++)
            {
                if (perks[i].Item.Id == expectedId)
                {
                    newIndex = i;
                    break;
                }
            }
            
            if (newIndex >= 0)
            {
                currentIndex = newIndex;
                UpdateCurrentPerk();
            }
            else
            {
                Close();
            }
        }

        public void Next()
        {
            if (perks.Count == 0) return;

            currentIndex = (currentIndex + 1) % perks.Count;
            UpdateCurrentPerk();
        }

        public void Previous()
        {
            if (perks.Count == 0) return;

            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = perks.Count - 1;
            }
            UpdateCurrentPerk();
        }

        public void Close()
        {
            navigation.Return();
        }

        private void UpdateCurrentPerk()
        {
            CurrentPerk = new ItemPerkViewModel(perks[currentIndex].Item, _ => { }, perks[currentIndex].Amount);
            BindChildViewModel(CurrentPerk);
            OnPropertyChanged(nameof(CurrentPerk));
            OnPropertyChanged(nameof(CanBurn));
            OnPropertyChanged(nameof(HasMultiplePerks));
        }
    }
}
