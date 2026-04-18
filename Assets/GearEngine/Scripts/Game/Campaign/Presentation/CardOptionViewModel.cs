using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine.Config;
using Scaffold.MVVM;

namespace GearEngine.Campaign.Presentation
{
    public sealed partial class CardOptionViewModel : ViewModel
    {
        public CardOptionViewModel(GearConfig gearConfig)
        {
            GearConfig = gearConfig ?? throw new ArgumentNullException(nameof(gearConfig));
        }

        public GearConfig GearConfig { get; }

        [ObservableProperty] private bool isSelected;

        internal void Select()
        {
            IsSelected = true;
        }

        internal void Deselect()
        {
            IsSelected = false;
        }
    }
}
