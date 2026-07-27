using Scaffold.Analytics;

namespace Scaffold.Tutorial.Events.Analytics
{
    public class TutorialStartedEvent : AnalyticsEvent
    {
        public string TutorialId { get; }

        public TutorialStartedEvent(string id) : base("tutorial_started")
        {
            TutorialId = id;
        }
    }
}
