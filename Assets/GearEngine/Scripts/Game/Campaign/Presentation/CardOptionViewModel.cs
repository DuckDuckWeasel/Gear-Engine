using System;
using GearEngine.GearEngine.Config;
using Scaffold.MVVM;

using Scaffold.MVVM;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GearEngine.Campaign.Presentation
{
    public sealed partial class CardOptionViewModel : ViewModel
    {
        private readonly Action<CardOptionViewModel> onPick;

        public CardOptionViewModel(GearConfig config, Action<CardOptionViewModel> onPick)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            this.onPick = onPick;
        }

        public GearConfig Config { get; }

        [ObservableProperty]
        private bool canPick = true;

        internal void Pick()
        {
            onPick?.Invoke(this);
        }
    }
}
