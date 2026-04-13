using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class SimulationControlViewModel : ViewModel
    {
        private IGearEngineService engineService;

        [ObservableProperty]
        private bool isRunning;

        public void Initialize(IGearEngineService engineService)
        {
            this.engineService = engineService;
            if (this.engineService != null)
            {
                IsRunning = this.engineService.IsRunning;
            }
        }

        protected override void Initialize()
        {
        }

        public void ToggleSimulation()
        {
            if (engineService == null)
            {
                return;
            }

            if (engineService.IsRunning)
            {
                engineService.Stop();
                IsRunning = false;
                Debug.Log($"<color=#ffaa00>[UI_Simulation]</color> Engine Manually STOPPED.");
            }
            else
            {
                engineService.Play();
                IsRunning = true;
                Debug.Log($"<color=#55ff55>[UI_Simulation]</color> Engine Manually STARTED.");
            }
        }
    }
}
