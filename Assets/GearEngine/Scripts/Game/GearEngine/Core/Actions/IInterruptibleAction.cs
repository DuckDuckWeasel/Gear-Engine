namespace GearEngine.Core.Actions
{
    /// <summary>
    /// Allows a running action to stop its asynchronous work immediately.
    /// </summary>
    public interface IInterruptibleAction
    {
        void Interrupt();
    }
}
