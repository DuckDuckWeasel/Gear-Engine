using System;
using GearEngine.Campaign.Services;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Presentation
{
    public sealed class MainViewModel : ViewModel
    {
        public CampaignTrackPreviewViewModel Track { get; private set; }
        public TrackStatsViewModel Stats { get; private set; }

        [Inject] private ITrackService trackService;

        protected override void Initialize()
        {
            base.Initialize();

            Track = new CampaignTrackPreviewViewModel(trackService.CurrentTrack);
            BindChildViewModel(Track);
            Stats = new TrackStatsViewModel(trackService);
            BindChildViewModel(Stats);
        }

        public void ClickedPlay()
        {
            try
            {
                navigation.Open(new SetupViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MainViewModel] GoToSetup failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void ClickedTalentPerks()
        {
            try
            {
                navigation.Open(new TalentPerksViewModel(), true, new NavigationOptions() { CloseAllViews = true });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MainViewModel] ClickedTalentPerks failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
