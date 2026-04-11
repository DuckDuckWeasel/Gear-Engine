using System;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer.Unity;

namespace Game.Race
{
    public sealed class RaceNavigationStartup : IStartable
    {
        private readonly INavigation navigation;
        private readonly TrackPreviewViewModel trackPreview;

        public RaceNavigationStartup(INavigation navigation, TrackPreviewViewModel trackPreview)
        {
            this.navigation = navigation;
            this.trackPreview = trackPreview;
        }

        public void Start()
        {
            try
            {
                navigation.Open(trackPreview);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[RaceNavigationStartup] Failed to open track preview: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
