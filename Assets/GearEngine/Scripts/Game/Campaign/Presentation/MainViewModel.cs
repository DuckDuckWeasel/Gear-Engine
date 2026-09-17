using System;
using System.Collections.Generic;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
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

        public bool CanNavigateTracks => tracks.Count > 1 && unlockedCount > 0;
        public bool IsTrackLocked => selectedIndex < 0 || !trackService.IsTrackUnlocked(tracks[selectedIndex].TrackId);
        public string TrackPosition => GetTrackStatus();

        private readonly List<TrackEntry> tracks = new List<TrackEntry>();
        private int selectedIndex = -1;
        private int unlockedCount;

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
            tracks.Clear();
            unlockedCount = 0;
            TrackEntry lockedPreview = null;
            foreach (TrackEntry entry in trackService.GetOrderedTracks())
            {
                if (entry?.Track == null)
                {
                    continue;
                }

                if (trackService.IsTrackUnlocked(entry.TrackId))
                {
                    tracks.Add(entry);
                    unlockedCount++;
                }
                else if (lockedPreview == null)
                {
                    lockedPreview = entry;
                }
            }

            if (lockedPreview != null)
            {
                tracks.Add(lockedPreview);
            }

            selectedIndex = tracks.FindIndex(entry => entry.Track == trackService.CurrentTrack);
            if (selectedIndex < 0 && tracks.Count > 0)
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

            selectedIndex = (selectedIndex + direction + tracks.Count) % tracks.Count;
            UpdateSelectedTrack();
        }

        private void UpdateSelectedTrack()
        {
            Track = selectedIndex < 0 ? null : new CampaignTrackPreviewViewModel(tracks[selectedIndex].Track);
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
            OnPropertyChanged(nameof(IsTrackLocked));
            OnPropertyChanged(nameof(TrackPosition));
        }

        private string GetTrackStatus()
        {
            if (selectedIndex < 0)
            {
                return "0 / 0";
            }

            if (!IsTrackLocked)
            {
                return $"{selectedIndex + 1} / {tracks.Count}";
            }

            string prerequisite = selectedIndex > 0
                ? tracks[selectedIndex - 1].Track.GetDisplayName().ToUpperInvariant()
                : "PREVIOUS TRACK";
            return $"TRACK LOCKED\nWIN 1ST ON {prerequisite} TO UNLOCK";
        }

        public void ClickedPlay()
        {
            try
            {
                if (IsTrackLocked || !trackService.TrySelectTrack(tracks[selectedIndex].TrackId))
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
