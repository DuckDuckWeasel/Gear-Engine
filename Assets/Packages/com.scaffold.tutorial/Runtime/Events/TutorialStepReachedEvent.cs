using Scaffold.Analytics;

namespace Scaffold.Tutorial.Events.Analytics
{
    public class TutorialStepReachedEvent : AnalyticsEvent
    {
        public string TutorialId { get; }
        public string StepName { get; }

        public TutorialStepReachedEvent(string id, string stepName) : base("tutorial_step_reached")
        {
            TutorialId = id;
            StepName = stepName;
        }
    }
}
