namespace Scaffold
{
    public interface ICompositeExecutionStatusProvider
    {
        CompositeExecutionStatus LastCompositeExecutionStatus { get; }
    }
}
