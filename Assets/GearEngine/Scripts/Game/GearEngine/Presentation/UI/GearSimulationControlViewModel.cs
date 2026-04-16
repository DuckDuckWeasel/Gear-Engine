using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;
using UnityEngine;
using VContainer;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class GearSimulationControlViewModel : ViewModel
    {
        [ObservableProperty]
        private bool isRunning;

        [Inject] private IGearEngineService engineService;

        protected override void Initialize()
        {
            if (engineService != null)
            {
                SetState(engineService.IsRunning);
            }
        }

        public void SetState(bool running)
        {
            IsRunning = running;
            if (running)
            {
                Debug.Log($"<color=#55ff55>[UI_Simulation]</color> Engine UI State: RUNNING.");
            }
            else
            {
                Debug.Log($"<color=#ffaa00>[UI_Simulation]</color> Engine UI State: STOPPED.");
            }
        }

        public void ToggleSimulation()
        {
            if (engineService == null) return;
            
            if (engineService.IsRunning)
            {
                engineService.Stop();
            }
            else
            {
                engineService.Play();
            }
            
            SetState(engineService.IsRunning);
        }
    }
}
