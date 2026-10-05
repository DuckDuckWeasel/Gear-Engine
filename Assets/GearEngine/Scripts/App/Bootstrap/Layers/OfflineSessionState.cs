namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class OfflineSessionState
    {
        public bool IsOffline { get; private set; }

        public void MarkOffline()
        {
            IsOffline = true;
        }

        public void MarkOnline()
        {
            IsOffline = false;
        }
    }
}
