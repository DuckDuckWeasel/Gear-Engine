using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Race
{
    public class RaceView : View<RaceViewModel>
    {
        [SerializeField]
        private Button raceButton;
        [SerializeField]
        private GameObject trackVisualRoot;
        [SerializeField]
        private GameObject gearBoardRoot;

        [Inject]
        public void Construct(RaceViewModel vm)
        {
            Bind(vm);
        }

        protected override void OnBind()
        {
            Bind<bool, bool>(() => viewModel.CanRace, OnCanRaceChanged);
            if (raceButton != null)
            {
                raceButton.onClick.AddListener(OnRaceButtonClicked);
            }
        }

        private void OnCanRaceChanged(bool canRace)
        {
            if (raceButton != null)
            {
                raceButton.interactable = canRace;
            }
        }

        private void OnRaceButtonClicked()
        {
            viewModel?.StartRace();
        }

        private void OnDestroy()
        {
            if (raceButton != null)
            {
                raceButton.onClick.RemoveListener(OnRaceButtonClicked);
            }
        }
    }
}
