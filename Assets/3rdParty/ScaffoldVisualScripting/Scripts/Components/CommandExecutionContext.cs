namespace Scaffold
{
    public sealed class CommandExecutionContext
    {
        public Block Block { get; set; }

        public Command Command { get; set; }

        public CommandTrack Track { get; set; }

        public Blackboard Blackboard { get; set; }

        public bool IsIncluded { get; set; }

        public bool IsPrimaryTrack { get; set; }

        public bool SuppressSelectionChanges { get; set; }
    }
}
