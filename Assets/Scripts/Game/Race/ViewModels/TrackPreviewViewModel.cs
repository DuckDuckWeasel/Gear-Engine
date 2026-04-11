using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Game.CarSimulation;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace Game.Race
{
    public partial class TrackPreviewViewModel : ViewModel
    {
        [ObservableProperty]
        private string trackName;

        private INavigation nav;
        private RaceViewModel raceScreen;

        [Inject]
        public void Construct(INavigation navigation, TrackDefinition trackDef, RaceViewModel raceScreen)
        {
            nav = navigation;
            this.raceScreen = raceScreen;
            TrackName = trackDef.TrackName;
        }

        public void NavigateToRace()
        {
            try
            {
                nav.Open(raceScreen);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrackPreviewViewModel] Navigation to Race failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
