using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.Race
{
    public class TrackPreviewView : View<TrackPreviewViewModel>
    {
        [SerializeField]
        private TextMeshProUGUI trackNameLabel;
        [SerializeField]
        private Button raceButton;

        [Inject]
        public void Construct(TrackPreviewViewModel vm)
        {
            Bind(vm);
        }

        protected override void OnBind()
        {
            Bind<string, string>(() => viewModel.TrackName, OnTrackNameChanged);
            if (raceButton != null)
            {
                raceButton.onClick.AddListener(OnRaceButtonClicked);
            }
        }

        private void OnTrackNameChanged(string name)
        {
            if (trackNameLabel != null)
            {
                trackNameLabel.text = name;
            }
        }

        private void OnRaceButtonClicked()
        {
            viewModel?.NavigateToRace();
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
