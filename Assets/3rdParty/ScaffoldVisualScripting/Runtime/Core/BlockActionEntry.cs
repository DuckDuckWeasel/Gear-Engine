namespace Scaffold.VisualScripting
{
    internal sealed class BlockActionEntry
    {
        public BlockActionEntry(ActionTrack track, int actionIndex, IAction action)
        {
            Track = track;
            ActionIndex = actionIndex;
            Action = action;
        }

        public ActionTrack Track { get; }

        public int ActionIndex { get; }

        public IAction Action { get; }
    }
}
