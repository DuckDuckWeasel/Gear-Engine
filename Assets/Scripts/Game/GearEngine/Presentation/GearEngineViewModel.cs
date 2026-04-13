using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using VContainer;

namespace GearEngine.GearEngine.Presentation
{
    public sealed class GearEngineViewModel : ViewModel
    {
        private readonly GearEngineStartData startData;

        [Inject] private IGearEngineService engineService;
        [Inject] private IGridManager gridManager;
        [Inject] private GearNodeFactory nodeFactory;
        [Inject] private BoardConfigSO boardConfig;
        [Inject] private IEventBus eventBus;
        [Inject] private GearEngineFeatureToggleSO featureToggle;

        public GearEngineFeatureToggleSO FeatureToggle => featureToggle;

        public SimulationControlViewModel SimControl { get; } = new SimulationControlViewModel();
        public GearInventoryViewModel Inventory { get; } = new GearInventoryViewModel();
        public BoardViewModel Board { get; } = new BoardViewModel();

        public GearEngineViewModel(GearEngineStartData startData)
        {
            this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
        }

        protected override void Initialize()
        {
            base.Initialize();

            BindChildViewModel(SimControl);
            BindChildViewModel(Inventory);
            BindChildViewModel(Board);

            SimControl.Initialize(engineService);
            Inventory.Initialize(engineService);

            if (startData.InventoryGears != null)
            {
                Inventory.LoadInventory(startData.InventoryGears);
            }

            Board.Initialize(engineService, gridManager, nodeFactory, boardConfig, eventBus, featureToggle);

            if (startData.BoardLayout != null)
            {
                Board.LoadLayout(startData.BoardLayout);
            }
        }
    }
}
