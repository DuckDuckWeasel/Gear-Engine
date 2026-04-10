using CommunityToolkit.Mvvm.ComponentModel;
using Game.CarSimulation;
using Game.GearEngine;
using Scaffold.MVVM;
using VContainer;

namespace Game.Race
{
    public partial class RaceViewModel : ViewModel
    {
        [ObservableProperty]
        private bool canRace = true;

        private IGridManager gridManager;
        private IRaceDriver raceDriver;

        [Inject]
        public void Construct(IGridManager gridManager, IRaceDriver raceDriver)
        {
            this.gridManager = gridManager;
            this.raceDriver = raceDriver;
        }

        public void StartRace()
        {
            if (!canRace)
            {
                return;
            }

            CanRace = false;
            gridManager.Play();
            raceDriver.StartDriving();
        }
    }
}
