using System;
using Scaffold.MVVM;
using UnityEngine;
using VContainer;

namespace Game.GearEngine.Presentation
{
    /// <summary>
    /// Binds <see cref="GearEngineView"/> in scenes that do not use navigation to create the view model.
    /// </summary>
    public sealed class GearEngineSceneBootstrap : MonoBehaviour
    {
        [Inject] private IObjectResolver resolver;
        [SerializeField] private GearEngineView gearEngineView;

        private void Start()
        {
            try
            {
                if (gearEngineView == null)
                {
                    Debug.LogError("[GearEngineSceneBootstrap] GearEngineView is not assigned.");
                    return;
                }

                var vm = new GearEngineViewModel();
                resolver.Inject(vm);
                resolver.Inject(gearEngineView);
                gearEngineView.Bind(vm);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearEngineSceneBootstrap] Failed to bootstrap presentation: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
