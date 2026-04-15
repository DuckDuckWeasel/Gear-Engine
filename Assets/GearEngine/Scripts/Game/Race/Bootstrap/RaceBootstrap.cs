using System;
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

        private INavigation navigation;

        [Inject]
        public void Construct(INavigation navigation)
        {
            this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        public void Initialize()
        {
            try
            {
                RaceStartData data = startData != null ? startData : CreateDefaultStartData();
                navigation.Open(new RaceViewModel(data));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaceBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static RaceStartData CreateDefaultStartData()
        {
            Debug.LogWarning("[RaceBootstrap] No RaceStartData assigned. Using empty defaults.");
            return new RaceStartData();
        }
    }
}
