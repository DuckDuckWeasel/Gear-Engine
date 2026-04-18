using System;
using GearEngine.CarSimulation.Tracks;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public class ActiveRaceView : View<ActiveRaceViewModel>
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

            track.Bind(viewModel.Track);
            hud.Bind(viewModel);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            SetRaceSceneRootsActive(true);
        }

        protected override void OnFocus()
        {
            base.OnFocus();
            SetRaceSceneRootsActive(true);
        }

        protected override void OnClose()
        {
            base.OnClose();
            SetRaceSceneRootsActive(false);
        }

        protected override void OnUnbind()
        {
            if (track != null)
            {
                track.ReleaseViewBinding();
            }

            SetRaceSceneRootsActive(false);
            base.OnUnbind();
        }

        private void SetRaceSceneRootsActive(bool active)
        {
            if (track != null)
            {
                track.gameObject.SetActive(active);
            }

            if (hud != null)
            {
                hud.gameObject.SetActive(active);
            }
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
