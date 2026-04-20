using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using VContainer;

namespace GearEngine.GearEngine.Presentation
{
    public partial class GearEngineViewModel : ViewModel
    {
        internal IDragService DragService => dragService;

        [ObservableProperty] private bool isSimulationRunning;

        internal BoardViewModel Board;
        internal GearInventoryViewModel Inventory;
        internal TrashZoneViewModel TrashZone;

        [Inject] private IGearEngineService engineService;
        [Inject] private IBoardService boardService;
        [Inject] private IRaceInventoryService inventoryService;
        [Inject] private IGearPresentationTransferService presentationTransferService;
        [Inject] private IDragService dragService;
        [Inject] private GearEngineFeatureToggleSO featureToggle;

        protected override void Initialize()
        {
            base.Initialize();
            Board = CreateBoard();
            BindChildViewModel(Board);
            Inventory = CreateInventory();
            BindChildViewModel(Inventory);
            TrashZone = CreateTrashZone();
            BindChildViewModel(TrashZone);
            Bind(() => Board.IsSimulationRunning, () => IsSimulationRunning);
        }

        private BoardViewModel CreateBoard()
        {
            return new BoardViewModel(boardService, inventoryService, engineService);
        }

        private GearInventoryViewModel CreateInventory()
        {
            return new GearInventoryViewModel(engineService, inventoryService);
        }

        private TrashZoneViewModel CreateTrashZone()
        {
            return new TrashZoneViewModel(engineService, Board, presentationTransferService, featureToggle);
        }

        internal void ToggleSimulation()
        {
            Board?.ToggleSimulation();
        }
    }
}
