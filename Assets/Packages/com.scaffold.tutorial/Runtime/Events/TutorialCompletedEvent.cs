using Scaffold.Analytics;

namespace Scaffold.Tutorial.Events.Analytics
{
    public class TutorialCompletedEvent : AnalyticsEvent
    {
        public string TutorialId { get; }
        public bool Skipped { get; }

        public TutorialCompletedEvent(string id, bool skipped) : base("tutorial_completed")
        {
            TutorialId = id;
            Skipped = skipped;
        }
    }
}
