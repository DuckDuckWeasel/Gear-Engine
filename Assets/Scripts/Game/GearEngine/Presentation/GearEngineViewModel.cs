using Game.GearEngine;
using Scaffold.Events;
using Scaffold.MVVM;
using VContainer;

namespace Game.GearEngine.Presentation
{
    public sealed class GearEngineViewModel : ViewModel
    {
        [Inject] private IObjectResolver objectResolver;
        [Inject] private IGearEngineService engineService;
        [Inject] private IGridManager gridManager;
        [Inject] private GearNodeFactory nodeFactory;
        [Inject] private GearViewFactory viewFactory;
        [Inject] private BoardConfigSO boardConfig;
        [Inject] private EventController eventController;
        [Inject] private GearInventoryLoadoutSO loadout;

        public IObjectResolver ObjectResolver => objectResolver;

        public SimulationControlViewModel SimControl { get; } = new SimulationControlViewModel();
        public GearInventoryViewModel Inventory { get; } = new GearInventoryViewModel();
        public BoardViewModel Board { get; } = new BoardViewModel();

        protected override void Initialize()
        {
            base.Initialize();
            BindChildViewModel(SimControl);
            BindChildViewModel(Inventory);
            BindChildViewModel(Board);

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
