using GearEngine.SceneFoundation.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.SceneFoundation.Bootstrap
{
    public class GlobalLoadingInstaller : IInstaller
    {
        private static GlobalLoadingOverlay globalInstance;
        private readonly GlobalLoadingOverlay globalLoadingPrefab;

        public GlobalLoadingInstaller(GlobalLoadingOverlay globalLoadingPrefab)
        {
            this.globalLoadingPrefab = globalLoadingPrefab;
        }

        public void Install(IContainerBuilder builder)
        {
            if (globalLoadingPrefab != null)
            {
                if (globalInstance == null)
                {
                    globalInstance = Object.Instantiate(globalLoadingPrefab);
                    globalInstance.name = "GlobalLoadingCanvas";
                    Object.DontDestroyOnLoad(globalInstance.gameObject);
                }

                // Injecta as dependências (como IEventBus) da scope atual na instância global
                builder.RegisterComponent(globalInstance);
            }
            else
            {
                Debug.LogError("[GlobalLoadingInstaller] globalLoadingPrefab is null! Cannot instantiate GlobalLoadingCanvas.");
            }
        }
    }
}
