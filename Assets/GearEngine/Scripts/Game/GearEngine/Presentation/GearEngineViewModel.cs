using System;
using Scaffold.MVVM;
using VContainer;

namespace GearEngine.GearEngine.Presentation
{
    public partial class GearEngineViewModel : ViewModel
    {
        public GearEngineViewModel(GearEngineStartData startData)
        {
            this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
        }
        
        public GearEngineCoreViewModel Core { get; private set; }
        public GearSimulationControlViewModel SimControl { get; private set; } = new GearSimulationControlViewModel();

        private readonly GearEngineStartData startData;

        [Inject] private IObjectResolver objectResolver;

        protected override void Initialize()
        {
            base.Initialize();

            Core = new GearEngineCoreViewModel(startData, null);
            objectResolver?.Inject(Core);
            BindChildViewModel(Core);

            objectResolver?.Inject(SimControl);
            BindChildViewModel(SimControl);
        }
    }
}