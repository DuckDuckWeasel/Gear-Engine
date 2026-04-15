using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using Scaffold.Navigation.Contracts;
using System;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.GearEngine.Presentation
{
    public sealed class GearTestSceneBootstrap : MonoBehaviour, IInitializable
    {
        [SerializeField] private GearEngineStartData startData;

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
                GearEngineStartData data = startData != null ? startData : new GearEngineStartData();
                navigation.Open(new GearEngineViewModel(data, null));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearTestSceneBootstrap] Failed to open gear engine screen: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
