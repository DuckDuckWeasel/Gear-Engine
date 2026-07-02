using Scaffold.Events.Contracts;

namespace GearEngine.Core.Config.Events
{
    /// <summary>
    /// Event used to broadcast the start and end of global async operations (e.g. Unity Cloud API calls).
    /// </summary>
    public sealed record GlobalLoadingEvent : ContextEvent
    {
        public bool IsLoading { get; }
        public string Message { get; }

        public GlobalLoadingEvent(bool isLoading, string message = null)
        {
            IsLoading = isLoading;
            Message = message;
        }
    }
}
