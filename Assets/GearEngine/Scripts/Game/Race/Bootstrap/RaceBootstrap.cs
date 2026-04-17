using System;
using GearEngine.CarSimulation.Simulation;
using GearEngine.Race;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Race.Bootstrap
{
    public sealed class RaceBootstrap : MonoBehaviour, IInitializable
    {
        [SerializeField]
        private RaceStartData startData;

        [Inject]
        private INavigation navigation;

        [Inject]
        private IRaceSessionRunner raceSessionRunner;

        private void Update()
        {
            raceSessionRunner?.Tick();
        }

        public void Initialize()
        {
            try
            {
                navigation.Open(new RaceViewModel(startData));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaceBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
