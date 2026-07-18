using Scaffold.Analytics;

namespace GearEngine.Core.Actions.ScaffoldActions.Analytics
{
    /// <summary>
    /// A generic analytics event that can be constructed at runtime 
    /// with an arbitrary event name and key-value parameters.
    /// </summary>
    public class DynamicAnalyticsEvent : AnalyticsEvent
    {
        public DynamicAnalyticsEvent(string eventName) : base(eventName)
        {
        }

        public void AddParameter(string key, object value)
        {
            SetParameter(key, value);
        }
    }
}
