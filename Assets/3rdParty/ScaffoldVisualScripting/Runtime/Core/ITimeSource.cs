namespace Scaffold.VisualScripting
{
    public interface ITimeSource
    {
        float DeltaTime { get; }

        double ElapsedSeconds { get; }

        long Frame { get; }
    }
}
