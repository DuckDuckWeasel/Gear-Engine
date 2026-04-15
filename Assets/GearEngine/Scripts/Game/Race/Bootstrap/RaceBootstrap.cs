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
        private ITrackSimulationRunner trackSimulationRunner;

        private void Update()
        {
            trackSimulationRunner?.Tick();
        }

        public void Initialize()
        {
            try
            {
                if (startData == null)
                {
                    throw new InvalidOperationException("[RaceBootstrap] RaceStartData is missing.");
                }

                navigation.Open(new RaceViewModel(startData));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaceBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
