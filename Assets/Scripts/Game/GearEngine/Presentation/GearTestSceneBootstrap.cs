using System;
using Game.GearEngine;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace Game.GearEngine.Presentation
{
    public sealed class GearTestSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GearEngineStartData startData;

        private INavigation navigation;

        [Inject]
        public void Construct(INavigation navigation)
        {
            this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        private void Start()
        {
            try
            {
                GearEngineStartData data = startData != null ? startData : new GearEngineStartData();
                navigation.Open(new GearEngineViewModel(data));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearTestSceneBootstrap] Failed to open gear engine screen: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
