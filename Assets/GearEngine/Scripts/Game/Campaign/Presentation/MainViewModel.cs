using System;
using System.Collections.Generic;
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

        public bool CanNavigateTracks => availableTracks.Count > 1;
        public string TrackPosition => selectedIndex < 0 ? "0 / 0" : $"{selectedIndex + 1} / {availableTracks.Count}";

        private readonly List<TrackEntry> availableTracks = new List<TrackEntry>();
        private int selectedIndex = -1;

        [Inject] private ITrackService trackService;

        protected override void Initialize()
        {
            base.Initialize();

            if (trackService == null)
            {
                throw new InvalidOperationException("[MainViewModel] Track service is required.");
            }

            RefreshTracks();
        }

        public void RefreshTracks()
        {
            availableTracks.Clear();
            foreach (TrackEntry entry in trackService.GetOrderedTracks())
            {
                if (entry?.Track != null && trackService.IsTrackUnlocked(entry.TrackId))
                {
                    availableTracks.Add(entry);
                }
            }

            selectedIndex = availableTracks.FindIndex(entry => entry.Track == trackService.CurrentTrack);
            if (selectedIndex < 0 && availableTracks.Count > 0)
            {
                selectedIndex = 0;
            }

            UpdateSelectedTrack();
        }

        public void NextTrack()
        {
            MoveTrack(1);
        }

        public void PreviousTrack()
        {
            MoveTrack(-1);
        }

        private void MoveTrack(int direction)
        {
            if (!CanNavigateTracks)
            {
                return;
            }

            selectedIndex = (selectedIndex + direction + availableTracks.Count) % availableTracks.Count;
            UpdateSelectedTrack();
        }

        private void UpdateSelectedTrack()
        {
            Track = selectedIndex < 0 ? null : new CampaignTrackPreviewViewModel(availableTracks[selectedIndex].Track);
            Stats = Track == null ? null : new TrackStatsViewModel(Track.Track, trackService.GetTrackProgress());
            if (Track != null)
            {
                BindChildViewModel(Track);
            }

            if (Stats != null)
            {
                BindChildViewModel(Stats);
            }

            OnPropertyChanged(nameof(Track));
            OnPropertyChanged(nameof(Stats));
            OnPropertyChanged(nameof(CanNavigateTracks));
            OnPropertyChanged(nameof(TrackPosition));
        }

        public void ClickedPlay()
        {
            try
            {
                if (selectedIndex < 0 || !trackService.TrySelectTrack(availableTracks[selectedIndex].TrackId))
                {
                    return;
                }

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
                ItemsScreenState config = ScriptableObject.CreateInstance<ItemsScreenState>();
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
                ItemsScreenState config = ScriptableObject.CreateInstance<ItemsScreenState>();
                config.TypeToDisplay = ItemScreenType.Gears;
                config.ShowBuyButton = false;
                config.ShowUnownedItems = false;
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
