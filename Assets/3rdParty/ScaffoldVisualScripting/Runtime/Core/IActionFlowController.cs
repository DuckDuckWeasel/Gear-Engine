namespace Scaffold.VisualScripting
{
    public interface IActionFlowController
    {
        void JumpTo(int actionIndex);

        void StopBlock();
    }
}
