using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Game.CarSimulation;
using Game.Race.Navigation;
using Scaffold.MVVM;
using Scaffold.Navigation;
using UnityEngine;
using VContainer;

namespace Game.Race
{
    public partial class TrackPreviewViewModel : ViewModel
    {
        [ObservableProperty]
        private string trackName;

        private INavigator navigator;
        private ViewConfig raceViewConfig;

        [Inject]
        public void Construct(INavigator navigator, TrackDefinition trackDef, RaceViewConfigRef raceViewConfigRef)
        {
            this.navigator = navigator;
            raceViewConfig = raceViewConfigRef.Config;
            TrackName = trackDef.TrackName;
        }

        public async void NavigateToRace()
        {
            try
            {
                await navigator.OpenAsync(raceViewConfig);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrackPreviewViewModel] Navigation to Race failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
