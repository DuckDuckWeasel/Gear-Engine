using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.Assertions;
using VContainer;

namespace GearEngine.GearEngine.Presentation
{
    public class GearEngineView : View<GearEngineViewModel>
    {
        [SerializeField] private GearEngineCoreViewComponent coreView;
        [SerializeField] private GearSimulationControlViewComponent simControlView;

        [Inject] private IObjectResolver objectResolver;

        protected override void OnBind()
        {
            Assert.IsNotNull(viewModel, "[GearEngineView] viewModel is null.");
            Assert.IsNotNull(viewModel.Core, "[GearEngineView] viewModel.Core is null.");
            Assert.IsNotNull(coreView, "[GearEngineView] coreView is null.");
            Assert.IsNotNull(viewModel.SimControl, "[GearEngineView] viewModel.SimControl is null.");
            Assert.IsNotNull(simControlView, "[GearEngineView] simControlView is null.");

            objectResolver?.Inject(viewModel.Core);
            coreView.Bind(viewModel.Core);

            objectResolver?.Inject(viewModel.SimControl);
            simControlView.Bind(viewModel.SimControl);
        }
    }
}

