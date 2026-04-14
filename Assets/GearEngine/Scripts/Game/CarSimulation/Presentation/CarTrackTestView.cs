using Scaffold.MVVM;
using UnityEngine;
using TrackViewComponent = GearEngine.CarSimulation.Tracks.Track;

namespace GearEngine.CarSimulation.Presentation
{
    /// <summary>Sample view for the spline track test scene: hosts <see cref="GearEngine.CarSimulation.Tracks.Track"/> as a reusable ViewComponent.</summary>
    public sealed class CarTrackTestView : View<TrackViewModel>
    {
        [SerializeField] private TrackViewComponent track;

        protected override void OnBind()
        {
            if (track == null)
            {
                Debug.LogError("[CarTrackTestView] Assign the Track ViewComponent reference.");
                return;
            }

            track.Bind(viewModel);
            viewModel.Toggle(true);
        }

        protected override void OnUnbind()
        {
            if (track != null)
            {
                track.Unbind();
            }

            base.OnUnbind();
        }
    }
}
