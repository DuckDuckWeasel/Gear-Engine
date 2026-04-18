using System;
using GearEngine.CarSimulation.Tracks;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ActiveRaceView : View<ActiveRaceViewModel>
    {
        [SerializeField] private Track track;
        [SerializeField] private RaceHudViewComponent hud;

        protected override void OnBind()
        {
            ValidateHierarchy();
            track.Bind(viewModel.Track);
            hud.Bind(viewModel);
        }

        protected override void OnUnbind()
        {
            track.Unbind();
            base.OnUnbind();
        }

        private void ValidateHierarchy()
        {
            RequireReference(track, nameof(track));
            RequireReference(hud, nameof(hud));
        }

        private void RequireReference(UnityEngine.Object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException($"[ActiveRaceView] {name} reference is missing.");
            }
        }
    }
}
