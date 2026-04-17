using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using VContainer;

namespace GearEngine.GearEngine.Presentation
{
    public class GearEngineView : View<GearEngineViewModel>
    {
        [SerializeField] private GearEngineCoreViewComponent coreView;
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI buttonText;

        protected override void OnBind()
        {
            coreView.Bind(viewModel.Core);
            Bind<bool, bool>(() => viewModel.IsRunning, OnSimulationStateChanged);
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(OnToggleButtonClicked);
            }
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(OnToggleButtonClicked);
            }
        }

        private void OnToggleButtonClicked()
        {
            viewModel.ToggleSimulation();
        }

        private void OnSimulationStateChanged(bool isRunning)
        {
            if (buttonText != null)
            {
                buttonText.text = isRunning ? "STOP ENGINE" : "START ENGINE";
                buttonText.color = isRunning ? Color.red : Color.green;
            }
        }
    }
}

