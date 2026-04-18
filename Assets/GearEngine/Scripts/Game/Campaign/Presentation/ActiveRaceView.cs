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
            if (track == null)
            {
                throw new InvalidOperationException(
                    "[ActiveRaceView] Track must be assigned on the scene instance (not baked into the prefab).");
            }

            track.gameObject.SetActive(true);
            track.Bind(viewModel.Track);
            hud.Bind(viewModel);
        }

        protected override void OnUnbind()
        {
            if (track != null)
            {
                track.ReleaseViewBinding();
                track.gameObject.SetActive(false);
            }

            base.OnUnbind();
        }

        private void ValidateHierarchy()
        {
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
