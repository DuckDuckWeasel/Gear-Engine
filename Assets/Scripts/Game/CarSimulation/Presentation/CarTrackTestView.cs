using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    /// <summary>Sample view for the spline track test scene: hosts <see cref="Track"/> as a reusable ViewComponent.</summary>
    public sealed class CarTrackTestView : View<TrackViewModel>
    {
        [SerializeField] private Track track;

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
