using Game.GearEngine;
using Scaffold.Events;
using Scaffold.MVVM;
using VContainer;

namespace Game.GearEngine.Presentation
{
    public sealed class GearEngineViewModel : ViewModel
    {
        [Inject] private IGearEngineService engineService;
        [Inject] private IGridManager gridManager;
        [Inject] private GearNodeFactory nodeFactory;
        [Inject] private GearViewFactory viewFactory;
        [Inject] private BoardConfigSO boardConfig;
        [Inject] private EventController eventController;
        [Inject] private GearInventoryLoadoutSO loadout;

        public SimulationControlViewModel SimControl { get; } = new SimulationControlViewModel();
        public GearInventoryViewModel Inventory { get; } = new GearInventoryViewModel();
        public BoardViewModel Board { get; } = new BoardViewModel();

        private bool gearEngineInitialized;

        public void InitializeGearEngine()
        {
            if (gearEngineInitialized)
            {
                return;
            }

            gearEngineInitialized = true;

            if (loadout?.StartingGears != null)
            {
                foreach (var config in loadout.StartingGears)
                {
                    if (config != null)
                    {
                        Inventory.AddGearToInventory(config.CreateRuntimeData());
                    }
                }
            }

            SimControl.Initialize(engineService);
            Inventory.Initialize(engineService);
            Board.Initialize(engineService, gridManager, nodeFactory, viewFactory, Inventory, boardConfig, eventController);
        }
    }
}
