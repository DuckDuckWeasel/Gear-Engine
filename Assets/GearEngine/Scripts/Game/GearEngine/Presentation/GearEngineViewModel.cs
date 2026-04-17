using System;
using CommunityToolkit.Mvvm.ComponentModel;
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

        public GearEngineCoreViewModel Core => core;
        private GearEngineCoreViewModel core;

        private readonly GearEngineStartData startData;

        [Inject] private IGridManager gridManager;
        [Inject] private IObjectResolver objectResolver;

        [ObservableProperty] private bool isRunning = false;

        protected override void Initialize()
        {
            base.Initialize();
            core = new GearEngineCoreViewModel(startData, null);
            objectResolver?.Inject(core);
            BindChildViewModel(core);
        }

        internal void ToggleSimulation()
        {
            if (gridManager.IsRunning)
            {
                gridManager.Stop();
            }
            else
            {
                gridManager.Play();
            }
        }
    }
}