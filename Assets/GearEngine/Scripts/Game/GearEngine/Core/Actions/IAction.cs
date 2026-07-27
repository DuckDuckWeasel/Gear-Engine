namespace GearEngine.Core.Actions
{
    public interface IAction : Scaffold.VisualScripting.IAction
    {
        void Execute(System.Action onComplete);
    }
}
