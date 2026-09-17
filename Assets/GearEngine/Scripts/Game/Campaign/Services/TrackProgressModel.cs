using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;

namespace GearEngine.Campaign.Services
{
    public partial class TrackProgressModel : Model
    {
        [ObservableProperty]
        private int currentTrackIndex;

        private readonly System.Collections.Generic.Dictionary<string, float> bestTimes = new System.Collections.Generic.Dictionary<string, float>();

        private readonly System.Collections.Generic.Dictionary<string, int> earnedStars = new System.Collections.Generic.Dictionary<string, int>();

        public int GetEarnedStars(string trackId)
        {
            return !string.IsNullOrEmpty(trackId) && earnedStars.TryGetValue(trackId, out int stars) ? stars : 0;
        }

        public void RecordEarnedStars(string trackId, int stars)
        {
            if (!string.IsNullOrEmpty(trackId))
            {
                earnedStars[trackId] = System.Math.Max(GetEarnedStars(trackId), System.Math.Clamp(stars, 0, 3));
            }
        }

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
