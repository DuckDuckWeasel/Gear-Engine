using System;
using GearEngine.CarSimulation.Tracks;
using GearEngine.FrustumFit;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public class ActiveRaceView : View<ActiveRaceViewModel>
    {
        [SerializeField] private TrackViewComponent track;
        [SerializeField] private BoardViewComponent board;
        [SerializeField] private RaceHudViewComponent hud;
        [SerializeField] private FrustumFitAnchor[] openTransitionAnchors;
        [SerializeField] private float openTransitionDurationSeconds = 0.35f;

        protected override void OnBind()
        {
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
            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(this, openTransitionAnchors, openTransitionDurationSeconds);
        }

        protected override void OnFocus()
        {
            base.OnFocus();
            SetRaceSceneRootsActive(true);
            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(this, openTransitionAnchors, openTransitionDurationSeconds);
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
                track.Unbind();
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

            if(board != null)
            {
                board.gameObject.SetActive(active);
            }
        }
    }
}
