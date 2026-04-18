using System;
using GearEngine.Campaign.Presentation;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Simulation;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap
{
    public sealed class CampaignBootstrap : MonoBehaviour, IInitializable
    {
        [Inject] private INavigation navigation;
        [Inject] private ITrackService trackService;
        [Inject] private TrackSimulationFactory trackFactory;
        [Inject] private IRaceSessionRunner raceSessionRunner;

        private void Update()
        {
            raceSessionRunner?.Tick();
        }

        public void Initialize()
        {
            try
            {
                LapRaceSession session = trackFactory.Create(trackService.CurrentCar, trackService.CurrentTrack);
                trackService.SetCurrentSession(session);
                raceSessionRunner.SetSession(session);
                navigation.Open(new MainViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CampaignBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
