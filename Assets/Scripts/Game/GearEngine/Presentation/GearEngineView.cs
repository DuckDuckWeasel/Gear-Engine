using Game.GearEngine;
using Scaffold.MVVM;
using UnityEngine;
using VContainer;

namespace Game.GearEngine.Presentation
{
    public class GearEngineView : View<GearEngineViewModel>
    {
        [SerializeField] private SimulationControlView simControlView;
        [SerializeField] private GearInventoryView inventoryView;
        [SerializeField] private BoardView boardView;

        [Inject] private IObjectResolver objectResolver;
        [Inject] private IGearEngineService engineService;
        [Inject] private BoardConfigSO boardConfig;
        [Inject] private GearViewFactory gearViewFactory;

        protected override void OnBind()
        {
            viewModel.InitializeGearEngine();

            inventoryView.SetObjectResolver(objectResolver);
            boardView.SetPresentationDependencies(engineService, boardConfig, gearViewFactory);

            simControlView.Bind(viewModel.SimControl);
            inventoryView.Bind(viewModel.Inventory);
            boardView.Bind(viewModel.Board);
        }
    }
}
