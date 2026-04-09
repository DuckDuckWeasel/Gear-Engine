using UnityEngine;
using UnityEngine.UI;
using Scaffold.MVVM;
using TMPro;
using VContainer;

namespace Game.GearEngine.Presentation
{
    public class SimulationControlView : View<SimulationControlViewModel>
    {
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI buttonText;

        [Inject]
        public void Construct(SimulationControlViewModel vm)
        {
            Bind(vm);
        }


        protected override void OnBind()
        {
            Bind<bool, bool>(() => viewModel.IsRunning, OnSimulationStateChanged);

            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(OnToggleButtonClicked);
            }
        }

        private void OnToggleButtonClicked()
        {
            viewModel?.ToggleSimulation();
        }

        private void OnSimulationStateChanged(bool isRunning)
        {
            if (buttonText != null)
            {
                buttonText.text = isRunning ? "STOP ENGINE" : "START ENGINE";
                buttonText.color = isRunning ? Color.red : Color.green;
            }
        }

        private void OnDestroy()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(OnToggleButtonClicked);
            }
        }
    }
}
