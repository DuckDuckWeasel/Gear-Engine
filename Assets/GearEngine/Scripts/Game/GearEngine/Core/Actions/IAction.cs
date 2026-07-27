namespace GearEngine.Core.Actions
{
    /// <summary>
    /// A pure C# interface for executable actions.
    /// Used by the Fungus InvokeActionCommand wrapper to execute logic independent of MonoBehaviours.
    /// </summary>
    public interface IAction
    {
        void Execute(System.Action onComplete);
    }
}
