namespace Scaffold
{
    public sealed class BlockCompositeExecutionContext
    {
        public int CommandIndex { get; set; }

        public Blackboard Blackboard { get; set; }

        public bool SuppressSelectionChanges { get; set; }
    }
}
