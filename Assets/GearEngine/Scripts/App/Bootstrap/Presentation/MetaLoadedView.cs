using System;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Presentation
{
    public sealed class MetaLoadedView : View<MetaLoadedViewModel>
    {
        [SerializeField]
        private TextMeshProUGUI messageLabel;

        protected override void OnBind()
        {
            if (messageLabel == null)
            {
                throw new InvalidOperationException("[MetaLoadedView] Assign messageLabel.");
            }

            messageLabel.text = "Meta loaded — backend reachable.";
        }
    }
}
