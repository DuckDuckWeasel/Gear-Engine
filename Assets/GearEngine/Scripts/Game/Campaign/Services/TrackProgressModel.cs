using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;

namespace GearEngine.Campaign.Services
{
    public partial class TrackProgressModel : Model
    {
        [ObservableProperty]
        private int currentTrackIndex;

        private readonly System.Collections.Generic.Dictionary<string, float> bestTimes = new System.Collections.Generic.Dictionary<string, float>();

        public float? GetBestTimeSeconds(string trackId)
        {
            return !string.IsNullOrEmpty(trackId) && bestTimes.TryGetValue(trackId, out float time) ? time : (float?)null;
        }

        public void RecordBestTime(string trackId, float time)
        {
            if (!string.IsNullOrEmpty(trackId) && time > 0f && !float.IsNaN(time) && !float.IsInfinity(time))
            {
                bestTimes[trackId] = time;
            }
        }

    }
}
