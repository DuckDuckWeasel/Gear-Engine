using Scaffold.Events.Contracts;

namespace Scaffold.Tutorial.Events
{
    public sealed record TutorialLoadingEvent : ContextEvent
    {
        public bool IsLoading { get; }
        public string Message { get; }

        public TutorialLoadingEvent(bool isLoading, string message = null)
        {
            IsLoading = isLoading;
            Message = message;
        }
    }
}
