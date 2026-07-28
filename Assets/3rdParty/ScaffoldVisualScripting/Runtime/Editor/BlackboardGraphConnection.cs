namespace Scaffold.VisualScripting.Editor
{
    public readonly struct BlackboardGraphConnection
    {
        public BlackboardGraphConnection(BlockDefinition source, BlockDefinition destination)
        {
            Source = source;
            Destination = destination;
        }

        public BlockDefinition Source { get; }

        public BlockDefinition Destination { get; }
    }
}
