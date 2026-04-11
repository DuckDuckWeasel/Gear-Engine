using System;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.GearEngine.Presentation
{
    /// <summary>
    /// Opens the gear engine screen through <see cref="INavigation"/> after the scope container is built.
    /// </summary>
    public sealed class GearEngineNavigationEntry : IStartable
    {
        private readonly INavigation navigation;

        [Inject]
        public GearEngineNavigationEntry(INavigation navigation)
        {
            this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        public void Start()
        {
            try
            {
                navigation.Open(new GearEngineViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearEngineNavigationEntry] Failed to open gear engine screen: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
