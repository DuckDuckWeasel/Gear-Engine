using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;
using Scaffold.MVVM.Contracts;
using Scaffold.MVVM.Binding;
using VContainer;
using UnityEngine;

namespace Game.GearEngine.Presentation
{
    public partial class SimulationControlViewModel : ViewModel
    {
        private IGridManager gridManager;

        [ObservableProperty]
        private bool isRunning;

        [Inject]
        public void Construct(IGridManager gridManager)
        {
            this.gridManager = gridManager;
            if (this.gridManager != null)
            {
                IsRunning = this.gridManager.IsRunning;
            }
        }

        protected override void Initialize()
        {
            // Usually called by Scaffold Navigation, but we sync in Construct for DI.
        }

        public void ToggleSimulation()
        {
            if (gridManager == null) return;

            if (gridManager.IsRunning)
            {
                gridManager.Stop();
                IsRunning = false;
                Debug.Log($"<color=#ffaa00>[UI_Simulation]</color> Engine Manually STOPPED.");
            }
            else
            {
                gridManager.Play();
                IsRunning = true;
                Debug.Log($"<color=#55ff55>[UI_Simulation]</color> Engine Manually STARTED.");
            }
        }
    }
}
