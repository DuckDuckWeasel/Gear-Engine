using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;

namespace GearEngine.Campaign.Presentation
{
    /// <summary>
    /// Represents one distinct perk type in the TalentPerks screen, including how many copies the player owns.
    /// </summary>
    public sealed partial class PerkItemViewModel : ViewModel
    {
        private readonly Action<PerkItemViewModel> onBurn;

        public PerkItemViewModel(string perkId, int count, Action<PerkItemViewModel> onBurn)
        {
            if (string.IsNullOrEmpty(perkId))
            {
                throw new ArgumentException("perkId cannot be null or empty.", nameof(perkId));
            }

            PerkId = perkId;
            Count = count;
            this.onBurn = onBurn ?? throw new ArgumentNullException(nameof(onBurn));
        }

        /// <summary>Unique identifier of this perk (matches the catalog / backend key).</summary>
        public string PerkId { get; }

        /// <summary>Number of copies of this perk the player owns.</summary>
        [ObservableProperty]
        private int count;

        /// <summary>True if the player has at least one copy to burn.</summary>
        public bool CanBurn => Count > 0;

        /// <summary>Called by the view's Burn button for this perk slot.</summary>
        internal void Burn()
        {
            onBurn?.Invoke(this);
        }
    }
}
