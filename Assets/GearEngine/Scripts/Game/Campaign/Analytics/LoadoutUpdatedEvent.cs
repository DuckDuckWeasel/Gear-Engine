using Scaffold.Analytics;

namespace GearEngine.Campaign.Analytics
{
    public class LoadoutUpdatedEvent : AnalyticsEvent
    {
        public LoadoutUpdatedEvent(int gearCount) : base("loadout_updated")
        {
            SetParameter("gear_count", gearCount);
        }
    }
}
