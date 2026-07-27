namespace Scaffold.VisualScripting
{
    public interface IActionMetadata
    {
        bool Enabled { get; }

        float Utility { get; }

        float Weight { get; }

        bool HasWeightOverride { get; }

        bool BlockDuringExecution { get; }
    }
}
