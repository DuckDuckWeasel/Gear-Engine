using System;
using GearEngine.Campaign.Presentation;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap
{
    public sealed class CampaignBootstrap : MonoBehaviour, IInitializable
    {
        [Inject] private INavigation navigation;

        public void Initialize()
        {
            try
            {
                navigation.Open(new MainViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CampaignBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
