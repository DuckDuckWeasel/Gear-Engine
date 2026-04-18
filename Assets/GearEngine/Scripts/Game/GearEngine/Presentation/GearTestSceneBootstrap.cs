using Scaffold.Navigation.Contracts;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.GearEngine.Presentation
{
    public sealed class GearTestSceneBootstrap : MonoBehaviour, IInitializable
    {
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
                navigation.Open(new GearEngineViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearTestSceneBootstrap] Failed to open gear engine screen: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
