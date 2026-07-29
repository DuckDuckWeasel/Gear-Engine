namespace Scaffold
{
    public readonly struct BlackboardSavePointReachedEvent
    {
        public BlackboardSavePointReachedEvent(
            VisualScripting.BlackboardRuntimeInstanceId runtimeInstanceId,
            string key,
            string description,
            bool resumeOnLoad,
            bool isStartPoint)
        {
            RuntimeInstanceId = runtimeInstanceId;
            Key = key ?? string.Empty;
            Description = description ?? string.Empty;
            ResumeOnLoad = resumeOnLoad;
            IsStartPoint = isStartPoint;
        }

        public VisualScripting.BlackboardRuntimeInstanceId RuntimeInstanceId { get; }

        public string Key { get; }

        public string Description { get; }

        public bool ResumeOnLoad { get; }

        public bool IsStartPoint { get; }
    }
}
