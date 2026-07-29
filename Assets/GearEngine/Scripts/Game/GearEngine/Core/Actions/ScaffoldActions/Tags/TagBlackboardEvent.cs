using GearEngine.GearEngine.Presentation.UI.Tags;

namespace Scaffold
{
    public readonly struct TagBlackboardEvent
    {
        public TagBlackboardEvent(TagSO tag, string eventName)
        {
            Tag = tag;
            EventName = eventName ?? string.Empty;
        }

        public TagSO Tag { get; }

        public string EventName { get; }
    }
}
