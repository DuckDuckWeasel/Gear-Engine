using Scaffold.Analytics;

namespace GearEngine.Campaign.Analytics
{
    public class RaceStartedEvent : AnalyticsEvent
    {
        public RaceStartedEvent(string trackId, string carId) : base("race_started")
        {
            SetParameter("track_id", trackId);
            SetParameter("car_id", carId);
        }
    }
}
