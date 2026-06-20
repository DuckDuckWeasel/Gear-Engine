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
                var config = ScriptableObject.CreateInstance<ItemsScreenState>();
                config.TypeToDisplay = ItemScreenType.Perks;
                config.ShowBuyButton = true;
                config.ShowUnownedItems = true;
                config.Title = "Storage";
                config.Subtitle = "MAX OUT YOUR GEAR";
                
                navigation.Open(new ItemsViewModel(config), true, new NavigationOptions() { CloseAllViews = true });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MainViewModel] ClickedTalentPerks failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void ClickedGears()
        {
            try
            {
                var config = ScriptableObject.CreateInstance<ItemsScreenState>();
                config.TypeToDisplay = ItemScreenType.Gears;
                config.ShowBuyButton = false;
                config.ShowUnownedItems = true;
                config.Title = "Garage";
                config.Subtitle = "FIX AND REPAIR";
                
                navigation.Open(new ItemsViewModel(config), true, new NavigationOptions() { CloseAllViews = true });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MainViewModel] ClickedGears failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
