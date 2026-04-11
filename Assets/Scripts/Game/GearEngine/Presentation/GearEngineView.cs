using Scaffold.MVVM;
using UnityEngine;

namespace Game.GearEngine.Presentation
{
    public class GearEngineView : View<GearEngineViewModel>
    {
        [SerializeField] private SimulationControlView simControlView;
        [SerializeField] private GearInventoryView inventoryView;
        [SerializeField] private BoardView boardView;

        protected override void OnBind()
        {
            inventoryView.SetObjectResolver(viewModel.ObjectResolver);

            simControlView.Bind(viewModel.SimControl);
            inventoryView.Bind(viewModel.Inventory);
            boardView.Bind(viewModel.Board);
        }
    }
}
