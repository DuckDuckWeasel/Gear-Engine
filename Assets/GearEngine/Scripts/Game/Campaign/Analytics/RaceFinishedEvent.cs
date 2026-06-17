using Scaffold.Analytics;

namespace GearEngine.Campaign.Analytics
{
    public class RaceFinishedEvent : AnalyticsEvent
    {
        public RaceFinishedEvent(string trackId, string carId, float raceTime, int lapCount, int score, bool isGoodResult) : base("race_finished")
        {
            SetParameter("track_id", trackId);
            SetParameter("car_id", carId);
            SetParameter("race_time", raceTime);
            SetParameter("lap_count", lapCount);
            SetParameter("score", score);
            SetParameter("is_good_result", isGoodResult);
        }
    }
}
