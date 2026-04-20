using Scaffold.MVVM;
using TMPro;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultStatSlotView : ViewComponent<ResultStatSlotViewModel>
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TextMeshProUGUI valueText;

        protected override void OnBind()
        {
            base.OnBind();
            if (labelText != null)
            {
                labelText.text = viewModel.Label;
            }

            if (valueText != null)
            {
                valueText.text = viewModel.Value;
            }
        }
    }
}
